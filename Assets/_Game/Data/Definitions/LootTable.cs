using System;
using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data
{
    [Serializable]
    public struct LootEntry
    {
        public RewardKind kind;
        [Min(1)] public int min;
        [Min(1)] public int max;
        [Min(0f)] public float weight;
    }

    /// <summary>Sürpriz paketin ödül tablosu. Olasılıklar ve miktarlar buradan ayarlanır.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Loot Table", fileName = "LootTable")]
    public sealed class LootTable : ScriptableObject
    {
        public LootEntry[] entries;

        public bool Roll(out RewardKind kind, out int amount)
        {
            kind = RewardKind.Coins;
            amount = 0;
            if (entries == null || entries.Length == 0) return false;

            float total = 0f;
            for (int i = 0; i < entries.Length; i++) total += entries[i].weight;
            if (total <= 0f) return false;

            float roll = UnityEngine.Random.value * total;
            int picked = entries.Length - 1;
            for (int i = 0; i < entries.Length; i++)
            {
                roll -= entries[i].weight;
                if (roll > 0f) continue;
                picked = i;
                break;
            }

            kind = entries[picked].kind;
            amount = UnityEngine.Random.Range(entries[picked].min, Mathf.Max(entries[picked].min, entries[picked].max) + 1);
            return true;
        }
    }
}
