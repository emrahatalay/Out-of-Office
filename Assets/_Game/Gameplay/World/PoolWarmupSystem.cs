using OutOfOffice.Core.Services;
using OutOfOffice.Data;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Koşuda kullanılacak nesneleri yükleme anında üretir. Böylece ilk engel, ilk altın ya da
    /// ilk efekt çıktığında Instantiate takılması olmaz.
    /// </summary>
    public sealed class PoolWarmupSystem : IStartable
    {
        readonly ContentCatalog _catalog;
        readonly VfxLibrary _vfx;
        readonly GameConfig _config;
        readonly IPoolService _pool;

        public PoolWarmupSystem(ContentCatalog catalog, VfxLibrary vfx, GameConfig config, IPoolService pool)
        {
            _catalog = catalog;
            _vfx = vfx;
            _config = config;
            _pool = pool;
        }

        public void Start()
        {
            foreach (var world in _catalog.worlds)
            {
                if (world == null) continue;

                _pool.Prewarm(world.groundSegmentPrefab, _config.prewarmGround);
                _pool.Prewarm(world.coinPrefab, _config.prewarmCoins);

                if (world.obstacles != null)
                    foreach (var obstacle in world.obstacles)
                        if (obstacle != null) _pool.Prewarm(obstacle.prefab, _config.prewarmPerObstacle);

                if (world.powerUps != null)
                    foreach (var powerUp in world.powerUps)
                        if (powerUp != null) _pool.Prewarm(powerUp.pickupPrefab, 1);
            }

            foreach (var character in _catalog.characters)
                if (character != null && character.automatedObstacle != null)
                    _pool.Prewarm(character.automatedObstacle.prefab, _config.prewarmPerObstacle);

            if (_vfx.bursts == null) return;
            foreach (var entry in _vfx.bursts) _pool.Prewarm(entry.prefab, _config.prewarmPerVfx);
        }
    }
}
