using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Bir mesleğin yeteneği: bekleme süresi + etkiler. Yeni yetenek = yeni asset.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Ability", fileName = "Ability_")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        public string id;
        [Tooltip("Game tablosundaki çeviri anahtarı.")]
        public string nameKey;
        [Min(0f)] public float cooldown = 20f;
        [Tooltip("0 ise etkiler anlıktır.")]
        [Min(0f)] public float duration;
        public EffectDefinition[] effects;

        [System.NonSerialized] string _effectId;
        public string EffectId => _effectId ??= "ability:" + id;
    }
}
