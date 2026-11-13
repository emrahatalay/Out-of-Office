using System;
using System.Collections.Generic;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Data;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Hangi mesleklerin yapay zekâya kaptırıldığını tutar. Oyuncu yakalanınca oynadığı meslek
    /// otomatikleşir; robotu sonraki koşularda engel olur. Altınla geri alınabilir.
    /// Geri alma bedeli, meslek her otomatikleştiğinde artar.
    /// Tüm kurallar CharacterDefinition'daki alanlardan gelir.
    /// </summary>
    public sealed class Workforce : IDisposable
    {
        const string SaveKey = "automated";
        const string CountKeyPrefix = "automated_count.";
        const int CostStep = 10;

        readonly ContentCatalog _catalog;
        readonly PlayerProfile _profile;
        readonly RunSession _session;
        readonly ISaveService _save;
        readonly ISignalBus _bus;
        readonly IDisposable _subscription;
        readonly HashSet<string> _automated = new HashSet<string>();

        public Workforce(ContentCatalog catalog, PlayerProfile profile, RunSession session, ISaveService save, ISignalBus bus)
        {
            _catalog = catalog;
            _profile = profile;
            _session = session;
            _save = save;
            _bus = bus;

            foreach (string id in _save.GetString(SaveKey).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                _automated.Add(id);

            _subscription = _bus.Subscribe<RunEndedSignal>(_ => AutomateCurrent());
        }

        public bool IsAutomated(CharacterDefinition character)
        {
            return character != null && character.canBeAutomated && _automated.Contains(character.id);
        }

        /// <summary>Otomatikleşen mesleklerin robot engellerini listeye ekler.</summary>
        public void CollectRobots(List<ObstacleDefinition> into)
        {
            var characters = _catalog.characters;
            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters[i];
                if (IsAutomated(character) && character.automatedObstacle != null) into.Add(character.automatedObstacle);
            }
        }

        /// <summary>
        /// Mesleği geri almanın şu anki bedeli: taban bedel x büyüme^(otomatikleşme sayısı - 1),
        /// 10'un katına yuvarlanır ve varsa üst sınırla kırpılır.
        /// </summary>
        public int GetBuybackCost(CharacterDefinition character)
        {
            if (character == null) return 0;

            int times = Mathf.Max(1, _save.GetInt(CountKeyPrefix + character.id));
            float raw = character.buybackCost * Mathf.Pow(Mathf.Max(1f, character.buybackGrowth), times - 1);
            int cost = Mathf.RoundToInt(raw / CostStep) * CostStep;
            return character.buybackMaxCost > 0 ? Mathf.Min(cost, character.buybackMaxCost) : cost;
        }

        public bool TryBuyback(CharacterDefinition character)
        {
            if (!IsAutomated(character) || !_profile.TrySpend(GetBuybackCost(character))) return false;

            _automated.Remove(character.id);
            Persist();
            return true;
        }

        void AutomateCurrent()
        {
            var character = _session.Character;
            if (character == null || !character.canBeAutomated || !_automated.Add(character.id)) return;

            string countKey = CountKeyPrefix + character.id;
            _save.SetInt(countKey, _save.GetInt(countKey) + 1);
            Persist();
            _bus.Fire(new ProfessionAutomatedSignal(character.id, character.nameKey));
        }

        void Persist()
        {
            _save.SetString(SaveKey, string.Join(",", _automated));
            _save.Flush();
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}
