using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>
    /// Günlük görev havuzundaki bir görev. Yeni görev = bu asset; ContentCatalog.missions'a eklenince
    /// günlük seçime girer. Kod gerekmez (yeni bir şey SAYMAK gerekiyorsa MissionMetric'e değer eklenir).
    /// </summary>
    [CreateAssetMenu(menuName = "Out of Office/Mission", fileName = "Mission_")]
    public sealed class MissionDefinition : ScriptableObject
    {
        public string id;
        [Tooltip("Görev metninin çeviri anahtarı (hedef sayı ayrıca gösterilir; ör. 'Altın topla' + 50).")]
        public string nameKey;
        [Tooltip("Neyin sayılacağı. Aynı türden iki görev aynı gün verilmez.")]
        public MissionMetric metric = MissionMetric.Coins;
        [Min(1)] public int target = 50;
        public RewardKind rewardKind = RewardKind.Coins;
        [Min(1)] public int rewardAmount = 50;
    }
}
