using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Kovalayan düşman. Yeni düşman = bu asset + bir görsel prefab.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Chaser", fileName = "Chaser_")]
    public sealed class ChaserDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        [Tooltip("Yalnızca görsel.")]
        public GameObject prefab;

        [Header("Mesafe (metre)")]
        [Tooltip("Normalde oyuncunun ne kadar gerisinde koşar.")]
        public float restGap = 11f;
        [Tooltip("Oyuncu tökezleyince ne kadar yaklaşır.")]
        public float closeGap = 2.4f;
        [Tooltip("Yakındayken oyuncunun ne kadar yanında durur; kameranın önünü kapatmasın diye.")]
        public float closeSideOffset = 1.3f;
        [Tooltip("Sersemlediğinde ne kadar geride kalır.")]
        public float stunnedGap = 22f;
        [Tooltip("Oyuncuyu yakalayınca ne kadar gerisinde durur. Yere düşen oyuncunun üstüne basmasın.")]
        public float catchGap = 1.9f;

        [Header("Davranış")]
        [Tooltip("Tökezlemeden sonra kaç saniye ensede kalır. Bu sürede ikinci çarpışma yakalanmaktır.")]
        public float threatSeconds = 5f;
        [Tooltip("Arayı kapatma hızı (m/sn).")]
        public float closeInSpeed = 14f;
        [Tooltip("Geri düşme hızı (m/sn).")]
        public float fallBackSpeed = 3f;
        [Tooltip("Oyuncunun şeridini ne kadar çabuk takip eder.")]
        public float sideFollowSpeed = 6f;
    }
}
