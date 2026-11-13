using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>
    /// Altınla satın alınıp koşu sırasında oyuncunun isteğiyle kullanılan aksesuar
    /// (ör. Ofis Sandalyesi). Yeni aksesuar = bu asset + etkiler.
    /// </summary>
    [CreateAssetMenu(menuName = "Out of Office/Accessory", fileName = "Accessory_")]
    public sealed class AccessoryDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        [Min(0)] public int price = 150;
        [Min(0f)] public float duration = 20f;
        public EffectDefinition[] effects;

        [System.NonSerialized] string _effectId;
        public string EffectId => _effectId ??= "accessory:" + id;
    }
}
