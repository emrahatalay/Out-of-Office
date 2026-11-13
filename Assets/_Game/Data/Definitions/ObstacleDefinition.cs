using UnityEngine;

namespace OutOfOffice.Data
{
    public enum AvoidType
    {
        ChangeLane,
        Jump,
        Slide
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
        [Min(0f)] public float spawnWeight = 1f;
    }
}
