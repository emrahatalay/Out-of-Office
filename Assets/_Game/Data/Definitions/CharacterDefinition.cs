using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Oynanabilir meslek. Yeni karakter = bu asset + bir görsel prefab.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Character", fileName = "Character_")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string id;
        [Tooltip("Game tablosundaki çeviri anahtarı.")]
        public string nameKey;
        [Tooltip("Koleksiyon kartındaki rengi.")]
        public Color themeColor = new Color(0.3f, 0.55f, 1f);
        [Tooltip("Yalnızca görsel. Çarpışma ve hareket kodu prefab'a bağlı değildir.")]
        public GameObject prefab;
        [Tooltip("Boş bırakılabilir (ör. Stajyer).")]
        public AbilityDefinition ability;

        [Header("Otomasyon")]
        [Tooltip("Kapalıysa bu meslek hiç otomatikleşmez; her zaman oynanabilir (ör. Stajyer).")]
        public bool canBeAutomated = true;
        [Tooltip("Mesleği ilk kez geri almanın altın bedeli.")]
        [Min(0)] public int buybackCost = 100;
        [Tooltip("Meslek her otomatikleştiğinde bedel bu katsayıyla çarpılır (1 = sabit bedel).")]
        [Min(1f)] public float buybackGrowth = 1.5f;
        [Tooltip("Bedelin çıkabileceği en yüksek değer. 0 = sınır yok.")]
        [Min(0)] public int buybackMaxCost = 0;
        [Tooltip("Meslek otomatikleşince koşularda engel olarak çıkan robotu.")]
        public ObstacleDefinition automatedObstacle;
    }
}
