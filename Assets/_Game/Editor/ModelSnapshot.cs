using System.IO;
using UnityEditor;
using UnityEngine;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// 3B modelden arayüz görseli çekmenin ortak adımları: ışık, çizim, saydam zeminli PNG ve
    /// içe aktarma ayarı. Kadrajı çağıran kurar (PortraitBuilder, ItemIconBuilder).
    /// </summary>
    static class ModelSnapshot
    {
        /// <summary>Çizim, çıktının bu katı boyutta yapılıp küçültülür (kenar yumuşatma).</summary>
        const int Supersample = 2;

        static readonly int CurveId = Shader.PropertyToID("_OooCurve");

        /// <summary>Kıvrımlı dünya değerleri son Play'den kalmış olabilir; görseller düz çizilir.</summary>
        public static void ResetWorldCurve()
        {
            Shader.SetGlobalVector(CurveId, Vector4.zero);
        }

        /// <summary>
        /// Ana ışık kameranın yanından-üstünden sıcak, kenar ışığı arkadan camgöbeği (oyunun neon
        /// vurgusu). <paramref name="cameraYaw"/>: kameranın modelin çevresindeki açısı (derece).
        /// </summary>
        public static void SetupLights(PreviewRenderUtility preview, float cameraYaw)
        {
            preview.lights[0].color = new Color(1f, 0.97f, 0.9f);
            preview.lights[0].intensity = 1.5f;
            preview.lights[0].transform.rotation = Quaternion.Euler(28f, cameraYaw + 150f, 0f);
            preview.lights[1].color = new Color(0.45f, 0.85f, 1f);
            preview.lights[1].intensity = 1.1f;
            preview.lights[1].transform.rotation = Quaternion.Euler(-10f, cameraYaw - 40f, 0f);
            preview.ambientColor = new Color(0.52f, 0.56f, 0.68f);

            var camera = preview.camera;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
        }

        public static Bounds VisualBounds(GameObject instance)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        /// <summary>Sahneyi çizer ve <paramref name="size"/> kenarlı, saydam zeminli, sRGB bir doku döndürür.</summary>
        public static Texture2D Shoot(PreviewRenderUtility preview, int size)
        {
            int renderSize = size * Supersample;
            preview.BeginPreview(new Rect(0f, 0f, renderSize, renderSize), GUIStyle.none);
            preview.Render(true);
            var result = ReadPixels(preview.camera.targetTexture, renderSize, size);
            preview.EndPreview();
            return result;
        }

        /// <summary>Dokuyu PNG olarak yazar, içe aktarır ve arayüz görseli olarak ayarlar.</summary>
        public static void Save(Texture2D texture, string path, int size)
        {
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            ConfigureTexture(path, size);
        }

        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
        }

        /// <summary>Çizimi sRGB'ye çevirip okur ve kutu ortalamasıyla çıktı boyutuna küçültür. Alfa korunur.</summary>
        static Texture2D ReadPixels(RenderTexture source, int renderSize, int size)
        {
            var srgb = RenderTexture.GetTemporary(renderSize, renderSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(source, srgb);

            var previous = RenderTexture.active;
            RenderTexture.active = srgb;
            var large = new Texture2D(renderSize, renderSize, TextureFormat.RGBA32, false);
            large.ReadPixels(new Rect(0f, 0f, renderSize, renderSize), 0, 0);
            large.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(srgb);

            var from = large.GetPixels32();
            var to = new Color32[size * size];
            int samples = Supersample * Supersample;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Renk alfayla ağırlıklı ortalanır: saydam piksellerin siyahı kenara sızmaz.
                    int r = 0, g = 0, b = 0, a = 0;
                    for (int sy = 0; sy < Supersample; sy++)
                    {
                        for (int sx = 0; sx < Supersample; sx++)
                        {
                            var pixel = from[(y * Supersample + sy) * renderSize + x * Supersample + sx];
                            r += pixel.r * pixel.a;
                            g += pixel.g * pixel.a;
                            b += pixel.b * pixel.a;
                            a += pixel.a;
                        }
                    }

                    to[y * size + x] = a == 0
                        ? new Color32(0, 0, 0, 0)
                        : new Color32((byte)(r / a), (byte)(g / a), (byte)(b / a), (byte)(a / samples));
                }
            }

            Object.DestroyImmediate(large);
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
            result.SetPixels32(to);
            result.Apply();
            return result;
        }

        static void ConfigureTexture(string path, int size)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = size;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            foreach (string platform in new[] { "Android", "iPhone" })
            {
                var settings = importer.GetPlatformTextureSettings(platform);
                settings.overridden = true;
                settings.maxTextureSize = size;
                settings.format = TextureImporterFormat.ASTC_4x4;
                importer.SetPlatformTextureSettings(settings);
            }

            importer.SaveAndReimport();
        }
    }
}
