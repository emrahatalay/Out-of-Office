using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Oyun durumunu değiştiren TEK yer. Diğer herkes istek sinyali atar,
    /// sonucu GameStateChangedSignal ile öğrenir.
    /// </summary>
    public sealed class GameFlow : IGameStateProvider, IPostStartable, IDisposable
    {
        readonly ISignalBus _bus;
        readonly StateMachine<GameState> _machine;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        public GameState Current => _machine.Current;

        public GameFlow(ISignalBus bus)
        {
            _bus = bus;

            _machine = new StateMachine<GameState>(GameState.Boot)
                .Configure(GameState.Boot, next: GameState.Menu)
                .Configure(GameState.Menu, onEnter: ResumeTime, next: GameState.Running)
                .Configure(GameState.Running, onEnter: ResumeTime, next: new[] { GameState.Paused, GameState.GameOver })
                .Configure(GameState.Paused, onEnter: StopTime, onExit: ResumeTime, next: new[] { GameState.Running, GameState.Menu })
                .Configure(GameState.GameOver, next: new[] { GameState.Running, GameState.Menu });

            _machine.Changed += (from, to) => _bus.Fire(new GameStateChangedSignal(from, to));

            _bus.Subscribe<StartRunRequestedSignal>(_ => _machine.TryGo(GameState.Running)).AddTo(_subscriptions);
            _bus.Subscribe<PauseRequestedSignal>(_ => _machine.TryGo(GameState.Paused)).AddTo(_subscriptions);
            _bus.Subscribe<ResumeRequestedSignal>(_ => Resume()).AddTo(_subscriptions);
            _bus.Subscribe<PauseToggleRequestedSignal>(_ => TogglePause()).AddTo(_subscriptions);
            _bus.Subscribe<ReturnToMenuRequestedSignal>(_ => _machine.TryGo(GameState.Menu)).AddTo(_subscriptions);
            _bus.Subscribe<PlayerCaughtSignal>(_ => _machine.TryGo(GameState.GameOver)).AddTo(_subscriptions);
            _bus.Subscribe<RunRevivedSignal>(_ => _machine.TryGo(GameState.Running)).AddTo(_subscriptions);
        }

        // Tüm sistemler kurulup arayüz bağlandıktan sonra menüye geçilir.
        public void PostStart()
        {
            _machine.TryGo(GameState.Menu);
        }

        void Resume()
        {
            if (_machine.Current == GameState.Paused) _machine.TryGo(GameState.Running);
        }

        void TogglePause()
        {
            if (_machine.Current == GameState.Running) _machine.TryGo(GameState.Paused);
            else Resume();
        }

        static void StopTime() { Time.timeScale = 0f; }
        static void ResumeTime() { Time.timeScale = 1f; }

        public void Dispose()
        {
            _subscriptions.Dispose();
            Time.timeScale = 1f;
        }
    }
}
