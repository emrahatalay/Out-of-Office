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
        readonly WorldTimeline _timeline;
        readonly PlayerStatus _player;
        readonly IPoolService _pool;
        readonly GameConfig _config;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly Queue<GameObject> _segments = new Queue<GameObject>();

        float _nextZ;

        public TrackSystem(WorldTimeline timeline, PlayerStatus player, IPoolService pool, GameConfig config, ISignalBus bus)
        {
            _timeline = timeline;
            _player = player;
            _pool = pool;
            _config = config;

            // İlk açılışta da, sonraki temizlikler gibi, oyuncunun bir parça gerisinden başla;
            // yoksa menü kamerasının baktığı karakterin arkası boş kalır.
            ResetStart();

            bus.Subscribe<RunStartedSignal>(_ => Clear()).AddTo(_subscriptions);
            bus.Subscribe<RunClearedSignal>(_ => Clear()).AddTo(_subscriptions);
        }

        // Menüde de çalışır; böylece oyuncu boşlukta durmaz. Her parça, kendi yerindeki dünyanın zeminidir:
        // sınırı geçen parçalar yeni dünyanın zemini olur, eskiler yerinde kalır.
        public void Tick()
        {
            var first = _timeline.Get(0);
            if (first == null) return;

            float length = first.segmentLength;
            float playerZ = _player.Position.z;

            while (_nextZ < playerZ + _config.trackAhead)
            {
                var world = _timeline.WorldAt(_nextZ);
                if (world == null || world.groundSegmentPrefab == null) break;

                _segments.Enqueue(_pool.Spawn(world.groundSegmentPrefab, new Vector3(0f, 0f, _nextZ), Quaternion.identity));
                _nextZ += length;
            }

            while (_segments.Count > 0 && _segments.Peek().transform.position.z + length < playerZ - _config.despawnBehind)
                _pool.Despawn(_segments.Dequeue());
        }

        void Clear()
        {
            while (_segments.Count > 0) _pool.Despawn(_segments.Dequeue());
            ResetStart();
        }

        void ResetStart()
        {
            var first = _timeline.Get(0);
            _nextZ = first != null ? -first.segmentLength : 0f;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
