using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Mağaza: ürünleri duyurur, altınla satın almayı yapar. Ürünler ContentCatalog.storeItems'tan gelir.</summary>
    public sealed class StoreSystem : IPostStartable, IDisposable
    {
        readonly ContentCatalog _catalog;
        readonly PlayerProfile _profile;
        readonly RewardSystem _rewards;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly IDisposable _subscription;

        public StoreSystem(ContentCatalog catalog, PlayerProfile profile, RewardSystem rewards, IGameStateProvider state, ISignalBus bus)
        {
            _catalog = catalog;
            _profile = profile;
            _rewards = rewards;
            _state = state;
            _bus = bus;
            _subscription = _bus.Subscribe<StorePurchaseRequestedSignal>(OnPurchaseRequested);
        }

        public void PostStart()
        {
            if (_catalog.storeItems == null) return;

            foreach (var item in _catalog.storeItems)
                if (item != null) _bus.Fire(new StoreItemListedSignal(item.id, item.nameKey, item.kind, item.amount, item.price));
        }

        void OnPurchaseRequested(StorePurchaseRequestedSignal signal)
        {
            if (_state.Current != GameState.Menu || _catalog.storeItems == null) return;

            foreach (var item in _catalog.storeItems)
            {
                if (item == null || item.id != signal.ItemId) continue;
                if (_profile.TrySpend(item.price)) _rewards.Grant(item.kind, item.amount);
                return;
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}
