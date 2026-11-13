using System;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Aksesuar envanteri: menüde altınla alınır, koşuda oyuncunun isteğiyle kullanılır.
    /// Aksesuarın ne yaptığı AccessoryDefinition'daki etkilerden gelir.
    /// </summary>
    public sealed class AccessorySystem : IPostStartable, IDisposable
    {
        const string CountKeyPrefix = "accessory.";

        readonly ContentCatalog _catalog;
        readonly PlayerProfile _profile;
        readonly EffectRunner _effects;
        readonly ISaveService _save;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        public AccessorySystem(ContentCatalog catalog, PlayerProfile profile, EffectRunner effects, ISaveService save, IGameStateProvider state, ISignalBus bus)
        {
            _catalog = catalog;
            _profile = profile;
            _effects = effects;
            _save = save;
            _state = state;
            _bus = bus;

            _bus.Subscribe<AccessoryUseRequestedSignal>(_ => Use()).AddTo(_subscriptions);
            _bus.Subscribe<AccessoryBuyRequestedSignal>(_ => Buy()).AddTo(_subscriptions);
        }

        AccessoryDefinition Current =>
            _catalog.accessories != null && _catalog.accessories.Length > 0 ? _catalog.accessories[0] : null;

        public void PostStart()
        {
            Announce();
        }

        void Use()
        {
            var accessory = Current;
            if (accessory == null || _state.Current != GameState.Running) return;

            int count = GetCount(accessory);
            if (count <= 0 || _effects.IsRunning(accessory.EffectId)) return;

            SetCount(accessory, count - 1);
            _effects.Run(accessory.EffectId, accessory.nameKey, accessory.effects, accessory.duration);
        }

        /// <summary>Aktif aksesuardan hediye eder (ör. sürpriz paketten).</summary>
        public void Grant(int amount)
        {
            var accessory = Current;
            if (accessory != null && amount > 0) SetCount(accessory, GetCount(accessory) + amount);
        }

        void Buy()
        {
            var accessory = Current;
            if (accessory == null || _state.Current != GameState.Menu || !_profile.TrySpend(accessory.price)) return;

            SetCount(accessory, GetCount(accessory) + 1);
        }

        int GetCount(AccessoryDefinition accessory)
        {
            return _save.GetInt(CountKeyPrefix + accessory.id);
        }

        void SetCount(AccessoryDefinition accessory, int count)
        {
            _save.SetInt(CountKeyPrefix + accessory.id, count);
            _save.Flush();
            Announce();
        }

        void Announce()
        {
            var accessory = Current;
            if (accessory != null) _bus.Fire(new AccessoryChangedSignal(accessory.nameKey, GetCount(accessory), accessory.price));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
