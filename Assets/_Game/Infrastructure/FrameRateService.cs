using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Infrastructure
{
    /// <summary>
    /// Mobilde Unity varsayılan olarak 30 FPS'e kilitler; koşu oyunu için bu yetersizdir.
    /// Ekranın yenileme hızını (en az 60, en çok 120) hedefler.
    /// </summary>
    public sealed class FrameRateService : IInitializable
    {
        const int MinFrameRate = 60;
        const int MaxFrameRate = 120;

        public void Initialize()
        {
            int refresh = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
            Application.targetFrameRate = Mathf.Clamp(refresh, MinFrameRate, MaxFrameRate);

            // Mobilde kare hızını targetFrameRate belirler; vSync kapalı olmalı.
            if (Application.isMobilePlatform) QualitySettings.vSyncCount = 0;
        }
    }
}
