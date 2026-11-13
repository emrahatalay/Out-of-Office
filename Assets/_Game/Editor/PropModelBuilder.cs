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

            public PropSpec(string name, int textureSize, float glow, string sharedMaterial = null)
            {
                Name = name;
                TextureSize = textureSize;
                Glow = glow;
                SharedMaterial = sharedMaterial;
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
            new PropSpec("Coin", 128, 1.6f),
            new PropSpec("Captcha", 256, 2.5f),
            new PropSpec("Hallucination", 256, 1.4f),
            new PropSpec("Shield", 256, 2f),
            new PropSpec("DronePickup", 256, 2.5f),
            new PropSpec("CarrierDrone", 256, 2.5f, "DronePickup"),
            new PropSpec("Magnet", 256, 3f),
            new PropSpec("Overtime", 256, 1.5f),
            new PropSpec("Espresso", 256, 2f),
            new PropSpec("Package", 256, 2f),
            new PropSpec("OfficeChair", 256, 2.5f),
            // Çevre
            new PropSpec("Column", 512, 2f),
            new PropSpec("Kiosk", 512, 2f),
            new PropSpec("Billboard", 512, 2f),
            // Uzak dekor oynanış nesnelerinden sönük kalır.
            new PropSpec("TowerA", 256, 1.1f),
            new PropSpec("TowerB", 256, 1.1f),
        };

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
                    ConfigureTexture(TexturePath(spec.Name), spec.TextureSize);
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
        public static bool TryAddModel(Transform parent, string prop, bool castShadows, Vector3 localPosition = default, float yaw = 0f)
        {
            var mesh = LoadMesh(prop);
            var material = LoadMaterial(prop);
            if (mesh == null || material == null) return false;

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
        static string MaterialPath(string name) { return $"{ArtDir}/{name}.mat"; }

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

        static void ConfigureTexture(string path, int maxSize)
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
            importer.anisoLevel = 1;
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
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(spec.Name)));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.2f);
            material.SetFloat("_Metallic", 0f);
            material.SetColor("_EmissionColor", Color.black);
            material.SetFloat("_GlowStrength", spec.Glow);
            EditorUtility.SetDirty(material);
        }
    }
}
