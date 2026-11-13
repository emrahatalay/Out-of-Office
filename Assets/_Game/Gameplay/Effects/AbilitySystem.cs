using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>Seçili mesleğin yeteneğini bekleme süresiyle birlikte çalıştırır.</summary>
    public sealed class AbilitySystem : IDisposable
    {
        readonly RunSession _session;
        readonly EffectRunner _effects;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        float _readyAt;

        public AbilitySystem(RunSession session, EffectRunner effects, IGameStateProvider state, ISignalBus bus)
        {
            _session = session;
            _effects = effects;
            _state = state;
            _bus = bus;

            _bus.Subscribe<AbilityRequestedSignal>(_ => TryUse()).AddTo(_subscriptions);
            _bus.Subscribe<RunStartedSignal>(_ => _readyAt = 0f).AddTo(_subscriptions);
        }

        void TryUse()
        {
            if (_state.Current != GameState.Running) return;

            var ability = _session.Character != null ? _session.Character.ability : null;
            if (ability == null || Time.time < _readyAt) return;

            _readyAt = Time.time + ability.cooldown;
            _effects.Run(ability.EffectId, ability.nameKey, ability.effects, ability.duration);
            _bus.Fire(new AbilityUsedSignal(ability.nameKey, ability.cooldown));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
