using System;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>Oyuncu bir şeye değdiğinde ne olacağına karar verir.</summary>
    public sealed class CollisionSystem : IDisposable
    {
        readonly PlayerRig _rig;
        readonly PlayerStatus _status;
        readonly IChaser _chaser;
        readonly ObstacleField _obstacles;
        readonly PickupField _pickups;
        readonly RunSession _session;
        readonly EffectRunner _effects;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;

        public CollisionSystem(
            PlayerRig rig,
            PlayerStatus status,
            IChaser chaser,
            ObstacleField obstacles,
            PickupField pickups,
            RunSession session,
            EffectRunner effects,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _rig = rig;
            _status = status;
            _chaser = chaser;
            _obstacles = obstacles;
            _pickups = pickups;
            _session = session;
            _effects = effects;
            _state = state;
            _bus = bus;

            _rig.Relay.Entered += OnTouched;
        }

        void OnTouched(Collider other)
        {
            if (_state.Current != GameState.Running) return;

            var obstacle = other.GetComponentInParent<ObstacleView>();
            if (obstacle != null)
            {
                HitObstacle(obstacle);
                return;
            }

            var pickup = other.GetComponentInParent<PickupView>();
            if (pickup != null) Collect(pickup);
        }

        void HitObstacle(ObstacleView obstacle)
        {
            if (obstacle.IsGhost) return;

            if (_status.HasShield)
            {
                _status.HasShield = false;
                _obstacles.Destroy(obstacle);
                _bus.Fire<ShieldBrokenSignal>();
                return;
            }

            if (_chaser.IsThreatening)
            {
                _bus.Fire<PlayerCaughtSignal>();
                return;
            }

            // İlk çarpışma affedilir ama kovalayan enseye gelir.
            _obstacles.Destroy(obstacle);
            _chaser.CloseIn();
            _bus.Fire<PlayerStumbledSignal>();
        }

        void Collect(PickupView pickup)
        {
            var powerUp = pickup.PowerUp;
            _pickups.Despawn(pickup);

            if (powerUp == null)
            {
                _session.Coins++;
                _bus.Fire(new CoinCollectedSignal(_session.Coins));
                return;
            }

            _bus.Fire(new PowerUpCollectedSignal(powerUp.id));
            _effects.Run(powerUp.EffectId, powerUp.nameKey, powerUp.effects, powerUp.duration);
        }

        public void Dispose()
        {
            if (_rig.Relay != null) _rig.Relay.Entered -= OnTouched;
        }
    }
}
