using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Bir bölümün içeriği: zemin, engeller, güçlendirmeler ve kovalayan.</summary>
    [CreateAssetMenu(menuName = "Out of Office/World", fileName = "World_")]
    public sealed class WorldDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;

        [Header("Zemin")]
        [Tooltip("Başlangıcı kendi orijininde olan, +Z yönünde uzanan parça. Kıvrımlı dünya köşe noktalarını büktüğü için mesh Z boyunca birkaç metrede bir bölünmüş olmalı; tek parça uzun yüzey düz kalır.")]
        public GameObject groundSegmentPrefab;
        [Min(1f)] public float segmentLength = 30f;

        [Header("İçerik")]
        public ChaserDefinition chaser;
        public ObstacleDefinition[] obstacles;
        public PowerUpDefinition[] powerUps;
        public GameObject coinPrefab;
    }
}
