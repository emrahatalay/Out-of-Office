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
        }

        void Slow(float scale, float seconds)
        {
            if (seconds <= 0f) return;
            Time.timeScale = scale;
            _remaining = seconds;
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
