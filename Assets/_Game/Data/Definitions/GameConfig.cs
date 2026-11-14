using OutOfOffice.Core.Services;
using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>Oyunun tüm ayar sayıları. Kod değiştirmeden Inspector'dan dengelenir.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Şeritler")]
        [Min(1)] public int laneCount = 3;
        public float laneWidth = 2.5f;
        [Tooltip("Şerit değiştirme yumuşaklığı (sn). Küçük = daha keskin.")]
        public float laneSmoothTime = 0.055f;
        [Tooltip("Şerit değiştirirken gövdenin en fazla yatma açısı.")]
        public float maxLeanDegrees = 16f;

        [Header("Hız ve zorluk")]
        public float startSpeed = 13f;
        public float maxSpeed = 30f;
        [Tooltip("Saniyede kazanılan hız.")]
        public float acceleration = 0.16f;
        [Tooltip("Zorluğun 0'dan 1'e çıkması kaç saniye sürer.")]
        public float difficultyRampSeconds = 120f;

        [Header("Zıplama ve kayma")]
        [Tooltip("Zıplamanın tepesi (m, ayak yüksekliği). 3,2 m: taksi (2 m), meslek robotu (2,4 m) ve sunucu kabini (2,6 m) zamanında zıplanırsa aşılır.")]
        public float jumpHeight = 3.2f;
        [Tooltip("Zıplamadaki yerçekimi (m/sn²). Küçüldükçe havada kalma uzar: 40 ile tepe 3,2 m'de ~0,8 sn.")]
        public float gravity = 40f;
        public float fastFallSpeed = 34f;
        public float slideDuration = 0.65f;
        [Tooltip("Yere inmeden bu kadar önce basılan zıplama, inince uygulanır.")]
        public float inputBufferSeconds = 0.15f;
        public float standingHeight = 1.8f;
        public float slidingHeight = 0.8f;
        public float bodyRadius = 0.4f;

        [Header("Karakter animasyonu (yalnızca Animator'ı olan modeller)")]
        [Tooltip("Koşu klibinin 1x hızda oynadığı koşu hızı (m/sn). Oyuncu hızlandıkça adımlar da hızlanır.")]
        [Min(0.1f)] public float runAnimationReferenceSpeed = 13f;
        [Tooltip("Koşu klibinin en yavaş oynatma çarpanı.")]
        [Min(0.1f)] public float runAnimationMinRate = 1f;
        [Tooltip("Koşu klibinin en hızlı oynatma çarpanı (ayaklar bulanıklaşmasın diye sınırlı).")]
        [Min(0.1f)] public float runAnimationMaxRate = 1.6f;
        [Tooltip("Animator parametrelerinin yumuşatma süresi (sn).")]
        [Min(0f)] public float animationDampTime = 0.08f;
        [Tooltip("Sandalye ve taşıyıcı dronun ölçülü olduğu karakterin insan ölçeği (Animator.humanScale; Yazılımcı). Başka boydaki karakterde bu görseller aynı oranda küçülür ya da büyür: oturak kalçaya, tutamak ellere denk gelir.")]
        [Min(0.1f)] public float poseReferenceHumanScale = 0.984f;

        [Header("Üretim")]
        [Tooltip("Engeller oyuncunun bu kadar önünde doğar.")]
        public float spawnAhead = 70f;
        [Tooltip("Zemin bu kadar ileriye kadar döşenir.")]
        public float trackAhead = 130f;
        public float despawnBehind = 14f;
        public float firstRowDistance = 30f;
        [Tooltip("Kolay seviyede iki engel sırası arasındaki tepki süresi (sn).")]
        public float reactionTimeEasy = 1.25f;
        public float reactionTimeHard = 0.72f;
        [Tooltip("Bir sıradan sonra nefes payı bırakma olasılığı.")]
        [Range(0f, 1f)] public float breatherChance = 0.12f;
        public float breatherMultiplier = 1.8f;
        [Range(0f, 1f)] public float coinTrailChance = 0.7f;
        [Min(1)] public int coinsPerTrail = 5;
        public float coinSpacing = 1.6f;
        [Range(0f, 1f)] public float powerUpChance = 0.08f;

        [Header("Öğrenen yönetmen")]
        [Tooltip("0: engeller rastgele. 1: engeller tamamen oyuncunun sevdiği şeritlere gider.")]
        [Range(0f, 1f)] public float adaptiveStrength = 0.6f;
        [Tooltip("Alışkanlık hafızası. Daha eski şerit tercihleri bu süreyle unutulur.")]
        public float adaptiveMemorySeconds = 45f;

        [Header("Kamera: koşu")]
        public Vector3 cameraOffset = new Vector3(0f, 4.3f, -6.2f);
        public float cameraLookAhead = 9f;
        public float cameraLookHeight = 1.3f;
        [Range(0f, 1f)] public float cameraSideFollow = 0.6f;
        public float cameraSmoothTime = 0.08f;
        [Tooltip("Kamera zıplamanın ne kadarını takip eder.")]
        [Range(0f, 1f)] public float cameraJumpFollow = 0.3f;
        [Tooltip("Uçarken kamera yüksekliğin ne kadarını takip eder; oyuncu kadrajda kalsın diye yüksektir.")]
        [Range(0f, 1f)] public float cameraFlightFollow = 0.8f;
        public float baseFieldOfView = 60f;
        [Tooltip("En yüksek hızdaki görüş açısı; hız hissini artırır.")]
        public float maxFieldOfView = 74f;

        [Header("Kamera: menü")]
        public Vector3 menuCameraOffset = new Vector3(4.25f, 1.9f, 3.5f);
        public Vector3 menuLookAt = new Vector3(0f, 0.45f, 0.2f);
        [Tooltip("Menü ile koşu kamerası arasındaki geçiş süresi (sn).")]
        public float cameraBlendSeconds = 0.7f;

        [Header("Kıvrımlı dünya (yalnızca görsel; oynanış düz kalır)")]
        public bool curveEnabled = true;
        [Tooltip("Kıvrımın ölçüldüğü uzaklık (m, kameradan). Aşağıdaki iki değer bu uzaklıktaki kaymadır.")]
        [Min(10f)] public float curveDistance = 80f;
        [Tooltip("Yol en çok bu kadar (m) sağa ya da sola kıvrılır.")]
        public float curveSideMeters = 9f;
        [Tooltip("Yol uzakta bu kadar (m) aşağı eğilir; ufuk hissi verir.")]
        public float curveDownMeters = 3f;
        [Tooltip("Kameradan bu kadar ileriye (m) kadar yol düz kalır; oyuncunun çevresi bükülmez.")]
        [Min(0f)] public float curveFlatDistance = 7f;
        [Tooltip("Bir kıvrımın sürdüğü yol (m): en az / en çok.")]
        public Vector2 curveSegmentMeters = new Vector2(110f, 240f);
        [Tooltip("Koşunun başında yol bu kadar (m) düz gider.")]
        public float curveFirstStraightMeters = 45f;
        [Tooltip("Bir kıvrımdan diğerine geçiş yumuşaklığı (sn).")]
        public float curveBlendSeconds = 1.4f;
        [Tooltip("Kıvrımdan sonra yolun düzleşme olasılığı. Kalanı ters yöne kıvrılır.")]
        [Range(0f, 1f)] public float curveStraightChance = 0.3f;
        [Tooltip("Her kıvrım, en büyük değerin en az bu oranı kadar güçlü olur.")]
        [Range(0f, 1f)] public float curveMinStrength = 0.55f;

        [Header("Sarsıntı ve ağır çekim")]
        public float shakeSeconds = 0.35f;
        public float stumbleShake = 0.35f;
        public float shieldShake = 0.25f;
        public float caughtShake = 0.6f;
        [Range(0.05f, 1f)] public float stumbleTimeScale = 0.4f;
        public float stumbleSlowSeconds = 0.12f;
        [Range(0.05f, 1f)] public float caughtTimeScale = 0.25f;
        public float caughtSlowSeconds = 0.7f;

        [Header("Havuz ön ısıtma (yükleme anında üretilir, koşu sırasında takılma olmaz)")]
        [Min(0)] public int prewarmGround = 6;
        [Min(0)] public int prewarmPerObstacle = 4;
        [Min(0)] public int prewarmCoins = 40;
        [Min(0)] public int prewarmPerVfx = 3;

        [Header("Anahtarla devam")]
        [Tooltip("İlk devam kaç anahtar. Aynı koşuda her devamda ikiye katlanır.")]
        [Min(1)] public int reviveBaseKeys = 1;
        [Tooltip("Devam teklifinin ekranda kalma süresi (sn).")]
        public float reviveOfferSeconds = 6f;
        [Tooltip("Devam edince önündeki bu kadar metre engelden temizlenir.")]
        public float reviveClearDistance = 45f;

        [Header("Puan")]
        public int coinScore = 10;

        [Header("Ayarlar: görüntü kalitesi")]
        [Tooltip("İlk açılışta seçili kalite. Oyuncu Ayarlar sekmesinden değiştirir; seçimi kayıtta durur.")]
        public GraphicsQuality defaultQuality = GraphicsQuality.Medium;
        [Tooltip("Düşük kalitede render ölçeği (1 = ekranın tam çözünürlüğü). Sahne bu oranda çizilip büyütülür.")]
        [Range(0.4f, 1f)] public float renderScaleLow = 0.65f;
        [Tooltip("Orta kalitede render ölçeği.")]
        [Range(0.4f, 1f)] public float renderScaleMedium = 0.8f;
        [Tooltip("Yüksek kalitede render ölçeği.")]
        [Range(0.4f, 1f)] public float renderScaleHigh = 1f;
        [Tooltip("Düşük kalitede post-processing (renk ayarı, bloom) açık mı. Kapalıyken eski cihazlarda belirgin kazanç sağlar.")]
        public bool postProcessingOnLow = false;

        [Header("Ayarlar: geliştirici")]
        [Tooltip("Ayarlar sekmesindeki 'GitHub' butonunun açtığı adres.")]
        public string developerUrl = "https://github.com/emrahatalay";

        public float LaneToX(int lane)
        {
            return (lane - (laneCount - 1) * 0.5f) * laneWidth;
        }
    }
}
