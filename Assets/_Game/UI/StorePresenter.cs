using System;
using System.Collections.Generic;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Mağaza sekmesi. Satırlar, oyunun duyurduğu ürünlerden kendiliğinden oluşur;
    /// yeni ürün eklemek için arayüze dokunmak gerekmez.
    /// </summary>
    public sealed class StorePresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly List<(Button button, int price)> _buyButtons = new List<(Button, int)>();
        VisualElement _list;
        int _coins;

        public StorePresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            _list = _document.rootVisualElement.Q("storeList");

            _bus.Subscribe<StoreItemListedSignal>(OnItemListed).AddTo(_subscriptions);
            _bus.Subscribe<ProfileChangedSignal>(OnProfileChanged).AddTo(_subscriptions);
        }

        void OnItemListed(StoreItemListedSignal item)
        {
            var row = new VisualElement();
            row.AddToClassList("store-row");

            var icon = new VisualElement();
            icon.AddToClassList("icon");
            icon.AddToClassList(item.Kind == RewardKind.Key ? "icon-key" : item.Kind == RewardKind.Coins ? "icon-coin" : "icon-chair");
            row.Add(icon);

            var name = new Label();
            name.AddToClassList("store-name");
            name.Localize(item.NameKey);
            row.Add(name);

            var amount = new Label("x" + item.Amount);
            amount.AddToClassList("store-amount");
            row.Add(amount);

            string id = item.Id;
            var buy = new Button(() => _bus.Fire(new StorePurchaseRequestedSignal(id)));
            buy.AddToClassList("chip");
            buy.AddToClassList("chip-button");

            var coin = new VisualElement { pickingMode = PickingMode.Ignore };
            coin.AddToClassList("icon");
            coin.AddToClassList("icon-coin");
            buy.Add(coin);

            var price = new Label(item.Price.ToString()) { pickingMode = PickingMode.Ignore };
            price.AddToClassList("chip-value");
            buy.Add(price);

            row.Add(buy);
            _list.Add(row);
            _buyButtons.Add((buy, item.Price));
            buy.SetEnabled(_coins >= item.Price);
        }

        void OnProfileChanged(ProfileChangedSignal signal)
        {
            _coins = signal.TotalCoins;
            foreach (var (button, price) in _buyButtons) button.SetEnabled(_coins >= price);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
