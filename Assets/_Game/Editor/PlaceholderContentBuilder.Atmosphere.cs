using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OutOfOffice.EditorTools
{
    /// <summary>
    /// Sahnenin havası: gökyüzü, ışık, sis, yansıma, post-processing ve yağmur. Hepsi koddan
    /// kurulur ki sahne her üretildiğinde aynı görünüm gelsin; ayar değiştirmek için buradaki
    /// sayıları değiştirip üreticiyi yeniden çalıştır.
    /// </summary>
    public static partial class PlaceholderContentBuilder
    {
        const string SkyShaderPath = Root + "/Shaders/NightSky.shader";
        const string AtmosphereDir = Root + "/Art/Atmosphere";
        const string SkyMaterialPath = AtmosphereDir + "/NightSky.mat";
        const string ReflectionPath = AtmosphereDir + "/SkyReflection.cubemap";
        const string VolumeProfilePath = AtmosphereDir + "/GameVolume.asset";

        // Gökyüzünün renkleri. Sis ufka yakın bir tondadır: uzaktaki nesneler gökyüzüne karışır.
        static readonly Color SkyZenith = new Color(0.015f, 0.02f, 0.07f);
        static readonly Color SkyHorizon = new Color(0.13f, 0.08f, 0.27f);
        static readonly Color SkyGround = new Color(0.05f, 0.05f, 0.12f);
        static readonly Color SkyGlowA = new Color(0.8f, 0.25f, 0.85f);
        static readonly Color SkyGlowB = new Color(0.2f, 0.75f, 1f);
        static readonly Color FogColor = new Color(0.17f, 0.1f, 0.31f);
        const float SkyGlowStrength = 0.3f;
        const float SkyGlowFalloff = 11f;
        const float SkyExponent = 0.42f;

        /// <summary>Işık, ortam, sis, gökyüzü ve yansıma. Sahne kurulurken çağrılır.</summary>
        static void BuildAtmosphere()
        {
            EnsureFolder(AtmosphereDir);

            // ---- Ana ışık: ay. Kameranın arkasından gelir ki oyuncunun kameraya dönük sırtı aydınlık kalsın.
            var moon = new GameObject("Moon Light").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.72f, 0.82f, 1f);
            moon.intensity = 1.25f;
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = 0.75f;
            moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // ---- Kenar ışığı: karşıdan gelen macenta. Siluetleri koyu zeminden ayırır; gölge atmaz.
            var rim = new GameObject("Rim Light").AddComponent<Light>();
            rim.type = LightType.Directional;
            rim.color = new Color(1f, 0.35f, 0.8f);
            rim.intensity = 0.55f;
            rim.shadows = LightShadows.None;
            rim.transform.rotation = Quaternion.Euler(28f, 155f, 0f);

            // ---- Ortam ışığı: üstten soğuk mavi, yandan mor, alttan koyu.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.36f, 0.42f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.32f, 0.58f);
            RenderSettings.ambientGroundColor = new Color(0.14f, 0.13f, 0.26f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 125f;

            RenderSettings.skybox = BuildSkyMaterial();
            RenderSettings.sun = moon;

            // ---- Yansıma: gökyüzünün küçük bir küp haritası. Parlak zemin ufkun ışığını yansıtır
            // (ıslak asfalt hissi); gerçek zamanlı yansıma yok, maliyeti sıfıra yakın.
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
            material.SetColor("_GroundColor", SkyGround);
            material.SetColor("_GlowColorA", SkyGlowA);
            material.SetColor("_GlowColorB", SkyGlowB);
            material.SetFloat("_GlowStrength", SkyGlowStrength);
            material.SetFloat("_GlowFalloff", SkyGlowFalloff);
            material.SetFloat("_Exponent", SkyExponent);
            material.SetFloat("_StarDensity", 0.06f);
            material.SetFloat("_StarBrightness", 1.6f);
            // Ay ileride, alçakta ve biraz sağda: kulelerin arasından görünür.
            material.SetVector("_MoonDirection", new Vector4(0.1f, 0.2f, 1f, 0f));
            material.SetColor("_MoonColor", new Color(0.7f, 0.88f, 1f));
            material.SetFloat("_MoonSize", 3.6f);
            material.SetFloat("_SkylineHeight", 0.085f);
            material.SetColor("_SkylineColor", new Color(0.035f, 0.035f, 0.09f));
            material.SetColor("_WindowColor", new Color(1.2f, 0.85f, 0.45f));
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Gökyüzünün geçişini ve şehir ışığını (yıldız ve ay olmadan) küçük bir küp haritasına yazar.</summary>
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

        // NightSky.shader'daki geçiş ve şehir ışığının aynısı (sRGB renkleriyle; küp haritası sRGB'dir).
        static Color SkyColor(Vector3 direction)
        {
            float up = Mathf.Clamp01(direction.y);
            float azimuth = Mathf.Atan2(direction.x, direction.z);
            var color = Color.Lerp(SkyHorizon, SkyZenith, Mathf.Pow(up, SkyExponent));
            var glow = Color.Lerp(SkyGlowA, SkyGlowB, 0.5f + 0.5f * Mathf.Sin(azimuth * 2f - 1.2f));
            color += glow * (Mathf.Exp(-up * SkyGlowFalloff) * SkyGlowStrength);
            color = Color.Lerp(color, SkyGround, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(-direction.y / 0.08f)));
            color.a = 1f;
            return color;
        }

        /// <summary>
        /// Post-processing: ışımayı yayan bloom, kontrast ve doygunluk, soğuk gölge / sıcak
        /// parlaklık tonlaması, hafif kenar karartması. Mobil için pahalı etkiler (alan
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

            var bloom = Override<Bloom>(profile);
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(0.75f);
            bloom.scatter.Override(0.66f);
            bloom.tint.Override(new Color(0.92f, 0.9f, 1f));
            bloom.highQualityFiltering.Override(false);

            var color = Override<ColorAdjustments>(profile);
            color.postExposure.Override(0.12f);
            color.contrast.Override(14f);
            color.saturation.Override(12f);
            color.colorFilter.Override(new Color(0.97f, 0.98f, 1f));

            var toning = Override<SplitToning>(profile);
            toning.shadows.Override(new Color(0.42f, 0.48f, 0.62f));
            toning.highlights.Override(new Color(0.6f, 0.5f, 0.52f));
            toning.balance.Override(-10f);

            var vignette = Override<Vignette>(profile);
            vignette.color.Override(new Color(0.03f, 0.02f, 0.1f));
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);

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

        /// <summary>
        /// Kameraya bağlı ince neon yağmuru. Damlalar dünya uzayında düşer; oyuncu hızlandıkça
        /// kameraya doğru akar ve hız hissini artırır. Az sayıda, ince, yarı saydam çizgi.
        /// </summary>
        static void BuildRain(Transform camera)
        {
            var particles = NewParticles("Rain", new Color(0.62f, 0.86f, 1f, 0.3f), 1.1f, 0.03f, 220);
            particles.transform.SetParent(camera, false);
            particles.transform.localPosition = new Vector3(0f, 7f, 16f);
            particles.transform.localRotation = Quaternion.identity;

            var main = particles.main;
            main.loop = true;
            main.startSpeed = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;

            var emission = particles.emission;
            emission.rateOverTime = 150f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(16f, 1f, 34f);

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.6f);
            velocity.y = new ParticleSystem.MinMaxCurve(-13f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.035f;
            renderer.lengthScale = 2f;
            // Kameranın kendi hızı da çizgiyi uzatır: koşarken yağmur öne doğru yatar.
            renderer.cameraVelocityScale = 0.02f;
        }
    }
}
