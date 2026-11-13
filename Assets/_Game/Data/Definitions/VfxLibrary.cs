using System;
using UnityEngine;

namespace OutOfOffice.Data
{
    public enum VfxEvent
    {
        CoinCollected,
        PowerUpCollected,
        Stumbled,
        ObstacleDestroyed,
        ShieldBroken,
        Landed,
        Caught
    }

    [Serializable]
    public struct VfxEntry
    {
        public VfxEvent trigger;
        [Tooltip("ParticleSystem içeren prefab.")]
        public GameObject prefab;
        [Tooltip("Havuza geri dönmeden önce kaç saniye yaşar.")]
        public float lifetime;
        public Vector3 offset;
    }

    /// <summary>
    /// Hangi olayda hangi efektin oynayacağı. Bir efekti değiştirmek için prefab'ı sürükle;
    /// kod değişmez.
    /// </summary>
    [CreateAssetMenu(menuName = "Out of Office/VFX Library", fileName = "VfxLibrary")]
    public sealed class VfxLibrary : ScriptableObject
    {
        public VfxEntry[] bursts;

        [Header("Sürekli efektler")]
        [Tooltip("Kameraya bağlanan hız çizgileri.")]
        public GameObject speedLinesPrefab;
        public float speedLinesMinSpeed = 17f;
        public float speedLinesMaxRate = 70f;
        [Tooltip("Süreli bir etki aktifken oyuncunun etrafında döner.")]
        public GameObject auraPrefab;

        public bool TryGet(VfxEvent trigger, out VfxEntry entry)
        {
            if (bursts != null)
            {
                for (int i = 0; i < bursts.Length; i++)
                {
                    if (bursts[i].trigger != trigger || bursts[i].prefab == null) continue;
                    entry = bursts[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
