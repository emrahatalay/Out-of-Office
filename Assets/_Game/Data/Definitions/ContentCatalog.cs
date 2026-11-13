using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Oyundaki tüm içeriğin listesi. Yeni karakter ya da dünya buraya eklenince oyunda görünür.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Content Catalog", fileName = "ContentCatalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        public CharacterDefinition[] characters;
        public WorldDefinition[] worlds;
    }
}
