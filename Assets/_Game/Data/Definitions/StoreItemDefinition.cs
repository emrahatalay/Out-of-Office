using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Mağazada altınla satılan ürün. Yeni ürün = bu asset; ContentCatalog'a eklenince mağazada görünür.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Store Item", fileName = "Store_")]
    public sealed class StoreItemDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public RewardKind kind = RewardKind.Accessory;
        [Min(1)] public int amount = 1;
        [Min(0)] public int price = 100;
    }
}
