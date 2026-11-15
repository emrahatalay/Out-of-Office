using UnityEngine;

namespace OutOfOffice.Data
{
    public enum AvoidType
    {
        ChangeLane,
        Jump,
        Slide
    }

    /// <summary>Engelin dönen bir parçası (dron pervanesi, tekerlek). Parça prefab'da ayrı bir çocuk nesnedir.</summary>
    [System.Serializable]
    public struct SpinPart
    {
        [Tooltip("Çocuk nesnenin adının başı (ör. \"Rotor\" Rotor0..Rotor3'ü yakalar).")]
        public string childName;
        [Tooltip("Dönüş ekseni (parçanın kendi yerelinde).")]
        public Vector3 axis;
        [Tooltip("Derece/sn. Eksi değer ters yöne döner.")]
        public float degreesPerSecond;
    }

    /// <summary>Tek şeritlik engel. Yeni engel = bu asset + bir prefab.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Obstacle", fileName = "Obstacle_")]
    public sealed class ObstacleDefinition : ScriptableObject
    {
        public string id;
        [Tooltip("Collider'ı yoksa görsel sınırlarına göre otomatik eklenir.")]
        public GameObject prefab;
        [Tooltip("Oyuncu bundan nasıl kaçar. Üretici, geçilebilir sıra kurarken buna bakar.")]
        public AvoidType avoidBy = AvoidType.ChangeLane;
        [Tooltip("Boşsa engel yerinde durur.")]
        public MovementPattern movement;
        [Tooltip("Bu zorluktan önce çıkmaz (0: baştan itibaren).")]
        [Range(0f, 1f)] public float minDifficulty;
        [Tooltip("Robot mu? Fişi Çek güçlendirmesi yalnızca robotları durdurur.")]
        public bool isRobot;
        [Tooltip("Sürekli dönen parçalar (pervane, tekerlek). Boşsa engel tek parça hareket eder.")]
        public SpinPart[] spinParts;
        [Min(0f)] public float spawnWeight = 1f;
    }
}
