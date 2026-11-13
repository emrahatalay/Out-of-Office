using System;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Kovalayan düşman. Davranışın tüm sayıları ChaserDefinition'dan gelir;
    /// yeni bir düşman için kod değil yeni bir asset gerekir.
    /// </summary>
    public sealed class ChaserSystem : IChaser, ITickable, IDisposable
    {
        const float IntroSeconds = 2f;
        const float CatchGap = 0.9f;

        readonly RunSession _session;
        readonly PlayerStatus _player;
        readonly IGameStateProvider _state;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        ChaserDefinition _definition;
        GameObject _visual;
        float _gap;
        float _x;
        float _threatTimer;
        float _introTimer;
        int _stunCount;

        public bool IsThreatening => _stunCount == 0 && _threatTimer > 0f;

        public ChaserSystem(RunSession session, PlayerStatus player, IGameStateProvider state, ISignalBus bus)
        {
            _session = session;
            _player = player;
            _state = state;

            bus.Subscribe<RunStartedSignal>(_ => OnRunStarted()).AddTo(_subscriptions);
            bus.Subscribe<RunClearedSignal>(_ => Hide()).AddTo(_subscriptions);
        }

        void OnRunStarted()
        {
            var definition = _session.World.chaser;
            if (definition != _definition)
            {
                _definition = definition;
                if (_visual != null) Object.Destroy(_visual);
                _visual = null;

                if (definition != null && definition.prefab != null)
                {
                    _visual = Object.Instantiate(definition.prefab);
                    _visual.name = "Chaser";
                    foreach (var collider in _visual.GetComponentsInChildren<Collider>()) collider.enabled = false;
                }
            }

            if (_definition == null) return;

            // Koşu, kovalayan ensedeyken başlar; sonra geri düşer.
            _gap = _definition.closeGap;
            _x = _player.Position.x + _definition.closeSideOffset;
            _threatTimer = 0f;
            _introTimer = IntroSeconds;
            _stunCount = 0;

            if (_visual != null) _visual.SetActive(true);
            Place();
        }

        void Hide()
        {
            if (_visual != null) _visual.SetActive(false);
        }

        public void CloseIn()
        {
            if (_definition == null || _stunCount > 0) return;
            _threatTimer = _definition.threatSeconds;
        }

        public void BeginStun()
        {
            _stunCount++;
            _threatTimer = 0f;
        }

        public void EndStun()
        {
            _stunCount = Mathf.Max(0, _stunCount - 1);
        }

        public void Tick()
        {
            if (_definition == null) return;

            var state = _state.Current;
            if (state != GameState.Running && state != GameState.GameOver) return;

            float dt = Time.deltaTime;
            float target;
            float speed;

            if (state == GameState.GameOver)
            {
                target = CatchGap;
                speed = _definition.closeInSpeed;
            }
            else if (_stunCount > 0)
            {
                target = _definition.stunnedGap;
                speed = _definition.closeInSpeed;
            }
            else if (_threatTimer > 0f)
            {
                _threatTimer -= dt;
                target = _definition.closeGap;
                speed = _definition.closeInSpeed;
            }
            else if (_introTimer > 0f)
            {
                _introTimer -= dt;
                target = _definition.closeGap;
                speed = _definition.closeInSpeed;
            }
            else
            {
                target = _definition.restGap;
                speed = _definition.fallBackSpeed;
            }

            _gap = Mathf.MoveTowards(_gap, target, speed * dt);
            // Yaklaştıkça oyuncunun yanına (pistin ortasına doğru) kayar; kamerayı kapatmaz.
            float playerX = _player.Position.x;
            float closeness = Mathf.InverseLerp(_definition.restGap, _definition.closeGap, _gap);
            float side = (playerX > 0.1f ? -1f : 1f) * _definition.closeSideOffset * closeness;
            _x = Mathf.Lerp(_x, playerX + side, 1f - Mathf.Exp(-_definition.sideFollowSpeed * dt));
            Place();
        }

        void Place()
        {
            if (_visual == null) return;
            // Geçici görsel için koşma zıplaması; gerçek modelde animasyon yapar.
            float bob = Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.12f;
            _visual.transform.position = new Vector3(_x, bob, _player.Position.z - _gap);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            if (_visual != null) Object.Destroy(_visual);
        }
    }
}
