using System;
using System.Collections.Generic;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Oyun olaylarını görsel efekte çevirir. Hangi olayda ne oynayacağı VfxLibrary asset'inde
    /// yazar; parçacıklar havuzdan gelir, koşu sırasında Instantiate yapılmaz.
    /// </summary>
    public sealed class VfxSystem : ITickable, IDisposable
    {
        struct Live
        {
            public GameObject Instance;
            public float EndsAt;
        }

        readonly VfxLibrary _library;
        readonly IPoolService _pool;
        readonly IPlayerStatus _player;
        readonly RunSession _session;
        readonly IGameStateProvider _state;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly List<Live> _live = new List<Live>(32);

        ParticleSystem _speedLines;
        GameObject _aura;
        int _timedEffects;

        public VfxSystem(
            VfxLibrary library,
            IPoolService pool,
            IPlayerStatus player,
            PlayerRig rig,
            Camera camera,
            RunSession session,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _library = library;
            _pool = pool;
            _player = player;
            _session = session;
            _state = state;

            if (library.speedLinesPrefab != null && camera != null)
                _speedLines = Object.Instantiate(library.speedLinesPrefab, camera.transform, false).GetComponent<ParticleSystem>();

            if (library.auraPrefab != null)
            {
                _aura = Object.Instantiate(library.auraPrefab, rig.Root, false);
                _aura.SetActive(false);
            }

            bus.Subscribe<CoinCollectedSignal>(_ => AtPlayer(VfxEvent.CoinCollected)).AddTo(_subscriptions);
            bus.Subscribe<PowerUpCollectedSignal>(_ => AtPlayer(VfxEvent.PowerUpCollected)).AddTo(_subscriptions);
            bus.Subscribe<PlayerStumbledSignal>(_ => AtPlayer(VfxEvent.Stumbled)).AddTo(_subscriptions);
            bus.Subscribe<ShieldBrokenSignal>(_ => AtPlayer(VfxEvent.ShieldBroken)).AddTo(_subscriptions);
            bus.Subscribe<PlayerLandedSignal>(_ => AtPlayer(VfxEvent.Landed)).AddTo(_subscriptions);
            bus.Subscribe<PlayerCaughtSignal>(_ => AtPlayer(VfxEvent.Caught)).AddTo(_subscriptions);
            bus.Subscribe<ObstacleDestroyedSignal>(s => Play(VfxEvent.ObstacleDestroyed, s.Position)).AddTo(_subscriptions);

            bus.Subscribe<TimedEffectStartedSignal>(_ => SetTimedEffects(_timedEffects + 1)).AddTo(_subscriptions);
            bus.Subscribe<TimedEffectEndedSignal>(_ => SetTimedEffects(_timedEffects - 1)).AddTo(_subscriptions);
            bus.Subscribe<RunStartedSignal>(_ => Clear()).AddTo(_subscriptions);
            bus.Subscribe<RunClearedSignal>(_ => Clear()).AddTo(_subscriptions);
        }

        void AtPlayer(VfxEvent trigger)
        {
            Play(trigger, _player.Position);
        }

        void Play(VfxEvent trigger, Vector3 position)
        {
            if (!_library.TryGet(trigger, out var entry)) return;

            var instance = _pool.Spawn(entry.prefab, position + entry.offset, Quaternion.identity);
            if (instance.TryGetComponent<ParticleSystem>(out var particles))
            {
                particles.Clear(true);
                particles.Play(true);
            }

            _live.Add(new Live { Instance = instance, EndsAt = Time.time + Mathf.Max(0.1f, entry.lifetime) });
        }

        public void Tick()
        {
            float now = Time.time;
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                if (_live[i].EndsAt > now) continue;

                _pool.Despawn(_live[i].Instance);
                int last = _live.Count - 1;
                _live[i] = _live[last];
                _live.RemoveAt(last);
            }

            if (_speedLines == null) return;

            float rate = 0f;
            if (_state.Current == GameState.Running)
            {
                float t = Mathf.InverseLerp(_library.speedLinesMinSpeed, Mathf.Max(_library.speedLinesMinSpeed + 1f, 30f), _session.Speed);
                rate = t * _library.speedLinesMaxRate;
            }

            var emission = _speedLines.emission;
            emission.rateOverTime = rate;
        }

        void SetTimedEffects(int count)
        {
            _timedEffects = Mathf.Max(0, count);
            if (_aura != null) _aura.SetActive(_timedEffects > 0);
        }

        void Clear()
        {
            for (int i = _live.Count - 1; i >= 0; i--) _pool.Despawn(_live[i].Instance);
            _live.Clear();
            SetTimedEffects(0);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
