using OutOfOffice.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace OutOfOffice.EditorTools
{
    public static partial class PlaceholderContentBuilder
    {
        // Geçici parçacık efektleri. Gerçek efektlere geçerken VfxLibrary asset'indeki
        // prefab alanlarını değiştirmek yeterlidir.
        static void BuildVfx()
        {
            var yellow = new Color(1f, 0.82f, 0.2f);
            var cyan = new Color(0.3f, 0.9f, 1f);
            var red = new Color(1f, 0.3f, 0.36f);
            var steel = new Color(0.7f, 0.76f, 0.86f);
            var dust = new Color(0.8f, 0.84f, 0.92f, 0.7f);

            var library = LoadOrCreate<VfxLibrary>(VfxLibraryPath);
            library.bursts = new[]
            {
                Entry(VfxEvent.CoinCollected, Burst("Vfx_CoinCollect", yellow, 10, 4f, 0.22f, 0.35f, 0.3f, 0.2f), 0.6f, new Vector3(0f, 1f, 0.6f)),
                Entry(VfxEvent.PowerUpCollected, Burst("Vfx_PowerUp", cyan, 26, 6f, 0.3f, 0.6f, 0f, 0.4f), 0.9f, new Vector3(0f, 1f, 0.6f)),
                Entry(VfxEvent.Stumbled, Burst("Vfx_Stumble", red, 22, 7f, 0.3f, 0.45f, 1.2f, 0.3f), 0.8f, new Vector3(0f, 0.9f, 0.8f)),
                Entry(VfxEvent.ObstacleDestroyed, Burst("Vfx_ObstacleDestroyed", steel, 28, 8f, 0.4f, 0.6f, 1.5f, 0.6f), 0.9f, new Vector3(0f, 1f, 0f)),
                Entry(VfxEvent.ShieldBroken, Burst("Vfx_ShieldBreak", cyan, 30, 9f, 0.28f, 0.5f, 0.4f, 0.5f), 0.8f, new Vector3(0f, 1f, 0.4f)),
                Entry(VfxEvent.Landed, Burst("Vfx_Land", dust, 8, 2.2f, 0.3f, 0.3f, 0f, 0.35f, true), 0.5f, new Vector3(0f, 0.05f, 0f)),
                Entry(VfxEvent.Caught, Burst("Vfx_Caught", red, 40, 8f, 0.4f, 0.8f, 0.6f, 0.5f), 1.2f, new Vector3(0f, 1f, 0f)),
            };
            library.speedLinesPrefab = SpeedLines();
            library.auraPrefab = Aura(cyan);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
        }

        static VfxEntry Entry(VfxEvent trigger, GameObject prefab, float lifetime, Vector3 offset)
        {
            return new VfxEntry { trigger = trigger, prefab = prefab, lifetime = lifetime, offset = offset };
        }

        static Material ParticleMaterial()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            var material = pipeline != null ? pipeline.defaultParticleMaterial : null;
            if (material == null) throw new System.InvalidOperationException("URP parçacık materyali bulunamadı.");
            return material;
        }

        static ParticleSystem NewParticles(string name, Color color, float lifetime, float size, int maxParticles)
        {
            var particles = new GameObject(name).AddComponent<ParticleSystem>();

            var main = particles.main;
            main.playOnAwake = true;
            main.startColor = color;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
            main.maxParticles = maxParticles;

            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = ParticleMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return particles;
        }

        /// <summary>Tek seferlik patlama.</summary>
        static GameObject Burst(string name, Color color, int count, float speed, float size, float lifetime, float gravity, float radius, bool flat = false)
        {
            var particles = NewParticles(name, color, lifetime, size, count);

            var main = particles.main;
            main.duration = 1f;
            main.loop = false;
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = particles.shape;
            shape.shapeType = flat ? ParticleSystemShapeType.Circle : ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            if (flat) shape.rotation = new Vector3(-90f, 0f, 0f);

            var shrink = particles.sizeOverLifetime;
            shrink.enabled = true;
            shrink.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            return SavePrefab(particles.gameObject);
        }

        /// <summary>Kameraya bağlı hız çizgileri: ekranın kenarlarından geriye akar.</summary>
        static GameObject SpeedLines()
        {
            var particles = NewParticles("Vfx_SpeedLines", new Color(1f, 1f, 1f, 0.4f), 0.5f, 0.07f, 120);
            particles.transform.localPosition = new Vector3(0f, 0f, 24f);

            var main = particles.main;
            main.loop = true;
            main.startSpeed = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = particles.emission;
            emission.rateOverTime = 0f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 5f;
            shape.radiusThickness = 0.3f;

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = 0f;
            velocity.y = 0f;
            velocity.z = -46f;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.05f;
            renderer.lengthScale = 1f;

            return SavePrefab(particles.gameObject);
        }

        /// <summary>Süreli etki aktifken oyuncunun ayaklarından yükselen halka.</summary>
        static GameObject Aura(Color color)
        {
            var particles = NewParticles("Vfx_Aura", color, 0.7f, 0.16f, 60);
            particles.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            var main = particles.main;
            main.loop = true;
            main.startSpeed = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = particles.emission;
            emission.rateOverTime = 28f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.75f;
            shape.radiusThickness = 0f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = 0f;
            velocity.y = 2.4f;
            velocity.z = 0f;

            return SavePrefab(particles.gameObject);
        }
    }
}
