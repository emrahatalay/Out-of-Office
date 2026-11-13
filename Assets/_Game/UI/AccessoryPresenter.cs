using System;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>Aksesuarın menüdeki satın alma satırı ve koşudaki kullanma butonu.</summary>
    public sealed class AccessoryPresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        Label _name;
        Label _count;
        Label _price;
        Label _hudCount;
        Button _buyButton;
        Button _useButton;
        int _coins;
        int _cost;

        public AccessoryPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var root = _document.rootVisualElement;
            _name = root.Q<Label>("accessoryName");
            _count = root.Q<Label>("accessoryCount");
            _price = root.Q<Label>("accessoryPrice");
            _hudCount = root.Q<Label>("hudAccessoryCount");
            _buyButton = root.Q<Button>("accessoryBuyButton");
            _useButton = root.Q<Button>("accessoryButton");

            _buyButton.clicked += () => _bus.Fire<AccessoryBuyRequestedSignal>();
            _useButton.clicked += () => _bus.Fire<AccessoryUseRequestedSignal>();

            _bus.Subscribe<AccessoryChangedSignal>(OnAccessoryChanged).AddTo(_subscriptions);
            _bus.Subscribe<ProfileChangedSignal>(OnProfileChanged).AddTo(_subscriptions);
        }

        void OnAccessoryChanged(AccessoryChangedSignal signal)
        {
            _cost = signal.Price;
            _name.Localize(signal.NameKey);
            _count.text = signal.Count.ToString();
            _price.text = signal.Price.ToString();
            _hudCount.text = signal.Count.ToString();
            _useButton.EnableInClassList(ScreenRouter.HiddenClass, signal.Count <= 0);
            _buyButton.SetEnabled(_coins >= _cost);
        }

        void OnProfileChanged(ProfileChangedSignal signal)
        {
            _coins = signal.TotalCoins;
            _buyButton.SetEnabled(_coins >= _cost);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
