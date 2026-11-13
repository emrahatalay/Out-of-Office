using System;
using System.Collections.Generic;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Oyuncunun önüne zemin parçası dizer, geride kalanı havuza geri verir.</summary>
    public sealed class TrackSystem : ITickable, IDisposable
    {
        readonly RunSession _session;
        readonly PlayerStatus _player;
        readonly IPoolService _pool;
        readonly GameConfig _config;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly Queue<GameObject> _segments = new Queue<GameObject>();

        WorldDefinition _world;
        float _nextZ;

        public TrackSystem(RunSession session, PlayerStatus player, IPoolService pool, GameConfig config, ISignalBus bus)
        {
            _session = session;
            _player = player;
            _pool = pool;
            _config = config;

            bus.Subscribe<RunStartedSignal>(_ => Clear()).AddTo(_subscriptions);
            bus.Subscribe<RunClearedSignal>(_ => Clear()).AddTo(_subscriptions);
        }

        // Menüde de çalışır; böylece oyuncu boşlukta durmaz.
        public void Tick()
        {
            var world = _session.World;
            if (world == null || world.groundSegmentPrefab == null) return;

            if (world != _world)
            {
                Clear();
                _world = world;
            }

            float length = world.segmentLength;
            float playerZ = _player.Position.z;

            while (_nextZ < playerZ + _config.trackAhead)
            {
                _segments.Enqueue(_pool.Spawn(world.groundSegmentPrefab, new Vector3(0f, 0f, _nextZ), Quaternion.identity));
                _nextZ += length;
            }

            while (_segments.Count > 0 && _segments.Peek().transform.position.z + length < playerZ - _config.despawnBehind)
                _pool.Despawn(_segments.Dequeue());
        }

        void Clear()
        {
            while (_segments.Count > 0) _pool.Despawn(_segments.Dequeue());
            _nextZ = _session.World != null ? -_session.World.segmentLength : 0f;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
