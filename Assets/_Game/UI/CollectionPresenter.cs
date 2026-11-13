using System;
using System.Collections.Generic;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Koleksiyon ızgarası. Kartlar, oyunun duyurduğu mesleklerden kendiliğinden oluşur;
    /// kataloğa yeni meslek eklenince burada da görünür.
    /// </summary>
    public sealed class CollectionPresenter : IStartable, IDisposable
    {
        sealed class Card
        {
            public Button Root;
            public VisualElement Avatar;
            public VisualElement Icon;
            public Label Name;
            public Label Sub;
            public VisualElement Price;
            public Label PriceValue;
        }

        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly Dictionary<string, Card> _cards = new Dictionary<string, Card>();
        IDisposable _subscription;
        VisualElement _grid;

        public CollectionPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            _grid = _document.rootVisualElement.Q("collectionGrid");
            _subscription = _bus.Subscribe<CharacterCardSignal>(OnCard);
        }

        void OnCard(CharacterCardSignal signal)
        {
            if (!_cards.TryGetValue(signal.Id, out var card))
            {
                card = Build(signal.Id);
                _cards.Add(signal.Id, card);
            }

            // Renk veriden gelir; bu yüzden satır içi stil kullanılır.
            card.Avatar.style.backgroundColor = signal.Color;
            card.Avatar.EnableInClassList("char-avatar--locked", signal.IsAutomated);
            card.Icon.EnableInClassList("icon-lock", signal.IsAutomated);
            card.Icon.EnableInClassList("icon-person", !signal.IsAutomated);

            card.Name.Localize(signal.NameKey);
            card.Sub.Localize(signal.IsAutomated ? "ui.automated" : signal.AbilityNameKey);
            card.Sub.EnableInClassList("char-sub--locked", signal.IsAutomated);

            card.Price.EnableInClassList(ScreenRouter.HiddenClass, !signal.IsAutomated);
            card.PriceValue.text = signal.BuybackCost.ToString();
            card.Root.EnableInClassList("char-card--selected", signal.IsSelected);
        }

        Card Build(string id)
        {
            var card = new Card { Root = new Button(() => _bus.Fire(new CharacterSelectRequestedSignal(id))) };
            card.Root.AddToClassList("char-card");

            card.Avatar = Child(card.Root, "char-avatar");
            card.Icon = Child(card.Avatar, "icon");
            card.Icon.AddToClassList("char-avatar-icon");

            card.Name = Text(card.Root, "char-name");
            card.Sub = Text(card.Root, "char-sub");

            card.Price = Child(card.Root, "char-price");
            var coin = Child(card.Price, "icon");
            coin.AddToClassList("icon-small");
            coin.AddToClassList("icon-coin");
            card.PriceValue = Text(card.Price, "char-price-value");

            _grid.Add(card.Root);
            return card;
        }

        static VisualElement Child(VisualElement parent, string className)
        {
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            element.AddToClassList(className);
            parent.Add(element);
            return element;
        }

        static Label Text(VisualElement parent, string className)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
