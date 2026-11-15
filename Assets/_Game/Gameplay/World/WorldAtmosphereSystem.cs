using System;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Gökyüzü, sis, güneş ve ortam ışığını oyuncunun mesafesine göre dünyalar arasında karıştırır.
    /// Yeni dünyanın havası sınırdan <c>GameConfig.worldBlendDistance</c> metre önce karışmaya başlar, sınırda tamamlanır.
    /// Menüde ilk dünyanın havası vardır. Gökyüzü materyali çalışma anında kopyalanır; asset'e yazılmaz.
    /// </summary>
    public sealed class WorldAtmosphereSystem : IPostStartable, ITickable, IDisposable
    {
        static readonly int ZenithColor = Shader.PropertyToID("_ZenithColor");
        static readonly int HorizonColor = Shader.PropertyToID("_HorizonColor");
        static readonly int CloudColor = Shader.PropertyToID("_CloudColor");
        static readonly int CloudShadeColor = Shader.PropertyToID("_CloudShadeColor");
        static readonly int CloudCoverage = Shader.PropertyToID("_CloudCoverage");

        readonly WorldTimeline _timeline;
        readonly RunSession _session;
        readonly GameConfig _config;
        readonly IGameStateProvider _state;
        Material _sky;

        public WorldAtmosphereSystem(WorldTimeline timeline, RunSession session, GameConfig config, IGameStateProvider state)
        {
            _timeline = timeline;
            _session = session;
            _config = config;
            _state = state;
        }

        public void PostStart()
        {
            if (RenderSettings.skybox != null)
            {
                _sky = new Material(RenderSettings.skybox) { name = "Sky (çalışma anı)" };
                RenderSettings.skybox = _sky;
            }

            Apply(0f);
        }

        public void Tick()
        {
            // Menüde (ve açılışta) ilk dünyanın havası; koşuda ve oyun sonunda oyuncunun yeri.
            var state = _state.Current;
            bool inRun = state == GameState.Running || state == GameState.Paused || state == GameState.GameOver;
            Apply(inRun ? _session.Distance : 0f);
        }

        void Apply(float distance)
        {
            if (_timeline.Count == 0) return;

            int index = _timeline.IndexAt(distance);
            var current = _timeline.Get(index).atmosphere;
            if (index + 1 < _timeline.Count)
            {
                var next = _timeline.Get(index + 1);
                float t = Mathf.InverseLerp(next.startDistance - _config.worldBlendDistance, next.startDistance, distance);
                if (t > 0f) current = Blend(current, next.atmosphere, Mathf.SmoothStep(0f, 1f, t));
            }

            Write(current);
        }

        static AtmosphereSettings Blend(AtmosphereSettings a, AtmosphereSettings b, float t)
        {
            return new AtmosphereSettings
            {
                zenithColor = Color.Lerp(a.zenithColor, b.zenithColor, t),
                horizonColor = Color.Lerp(a.horizonColor, b.horizonColor, t),
                cloudColor = Color.Lerp(a.cloudColor, b.cloudColor, t),
                cloudShadeColor = Color.Lerp(a.cloudShadeColor, b.cloudShadeColor, t),
                cloudCoverage = Mathf.Lerp(a.cloudCoverage, b.cloudCoverage, t),
                sunColor = Color.Lerp(a.sunColor, b.sunColor, t),
                sunIntensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t),
                ambientSky = Color.Lerp(a.ambientSky, b.ambientSky, t),
                ambientEquator = Color.Lerp(a.ambientEquator, b.ambientEquator, t),
                ambientGround = Color.Lerp(a.ambientGround, b.ambientGround, t),
                fogStart = Mathf.Lerp(a.fogStart, b.fogStart, t),
                fogEnd = Mathf.Lerp(a.fogEnd, b.fogEnd, t),
            };
        }

        void Write(in AtmosphereSettings a)
        {
            RenderSettings.fogColor = a.horizonColor;
            RenderSettings.fogStartDistance = a.fogStart;
            RenderSettings.fogEndDistance = a.fogEnd;
            RenderSettings.ambientSkyColor = a.ambientSky;
            RenderSettings.ambientEquatorColor = a.ambientEquator;
            RenderSettings.ambientGroundColor = a.ambientGround;

            var sun = RenderSettings.sun;
            if (sun != null)
            {
                sun.color = a.sunColor;
                sun.intensity = a.sunIntensity;
            }

            if (_sky == null) return;
            _sky.SetColor(ZenithColor, a.zenithColor);
            _sky.SetColor(HorizonColor, a.horizonColor);
            _sky.SetColor(CloudColor, a.cloudColor);
            _sky.SetColor(CloudShadeColor, a.cloudShadeColor);
            _sky.SetFloat(CloudCoverage, a.cloudCoverage);
        }

        public void Dispose()
        {
            if (_sky != null) UnityEngine.Object.Destroy(_sky);
        }
    }
}
