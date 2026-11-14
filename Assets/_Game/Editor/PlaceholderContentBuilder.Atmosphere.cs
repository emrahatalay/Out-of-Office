using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Sahnenin havası: gündüz gökyüzü, güneş, sis, yansıma ve post-processing. Hepsi koddan
    /// kurulur ki sahne her üretildiğinde aynı görünüm gelsin; ayar değiştirmek için buradaki
    /// sayıları değiştirip üreticiyi yeniden çalıştır.
    /// </summary>
    public static partial class PlaceholderContentBuilder
    {
        const string SkyShaderPath = Root + "/Shaders/Sky.shader";
        const string AtmosphereDir = Root + "/Art/Atmosphere";
        const string SkyMaterialPath = AtmosphereDir + "/Sky.mat";
        const string ReflectionPath = AtmosphereDir + "/SkyReflection.cubemap";
        const string VolumeProfilePath = AtmosphereDir + "/GameVolume.asset";

        // Gökyüzünün renkleri. Sis ufuk rengiyle AYNIDIR: uzaktaki nesneler gökyüzüne karışır ve
        // aşağı kıvrılan yolun ucunda dikiş görünmez.
        static readonly Color SkyZenith = new Color(0.16f, 0.5f, 1f);
        static readonly Color SkyHorizon = new Color(0.7f, 0.88f, 1f);
        const float SkyExponent = 0.55f;

        // Güneşin sahnedeki yönü; hem ışık hem gökyüzündeki disk bundan türetilir.
        static readonly Quaternion SunRotation = Quaternion.Euler(50f, -30f, 0f);

        /// <summary>Işık, ortam, sis, gökyüzü ve yansıma. Sahne kurulurken çağrılır.</summary>
        static void BuildAtmosphere()
        {
            EnsureFolder(AtmosphereDir);

            // ---- Ana ışık: güneş. Kameranın arkasından gelir ki oyuncunun kameraya dönük sırtı
            // aydınlık kalsın ve gölgeler ileriye, yolun üstüne düşsün.
            var sun = new GameObject("Sun Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.intensity = 1.45f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.55f;
            sun.transform.rotation = SunRotation;

            // ---- Dolgu ışığı: karşıdan gelen gök mavisi. Güneşe dönük olmayan yüzler kararmaz; gölge atmaz.
            var fill = new GameObject("Fill Light").AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.7f, 0.85f, 1f);
            fill.intensity = 0.35f;
            fill.shadows = LightShadows.None;
            fill.transform.rotation = Quaternion.Euler(28f, 155f, 0f);

            // ---- Ortam ışığı: üstten gök mavisi, yandan sıcak beyaz, alttan zeminden seken açık ton.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.58f, 0.72f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.64f, 0.74f);
            RenderSettings.ambientGroundColor = new Color(0.4f, 0.4f, 0.48f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = SkyHorizon;
            RenderSettings.fogStartDistance = 62f;
            RenderSettings.fogEndDistance = 135f;

            RenderSettings.skybox = BuildSkyMaterial();
            RenderSettings.sun = sun;

            // ---- Yansıma: gökyüzünün küçük bir küp haritası. Zemin gökyüzünün açık rengini hafifçe
            // yansıtır; gerçek zamanlı yansıma yok, maliyeti sıfıra yakın.
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = BuildReflectionCubemap();
            RenderSettings.reflectionIntensity = 1f;
        }

        static Material BuildSkyMaterial()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(SkyShaderPath);
            if (shader == null) throw new System.InvalidOperationException("Gökyüzü shader'ı bulunamadı: " + SkyShaderPath);

            var material = AssetDatabase.LoadAssetAtPath<Material>(SkyMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, SkyMaterialPath);
            }

            if (material.shader != shader) material.shader = shader;
            material.SetColor("_ZenithColor", SkyZenith);
            material.SetColor("_HorizonColor", SkyHorizon);
            material.SetFloat("_Exponent", SkyExponent);
            // Güneş ışığın geldiği yöndedir (kameranın arkasında); koşarken görünmez, menüde görünebilir.
            Vector3 toSun = SunRotation * Vector3.back;
            material.SetVector("_SunDirection", new Vector4(toSun.x, toSun.y, toSun.z, 0f));
            material.SetColor("_SunColor", new Color(1.6f, 1.45f, 1.1f));
            material.SetFloat("_SunSize", 3f);
            material.SetColor("_CloudColor", Color.white);
            material.SetColor("_CloudShadeColor", new Color(0.86f, 0.92f, 1f));
            material.SetFloat("_CloudCount", 14f);
            material.SetFloat("_CloudHeightScale", 8f);
            material.SetFloat("_CloudCoverage", 0.47f);
            material.SetFloat("_CloudSoftness", 0.07f);
            material.SetFloat("_CloudSpeed", 0.01f);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Gökyüzünün geçişini (güneş ve bulut olmadan) küçük bir küp haritasına yazar.</summary>
        static Cubemap BuildReflectionCubemap()
        {
            const int size = 32;
            var cubemap = new Cubemap(size, TextureFormat.RGBA32, true) { name = "SkyReflection" };
            var pixels = new Color[size * size];

            for (int face = 0; face < 6; face++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + 0.5f) / size * 2f - 1f;
                        float v = (y + 0.5f) / size * 2f - 1f;
                        pixels[y * size + x] = SkyColor(FaceDirection((CubemapFace)face, u, v));
                    }
                }

                cubemap.SetPixels(pixels, (CubemapFace)face);
            }

            cubemap.Apply(true, true);

            // Var olan asset'in içeriği yenilenir; GUID değişmez, sahnedeki bağ kopmaz.
            var existing = AssetDatabase.LoadAssetAtPath<Cubemap>(ReflectionPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(cubemap, existing);
                Object.DestroyImmediate(cubemap);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            AssetDatabase.CreateAsset(cubemap, ReflectionPath);
            return cubemap;
        }

        static Vector3 FaceDirection(CubemapFace face, float u, float v)
        {
            switch (face)
            {
                case CubemapFace.PositiveX: return new Vector3(1f, -v, -u).normalized;
                case CubemapFace.NegativeX: return new Vector3(-1f, -v, u).normalized;
                case CubemapFace.PositiveY: return new Vector3(u, 1f, v).normalized;
                case CubemapFace.NegativeY: return new Vector3(u, -1f, -v).normalized;
                case CubemapFace.PositiveZ: return new Vector3(u, -v, 1f).normalized;
                default: return new Vector3(-u, -v, -1f).normalized;
            }
        }

        // Sky.shader'daki tepe-ufuk geçişinin aynısı (sRGB renkleriyle; küp haritası sRGB'dir).
        static Color SkyColor(Vector3 direction)
        {
            var color = Color.Lerp(SkyHorizon, SkyZenith, Mathf.Pow(Mathf.Clamp01(direction.y), SkyExponent));
            color.a = 1f;
            return color;
        }

        /// <summary>
        /// Post-processing: ışıyan parçaları hafifçe yayan bloom, canlı doygunluk, serin gölge /
        /// sıcak parlaklık tonlaması, çok hafif kenar karartması. Mobil için pahalı etkiler (alan
        /// derinliği, hareket bulanıklığı, renk sapması) yoktur.
        /// </summary>
        static VolumeProfile BuildVolumeProfile()
        {
            EnsureFolder(AtmosphereDir);
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumeProfilePath);
            }

            var tonemapping = Override<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            // Gündüz sahnesi zaten aydınlıktır; eşik yüksek tutulur ki yalnızca ışıyan parçalar yayılsın.
            var bloom = Override<Bloom>(profile);
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(0.4f);
            bloom.scatter.Override(0.6f);
            bloom.tint.Override(Color.white);
            bloom.highQualityFiltering.Override(false);

            var color = Override<ColorAdjustments>(profile);
            color.postExposure.Override(0f);
            color.contrast.Override(10f);
            color.saturation.Override(22f);
            color.colorFilter.Override(Color.white);

            var toning = Override<SplitToning>(profile);
            toning.shadows.Override(new Color(0.46f, 0.5f, 0.6f));
            toning.highlights.Override(new Color(0.56f, 0.52f, 0.46f));
            toning.balance.Override(0f);

            var vignette = Override<Vignette>(profile);
            vignette.color.Override(new Color(0.16f, 0.26f, 0.5f));
            vignette.intensity.Override(0.14f);
            vignette.smoothness.Override(0.5f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        static T Override<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
            {
                component = profile.Add<T>(true);
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            component.active = true;
            return component;
        }
    }
}
