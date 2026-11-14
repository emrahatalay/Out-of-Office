using System.Linq;
using OutOfOffice.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Gölgeler: gerçek zamanlı gölge haritası KAPALIDIR (mobilde gölge geçişi sahneyi ikinci kez
    /// çizer). Yerine her karakterin, robotun ve engelin altına yumuşak bir "damla gölge" konur:
    /// 'Blob Shadow' shader'lı tek quad, zeminle çarpılarak koyulaştırır.
    /// </summary>
    public static partial class PlaceholderContentBuilder
    {
        const string BlobShadowShaderPath = Root + "/Shaders/BlobShadow.shader";
        const string BlobShadowMeshPath = MeshDir + "/BlobShadowQuad.asset";

        /// <summary>Damla gölgenin materyali. Gölgenin rengi gökyüzünün serin tonudur; siyah gölge gündüz sahnesinde kirli durur.</summary>
        static Material BlobShadowMaterial()
        {
            EnsureFolder(MaterialDir);
            string path = $"{MaterialDir}/BlobShadow.mat";
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(BlobShadowShaderPath);
            if (shader == null) throw new System.InvalidOperationException("Damla gölge shader'ı bulunamadı: " + BlobShadowShaderPath);

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.shader != shader) material.shader = shader;
            material.SetColor("_ShadowColor", new Color(0.36f, 0.38f, 0.56f));
            material.SetFloat("_Strength", 0.78f);
            material.SetFloat("_Softness", 0.65f);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>1x1 m, yere yatık (XZ düzleminde, yukarı bakan) quad. Ölçeği gölgenin boyudur.</summary>
        static Mesh BlobShadowMesh()
        {
            EnsureFolder(MeshDir);
            var mesh = new Mesh { name = "BlobShadowQuad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f), new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, -0.5f),
            };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
            mesh.normals = Enumerable.Repeat(Vector3.up, 4).ToArray();
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(BlobShadowMeshPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, BlobShadowMeshPath);
                return mesh;
            }

            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        /// <summary>
        /// Prefab'a zemin hizasında damla gölge ekler. <paramref name="size"/> gölgenin genişliği (X)
        /// ve derinliğidir (Z); havada süzülen nesnelerde de gölge yerdedir.
        /// </summary>
        static void BlobShadow(GameObject root, Vector2 size, float z = 0f)
        {
            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(root.transform, false);
            // Zeminin hemen üstü; asıl titreme önleme shader'daki Offset'tedir.
            shadow.transform.localPosition = new Vector3(0f, 0.02f, z);
            shadow.transform.localScale = new Vector3(size.x, 1f, size.y);
            shadow.AddComponent<MeshFilter>().sharedMesh = BlobShadowMesh();
            var renderer = shadow.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = BlobShadowMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        /// <summary>Oyuncu ve kovalayan için tek başına damla gölge prefab'ı (VfxLibrary.blobShadowPrefab).</summary>
        static GameObject BlobShadowPrefab()
        {
            var root = new GameObject("Vfx_BlobShadow");
            BlobShadow(root, Vector2.one);
            return SavePrefab(root);
        }

        /// <summary>
        /// URP asset'lerinde gerçek zamanlı gölgeyi kapatır (ana ışık ve ek ışıklar). Gölge geçişi
        /// hiç çalışmaz; shader'ların gölge varyantları da çıktıdan ayıklanır.
        /// </summary>
        static void ApplyRenderSettings()
        {
            var assets = new System.Collections.Generic.HashSet<RenderPipelineAsset>();
            if (GraphicsSettings.defaultRenderPipeline != null) assets.Add(GraphicsSettings.defaultRenderPipeline);
            for (int i = 0; i < QualitySettings.count; i++)
            {
                var asset = QualitySettings.GetRenderPipelineAssetAt(i);
                if (asset != null) assets.Add(asset);
            }

            foreach (var asset in assets.OfType<UniversalRenderPipelineAsset>())
            {
                var serialized = new SerializedObject(asset);
                foreach (string property in new[] { "m_MainLightShadowsSupported", "m_AdditionalLightShadowsSupported", "m_SoftShadowsSupported", "m_AnyShadowsSupported" })
                {
                    var field = serialized.FindProperty(property);
                    if (field != null) field.boolValue = false;
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
            }
        }
    }
}
