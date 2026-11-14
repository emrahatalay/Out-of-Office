using System;
using System.Collections.Generic;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Koleksiyon ızgarası. Kartlar, oyunun duyurduğu mesleklerden kendiliğinden oluşur;
    /// kataloğa yeni meslek eklenince burada da görünür. Her kartta mesleğin gerçek çizimi
    /// (portresi) durur; otomatikleşen mesleğin portresi kararır ve üstüne kilit gelir.
    /// </summary>
    public sealed class CollectionPresenter : IStartable, IDisposable
    {
        sealed class Card
        {
            public Button Root;
            public VisualElement Tile;
            public VisualElement Portrait;
            public VisualElement Lock;
            public Label Name;
            public Label Sub;
            public VisualElement Price;
            public Label PriceValue;
        }

        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly IArtProvider _art;
        readonly Dictionary<string, Card> _cards = new Dictionary<string, Card>();
        IDisposable _subscription;
        VisualElement _grid;

        public CollectionPresenter(UIDocument document, ISignalBus bus, IArtProvider art)
        {
            _document = document;
            _bus = bus;
            _art = art;
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
            card.Tile.style.backgroundColor = signal.Color;
            card.Portrait.EnableInClassList("portrait-image--locked", signal.IsAutomated);
            card.Lock.EnableInClassList(ScreenRouter.HiddenClass, !signal.IsAutomated);

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

            card.Tile = Child(card.Root, "portrait-tile");
            card.Tile.AddToClassList("char-portrait-tile");
            Child(card.Tile, "portrait-spot");
            card.Portrait = Child(card.Tile, "portrait-image");
            // Görsel veriden gelir (satır içi); meslek değişmediği için bir kez bağlanır.
            card.Portrait.style.backgroundImage = new StyleBackground(_art.GetCharacterPortrait(id));
            card.Lock = Child(card.Tile, "char-lock");
            var lockIcon = Child(Child(card.Lock, "char-lock-badge"), "icon");
            lockIcon.AddToClassList("icon-lock");

            card.Name = Text(card.Root, "char-name");
            card.Sub = Text(card.Root, "char-sub");

            card.Price = Child(card.Root, "char-price");
            var coin = Child(card.Price, "icon");
            coin.AddToClassList("icon-small");
            coin.AddToClassList("icon-coin");
            card.PriceValue = Text(card.Price, "char-price-value");

            // Seçim göstergeleri; yalnızca "char-card--selected" sınıfıyla görünür.
            Child(card.Root, "char-frame");
            Text(Child(card.Root, "char-check"), "char-check-label").Localize("ui.selected");

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
