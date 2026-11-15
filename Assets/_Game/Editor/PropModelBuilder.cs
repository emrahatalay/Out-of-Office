using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Gerçek eşya, engel ve çevre modellerini oyuna hazırlar: içe aktarma ayarları (mobil
    /// sıkıştırma) ve kıvrımlı dünya materyali. Prefab'ı bu sınıf kurmaz; içerik üretici
    /// (PlaceholderContentBuilder) çarpışma kutusunu kendi koyar ve görsel olarak buradaki
    /// modeli ister (<see cref="TryAddModel"/>). Model yoksa geçici küpler kullanılır.
    ///
    /// Kaynak dosyalar Art/Props altındadır (&lt;Ad&gt;.fbx + &lt;Ad&gt;_BaseColor.png); onları
    /// Tools/ModelPipeline betikleri üretir. Dokunun alfası ışıma maskesidir.
    ///
    /// Boyanabilir modeller (kuleler): kaynak dokudan açık, renksiz bir kopya üretilir
    /// (&lt;Ad&gt;_Neutral.png). Rengi yerleştirirken verilen boya belirler; boya mesh'in köşe
    /// rengine yazılır, böylece aynı model tek materyalle farklı renklerde durur.
    /// </summary>
    public static class PropModelBuilder
    {
        const string ArtDir = "Assets/_Game/Art/Props";
        const string WorldShaderPath = "Assets/_Game/Shaders/CurvedLit.shader";

        readonly struct PropSpec
        {
            public readonly string Name;
            /// <summary>Dokunun en büyük boyutu (bütçe: docs/GORSEL_DIL.md).</summary>
            public readonly int TextureSize;
            /// <summary>Neon bölgelerin ışıma gücü; bloom'un parlatması için 1'in üstünde.</summary>
            public readonly float Glow;
            /// <summary>Kendi dokusu yerine başka bir modelin materyalini kullanır (aynı modelin farklı ölçüsü).</summary>
            public readonly string SharedMaterial;
            /// <summary>Dokusu renksiz açık tona çevrilir; rengi yerleştirirken verilen boya belirler.</summary>
            public readonly bool Tintable;

            public PropSpec(string name, int textureSize, float glow, string sharedMaterial = null, bool tintable = false)
            {
                Name = name;
                TextureSize = textureSize;
                Glow = glow;
                SharedMaterial = sharedMaterial;
                Tintable = tintable;
            }
        }

        static readonly PropSpec[] Props =
        {
            // Engeller
            new PropSpec("ServerRack", 512, 3f),
            new PropSpec("CookieWall", 512, 1.2f),
            new PropSpec("RobotVacuum", 512, 3f),
            new PropSpec("CargoDrone", 512, 2.5f),
            new PropSpec("AutoTaxi", 512, 3f),
            new PropSpec("DevBot", 512, 2.5f),
            new PropSpec("DesignBot", 512, 2.5f),
            // Toplananlar ve aksesuar
            // Altının dokusu koddan çizilir (make_coin.py); ışıma çok hafif: fazlası bloom'la beyaza kaçıp amblemi siliyordu.
            new PropSpec("Coin", 256, 1f),
            new PropSpec("Captcha", 256, 2.5f),
            new PropSpec("Hallucination", 256, 1.4f),
            new PropSpec("Shield", 256, 2f),
            new PropSpec("DronePickup", 256, 2.5f),
            new PropSpec("CarrierDrone", 256, 2.5f, "DronePickup"),
            new PropSpec("Magnet", 256, 3f),
            new PropSpec("Overtime", 256, 1.5f),
            new PropSpec("Espresso", 256, 2f),
            new PropSpec("PromptInjection", 256, 2.5f),
            new PropSpec("Unplug", 256, 2.5f),
            new PropSpec("Package", 256, 2f),
            new PropSpec("OfficeChair", 256, 2.5f),
            // Çevre
            // Kolon da boyanabilir: gövdesi açık renge döner, neon çizgileri kendi renginde kalır.
            new PropSpec("Column", 512, 1.4f, tintable: true),
            new PropSpec("Kiosk", 512, 2f),
            new PropSpec("Billboard", 512, 2f),
            // Binalar boyanabilir: her kopya ayrı bir pastel renk alır. Gündüz pencereler ışık
            // saçmaz, gökyüzünü yansıtan cam gibi hafifçe parlar. Ekranın büyük bölümünü
            // kapladıkları için dokuları 1024'tür (256'da pencere ızgarası lekeye dönüyordu).
            new PropSpec("TowerA", 1024, 0.15f, tintable: true),
            new PropSpec("TowerB", 1024, 0.15f, tintable: true),
        };

        // Renksiz dokunun ayarları: gövdenin en koyu ve en açık yeri bu aralığa yayılır. Taban
        // boya çarpılınca renk canlı kalacak kadar yüksek, pencere ve kat çizgileri okunacak kadar
        // alçaktır (0,6'da bina düz ve soluk görünüyordu). Işıyan pikseller kendi rengini korur.
        const float NeutralFloor = 0.42f;
        const float NeutralCeiling = 1f;

        [MenuItem("Out of Office/Eşya ve Çevre Modellerini Kur")]
        public static void BuildAllFromMenu()
        {
            Debug.Log(BuildAll());
        }

        public static string BuildAll()
        {
            int built = 0;
            foreach (var spec in Props)
            {
                if (!File.Exists(ModelPath(spec.Name)))
                {
                    Debug.LogWarning($"Model yok, atlandı: {ModelPath(spec.Name)}");
                    continue;
                }

                ConfigureModel(ModelPath(spec.Name));
                if (spec.SharedMaterial == null)
                {
                    // Boyanabilir çevre modelleri (bina, kolon) yola yatık açıyla görünür; anizotropik
                    // süzme olmadan uzaktaki yüzleri bulanıklaşır.
                    int aniso = spec.Tintable ? 4 : 1;
                    ConfigureTexture(TexturePath(spec.Name), spec.TextureSize, aniso);
                    if (spec.Tintable)
                    {
                        BuildNeutralTexture(TexturePath(spec.Name), NeutralTexturePath(spec.Name));
                        ConfigureTexture(NeutralTexturePath(spec.Name), spec.TextureSize, aniso);
                    }

                    BuildMaterial(spec);
                }

                built++;
            }

            AssetDatabase.SaveAssets();
            return $"Eşya ve çevre modelleri hazır: {built}/{Props.Length}. " +
                   "Prefab'lara girmesi için Placeholder İçeriği Yeniden Oluştur'u çalıştır.";
        }

        /// <summary>
        /// Gerçek model kuruluysa <paramref name="parent"/>'ın altına görsel olarak ekler ve true döner.
        /// Çarpışma şekli eklenmez; onu çağıran koyar.
        /// </summary>
        /// <param name="tint">
        /// Verilirse model bu renge boyanır (mesh'in kopyasının köşe rengine yazılır). Kopya
        /// asset değildir: çağıran onu birleştirilmiş bir mesh'e katmalıdır (CombineByMaterial).
        /// </param>
        public static bool TryAddModel(Transform parent, string prop, bool castShadows, Vector3 localPosition = default, float yaw = 0f, Color? tint = null)
        {
            var mesh = LoadMesh(prop);
            var material = LoadMaterial(prop);
            if (mesh == null || material == null) return false;
            if (tint.HasValue) mesh = Tinted(mesh, tint.Value);

            // Model iç içe prefab olarak değil düz mesh olarak eklenir: tek nesne, tek draw call;
            // üretici isterse aynı materyalli kopyaları tek mesh'te birleştirebilir.
            var visual = new GameObject(prop);
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;

            var renderer = visual.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return true;
        }

        public static bool Exists(string prop)
        {
            return LoadMesh(prop) != null && LoadMaterial(prop) != null;
        }

        static string ModelPath(string name) { return $"{ArtDir}/{name}.fbx"; }
        static string TexturePath(string name) { return $"{ArtDir}/{name}_BaseColor.png"; }
        static string NeutralTexturePath(string name) { return $"{ArtDir}/{name}_Neutral.png"; }
        static string MaterialPath(string name) { return $"{ArtDir}/{name}.mat"; }

        static Mesh Tinted(Mesh source, Color tint)
        {
            var mesh = Object.Instantiate(source);
            mesh.name = source.name;
            var colors = new Color32[mesh.vertexCount];
            Color32 color = tint;
            for (int i = 0; i < colors.Length; i++) colors[i] = color;
            mesh.colors32 = colors;
            return mesh;
        }

        /// <summary>
        /// Kaynak dokunun açık ve renksiz kopyasını yazar. Kaynak dosya değişmez (onu model hattı
        /// üretir), bu yüzden tekrar çalıştırmak aynı sonucu verir. Işıma maskesi (alfa) korunur.
        /// </summary>
        static void BuildNeutralTexture(string sourcePath, string targetPath)
        {
            // İçe aktarılmış doku okunamaz ve sıkıştırılmıştır; dosyanın kendisi okunur.
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(sourcePath));
            var pixels = texture.GetPixels32();

            // Gövdenin (ışımayan piksellerin) parlaklık aralığı: uçtaki %5'ler atılır.
            var histogram = new int[256];
            int body = 0;
            foreach (var pixel in pixels)
            {
                if (pixel.a < 250) continue;
                histogram[Luminance(pixel)]++;
                body++;
            }

            int low = Percentile(histogram, body, 0.05f);
            int high = Mathf.Max(low + 1, Percentile(histogram, body, 0.95f));

            for (int i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                float value = Mathf.Lerp(NeutralFloor, NeutralCeiling, Mathf.InverseLerp(low, high, Luminance(pixel)));
                var color = Color.Lerp(new Color(value, value, value), (Color)pixel, 1f - pixel.a / 255f);
                color.a = pixel.a / 255f;
                pixels[i] = color;
            }

            texture.SetPixels32(pixels);
            File.WriteAllBytes(targetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(targetPath);
        }

        static int Luminance(Color32 pixel)
        {
            return (pixel.r * 299 + pixel.g * 587 + pixel.b * 114) / 1000;
        }

        static int Percentile(int[] histogram, int total, float fraction)
        {
            int target = Mathf.RoundToInt(total * fraction);
            int seen = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                seen += histogram[i];
                if (seen >= target) return i;
            }

            return histogram.Length - 1;
        }

        static Mesh LoadMesh(string prop)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(ModelPath(prop)))
                if (asset is Mesh mesh) return mesh;
            return null;
        }

        static Material LoadMaterial(string prop)
        {
            foreach (var spec in Props)
                if (spec.Name == prop) return AssetDatabase.LoadAssetAtPath<Material>(MaterialPath(spec.SharedMaterial ?? spec.Name));
            return null;
        }

        static void ConfigureModel(string path)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importCameras = false;
            importer.importLights = false;
            // Çalışma zamanında okunmaz: bellekte tek (GPU) kopya kalır. Üretici zemin dekorunu
            // editörde birleştirirken yine okuyabilir.
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.weldVertices = true;
            importer.indexFormat = ModelImporterIndexFormat.UInt16;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.None;
            importer.SaveAndReimport();
        }

        static void ConfigureTexture(string path, int maxSize, int anisoLevel = 1)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            // Alfa saydamlık değil ışıma maskesidir; renkler alfaya göre değiştirilmez.
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = anisoLevel;
            importer.maxTextureSize = maxSize;
            importer.textureCompression = TextureImporterCompression.Compressed;

            foreach (string platform in new[] { "Android", "iPhone" })
            {
                var settings = importer.GetPlatformTextureSettings(platform);
                settings.overridden = true;
                settings.maxTextureSize = maxSize;
                settings.format = TextureImporterFormat.ASTC_6x6;
                settings.compressionQuality = 50;
                importer.SetPlatformTextureSettings(settings);
            }

            importer.SaveAndReimport();
        }

        static void BuildMaterial(PropSpec spec)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(WorldShaderPath);
            if (shader == null) throw new System.InvalidOperationException("Kıvrımlı dünya shader'ı bulunamadı: " + WorldShaderPath);

            string path = MaterialPath(spec.Name);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.shader != shader) material.shader = shader;
            string texturePath = spec.Tintable ? NeutralTexturePath(spec.Name) : TexturePath(spec.Name);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.2f);
            material.SetFloat("_Metallic", 0f);
            material.SetColor("_EmissionColor", Color.black);
            material.SetFloat("_GlowStrength", spec.Glow);
            EditorUtility.SetDirty(material);
        }
    }
}
