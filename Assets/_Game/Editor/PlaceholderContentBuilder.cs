using System.Collections.Generic;
using System.IO;
using OutOfOffice.Data;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Geçici (placeholder) içeriği baştan kurar: prefab'lar, tanım asset'leri, çeviri
    /// tabloları ve oyun sahnesi. Tekrar çalıştırmak güvenlidir; var olan asset'lerin
    /// GUID'leri korunur, yalnızca içerikleri yenilenir.
    ///
    /// Gerçek modele geçiş: ilgili prefab'ı Assets/_Game/Prefabs altında kendi modelinle
    /// değiştir ya da tanım asset'indeki "prefab" alanına yeni prefab'ı sürükle.
    /// </summary>
    public static partial class PlaceholderContentBuilder
    {
        const string Root = "Assets/_Game";
        const string MaterialDir = Root + "/Art/Placeholder/Materials";
        const string MeshDir = Root + "/Art/Placeholder/Meshes";
        const string PrefabDir = Root + "/Prefabs/Placeholder";
        const string VfxLibraryPath = Root + "/Content/VfxLibrary.asset";
        const string UiSpriteDir = Root + "/UI/Sprites";
        const string DataDir = Root + "/Content";
        const string ConfigPath = DataDir + "/GameConfig.asset";
        const string CatalogPath = DataDir + "/ContentCatalog.asset";
        const string LocalizationDir = Root + "/Localization";
        const string SceneDir = Root + "/Scenes";
        const string ScenePath = SceneDir + "/Game.unity";
        const string UxmlPath = Root + "/UI/Screens/GameUI.uxml";
        const string ThemePath = Root + "/UI/Theme/GameTheme.tss";
        const string PanelSettingsPath = Root + "/UI/Theme/GamePanelSettings.asset";

        [MenuItem("Out of Office/Placeholder İçeriği Yeniden Oluştur")]
        public static void BuildAllFromMenu()
        {
            Debug.Log(BuildAll());
        }

        public static string BuildAll()
        {
            ConfigureUiSprites();
            BuildData(BuildPrefabs());
            BuildVfx();
            BuildSfx();
            string localization = BuildLocalization();
            BuildScene();
            ApplyMobileSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);
            return $"Placeholder içerik hazır. Karakter: {catalog.characters.Length}, engel: {catalog.worlds[0].obstacles.Length}, " +
                   $"güçlendirme: {catalog.worlds[0].powerUps.Length}. {localization} Sahne: {ScenePath}";
        }

        /// <summary>GameConfig'i koddaki önerilen değerlere döndürür. Kendi dengelemen silinir.</summary>
        [MenuItem("Out of Office/Oyun Ayarlarını Varsayılana Döndür")]
        public static void ResetConfigToDefaults()
        {
            var config = LoadOrCreate<GameConfig>(ConfigPath);
            var defaults = ScriptableObject.CreateInstance<GameConfig>();
            string name = config.name;
            EditorUtility.CopySerialized(defaults, config);
            config.name = name;
            Object.DestroyImmediate(defaults);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Mağaza gereksinimleri: Android'de IL2CPP + ARM64. Ekran dikey.</summary>
        static void ApplyMobileSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        /// <summary>Arayüz görselleri: sıkıştırmasız, mipmap'siz, 9 dilim için boyutu korunmuş.</summary>
        internal static void ConfigureUiSprites()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { UiSpriteDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                bool upToDate = importer.textureType == TextureImporterType.Sprite
                                && !importer.mipmapEnabled
                                && importer.textureCompression == TextureImporterCompression.Uncompressed;
                if (upToDate) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // ------------------------------------------------------------ Yardımcılar

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        const string WorldShaderPath = Root + "/Shaders/CurvedLit.shader";

        /// <summary>Oyun dünyasındaki tüm yüzeylerin shader'ı: URP ışıklandırması + kıvrımlı dünya bükmesi.</summary>
        static Shader WorldShader()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(WorldShaderPath);
            if (shader == null) throw new System.InvalidOperationException("Kıvrımlı dünya shader'ı bulunamadı: " + WorldShaderPath);
            return shader;
        }

        /// <param name="emission">0'dan büyükse materyal kendi rengiyle parlar (bloom ile birlikte ışıldar).</param>
        static Material Mat(string name, Color color, float smoothness = 0.25f, float emission = 0f)
        {
            EnsureFolder(MaterialDir);
            string path = $"{MaterialDir}/{name}.mat";

            var shader = WorldShader();
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            // Eski içerik URP/Lit ile üretilmiş olabilir; her seferinde doğru shader'a alınır.
            if (material.shader != shader) material.shader = shader;
            material.shaderKeywords = System.Array.Empty<string>();
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", emission > 0f ? color * emission : Color.black);

            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Collider'sız tek bir primitif parça. Çarpışma şekli prefab kökünde ayrıca tanımlanır.</summary>
        static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Vector3 euler = default)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());

            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localEulerAngles = euler;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        /// <summary>
        /// Z boyunca uzun bir kutuyu kısa dilimler halinde kurar. Kıvrımlı dünya yalnızca köşe
        /// noktalarını bükebildiği için uzun tek parça düz kalır, üstündeki nesnelerle ayrışırdı.
        /// </summary>
        static void SlicedPart(Transform parent, string name, Vector3 position, Vector3 scale, Material material, float sliceLength = 2.5f)
        {
            int slices = Mathf.Max(1, Mathf.CeilToInt(scale.z / sliceLength));
            float step = scale.z / slices;
            float start = position.z - scale.z * 0.5f;

            for (int i = 0; i < slices; i++)
                Part(parent, PrimitiveType.Cube, name + i, new Vector3(position.x, position.y, start + step * (i + 0.5f)), new Vector3(scale.x, scale.y, step), material);
        }

        /// <summary>
        /// Aynı materyali kullanan parçaları tek mesh'te birleştirir: daha az draw call, daha az
        /// Transform. Oluşan mesh çalışma zamanında okunamaz işaretlenir; bellekte tek kopya kalır.
        /// </summary>
        static void CombineByMaterial(GameObject root, bool castShadows)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            var filters = root.GetComponentsInChildren<MeshFilter>();
            var toRoot = root.transform.worldToLocalMatrix;

            foreach (var filter in filters)
            {
                var material = filter.GetComponent<MeshRenderer>().sharedMaterial;
                if (!groups.TryGetValue(material, out var list)) groups.Add(material, list = new List<CombineInstance>());
                list.Add(new CombineInstance { mesh = filter.sharedMesh, transform = toRoot * filter.transform.localToWorldMatrix });
            }

            // Boyalı modeller asset olmayan geçici mesh kopyaları kullanır; birleştirme bitince atılırlar.
            var temporary = new List<Mesh>();
            foreach (var filter in filters)
            {
                if (!EditorUtility.IsPersistent(filter.sharedMesh)) temporary.Add(filter.sharedMesh);
                Object.DestroyImmediate(filter.gameObject);
            }

            EnsureFolder(MeshDir);
            foreach (var group in groups)
            {
                string path = $"{MeshDir}/{root.name}_{group.Key.name}.asset";

                var mesh = new Mesh { name = $"{root.name}_{group.Key.name}" };
                mesh.CombineMeshes(group.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                MeshUtility.Optimize(mesh);
                mesh.UploadMeshData(true);

                // Var olan asset'in içeriği yenilenir; GUID değişmez. Silip yeniden oluşturmak
                // ara sıra prefab'daki mesh bağını koparıyordu (yeni GUID ile eski .meta çakışıyor).
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(mesh, existing);
                    Object.DestroyImmediate(mesh);
                    mesh = existing;
                    EditorUtility.SetDirty(mesh);
                }
                else
                {
                    AssetDatabase.CreateAsset(mesh, path);
                }

                var part = new GameObject(group.Key.name);
                part.transform.SetParent(root.transform, false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = part.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = group.Key;
                renderer.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            foreach (var mesh in temporary) Object.DestroyImmediate(mesh);
        }

        static GameObject SavePrefab(GameObject root)
        {
            EnsureFolder(PrefabDir);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/{root.name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void BoxTrigger(GameObject root, Vector3 center, Vector3 size)
        {
            var box = root.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = center;
            box.size = size;
        }

        static void SphereTrigger(GameObject root, Vector3 center, float radius)
        {
            var sphere = root.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.center = center;
            sphere.radius = radius;
        }
    }
}
