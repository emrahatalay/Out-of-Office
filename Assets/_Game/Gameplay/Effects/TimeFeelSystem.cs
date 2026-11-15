using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Çarpışma anlarında kısa ağır çekim ("hit-stop"). Duraklatmaya karışmaz:
    /// zaman ölçeğini yalnızca koşu ya da oyun sonu durumunda değiştirir.
    /// </summary>
    public sealed class TimeFeelSystem : ITickable, IDisposable
    {
        readonly GameConfig _config;
        readonly IGameStateProvider _state;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        float _remaining;

        public TimeFeelSystem(GameConfig config, IGameStateProvider state, ISignalBus bus)
        {
            _config = config;
            _state = state;

            bus.Subscribe<PlayerStumbledSignal>(_ => Slow(_config.stumbleTimeScale, _config.stumbleSlowSeconds)).AddTo(_subscriptions);
            bus.Subscribe<GameStateChangedSignal>(OnStateChanged).AddTo(_subscriptions);
            bus.Subscribe<TutorialSlowMotionSignal>(OnTutorialSlow).AddTo(_subscriptions);
        }

        void Slow(float scale, float seconds)
        {
            if (seconds <= 0f) return;
            Time.timeScale = scale;
            _remaining = seconds;
        }

        // Öğretici yavaşlatması süreyle değil bayrakla çalışır: oyuncu hareketi yapana kadar sürer.
        void OnTutorialSlow(TutorialSlowMotionSignal signal)
        {
            if (signal.On)
            {
                _remaining = 0f;
                Time.timeScale = _config.tutorialTimeScale;
                return;
            }

            // Başka bir yavaşlatma (ör. tökezleme) sürüyorsa onu bozma.
            if (_remaining <= 0f && _state.Current == GameState.Running) Time.timeScale = 1f;
        }

        void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.To == GameState.GameOver)
            {
                Slow(_config.caughtTimeScale, _config.caughtSlowSeconds);
                return;
            }

            // Menü ve duraklatma kendi zaman ölçeğini GameFlow'dan alır.
            if (signal.To == GameState.Menu || signal.To == GameState.Paused || signal.To == GameState.Running) _remaining = 0f;
        }

        public void Tick()
        {
            if (_remaining <= 0f) return;

            _remaining -= Time.unscaledDeltaTime;
            if (_remaining > 0f) return;

            var state = _state.Current;
            if (state == GameState.Running || state == GameState.GameOver) Time.timeScale = 1f;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
