using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Oyundaki tüm içeriğin listesi. Yeni karakter ya da dünya buraya eklenince oyunda görünür.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Content Catalog", fileName = "ContentCatalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        public CharacterDefinition[] characters;
        public WorldDefinition[] worlds;
        [Tooltip("Altınla alınıp koşuda kullanılan aksesuarlar. İlk sıradaki aktif aksesuardır.")]
        public AccessoryDefinition[] accessories;
        [Tooltip("Sürpriz paketlerden ne çıkacağı.")]
        public LootTable packageLoot;
        [Tooltip("Mağazada satılan ürünler, görünme sırasıyla.")]
        public StoreItemDefinition[] storeItems;
    }
}
