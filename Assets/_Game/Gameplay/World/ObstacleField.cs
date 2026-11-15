using System;
using System.Collections.Generic;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Sahnedeki tüm engelleri yönetir: üretim, hareket, geride kalanı toplama.
    /// Engelleri tek tek tanımaz; her şeyi ObstacleDefinition'dan okur.
    /// </summary>
    public sealed class ObstacleField : IObstacleField, ITickable, IDisposable
    {
        readonly IPoolService _pool;
        readonly PlayerStatus _player;
        readonly GameConfig _config;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly List<ObstacleView> _active = new List<ObstacleView>(64);
        int _ghostCount;
        int _unplugCount;

        public ObstacleField(IPoolService pool, PlayerStatus player, GameConfig config, IGameStateProvider state, ISignalBus bus)
        {
            _pool = pool;
            _player = player;
            _config = config;
            _state = state;
            _bus = bus;

            bus.Subscribe<RunStartedSignal>(_ => Clear()).AddTo(_subscriptions);
            bus.Subscribe<RunClearedSignal>(_ => Clear()).AddTo(_subscriptions);
        }

        public ObstacleView Spawn(ObstacleDefinition definition, Vector3 position)
        {
            var instance = _pool.Spawn(definition.prefab, position, Quaternion.identity);
            if (!instance.TryGetComponent<ObstacleView>(out var view)) view = instance.AddComponent<ObstacleView>();

            view.Bind(definition, position);
            view.SetGhost(IsHarmless(view));
            _active.Add(view);
            return view;
        }

        /// <summary>Engeli efektle birlikte yok eder.</summary>
        public void Destroy(ObstacleView view)
        {
            int index = _active.IndexOf(view);
            if (index < 0) return;

            _bus.Fire(new ObstacleDestroyedSignal(view.transform.position));
            RemoveAt(index);
        }

        public void BeginGhost()
        {
            _ghostCount++;
            if (_ghostCount == 1) ApplyGhost(true);
        }

        public void EndGhost()
        {
            if (_ghostCount == 0) return;
            _ghostCount--;
            if (_ghostCount == 0) ApplyGhost(false);
        }

        public void BeginUnplugRobots()
        {
            _unplugCount++;
            if (_unplugCount == 1) RefreshGhost();
        }

        public void EndUnplugRobots()
        {
            if (_unplugCount == 0) return;
            _unplugCount--;
            if (_unplugCount == 0) RefreshGhost();
        }

        public int DestroyInRange(float fromZ, float range)
        {
            int destroyed = 0;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                float z = _active[i].transform.position.z;
                if (z < fromZ || z > fromZ + range) continue;
                _bus.Fire(new ObstacleDestroyedSignal(_active[i].transform.position));
                RemoveAt(i);
                destroyed++;
            }

            return destroyed;
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running) return;

            float dt = Time.deltaTime;
            float cullZ = _player.Position.z - _config.despawnBehind;
            float halfTrack = (_config.laneCount - 1) * 0.5f * _config.laneWidth;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var view = _active[i];
                var movement = view.Definition.movement;
                // Fişi çekilmiş robot yerinde donar.
                if (movement != null && !IsUnplugged(view))
                {
                    view.Age += dt;
                    view.transform.position = movement.Evaluate(view.Origin, view.Age, _config.laneWidth, halfTrack);
                }

                if (!IsUnplugged(view)) view.AdvanceSpin(dt);

                if (view.transform.position.z < cullZ) RemoveAt(i);
            }
        }

        void Clear()
        {
            for (int i = _active.Count - 1; i >= 0; i--) RemoveAt(i);
            _ghostCount = 0;
            _unplugCount = 0;
        }

        void ApplyGhost(bool ghost)
        {
            RefreshGhost();
        }

        bool IsUnplugged(ObstacleView view) => _unplugCount > 0 && view.Definition.isRobot;

        bool IsHarmless(ObstacleView view) => _ghostCount > 0 || IsUnplugged(view);

        void RefreshGhost()
        {
            for (int i = 0; i < _active.Count; i++) _active[i].SetGhost(IsHarmless(_active[i]));
        }

        void RemoveAt(int index)
        {
            var view = _active[index];
            int last = _active.Count - 1;
            _active[index] = _active[last];
            _active.RemoveAt(last);

            if (view == null) return;
            view.SetGhost(false);
            _pool.Despawn(view.gameObject);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
