using OutOfOffice.Core.Run;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.State;

namespace OutOfOffice.Core.Signals
{
    // Kural: sinyaller yalnızca id ve değer taşır, nesne referansı taşımaz.
    //
    // İSTEK sinyalleri ("...Requested"): biri bir şey olmasını istiyor (buton, klavye, dokunma).
    // OLAY sinyalleri (geçmiş zaman): bir şey oldu, ilgilenen dinler.

    // ---------------------------------------------------------------- İstekler

    public readonly struct StartRunRequestedSignal { }
    public readonly struct PauseRequestedSignal { }
    public readonly struct PauseToggleRequestedSignal { }
    public readonly struct ResumeRequestedSignal { }
    public readonly struct ReturnToMenuRequestedSignal { }
    public readonly struct AbilityRequestedSignal { }

    /// <summary>Oyuncu menünün alt çubuğundaki bir sekmeye dokundu (ses ve geri bildirim için).</summary>
    public readonly struct MenuTabClickedSignal { }

    // ---- Ayarlar sekmesi

    /// <summary>Dili seç. Kod, Localization'daki dilin kodudur ("tr", "en").</summary>
    public readonly struct LanguageSelectRequestedSignal
    {
        public readonly string Code;
        public LanguageSelectRequestedSignal(string code) { Code = code; }
    }

    public readonly struct SoundSetRequestedSignal
    {
        public readonly bool On;
        public SoundSetRequestedSignal(bool on) { On = on; }
    }

    public readonly struct QualitySetRequestedSignal
    {
        public readonly GraphicsQuality Quality;
        public QualitySetRequestedSignal(GraphicsQuality quality) { Quality = quality; }
    }

    /// <summary>Geliştiricinin sayfasını tarayıcıda aç.</summary>
    public readonly struct DeveloperPageRequestedSignal { }

    /// <summary>Sahip olunan aksesuarı kullan (çift dokunuş, HUD butonu ya da E tuşu).</summary>
    public readonly struct AccessoryUseRequestedSignal { }
    public readonly struct AccessoryBuyRequestedSignal { }

    /// <summary>Koleksiyondan bir mesleği seç.</summary>
    public readonly struct CharacterSelectRequestedSignal
    {
        public readonly string Id;
        public CharacterSelectRequestedSignal(string id) { Id = id; }
    }

    public readonly struct StorePurchaseRequestedSignal
    {
        public readonly string ItemId;
        public StorePurchaseRequestedSignal(string itemId) { ItemId = itemId; }
    }

    /// <summary>Anahtar harcayıp yakalandığın yerden devam et / teklifi geri çevir.</summary>
    public readonly struct ReviveRequestedSignal { }
    public readonly struct ReviveDeclinedSignal { }

    /// <summary>Seçili (otomatikleşmiş) mesleği altınla geri al.</summary>
    public readonly struct BuybackRequestedSignal { }

    public readonly struct CharacterCycleRequestedSignal
    {
        public readonly int Step;
        public CharacterCycleRequestedSignal(int step) { Step = step; }
    }

    public readonly struct MoveInputSignal
    {
        public readonly MoveDirection Direction;
        public MoveInputSignal(MoveDirection direction) { Direction = direction; }
    }

    // ---------------------------------------------------------------- Oyun akışı

    public readonly struct GameStateChangedSignal
    {
        public readonly GameState From;
        public readonly GameState To;
        public GameStateChangedSignal(GameState from, GameState to) { From = from; To = to; }
    }

    /// <summary>Yeni koşu için oturum hazırlandı; oynanış sistemleri kendini sıfırlar.</summary>
    public readonly struct RunStartedSignal { }

    /// <summary>Menüye dönüldü; sahnedeki koşu kalıntıları temizlenir.</summary>
    public readonly struct RunClearedSignal { }

    /// <summary>Oyuncu yakalandı ve yeterli anahtarı var; karar bekleniyor.</summary>
    public readonly struct ReviveOfferedSignal
    {
        public readonly int KeyCost;
        public readonly float Seconds;
        public ReviveOfferedSignal(int keyCost, float seconds) { KeyCost = keyCost; Seconds = seconds; }
    }

    /// <summary>Anahtar kullanıldı; koşu kaldığı yerden sürüyor.</summary>
    public readonly struct RunRevivedSignal { }

    public readonly struct PackageCollectedSignal { }

