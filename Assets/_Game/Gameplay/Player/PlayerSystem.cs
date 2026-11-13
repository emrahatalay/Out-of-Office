using System;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Oyuncunun hareketi: ileri koşu, şerit değiştirme, zıplama, kayma.</summary>
    public sealed class PlayerSystem : ITickable, IDisposable
    {
        readonly GameConfig _config;
        readonly RunSession _session;
        readonly PlayerStatus _status;
        readonly PlayerRig _rig;
        readonly CharacterSelection _selection;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        const float FlightClimbSpeed = 14f;

        int _lane;
        float _x;
        float _y;
        float _z;
        float _sideSpeed;
        float _verticalSpeed;
        float _slideTimer;
        float _jumpBufferedUntil;
        bool _grounded = true;
        bool _flying;

        public PlayerSystem(
            GameConfig config,
            RunSession session,
            PlayerStatus status,
            PlayerRig rig,
            CharacterSelection selection,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _config = config;
            _session = session;
            _status = status;
            _rig = rig;
            _selection = selection;
            _state = state;
            _bus = bus;

            _bus.Subscribe<RunStartedSignal>(_ => ResetToStart()).AddTo(_subscriptions);
            _bus.Subscribe<RunClearedSignal>(_ => ResetToStart()).AddTo(_subscriptions);
            _bus.Subscribe<MoveInputSignal>(OnMoveInput).AddTo(_subscriptions);
            _bus.Subscribe<CharacterSelectedSignal>(_ => _rig.SetVisual(_selection.Current.prefab)).AddTo(_subscriptions);
            _bus.Subscribe<GameStateChangedSignal>(OnStateChanged).AddTo(_subscriptions);
            _bus.Subscribe<PlayerStumbledSignal>(_ => _rig.Stumble()).AddTo(_subscriptions);

            ResetToStart();
        }

        void ResetToStart()
        {
            _lane = _config.laneCount / 2;
            _x = _config.LaneToX(_lane);
            _y = 0f;
            _z = 0f;
            _sideSpeed = 0f;
            _verticalSpeed = 0f;
            _grounded = true;
            _slideTimer = 0f;
            _jumpBufferedUntil = 0f;
            _flying = false;
            _status.ResetModifiers();
            _rig.SetSliding(false);
            _rig.Animate(0f, 0f, 0f, true, 0f);
            Apply();
        }

        void OnStateChanged(GameStateChangedSignal signal)
        {
            // Yakalanınca Tick durur; görsel koşu döngüsünde takılı kalmasın diye durdurulur.
            if (signal.To == GameState.GameOver)
            {
                _rig.Animate(0f, 0f, 0f, true, 0f);
                _rig.SetCaught(true);
            }
            else if (signal.From == GameState.GameOver)
            {
                // Anahtarla devam ya da yeni koşu: ayağa kalkar.
                _rig.SetCaught(false);
            }
        }

        void OnMoveInput(MoveInputSignal signal)
        {
            if (_state.Current != GameState.Running) return;

            switch (signal.Direction)
            {
                case MoveDirection.Left: ChangeLane(-1); break;
                case MoveDirection.Right: ChangeLane(1); break;
                case MoveDirection.Up: Jump(); break;
                case MoveDirection.Down: Slide(); break;
            }
        }

        void ChangeLane(int step)
        {
            int target = Mathf.Clamp(_lane + step, 0, _config.laneCount - 1);
            if (target == _lane) return;

            int from = _lane;
            _lane = target;
            _status.Lane = _lane;
            _bus.Fire(new LaneChangedSignal(from, target));
        }

        void Jump()
        {
            if (_flying) return;

            if (!_grounded)
            {
                // Girdi tamponu: inişten hemen önce basılan zıplama kaybolmaz.
                _jumpBufferedUntil = Time.time + _config.inputBufferSeconds;
                return;
            }

            StopSliding();
            _grounded = false;
            _verticalSpeed = Mathf.Sqrt(2f * _config.gravity * _config.jumpHeight * Mathf.Max(1f, _status.JumpMultiplier));
            _bus.Fire<PlayerJumpedSignal>();
        }

        void Slide()
        {
            if (_flying) return;

            // Havadayken aşağı kaydırma: hızlı in, sonra kay.
            if (!_grounded) _verticalSpeed = -_config.fastFallSpeed;

            _slideTimer = _config.slideDuration;
            _rig.SetSliding(true);
            _bus.Fire<PlayerSlidSignal>();
        }

        void StopSliding()
        {
            if (_slideTimer <= 0f) return;
            _slideTimer = 0f;
            _rig.SetSliding(false);
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running) return;

            float dt = Time.deltaTime;

            _z += _session.Speed * dt;
            _x = Mathf.SmoothDamp(_x, _config.LaneToX(_lane), ref _sideSpeed, _config.laneSmoothTime, Mathf.Infinity, dt);

            float flightHeight = _status.FlightHeight;
            if (flightHeight > 0f)
            {
                // Uçuş: engellerin üstünde süzül. Zıplama ve kayma kapalı.
                if (!_flying)
                {
                    _flying = true;
                    _grounded = false;
                    StopSliding();
                }

                _verticalSpeed = 0f;
                _y = Mathf.MoveTowards(_y, flightHeight, FlightClimbSpeed * dt);
            }
            else if (!_grounded)
            {
                _flying = false;
                _verticalSpeed -= _config.gravity * dt;
                _y += _verticalSpeed * dt;
                if (_y <= 0f)
                {
                    _y = 0f;
                    _verticalSpeed = 0f;
                    _grounded = true;
                    _bus.Fire<PlayerLandedSignal>();
                    if (Time.time <= _jumpBufferedUntil) Jump();
                }
            }

            if (_slideTimer > 0f && _grounded)
            {
                _slideTimer -= dt;
                if (_slideTimer <= 0f) _rig.SetSliding(false);
            }

            _rig.Animate(dt, _sideSpeed, _verticalSpeed, _grounded, _session.Speed);
            Apply();
        }

        void Apply()
        {
            var position = new Vector3(_x, _y, _z);
            _rig.Root.position = position;
            _status.Position = position;
            _status.Lane = _lane;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
