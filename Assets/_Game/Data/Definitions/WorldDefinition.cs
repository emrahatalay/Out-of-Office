using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>
    /// Bir dünyanın havası: gökyüzü, sis, ışık ve ortam rengi. Dünyalar arasında geçerken bu değerler
    /// yumuşakça karışır. Sis rengi ufuk rengiyle AYNI olmalıdır (uzak nesneler gökyüzüne karışsın).
    /// </summary>
    [System.Serializable]
    public struct AtmosphereSettings
    {
        public Color zenithColor;
        public Color horizonColor;
        public Color cloudColor;
        public Color cloudShadeColor;
        [Range(0f, 1f)] public float cloudCoverage;
        public Color sunColor;
        [Min(0f)] public float sunIntensity;
        public Color ambientSky;
        public Color ambientEquator;
        public Color ambientGround;
        [Min(0f)] public float fogStart;
        [Min(1f)] public float fogEnd;
    }

    /// <summary>Bir bölümün içeriği: zemin, engeller, güçlendirmeler ve kovalayan.</summary>
    [CreateAssetMenu(menuName = "Out of Office/World", fileName = "World_")]
    public sealed class WorldDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;

        [Header("Sıra")]
        [Tooltip("Bu dünya koşunun kaçıncı metresinde başlar. Katalogdaki ilk dünya 0'dır; liste bu değere göre artan sıralı olmalı.")]
        [Min(0f)] public float startDistance;

        [Header("Hava")]
        public AtmosphereSettings atmosphere;

        [Header("Zemin")]
        [Tooltip("Başlangıcı kendi orijininde olan, +Z yönünde uzanan parça. Kıvrımlı dünya köşe noktalarını büktüğü için mesh Z boyunca birkaç metrede bir bölünmüş olmalı; tek parça uzun yüzey düz kalır.")]
        public GameObject groundSegmentPrefab;
        [Min(1f), Tooltip("Tüm dünyalarda AYNI olmalı (zemin parçaları kesintisiz birleşsin).")] public float segmentLength = 30f;

        [Header("İçerik")]
        public ChaserDefinition chaser;
        public ObstacleDefinition[] obstacles;
        public PowerUpDefinition[] powerUps;
        public GameObject coinPrefab;
    }
}