    /// <summary>Oyun sonunda bir sürpriz paket açıldı.</summary>
    public readonly struct PackageOpenedSignal
    {
        public readonly RewardKind Kind;
        public readonly int Amount;
        public PackageOpenedSignal(RewardKind kind, int amount) { Kind = kind; Amount = amount; }
    }

    public readonly struct RunEndedSignal
    {
        public readonly int Score;
        public readonly int Coins;
        public readonly int BestScore;
        public readonly bool IsNewBest;

        public RunEndedSignal(int score, int coins, int bestScore, bool isNewBest)
        {
            Score = score;
            Coins = coins;
            BestScore = bestScore;
            IsNewBest = isNewBest;
        }
    }

    // ---------------------------------------------------------------- Oyuncu

    public readonly struct LaneChangedSignal
    {
        public readonly int From;
        public readonly int To;
        public LaneChangedSignal(int from, int to) { From = from; To = to; }
    }

    public readonly struct PlayerJumpedSignal { }
    public readonly struct PlayerLandedSignal { }
    public readonly struct PlayerSlidSignal { }

    /// <summary>Engele çarptı ama yakalanmadı; kovalayan ensesine geldi.</summary>
    public readonly struct PlayerStumbledSignal { }

    public readonly struct PlayerCaughtSignal { }
    public readonly struct ShieldBrokenSignal { }

    // ---------------------------------------------------------------- Dünyalar

    /// <summary>Oyuncu yeni bir dünyanın sınırını geçti (koşunun başındaki ilk dünya için atılmaz).</summary>
    public readonly struct WorldChangedSignal
    {
        public readonly string Id;
        public readonly string NameKey;
        public readonly int Index;
        /// <summary>Dünya adı afişinin ekranda kalma süresi (sn).</summary>
        public readonly float BannerSeconds;

        public WorldChangedSignal(string id, string nameKey, int index, float bannerSeconds)
        {
            Id = id;
            NameKey = nameKey;
            Index = index;
            BannerSeconds = bannerSeconds;
        }
    }

    // ---------------------------------------------------------------- Öğretici

    /// <summary>Öğreticinin 3B okunun yönü. None: ok yok.</summary>
    public enum TutorialCue
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Öğretici ipucu değişti. Anahtar boşsa ipucu gizlenir. Çeviri anahtarı taşır; metni arayüz bağlar.
    /// Cue sahnedeki okun yönünü söyler.
    /// </summary>
    public readonly struct TutorialHintSignal
    {
        public readonly string TextKey;
        public readonly TutorialCue Cue;

        public TutorialHintSignal(string textKey, TutorialCue cue = TutorialCue.None)
        {
            TextKey = textKey;
            Cue = cue;
        }
    }

    /// <summary>Öğretici sürerken rastgele engel ve altın dizilmez; öğretici kendi engellerini koyar.</summary>
    public readonly struct TutorialActiveSignal
    {
        public readonly bool Active;
        public TutorialActiveSignal(bool active) { Active = active; }
    }

    /// <summary>Öğretici oyuncunun hareket etmesini beklerken oyunu yavaşlatır, bitince normale döndürür.</summary>
    public readonly struct TutorialSlowMotionSignal
    {
        public readonly bool On;
        public TutorialSlowMotionSignal(bool on) { On = on; }
    }

    // ---------------------------------------------------------------- Puan ve toplananlar

    public readonly struct ScoreChangedSignal
    {
        public readonly int Score;
        public ScoreChangedSignal(int score) { Score = score; }
    }

    public readonly struct PowerUpCollectedSignal
    {
        public readonly string Id;
        public PowerUpCollectedSignal(string id) { Id = id; }
    }

    /// <summary>Bir engel yok edildi (çarpma, kalkan ya da yetenek). Geride kalıp toplananlar sayılmaz.</summary>
    public readonly struct ObstacleDestroyedSignal
    {
        public readonly UnityEngine.Vector3 Position;
        public ObstacleDestroyedSignal(UnityEngine.Vector3 position) { Position = position; }
    }

    public readonly struct CoinCollectedSignal
    {
        public readonly int RunCoins;
        public CoinCollectedSignal(int runCoins) { RunCoins = runCoins; }
    }

