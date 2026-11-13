using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>Oyun durumuna göre hangi ekranın görüneceğini seçer; geçişleri USS animasyonu yapar.</summary>
    public sealed class ScreenRouter : IStartable, IDisposable
    {
        public const string HiddenClass = "hidden";
        const string OutClass = "screen--out";
        const int FadeMilliseconds = 240;

        readonly UIDocument _document;
        readonly ISignalBus _bus;
        IDisposable _subscription;
        VisualElement _menu;
        VisualElement _hud;
        VisualElement _pause;
        VisualElement _gameOver;

        public ScreenRouter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var root = _document.rootVisualElement;
            UiText.LocalizeTree(root);
            SafeArea.Apply(root);

            _menu = root.Q("menuScreen");
            _hud = root.Q("hudScreen");
            _pause = root.Q("pauseScreen");
            _gameOver = root.Q("gameOverScreen");

            _subscription = _bus.Subscribe<GameStateChangedSignal>(OnStateChanged);
        }

        void OnStateChanged(GameStateChangedSignal signal)
        {
            var state = signal.To;
            Show(_menu, state == GameState.Menu);
            Show(_hud, state == GameState.Running || state == GameState.Paused);
            Show(_pause, state == GameState.Paused);
            Show(_gameOver, state == GameState.GameOver);
        }

        static void Show(VisualElement screen, bool visible)
        {
            if (screen == null) return;

            if (visible)
            {
                // Önce görünür yap, bir sonraki karede sınıfı kaldır ki geçiş oynasın.
                screen.RemoveFromClassList(HiddenClass);
                screen.schedule.Execute(() => screen.RemoveFromClassList(OutClass)).StartingIn(20);
            }
            else if (!screen.ClassListContains(OutClass))
            {
                screen.AddToClassList(OutClass);
                screen.schedule.Execute(() =>
                {
                    if (screen.ClassListContains(OutClass)) screen.AddToClassList(HiddenClass);
                }).StartingIn(FadeMilliseconds);
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
