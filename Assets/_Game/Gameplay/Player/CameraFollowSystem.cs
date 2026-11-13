using System;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Kamera: menüde karakteri gösterir, koşuda arkadan takip eder. Hız arttıkça görüş açısı
    /// açılır; çarpışmalarda sarsılır. Tüm sayılar GameConfig'ten gelir.
    /// </summary>
    public sealed class CameraFollowSystem : ILateTickable, IDisposable
    {
        const float ShakeFrequency = 34f;

        readonly Camera _camera;
        readonly IPlayerStatus _player;
        readonly RunSession _session;
        readonly GameConfig _config;
        readonly IGameStateProvider _state;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        float _x;
        float _xVelocity;
        float _y;
        float _yVelocity;
        float _blend;
        float _shakeTime;
        float _shakeAmplitude;

        public CameraFollowSystem(
            Camera camera,
            IPlayerStatus player,
            RunSession session,
            GameConfig config,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _camera = camera;
            _player = player;
            _session = session;
            _config = config;
            _state = state;

            bus.Subscribe<PlayerStumbledSignal>(_ => Shake(_config.stumbleShake)).AddTo(_subscriptions);
            bus.Subscribe<ShieldBrokenSignal>(_ => Shake(_config.shieldShake)).AddTo(_subscriptions);
            bus.Subscribe<PlayerCaughtSignal>(_ => Shake(_config.caughtShake)).AddTo(_subscriptions);
        }

        void Shake(float amplitude)
        {
            _shakeAmplitude = Mathf.Max(_shakeAmplitude, amplitude);
            _shakeTime = _config.shakeSeconds;
        }

        public void LateTick()
        {
            if (_camera == null) return;

            // Kamera gerçek zamanla hareket eder; ağır çekimde de akıcı kalır.
            float dt = Time.unscaledDeltaTime;
            var state = _state.Current;
            var player = _player.Position;
            bool inMenu = state == GameState.Menu || state == GameState.Boot;

            // Koşu pozu
            _x = Mathf.SmoothDamp(_x, player.x * _config.cameraSideFollow, ref _xVelocity, _config.cameraSmoothTime, Mathf.Infinity, dt);
            float heightFollow = _player.FlightHeight > 0f ? _config.cameraFlightFollow : _config.cameraJumpFollow;
            _y = Mathf.SmoothDamp(_y, player.y * heightFollow, ref _yVelocity, _config.cameraSmoothTime * 2f, Mathf.Infinity, dt);

            var runPosition = new Vector3(_x, _y, player.z) + _config.cameraOffset;
            var runLook = new Vector3(_x, _config.cameraLookHeight + _y, player.z + _config.cameraLookAhead);
            var runRotation = Quaternion.LookRotation(runLook - runPosition, Vector3.up);

            // Menü pozu
            var menuPosition = player + _config.menuCameraOffset;
            var menuRotation = Quaternion.LookRotation(player + _config.menuLookAt - menuPosition, Vector3.up);

            float blendSpeed = dt / Mathf.Max(0.01f, _config.cameraBlendSeconds);
            _blend = Mathf.MoveTowards(_blend, inMenu ? 0f : 1f, blendSpeed);
            float eased = _blend * _blend * (3f - 2f * _blend);

            var position = Vector3.Lerp(menuPosition, runPosition, eased);
            var rotation = Quaternion.Slerp(menuRotation, runRotation, eased);

            if (_shakeTime > 0f && state != GameState.Paused)
            {
                _shakeTime -= dt;
                float strength = _shakeAmplitude * Mathf.Clamp01(_shakeTime / _config.shakeSeconds);
                float t = Time.unscaledTime * ShakeFrequency;
                position += rotation * new Vector3(
                    (Mathf.PerlinNoise(t, 0.3f) - 0.5f) * 2f * strength,
                    (Mathf.PerlinNoise(0.7f, t) - 0.5f) * 2f * strength,
                    0f);
                if (_shakeTime <= 0f) _shakeAmplitude = 0f;
            }

            _camera.transform.SetPositionAndRotation(position, rotation);

            // Hız arttıkça görüş açısı açılır.
            float speed01 = state == GameState.Running
                ? Mathf.InverseLerp(_config.startSpeed, _config.maxSpeed, _session.Speed)
                : 0f;
            float targetFov = Mathf.Lerp(_config.baseFieldOfView, _config.maxFieldOfView, speed01);
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFov, 1f - Mathf.Exp(-3f * dt));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
