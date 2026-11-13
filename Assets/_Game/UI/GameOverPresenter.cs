using System;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    public sealed class GameOverPresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        Label _scoreValue;
        Label _bestValue;
        Label _coinsValue;
        VisualElement _newBestBadge;
        VisualElement _automatedBadge;
        Label _automatedName;

        public GameOverPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var screen = _document.rootVisualElement.Q("gameOverScreen");

            _scoreValue = screen.Q<Label>("finalScoreValue");
            _bestValue = screen.Q<Label>("finalBestValue");
            _coinsValue = screen.Q<Label>("finalCoinsValue");
            _newBestBadge = screen.Q("newBestBadge");
            _automatedBadge = screen.Q("automatedBadge");
            _automatedName = screen.Q<Label>("automatedName");

            screen.Q<Button>("retryButton").clicked += () => _bus.Fire<StartRunRequestedSignal>();
            screen.Q<Button>("gameOverMenuButton").clicked += () => _bus.Fire<ReturnToMenuRequestedSignal>();

            _bus.Subscribe<RunEndedSignal>(OnRunEnded).AddTo(_subscriptions);
            _bus.Subscribe<ProfessionAutomatedSignal>(OnProfessionAutomated).AddTo(_subscriptions);
            // Rozet koşu başında gizlenir; böylece sinyallerin geliş sırası önemli olmaz.
            _bus.Subscribe<RunStartedSignal>(_ => _automatedBadge.AddToClassList(ScreenRouter.HiddenClass)).AddTo(_subscriptions);
        }

        void OnProfessionAutomated(ProfessionAutomatedSignal signal)
        {
            _automatedName.Localize(signal.NameKey);
            _automatedBadge.RemoveFromClassList(ScreenRouter.HiddenClass);
        }

        void OnRunEnded(RunEndedSignal signal)
        {
            _scoreValue.text = signal.Score.ToString();
            _bestValue.text = signal.BestScore.ToString();
            _coinsValue.text = signal.Coins.ToString();
            _newBestBadge.EnableInClassList(ScreenRouter.HiddenClass, !signal.IsNewBest);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
