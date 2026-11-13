using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Yoldan toplanan güçlendirme: süre + etkiler. Yeni güçlendirme = yeni asset.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Power Up", fileName = "PowerUp_")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public GameObject pickupPrefab;
        [Min(0f)] public float duration = 6f;
        public EffectDefinition[] effects;
        [Min(0f)] public float spawnWeight = 1f;

        [System.NonSerialized] string _effectId;
        public string EffectId => _effectId ??= "powerup:" + id;
    }
}
