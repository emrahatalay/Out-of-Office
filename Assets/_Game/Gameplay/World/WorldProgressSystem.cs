using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Koşu boyunca oyuncunun hangi dünyada olduğunu izler. Sınırı geçince <c>RunSession.World</c>'ü günceller
    /// ve <see cref="WorldChangedSignal"/> atar (arayüz afiş gösterir). Dünyanın içeriğini (zemin, engel) kendisi
    /// değiştirmez; onlar <see cref="WorldTimeline"/>'dan mesafeye göre okur.
    /// </summary>
    public sealed class WorldProgressSystem : ITickable, IDisposable
    {
        readonly RunSession _session;
        readonly WorldTimeline _timeline;
        readonly GameConfig _config;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        int _index;

        public WorldProgressSystem(RunSession session, WorldTimeline timeline, GameConfig config, IGameStateProvider state, ISignalBus bus)
        {
            _session = session;
            _timeline = timeline;
            _config = config;
            _state = state;
            _bus = bus;

            _bus.Subscribe<RunStartedSignal>(_ => _index = 0).AddTo(_subscriptions);
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running || _timeline.Count < 2) return;

            int index = _timeline.IndexAt(_session.Distance);
            if (index == _index) return;

            _index = index;
            var world = _timeline.Get(index);
            _session.World = world;
            _bus.Fire(new WorldChangedSignal(world.id, world.nameKey, index, _config.worldBannerSeconds));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
