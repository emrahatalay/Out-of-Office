using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Gerçek karakter modellerini oyuna hazırlar: içe aktarma ayarları (mobil için sıkıştırma,
    /// Humanoid iskelet), kıvrımlı dünya materyali, ortak Animator denetleyicisi ve prefab.
    ///
    /// Kaynak dosyalar Art/Characters altındadır (model: &lt;Ad&gt;/&lt;Ad&gt;.fbx + &lt;Ad&gt;_BaseColor.png,
    /// klipler: Animations/Run.fbx ve Actions.fbx). Klipler Humanoid olduğu için tek set üç
    /// karakterde de oynar; yeni karakter için yalnızca model ve doku eklenir.
    /// Tekrar çalıştırmak güvenlidir; prefab ve denetleyicinin GUID'leri korunur.
    /// </summary>
    public static class CharacterModelBuilder
    {
        const string ArtDir = "Assets/_Game/Art/Characters";
        const string AnimationDir = ArtDir + "/Animations";
        const string ControllerPath = AnimationDir + "/Character.controller";
        const string ChaserControllerPath = AnimationDir + "/Chaser.controller";
        const string PrefabDir = "Assets/_Game/Prefabs/Characters";
        const string WorldShaderPath = "Assets/_Game/Shaders/CurvedLit.shader";

        // Klipler bu karakterin iskeletiyle kaydedildi; klip dosyaları duruş pozunu ondan alır.
        const string ClipSourceCharacter = "Developer";

        // Doku üst sınırı: karakter ekranda küçük görünür, 1024 fazlasıyla yeter.
        const int MaxTextureSize = 1024;

        readonly struct CharacterSpec
        {
            public readonly string Name;
            public readonly float Height;
            /// <summary>Üretilen prefab'ın adı (Prefabs/Characters altında).</summary>
            public readonly string PrefabName;
            /// <summary>Kovalayan mı? Öyleyse oyuncununki yerine kovalayan denetleyicisini alır.</summary>
            public readonly bool Chaser;
            /// <summary>Dokudaki neon bölgelerin ışıma gücü (maske dokunun alfasındadır).</summary>
            public readonly float Glow;

            public CharacterSpec(string name, float height, float glow, string prefabName = null, bool chaser = false)
            {
                Name = name;
                Height = height;
                Glow = glow;
                PrefabName = prefabName ?? "Character_" + name;
                Chaser = chaser;
            }
        }

        // Boylar metre. Çarpışma kapsülü GameConfig'den gelir; bunlar yalnızca görseldir.
        static readonly CharacterSpec[] Characters =
        {
            new CharacterSpec("Developer", 1.75f, 2.5f),
            new CharacterSpec("Designer", 1.7f, 2.5f),
            new CharacterSpec("Intern", 1.55f, 2f),
            new CharacterSpec("HR9000", 2.3f, 3f, "Chaser_HR9000", chaser: true),
        };

        readonly struct ClipSpec
        {
            public readonly string Name;
            /// <summary>Animations klasöründeki kaynak dosya (uzantısız) ve içindeki kaydın adından bir parça.</summary>
            public readonly string File, Take;
            public readonly bool Loop;
            /// <summary>Kaynak kayıttaki kare aralığı (30 kare/sn). Son kare 0 ise kaydın tamamı alınır.</summary>
            public readonly int FirstFrame, LastFrame;
            /// <summary>Dikey hareket pozda kalsın mı? Zıplamada kalmaz: yüksekliği oynanış verir.</summary>
            public readonly bool KeepHeight;
            /// <summary>Yatay hareket pozda kalsın mı? İleri giden kayıtlarda kapalıdır: konumu oynanış verir.</summary>
            public readonly bool KeepPlanar;

            public ClipSpec(string name, string file, string take, bool loop, int firstFrame, int lastFrame, bool keepHeight, bool keepPlanar)
            {
                Name = name;
                File = file;
                Take = take;
                Loop = loop;
                FirstFrame = firstFrame;
                LastFrame = lastFrame;
                KeepHeight = keepHeight;
                KeepPlanar = keepPlanar;
            }
        }

        // Kare aralıkları kaynak kayıtlardan ölçüldü:
        // - Zıplama: yalnızca havadaki bölüm (16-32). Çömelme ve iniş atılır; klip AirPhase ile sürülür.
        // - Kayma: alçalma ve yerde kayma (6-21). Son karede bekler, kalkışı Koşu'ya geçiş yapar.
        // - Tökezleme: sırttan darbe yiyip sendeleme ve toparlanma (3-40); ileri gidişi oynanış verir.
        // - Yakalanma: geriye düşüş (3-68); yerde kalır.
        // - Oturma: yalnızca dik oturulan bölüm (0-38); kaydın devamında karakter öne kapanır. Kayıtta
        //   karakter merkezden 18 cm yana ve 27 cm geriye kaymış oturur; yatay konum pozda tutulmaz,
        //   gövde ağırlık merkeziyle şeridin ortasına gelir.
        // - Oturma ve asılma: döngü; sandalyenin oturağı ve dronun tutamağı bu pozlara göre ölçülüdür.
        static readonly ClipSpec[] Clips =
        {
            new ClipSpec("Idle", "Actions", "Idle", true, 0, 0, true, true),
            new ClipSpec("Run", "Run", "running", true, 0, 0, true, true),
            new ClipSpec("Jump", "Actions", "Regular_Jump", false, 16, 32, false, true),
            new ClipSpec("Slide", "Actions", "slide_light", false, 6, 21, true, false),
            new ClipSpec("Stumble", "Actions2", "Hit_in_Back", false, 3, 40, true, false),
            new ClipSpec("Caught", "Actions2", "falling_down", false, 3, 68, true, true),
            new ClipSpec("Seated", "Actions2", "Chair_Sit", true, 0, 38, true, false),
            new ClipSpec("Hanging", "Actions2", "Bar_Hang", true, 0, 0, true, true),
        };

        /// <summary>Gerçek prefab kuruluysa onu, değilse verilen geçici prefab'ı döndürür.</summary>
        public static GameObject PrefabOr(string prefabName, GameObject placeholder)
        {
            var real = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{prefabName}.prefab");
            return real != null ? real : placeholder;
        }

        [MenuItem("Out of Office/Karakter Modellerini Kur")]
        public static void BuildAllFromMenu()
        {
            Debug.Log(BuildAll());
        }

        public static string BuildAll()
        {
            // Sıra önemli: önce modeller (avatarlar oluşsun), sonra klipler, en son prefab'lar.
            foreach (var spec in Characters) ImportCharacter(spec);

            var clips = ImportClips((ModelImporter)AssetImporter.GetAtPath(ModelPath(ClipSourceCharacter)));
            var controller = BuildController(clips);
            var chaserController = BuildChaserController(clips);

            int built = 0;
            foreach (var spec in Characters)
                if (BuildPrefab(spec, spec.Chaser ? chaserController : controller)) built++;

            AssetDatabase.SaveAssets();
            return $"Karakter modelleri hazır: {built}/{Characters.Length}. Klip: {controller.animationClips.Length}. " +
                   "Tanım asset'lerine bağlamak için Placeholder İçeriği Yeniden Oluştur'u çalıştır.";
        }

        // ------------------------------------------------------------ Karakter

        static string ModelPath(string name) { return $"{ArtDir}/{name}/{name}.fbx"; }
        static string TexturePath(string name) { return $"{ArtDir}/{name}/{name}_BaseColor.png"; }

        static void ImportCharacter(CharacterSpec spec)
        {
            if (!File.Exists(ModelPath(spec.Name)))
            {
                Debug.LogWarning($"Karakter modeli yok, atlandı: {ModelPath(spec.Name)}");
                return;
            }

            ConfigureTexture(TexturePath(spec.Name));
            ConfigureModel(ModelPath(spec.Name), spec.Height);
        }

        static bool BuildPrefab(CharacterSpec spec, AnimatorController controller)
        {
            string dir = $"{ArtDir}/{spec.Name}";
            string modelPath = ModelPath(spec.Name);
            string texturePath = TexturePath(spec.Name);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) return false;

            var avatar = LoadSubAsset<Avatar>(modelPath);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                Debug.LogWarning($"{spec.Name}: Humanoid avatar kurulamadı; klipler oynamaz. Modelin iskeletini kontrol et.");

            var material = BuildMaterial($"{dir}/{spec.Name}.mat", texturePath, spec.Glow);

            // Kök boş nesnedir; model onun çocuğudur. Yön ya da ölçek düzeltmesi gerekirse
            // FBX'e dokunmadan burada yapılır.
            var root = new GameObject(spec.PrefabName);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            instance.name = "Model";

            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

                if (renderer is SkinnedMeshRenderer skinned)
                {
                    skinned.quality = SkinQuality.Bone2;
                    skinned.updateWhenOffscreen = false;
                    skinned.skinnedMotionVectors = false;
                }
            }

            var animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            EnsureFolder(PrefabDir);
            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/{root.name}.prefab");
            Object.DestroyImmediate(root);
            return true;
        }

        static void ConfigureTexture(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = false;
            importer.isReadable = false;
            // Alfa saydamlık değil ışıma maskesidir (model hattı yazar).
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.maxTextureSize = MaxTextureSize;
            importer.textureCompression = TextureImporterCompression.Compressed;

            // Mobilde ASTC 6x6: 1024'lük doku ~0,6 MB (sıkıştırmasız RGB'nin altıda biri).
            foreach (string platform in new[] { "Android", "iPhone" })
            {
                var settings = importer.GetPlatformTextureSettings(platform);
                settings.overridden = true;
                settings.maxTextureSize = MaxTextureSize;
                settings.format = TextureImporterFormat.ASTC_6x6;
                settings.compressionQuality = 50;
                importer.SetPlatformTextureSettings(settings);
            }

            importer.SaveAndReimport();
        }

        static void ConfigureModel(string path, float targetHeight)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            ApplySharedModelSettings(importer);
            importer.importAnimation = false;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            // Kemik Transform'ları sahneye çıkmaz: daha az nesne, daha ucuz animasyon.
            importer.optimizeGameObjects = true;
            importer.SaveAndReimport();

            // Model hangi ölçüyle gelirse gelsin hedef boya getirilir.
            float height = MeasureHeight(path);
            if (height <= 0f || Mathf.Abs(height - targetHeight) < 0.005f) return;

            importer.globalScale *= targetHeight / height;
            // Avatar eski ölçüdeki kemik boylarını hatırlar; yeni ölçüyle baştan eşlensin.
            var description = importer.humanDescription;
            description.human = new HumanBone[0];
            description.skeleton = new SkeletonBone[0];
            importer.humanDescription = description;
            importer.SaveAndReimport();
        }

        static void ApplySharedModelSettings(ModelImporter importer)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.weldVertices = true;
            importer.indexFormat = ModelImporterIndexFormat.UInt16;
            importer.importNormals = ModelImporterNormals.Import;
            // Normal haritası yok; teğet verisi boşa bellek olurdu.
            importer.importTangents = ModelImporterTangents.None;
            importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.maxBonesPerVertex = 2;
            importer.minBoneWeight = 0.01f;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;
        }

        /// <summary>
        /// Modelin duruş pozundaki gerçek boyu. Renderer sınırları kullanılmaz: skinli mesh'te
        /// animasyon payı içerir ve olduğundan büyük çıkar.
        /// </summary>
        static float MeasureHeight(string modelPath)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) return 0f;

            var instance = Object.Instantiate(model);
            var baked = new Mesh();
            var vertices = new List<Vector3>();
            float min = float.MaxValue, max = float.MinValue;

            foreach (var skinned in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinned.BakeMesh(baked, true);
                baked.GetVertices(vertices);
                var transform = skinned.transform;
                foreach (var vertex in vertices)
                {
                    float y = (transform.rotation * vertex + transform.position).y;
                    if (y < min) min = y;
                    if (y > max) max = y;
                }
            }

            Object.DestroyImmediate(baked);
            Object.DestroyImmediate(instance);
            return max > min ? max - min : 0f;
        }

        static Material BuildMaterial(string path, string texturePath, float glow)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(WorldShaderPath);
            if (shader == null) throw new System.InvalidOperationException("Kıvrımlı dünya shader'ı bulunamadı: " + WorldShaderPath);

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.shader != shader) material.shader = shader;
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.15f);
            material.SetFloat("_Metallic", 0f);
            material.SetColor("_EmissionColor", Color.black);
            material.SetFloat("_GlowStrength", glow);
            material.enableInstancing = false;
            EditorUtility.SetDirty(material);
            return material;
        }

        // ------------------------------------------------------------ Animasyon

        /// <summary>Kaynak dosyaların klip ayarlarını yazar ve klipleri adlarıyla döndürür.</summary>
        static Dictionary<string, AnimationClip> ImportClips(ModelImporter restPoseSource)
        {
            var files = new List<string>();
            foreach (var spec in Clips)
                if (!files.Contains(spec.File)) files.Add(spec.File);

            var clips = new Dictionary<string, AnimationClip>();
            foreach (string file in files)
            {
                string path = $"{AnimationDir}/{file}.fbx";
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"Animasyon dosyası yok: {path}");
                    continue;
                }

                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                ApplySharedModelSettings(importer);
                importer.importAnimation = true;
                importer.optimizeGameObjects = false;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

                // Önce kendi iskeletiyle içe aktarılır (kemik boyları ve kayıt listesi oluşur),
                // sonra duruş pozu modelden alınır.
                ClearRig(importer);
                importer.SaveAndReimport();
                if (restPoseSource != null) CopyRestPose(importer, restPoseSource);

                var takes = importer.defaultClipAnimations;

                var configured = new List<ModelImporterClipAnimation>();
                foreach (var spec in Clips)
                {
                    if (spec.File != file) continue;

                    var take = System.Array.Find(takes, t => t.takeName.Contains(spec.Take));
                    if (take == null)
                    {
                        Debug.LogWarning($"{path}: '{spec.Take}' kaydı yok. Var olanlar: {string.Join(", ", System.Array.ConvertAll(takes, t => t.takeName))}");
                        continue;
                    }

                    configured.Add(Configure(take, spec));
                }

                importer.clipAnimations = configured.ToArray();
                importer.SaveAndReimport();

                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__")) clips[clip.name] = clip;
            }

            return clips;
        }

        static void ClearRig(ModelImporter importer)
        {
            var description = importer.humanDescription;
            description.human = new HumanBone[0];
            description.skeleton = new SkeletonBone[0];
            importer.humanDescription = description;
        }

        /// <summary>
        /// Klip dosyaları T pozunda değil ilk karelerinde açılır; Unity avatarı o pozdan kurarsa
        /// tüm klipler kayık oynar. Kemik eşlemesi ve T pozu (dönüşler) aynı iskeleti taşıyan
        /// modelden alınır; kemik boyları dosyanın kendisinde kalır.
        /// </summary>
        static void CopyRestPose(ModelImporter importer, ModelImporter source)
        {
            var description = importer.humanDescription;
            var reference = source.humanDescription;
            var skeleton = description.skeleton;

            for (int i = 0; i < skeleton.Length; i++)
            {
                int match = System.Array.FindIndex(reference.skeleton, bone => bone.name == skeleton[i].name);
                if (match >= 0) skeleton[i].rotation = reference.skeleton[match].rotation;
            }

            description.skeleton = skeleton;
            description.human = reference.human;
            importer.humanDescription = description;
        }

        static ModelImporterClipAnimation Configure(ModelImporterClipAnimation take, ClipSpec spec)
        {
            var clip = new ModelImporterClipAnimation
            {
                name = spec.Name,
                takeName = take.takeName,
                firstFrame = spec.LastFrame > 0 ? spec.FirstFrame : take.firstFrame,
                lastFrame = spec.LastFrame > 0 ? spec.LastFrame : take.lastFrame,
                loopTime = spec.Loop,
                loopPose = spec.Loop,
                // Yön her zaman pozda kalır: karakter oynanışın baktığı yöne bakar.
                lockRootRotation = true,
                keepOriginalOrientation = true,
                lockRootHeightY = spec.KeepHeight,
                keepOriginalPositionY = true,
                heightFromFeet = false,
                lockRootPositionXZ = spec.KeepPlanar,
                keepOriginalPositionXZ = spec.KeepPlanar,
            };
            return clip;
        }

        /// <summary>
        /// Oyuncu denetleyicisi. Parametreleri yalnızca PlayerRig yazar:
        /// Moving, Grounded, Sliding, RunRate (koşu klibinin hızı), AirPhase (zıplama klibinin
        /// konumu: kalkış 0, tepe 0.5, iniş 1), Pose (0 yok, 1 oturuyor, 2 asılı), Caught, Stumble (tetik).
        /// </summary>
        static AnimatorController BuildController(Dictionary<string, AnimationClip> clips)
        {
            var controller = ResetController(ControllerPath);
            controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            // Varsayılan "yerde": PlayerRig ilk değeri yazana kadar karakter zıplama pozuna girmesin.
            controller.AddParameter(new AnimatorControllerParameter { name = "Grounded", type = AnimatorControllerParameterType.Bool, defaultBool = true });
            controller.AddParameter("Sliding", AnimatorControllerParameterType.Bool);
            controller.AddParameter(new AnimatorControllerParameter { name = "RunRate", type = AnimatorControllerParameterType.Float, defaultFloat = 1f });
            controller.AddParameter("AirPhase", AnimatorControllerParameterType.Float);
            controller.AddParameter("Pose", AnimatorControllerParameterType.Int);
            controller.AddParameter("Caught", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Stumble", AnimatorControllerParameterType.Trigger);

            var machine = controller.layers[0].stateMachine;
            var idle = State(machine, clips, "Idle", 260f, 0f);
            var run = State(machine, clips, "Run", 260f, 90f);
            var jump = State(machine, clips, "Jump", 520f, 90f);
            var slide = State(machine, clips, "Slide", 260f, 180f);
            var stumble = State(machine, clips, "Stumble", 520f, 180f);
            var caught = State(machine, clips, "Caught", 520f, 0f);
            var seated = State(machine, clips, "Seated", 0f, 90f);
            var hanging = State(machine, clips, "Hanging", 0f, 180f);
            machine.defaultState = idle;

            run.speedParameterActive = true;
            run.speedParameter = "RunRate";
            jump.timeParameterActive = true;
            jump.timeParameter = "AirPhase";

            Link(idle.AddTransition(run), 0.1f).AddCondition(AnimatorConditionMode.If, 0f, "Moving");
            Link(run.AddTransition(idle), 0.15f).AddCondition(AnimatorConditionMode.IfNot, 0f, "Moving");

            // "Herhangi bir durumdan" geçişler sırayla denenir; öncelik bu sıradır.
            var toCaught = FromAny(machine, caught, 0.1f);
            toCaught.AddCondition(AnimatorConditionMode.If, 0f, "Caught");

            var toHanging = FromAny(machine, hanging, 0.15f);
            toHanging.AddCondition(AnimatorConditionMode.Equals, 2f, "Pose");
            toHanging.AddCondition(AnimatorConditionMode.IfNot, 0f, "Caught");

            var toSeated = FromAny(machine, seated, 0.15f);
            toSeated.AddCondition(AnimatorConditionMode.Equals, 1f, "Pose");
            toSeated.AddCondition(AnimatorConditionMode.IfNot, 0f, "Caught");

            var toStumble = FromAny(machine, stumble, 0.08f);
            toStumble.AddCondition(AnimatorConditionMode.If, 0f, "Stumble");
            toStumble.AddCondition(AnimatorConditionMode.Equals, 0f, "Pose");
            toStumble.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            toStumble.AddCondition(AnimatorConditionMode.IfNot, 0f, "Caught");

            var toJump = FromAny(machine, jump, 0.06f);
            toJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            toJump.AddCondition(AnimatorConditionMode.Equals, 0f, "Pose");
            toJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Caught");

            var jumpToSlide = Link(jump.AddTransition(slide), 0.06f);
            jumpToSlide.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            jumpToSlide.AddCondition(AnimatorConditionMode.If, 0f, "Sliding");

            var jumpToRun = Link(jump.AddTransition(run), 0.08f);
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0f, "Moving");

            var jumpToIdle = Link(jump.AddTransition(idle), 0.12f);
            jumpToIdle.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Moving");

            Link(run.AddTransition(slide), 0.08f).AddCondition(AnimatorConditionMode.If, 0f, "Sliding");
            Link(stumble.AddTransition(slide), 0.08f).AddCondition(AnimatorConditionMode.If, 0f, "Sliding");

            var slideToRun = Link(slide.AddTransition(run), 0.12f);
            slideToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            slideToRun.AddCondition(AnimatorConditionMode.If, 0f, "Moving");

            var slideToIdle = Link(slide.AddTransition(idle), 0.15f);
            slideToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            slideToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Moving");

            // Sendeleme klibi bitince koşuya döner.
            var stumbleToRun = stumble.AddTransition(run);
            stumbleToRun.hasExitTime = true;
            stumbleToRun.exitTime = 0.85f;
            stumbleToRun.hasFixedDuration = true;
            stumbleToRun.duration = 0.15f;

            Link(caught.AddTransition(idle), 0.2f).AddCondition(AnimatorConditionMode.IfNot, 0f, "Caught");
            Link(seated.AddTransition(run), 0.15f).AddCondition(AnimatorConditionMode.NotEqual, 1f, "Pose");
            Link(hanging.AddTransition(run), 0.15f).AddCondition(AnimatorConditionMode.NotEqual, 2f, "Pose");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        /// <summary>
        /// Kovalayanın denetleyicisi: oyuncunun klip setinden koşu ve bekleme. Hep koşar;
        /// oyuncuyu yakalayıp başına gelince (Caught, ChaserSystem yazar) durur.
        /// </summary>
        static AnimatorController BuildChaserController(Dictionary<string, AnimationClip> clips)
        {
            var controller = ResetController(ChaserControllerPath);
            controller.AddParameter("Caught", AnimatorControllerParameterType.Bool);

            var machine = controller.layers[0].stateMachine;
            var run = State(machine, clips, "Run", 260f, 0f);
            var idle = State(machine, clips, "Idle", 260f, 90f);
            machine.defaultState = run;

            Link(run.AddTransition(idle), 0.25f).AddCondition(AnimatorConditionMode.If, 0f, "Caught");
            Link(idle.AddTransition(run), 0.15f).AddCondition(AnimatorConditionMode.IfNot, 0f, "Caught");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        /// <summary>Denetleyiciyi boşaltır (yoksa oluşturur); GUID korunur, prefab bağları kopmaz.</summary>
        static AnimatorController ResetController(string path)
        {
            EnsureFolder(AnimationDir);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            for (int i = controller.parameters.Length - 1; i >= 0; i--) controller.RemoveParameter(i);

            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states) machine.RemoveState(child.state);
            foreach (var transition in machine.anyStateTransitions) machine.RemoveAnyStateTransition(transition);
            return controller;
        }

        static AnimatorState State(AnimatorStateMachine machine, Dictionary<string, AnimationClip> clips, string name, float x, float y)
        {
            var state = machine.AddState(name, new Vector3(x, y));
            state.motion = ClipOrNull(clips, name);
            if (state.motion == null) Debug.LogWarning($"'{name}' klibi yok; durum boş kaldı.");
            return state;
        }

        static AnimatorStateTransition FromAny(AnimatorStateMachine machine, AnimatorState target, float seconds)
        {
            var transition = Link(machine.AddAnyStateTransition(target), seconds);
            transition.canTransitionToSelf = false;
            return transition;
        }

        static AnimationClip ClipOrNull(Dictionary<string, AnimationClip> clips, string name)
        {
            return clips.TryGetValue(name, out var clip) ? clip : null;
        }

        static AnimatorStateTransition Link(AnimatorStateTransition transition, float seconds)
        {
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = seconds;
            transition.offset = 0f;
            transition.interruptionSource = TransitionInterruptionSource.Destination;
            return transition;
        }

        // ------------------------------------------------------------ Yardımcılar

        static T LoadSubAsset<T>(string path) where T : Object
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is T typed) return typed;
            return null;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
