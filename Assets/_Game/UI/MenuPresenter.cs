using System;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    public sealed class MenuPresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly IArtProvider _art;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        Label _characterName;
        Label _characterAbility;
        Label _bestValue;
        Label _coinsValue;
        Label _buybackCost;
        Label _keysValue;
        VisualElement _portraitTile;
        VisualElement _portrait;
        VisualElement _abilityChip;
        VisualElement _automatedChip;
        Button _playButton;
        Button _buybackButton;
        int _coins;
        int _cost;
        bool _automated;

        public MenuPresenter(UIDocument document, ISignalBus bus, IArtProvider art)
        {
            _document = document;
            _bus = bus;
            _art = art;
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
            _portraitTile = screen.Q("characterPortraitTile");
            _portrait = screen.Q("characterPortrait");
            _abilityChip = screen.Q("abilityChip");
            _automatedChip = screen.Q("automatedChip");
            _playButton = screen.Q<Button>("playButton");
            _buybackButton = screen.Q<Button>("buybackButton");

            _playButton.clicked += () => _bus.Fire<StartRunRequestedSignal>();
            _buybackButton.clicked += () => _bus.Fire<BuybackRequestedSignal>();
            screen.Q<Button>("prevCharacterButton").clicked += () => _bus.Fire(new CharacterCycleRequestedSignal(-1));
            screen.Q<Button>("nextCharacterButton").clicked += () => _bus.Fire(new CharacterCycleRequestedSignal(1));

            _bus.Subscribe<CharacterSelectedSignal>(OnCharacterSelected).AddTo(_subscriptions);
            _bus.Subscribe<CharacterCardSignal>(OnCharacterCard).AddTo(_subscriptions);
            _bus.Subscribe<ProfileChangedSignal>(OnProfileChanged).AddTo(_subscriptions);
        }

        void OnCharacterSelected(CharacterSelectedSignal signal)
        {
            _automated = signal.IsAutomated;
            _cost = signal.BuybackCost;

            _characterName.Localize(signal.NameKey);
            _characterAbility.Localize(signal.AbilityNameKey);
            _buybackCost.text = _cost.ToString();
            _portrait.style.backgroundImage = new StyleBackground(_art.GetCharacterPortrait(signal.Id));
            _portrait.EnableInClassList("portrait-image--locked", _automated);

            bool hasAbility = !string.IsNullOrEmpty(signal.AbilityNameKey);
            _abilityChip.EnableInClassList(ScreenRouter.HiddenClass, _automated || !hasAbility);
            _automatedChip.EnableInClassList(ScreenRouter.HiddenClass, !_automated);
            _playButton.EnableInClassList(ScreenRouter.HiddenClass, _automated);
            _buybackButton.EnableInClassList(ScreenRouter.HiddenClass, !_automated);
            RefreshBuyback();
        }

        // Tema rengi yalnızca kart sinyalinde taşınır; seçili mesleğinki portrenin zemini olur.
        void OnCharacterCard(CharacterCardSignal signal)
        {
            if (signal.IsSelected) _portraitTile.style.backgroundColor = signal.Color;
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
