using System;
using UnityEngine;

namespace OutOfOffice.Data
{
    public enum SfxEvent
    {
        Jumped,
        Slid,
        LaneChanged,
        CoinCollected,
        PowerUpCollected,
        Stumbled,
        ShieldBroken,
        Caught,
        UiClicked
    }

    [Serializable]
    public struct SfxEntry
    {
        public SfxEvent trigger;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Tooltip("Her çalışta perde bu kadar yukarı ya da aşağı rastgele oynar (0 = hep aynı). Sık tekrar eden sesler tekdüze olmasın diye.")]
        [Range(0f, 0.3f)] public float pitchVariation;
    }

    /// <summary>
    /// Hangi olayda hangi sesin çalacağı. Bir sesi değiştirmek için klibi sürükle; kod değişmez.
    /// </summary>
    [CreateAssetMenu(menuName = "Out of Office/SFX Library", fileName = "SfxLibrary")]
    public sealed class SfxLibrary : ScriptableObject
    {
        public SfxEntry[] clips;

        [Tooltip("Bütün efektlerin ortak ses düzeyi.")]
        [Range(0f, 1f)] public float masterVolume = 0.8f;
        [Tooltip("Aynı anda çalabilen ses sayısı. Dolunca en eski ses kesilir.")]
        [Min(1)] public int voices = 6;

        [Header("Altın serisi")]
        [Tooltip("Art arda altın toplanırken iki altın arasında en fazla bu kadar saniye geçerse seri sürer.")]
        [Min(0f)] public float coinStreakWindow = 0.6f;
        [Tooltip("Serideki her altında perde bu kadar yükselir.")]
        [Min(0f)] public float coinStreakPitchStep = 0.04f;
        [Tooltip("Serinin çıkabileceği en yüksek perde çarpanı.")]
        [Min(1f)] public float coinStreakMaxPitch = 1.5f;

        public bool TryGet(SfxEvent trigger, out SfxEntry entry)
        {
            if (clips != null)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i].trigger != trigger || clips[i].clip == null) continue;
                    entry = clips[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
