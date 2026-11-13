using System;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Seçili mesleği tutar, kaydeder ve duyurur; geri satın almayı yönetir.</summary>
    public sealed class CharacterSelection : IPostStartable, IDisposable
    {
        const string SaveKey = "character";

        readonly ContentCatalog _catalog;
        readonly Workforce _workforce;
        readonly ISaveService _save;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        int _index;

        public CharacterDefinition Current => _catalog.characters[_index];

        public CharacterSelection(ContentCatalog catalog, Workforce workforce, ISaveService save, IGameStateProvider state, ISignalBus bus)
        {
            _catalog = catalog;
            _workforce = workforce;
            _save = save;
            _state = state;
            _bus = bus;

            string savedId = _save.GetString(SaveKey);
            _index = Math.Max(0, Array.FindIndex(_catalog.characters, c => c != null && c.id == savedId));

            _bus.Subscribe<CharacterCycleRequestedSignal>(OnCycle).AddTo(_subscriptions);
            _bus.Subscribe<BuybackRequestedSignal>(_ => OnBuyback()).AddTo(_subscriptions);
            _bus.Subscribe<ProfessionAutomatedSignal>(_ => Announce()).AddTo(_subscriptions);
        }

        public void PostStart()
        {
            Announce();
        }

        /// <summary>
        /// Koşu için oynanabilir mesleği döner. Seçili meslek otomatikleşmişse ilk uygun
        /// mesleğe (ör. Stajyer) geçer; böylece "tekrar dene" kilitli meslekle başlamaz.
        /// </summary>
        public CharacterDefinition ResolvePlayable()
        {
            if (!_workforce.IsAutomated(Current)) return Current;

            for (int i = 0; i < _catalog.characters.Length; i++)
            {
                if (_workforce.IsAutomated(_catalog.characters[i])) continue;
                Select(i);
                break;
            }

            return Current;
        }

        void OnCycle(CharacterCycleRequestedSignal signal)
        {
            if (_state.Current != GameState.Menu) return;

            int count = _catalog.characters.Length;
            Select(((_index + signal.Step) % count + count) % count);
        }

        void OnBuyback()
        {
            if (_state.Current == GameState.Menu && _workforce.TryBuyback(Current)) Announce();
        }

        void Select(int index)
        {
            _index = index;
            _save.SetString(SaveKey, Current.id);
            _save.Flush();
            Announce();
        }

        void Announce()
        {
            var character = Current;
            string abilityKey = character.ability != null ? character.ability.nameKey : string.Empty;
            _bus.Fire(new CharacterSelectedSignal(
                character.id,
                character.nameKey,
                abilityKey,
                _workforce.IsAutomated(character),
                _workforce.GetBuybackCost(character)));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
