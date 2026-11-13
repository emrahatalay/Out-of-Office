using System;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    public sealed class MenuPresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        Label _characterName;
        Label _characterAbility;
        Label _bestValue;
        Label _coinsValue;
        Label _buybackCost;
        Label _keysValue;
        VisualElement _abilityChip;
        VisualElement _automatedChip;
        Button _playButton;
        Button _buybackButton;
        int _coins;
        int _cost;
        bool _automated;

        public MenuPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var screen = _document.rootVisualElement.Q("menuScreen");

            _characterName = screen.Q<Label>("characterName");
            _characterAbility = screen.Q<Label>("characterAbility");
            _bestValue = screen.Q<Label>("menuBestValue");
            _coinsValue = screen.Q<Label>("menuCoinsValue");
            _buybackCost = screen.Q<Label>("buybackCost");
            _keysValue = screen.Q<Label>("menuKeysValue");
            _abilityChip = screen.Q("abilityChip");
            _automatedChip = screen.Q("automatedChip");
            _playButton = screen.Q<Button>("playButton");
            _buybackButton = screen.Q<Button>("buybackButton");

            _playButton.clicked += () => _bus.Fire<StartRunRequestedSignal>();
            _buybackButton.clicked += () => _bus.Fire<BuybackRequestedSignal>();
            screen.Q<Button>("languageButton").clicked += () => _bus.Fire<LanguageCycleRequestedSignal>();
            screen.Q<Button>("prevCharacterButton").clicked += () => _bus.Fire(new CharacterCycleRequestedSignal(-1));
            screen.Q<Button>("nextCharacterButton").clicked += () => _bus.Fire(new CharacterCycleRequestedSignal(1));

            _bus.Subscribe<CharacterSelectedSignal>(OnCharacterSelected).AddTo(_subscriptions);
            _bus.Subscribe<ProfileChangedSignal>(OnProfileChanged).AddTo(_subscriptions);
        }

        void OnCharacterSelected(CharacterSelectedSignal signal)
        {
            _automated = signal.IsAutomated;
            _cost = signal.BuybackCost;

            _characterName.Localize(signal.NameKey);
            _characterAbility.Localize(signal.AbilityNameKey);
            _buybackCost.text = _cost.ToString();

            bool hasAbility = !string.IsNullOrEmpty(signal.AbilityNameKey);
            _abilityChip.EnableInClassList(ScreenRouter.HiddenClass, _automated || !hasAbility);
            _automatedChip.EnableInClassList(ScreenRouter.HiddenClass, !_automated);
            _playButton.EnableInClassList(ScreenRouter.HiddenClass, _automated);
            _buybackButton.EnableInClassList(ScreenRouter.HiddenClass, !_automated);
            RefreshBuyback();
        }

        void OnProfileChanged(ProfileChangedSignal signal)
        {
            _coins = signal.TotalCoins;
            _bestValue.text = signal.BestScore.ToString();
            _coinsValue.text = signal.TotalCoins.ToString();
            _keysValue.text = signal.Keys.ToString();
            RefreshBuyback();
        }

        // Altın yetmiyorsa buton soluk kalır; oyuncu başka mesleğe geçebilir.
        void RefreshBuyback()
        {
            _buybackButton.SetEnabled(_coins >= _cost);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
