using OutOfOffice.Core.Run;
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
    public readonly struct LanguageCycleRequestedSignal { }
    public readonly struct AbilityRequestedSignal { }

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

    /// <summary>Oyuncu yakalandı ve oynadığı meslek otomatikleşti.</summary>
    public readonly struct ProfessionAutomatedSignal
    {
        public readonly string Id;
        public readonly string NameKey;
        public ProfessionAutomatedSignal(string id, string nameKey) { Id = id; NameKey = nameKey; }
    }

    public readonly struct ProfileChangedSignal
    {
        public readonly int BestScore;
        public readonly int TotalCoins;
        public ProfileChangedSignal(int bestScore, int totalCoins) { BestScore = bestScore; TotalCoins = totalCoins; }
    }

    public readonly struct LocaleChangedSignal
    {
        public readonly string Code;
        public LocaleChangedSignal(string code) { Code = code; }
    }
}
