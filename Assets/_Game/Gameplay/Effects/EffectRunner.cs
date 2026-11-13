using System;
using System.Collections.Generic;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Etkileri uygular ve süresi dolunca geri alır. Güçlendirme de yetenek de buradan geçer;
    /// hangi etkinin ne yaptığını bilmez.
    /// </summary>
    public sealed class EffectRunner : ITickable, IDisposable
    {
        sealed class Entry
        {
            public string Id;
            public EffectDefinition[] Effects;
            public float Remaining;
        }

        readonly EffectContext _context;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly List<Entry> _active = new List<Entry>();

        public EffectRunner(EffectContext context, IGameStateProvider state, ISignalBus bus)
        {
            _context = context;
            _state = state;
            _bus = bus;

            _bus.Subscribe<RunStartedSignal>(_ => StopAll()).AddTo(_subscriptions);
            _bus.Subscribe<RunClearedSignal>(_ => StopAll()).AddTo(_subscriptions);
            _bus.Subscribe<RunEndedSignal>(_ => StopAll()).AddTo(_subscriptions);
            _bus.Subscribe<ShieldBrokenSignal>(_ => EndShieldBound()).AddTo(_subscriptions);
        }

        public bool IsRunning(string id)
        {
            for (int i = 0; i < _active.Count; i++)
                if (_active[i].Id == id) return true;
            return false;
        }

        // Kalkan kırılınca ona bağlı süreli etkiler (ör. sandalye) hemen biter.
        void EndShieldBound()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var entry = _active[i];
                bool bound = false;
                foreach (var effect in entry.Effects)
                    if (effect != null && effect.EndsWhenShieldBreaks) bound = true;
                if (!bound) continue;

                _active.RemoveAt(i);
                End(entry);
            }
        }

        /// <param name="duration">0 ise etkiler anlıktır ve geri alınmaz.</param>
        public void Run(string id, string nameKey, EffectDefinition[] effects, float duration)
        {
            if (effects == null || effects.Length == 0) return;

            // Aynı etki zaten sürüyorsa yalnızca süresi yenilenir.
            Entry running = null;
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].Id != id) continue;
                running = _active[i];
                break;
            }

            if (running != null)
            {
                running.Remaining = duration;
                _bus.Fire(new TimedEffectStartedSignal(id, nameKey, duration));
                return;
            }

            foreach (var effect in effects)
                if (effect != null) effect.Apply(_context);

            if (duration <= 0f) return;

            _active.Add(new Entry { Id = id, Effects = effects, Remaining = duration });
            _bus.Fire(new TimedEffectStartedSignal(id, nameKey, duration));
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running) return;

            float dt = Time.deltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var entry = _active[i];
                entry.Remaining -= dt;
                if (entry.Remaining > 0f) continue;

                _active.RemoveAt(i);
                End(entry);
            }
        }

        void StopAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) End(_active[i]);
            _active.Clear();
        }

        void End(Entry entry)
        {
            foreach (var effect in entry.Effects)
                if (effect != null) effect.Remove(_context);

            _bus.Fire(new TimedEffectEndedSignal(entry.Id));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
