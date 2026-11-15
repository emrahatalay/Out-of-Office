using System;
using System.Collections.Generic;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Sahnedeki altın ve güçlendirmeleri yönetir.</summary>
    public sealed class PickupField : ITickable, IDisposable
    {
        const float SpinDegreesPerSecond = 180f;
        const float BobSpeed = 3.2f;
        const float BobHeight = 0.12f;
        const float MagnetSpeed = 60f;
        const float AutoCollectSpeed = 110f;

        readonly IPoolService _pool;
        readonly PlayerStatus _player;
        readonly GameConfig _config;
        readonly IGameStateProvider _state;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly List<PickupView> _active = new List<PickupView>(128);

        public PickupField(IPoolService pool, PlayerStatus player, GameConfig config, IGameStateProvider state, ISignalBus bus)
        {
            _pool = pool;
            _player = player;
            _config = config;
            _state = state;

            bus.Subscribe<RunStartedSignal>(_ => Clear()).AddTo(_subscriptions);
            bus.Subscribe<RunClearedSignal>(_ => Clear()).AddTo(_subscriptions);
        }

        /// <param name="powerUp">Altın için null.</param>
        public void Spawn(GameObject prefab, PowerUpDefinition powerUp, Vector3 position)
        {
            if (prefab == null) return;

            var instance = _pool.Spawn(prefab, position, Quaternion.identity);
            if (!instance.TryGetComponent<PickupView>(out var view)) view = instance.AddComponent<PickupView>();

            view.Bind(powerUp);
            view.BaseY = position.y;
            _active.Add(view);
        }

        public void Despawn(PickupView view)
        {
            int index = _active.IndexOf(view);
            if (index >= 0) RemoveAt(index);
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running) return;

            float spin = SpinDegreesPerSecond * Time.deltaTime;
            float cullZ = _player.Position.z - _config.despawnBehind;
            float time = Time.time * BobSpeed;

            float magnet = _player.MagnetRadius;
            float autoRange = _player.AutoCollectRange;
            var playerPosition = _player.Position;
            float step = MagnetSpeed * Time.deltaTime;
            float autoStep = AutoCollectSpeed * Time.deltaTime;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var view = _active[i];
                var transform = view.transform;
                var position = transform.position;
                if (position.z < cullZ)
                {
                    RemoveAt(i);
                    continue;
                }

                // Prompt Injection: ileride, her şeritteki altın hızla gelir (şerit sınırı yok).
                if (autoRange > 0f && view.PowerUp == null)
                {
                    float ahead = position.z - playerPosition.z;
                    if (ahead < autoRange && ahead > -2f)
                    {
                        transform.position = Vector3.MoveTowards(position, playerPosition, autoStep);
                        continue;
                    }
                }

                // Mıknatıs: menzildeki altınlar oyuncuya uçar, toplama yine çarpışmayla olur.
                if (magnet > 0f && view.PowerUp == null)
                {
                    float ahead = position.z - playerPosition.z;
                    if (ahead < magnet && ahead > -2f && Mathf.Abs(position.x - playerPosition.x) < magnet)
                    {
                        transform.position = Vector3.MoveTowards(position, playerPosition, step);
                        continue;
                    }
                }

                // Dönme + hafif süzülme. Faz, konumdan gelir.
                position.y = view.BaseY + Mathf.Sin(time + position.z * 0.6f) * BobHeight;
                transform.SetPositionAndRotation(position, transform.rotation * Quaternion.Euler(0f, spin, 0f));
            }
        }

        void Clear()
        {
            for (int i = _active.Count - 1; i >= 0; i--) RemoveAt(i);
        }

        void RemoveAt(int index)
        {
            var view = _active[index];
            int last = _active.Count - 1;
            _active[index] = _active[last];
            _active.RemoveAt(last);

            if (view != null) _pool.Despawn(view.gameObject);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
