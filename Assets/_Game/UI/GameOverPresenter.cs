using System;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Oyun sonu kartı. Önce (anahtar varsa) devam teklifi, sonra sonuçlar:
    /// skor, otomatikleşen meslek ve sürpriz paketlerden çıkanlar.
    /// </summary>
    public sealed class GameOverPresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        Label _scoreValue;
        Label _bestValue;
        Label _coinsValue;
        Label _automatedName;
        Label _reviveCost;
        VisualElement _newBestBadge;
        VisualElement _automatedBadge;
        VisualElement _reviveGroup;
        VisualElement _resultGroup;
        VisualElement _rewardsBlock;
        VisualElement _rewardsRow;

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
            _automatedName = screen.Q<Label>("automatedName");
            _reviveCost = screen.Q<Label>("reviveCost");
            _newBestBadge = screen.Q("newBestBadge");
            _automatedBadge = screen.Q("automatedBadge");
            _reviveGroup = screen.Q("reviveGroup");
            _resultGroup = screen.Q("resultGroup");
            _rewardsBlock = screen.Q("rewardsBlock");
            _rewardsRow = screen.Q("rewardsRow");

            screen.Q<Button>("reviveButton").clicked += () => _bus.Fire<ReviveRequestedSignal>();
            screen.Q<Button>("giveUpButton").clicked += () => _bus.Fire<ReviveDeclinedSignal>();
            screen.Q<Button>("retryButton").clicked += () => _bus.Fire<StartRunRequestedSignal>();
            screen.Q<Button>("gameOverMenuButton").clicked += () => _bus.Fire<ReturnToMenuRequestedSignal>();

            _bus.Subscribe<ReviveOfferedSignal>(OnReviveOffered).AddTo(_subscriptions);
            _bus.Subscribe<RunEndedSignal>(OnRunEnded).AddTo(_subscriptions);
            _bus.Subscribe<ProfessionAutomatedSignal>(OnProfessionAutomated).AddTo(_subscriptions);
            _bus.Subscribe<PackageOpenedSignal>(OnPackageOpened).AddTo(_subscriptions);
            // Rozet ve ödüller koşu başında temizlenir; böylece sinyallerin geliş sırası önemli olmaz.
            _bus.Subscribe<RunStartedSignal>(_ => ResetCard()).AddTo(_subscriptions);
        }

        void ResetCard()
        {
            _automatedBadge.AddToClassList(ScreenRouter.HiddenClass);
            _rewardsBlock.AddToClassList(ScreenRouter.HiddenClass);
            _rewardsRow.Clear();
        }

        void OnReviveOffered(ReviveOfferedSignal signal)
        {
            _reviveCost.text = signal.KeyCost.ToString();
            ShowGroup(reviveOffer: true);
        }

        void OnRunEnded(RunEndedSignal signal)
        {
            _scoreValue.text = signal.Score.ToString();
            _bestValue.text = signal.BestScore.ToString();
            _coinsValue.text = signal.Coins.ToString();
            _newBestBadge.EnableInClassList(ScreenRouter.HiddenClass, !signal.IsNewBest);
            ShowGroup(reviveOffer: false);
        }

        void ShowGroup(bool reviveOffer)
        {
            _reviveGroup.EnableInClassList(ScreenRouter.HiddenClass, !reviveOffer);
            _resultGroup.EnableInClassList(ScreenRouter.HiddenClass, reviveOffer);
        }

        void OnProfessionAutomated(ProfessionAutomatedSignal signal)
        {
            _automatedName.Localize(signal.NameKey);
            _automatedBadge.RemoveFromClassList(ScreenRouter.HiddenClass);
        }

        // Her açılan paket için bir ödül çipi eklenir.
        void OnPackageOpened(PackageOpenedSignal signal)
        {
            var chip = new VisualElement();
            chip.AddToClassList("stat");
            chip.AddToClassList("reward");

            var icon = new VisualElement();
            icon.AddToClassList("icon");
            icon.AddToClassList(signal.Kind == RewardKind.Coins ? "icon-coin" : signal.Kind == RewardKind.Key ? "icon-key" : "icon-chair");
            chip.Add(icon);

            var amount = new Label((signal.Kind == RewardKind.Coins ? "+" : "x") + signal.Amount);
            amount.AddToClassList("stat-value");
            chip.Add(amount);

            _rewardsRow.Add(chip);
            _rewardsBlock.RemoveFromClassList(ScreenRouter.HiddenClass);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