    /// <summary>Süreli bir etki başladı (güçlendirme ya da yetenek).</summary>
    public readonly struct TimedEffectStartedSignal
    {
        public readonly string Id;
        public readonly string NameKey;
        public readonly float Duration;

        public TimedEffectStartedSignal(string id, string nameKey, float duration)
        {
            Id = id;
            NameKey = nameKey;
            Duration = duration;
        }
    }

    public readonly struct TimedEffectEndedSignal
    {
        public readonly string Id;
        public TimedEffectEndedSignal(string id) { Id = id; }
    }

    public readonly struct AbilityUsedSignal
    {
        public readonly string NameKey;
        public readonly float Cooldown;
        public AbilityUsedSignal(string nameKey, float cooldown) { NameKey = nameKey; Cooldown = cooldown; }
    }

    // ---------------------------------------------------------------- Profil ve ayarlar

    public readonly struct CharacterSelectedSignal
    {
        public readonly string Id;
        public readonly string NameKey;
        public readonly string AbilityNameKey;
        /// <summary>Meslek yapay zekâya kaptırıldı; geri alınana kadar oynanamaz.</summary>
        public readonly bool IsAutomated;
        public readonly int BuybackCost;

        public CharacterSelectedSignal(string id, string nameKey, string abilityNameKey, bool isAutomated, int buybackCost)
        {
            Id = id;
            NameKey = nameKey;
            AbilityNameKey = abilityNameKey;
            IsAutomated = isAutomated;
            BuybackCost = buybackCost;
        }
    }

    /// <summary>
    /// Koleksiyondaki bir meslek kartının güncel hali. Seçim, otomasyon ya da geri alma
    /// olduğunda her meslek için yeniden duyurulur.
    /// </summary>
    public readonly struct CharacterCardSignal
    {
        public readonly string Id;
        public readonly string NameKey;
        public readonly string AbilityNameKey;
        public readonly UnityEngine.Color Color;
        public readonly bool IsAutomated;
        public readonly int BuybackCost;
        public readonly bool IsSelected;

        public CharacterCardSignal(string id, string nameKey, string abilityNameKey, UnityEngine.Color color, bool isAutomated, int buybackCost, bool isSelected)
        {
            Id = id;
            NameKey = nameKey;
            AbilityNameKey = abilityNameKey;
            Color = color;
            IsAutomated = isAutomated;
            BuybackCost = buybackCost;
            IsSelected = isSelected;
        }
    }

    /// <summary>Oyuncu yakalandı ve oynadığı meslek otomatikleşti.</summary>
    public readonly struct ProfessionAutomatedSignal
    {
        public readonly string Id;
        public readonly string NameKey;
        public ProfessionAutomatedSignal(string id, string nameKey) { Id = id; NameKey = nameKey; }
    }

    /// <summary>Mağazada satılan bir ürün. Açılışta her ürün için bir kez duyurulur.</summary>
    public readonly struct StoreItemListedSignal
    {
        public readonly string Id;
        public readonly string NameKey;
        public readonly RewardKind Kind;
        public readonly int Amount;
        public readonly int Price;

        public StoreItemListedSignal(string id, string nameKey, RewardKind kind, int amount, int price)
        {
            Id = id;
            NameKey = nameKey;
            Kind = kind;
            Amount = amount;
            Price = price;
        }
    }

    public readonly struct AccessoryChangedSignal
    {
        public readonly string NameKey;
        public readonly int Count;
        public readonly int Price;
        public AccessoryChangedSignal(string nameKey, int count, int price) { NameKey = nameKey; Count = count; Price = price; }
    }

    public readonly struct ProfileChangedSignal
    {
        public readonly int BestScore;
        public readonly int TotalCoins;
        public readonly int Keys;

        public ProfileChangedSignal(int bestScore, int totalCoins, int keys)
        {
            BestScore = bestScore;
            TotalCoins = totalCoins;
            Keys = keys;
        }
    }

    public readonly struct LocaleChangedSignal
    {
        public readonly string Code;
        public LocaleChangedSignal(string code) { Code = code; }
    }

    /// <summary>Ses ya da görüntü kalitesi değişti (açılışta da bir kez atılır).</summary>
    public readonly struct SettingsChangedSignal
    {
        public readonly bool SoundOn;
        public readonly GraphicsQuality Quality;

        public SettingsChangedSignal(bool soundOn, GraphicsQuality quality)
        {
            SoundOn = soundOn;
            Quality = quality;
        }
    }
}
