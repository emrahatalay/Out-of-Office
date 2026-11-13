using System;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using OutOfOffice.Data;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Koşuda toplanan sürpriz paketleri sayar, koşu sonuçlanınca açar ve ödülü verir.
    /// Ne çıkacağı ContentCatalog.packageLoot tablosundan gelir.
    /// </summary>
    public sealed class RewardSystem : IDisposable
    {
        readonly ContentCatalog _catalog;
        readonly RunSession _session;
        readonly PlayerProfile _profile;
        readonly AccessorySystem _accessories;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        public RewardSystem(ContentCatalog catalog, RunSession session, PlayerProfile profile, AccessorySystem accessories, ISignalBus bus)
        {
            _catalog = catalog;
            _session = session;
            _profile = profile;
            _accessories = accessories;
            _bus = bus;

            _bus.Subscribe<PackageCollectedSignal>(_ => _session.Packages++).AddTo(_subscriptions);
            _bus.Subscribe<RunEndedSignal>(_ => OpenPackages()).AddTo(_subscriptions);
        }

        void OpenPackages()
        {
            var loot = _catalog.packageLoot;
            int count = _session.Packages;
            _session.Packages = 0;
            if (loot == null) return;

            for (int i = 0; i < count; i++)
            {
                if (!loot.Roll(out var kind, out int amount)) continue;

                Grant(kind, amount);
                _bus.Fire(new PackageOpenedSignal(kind, amount));
            }
        }

        /// <summary>Ödülü oyuncuya verir. Paketler ve mağaza aynı yolu kullanır.</summary>
        public void Grant(RewardKind kind, int amount)
        {
            switch (kind)
            {
                case RewardKind.Coins: _profile.AddCoins(amount); break;
                case RewardKind.Key: _profile.AddKeys(amount); break;
                case RewardKind.Accessory: _accessories.Grant(amount); break;
            }
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
