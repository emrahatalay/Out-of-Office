using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Oyuncunun kalıcı ilerlemesi: rekor ve altın. Değişince ProfileChangedSignal atar.</summary>
    public sealed class PlayerProfile : IPostStartable
    {
        const string BestKey = "best_score";
        const string CoinsKey = "total_coins";

        readonly ISaveService _save;
        readonly ISignalBus _bus;

        public int BestScore { get; private set; }
        public int TotalCoins { get; private set; }

        public PlayerProfile(ISaveService save, ISignalBus bus)
        {
            _save = save;
            _bus = bus;
            BestScore = _save.GetInt(BestKey);
            TotalCoins = _save.GetInt(CoinsKey);
        }

        public void PostStart()
        {
            Announce();
        }

        /// <returns>Yeni rekor mu?</returns>
        public bool SubmitRun(int score, int coins)
        {
            bool isNewBest = score > BestScore;
            if (isNewBest) BestScore = score;
            TotalCoins += coins;
            Persist();
            return isNewBest;
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > TotalCoins) return false;
            TotalCoins -= amount;
            Persist();
            return true;
        }

        void Persist()
        {
            _save.SetInt(BestKey, BestScore);
            _save.SetInt(CoinsKey, TotalCoins);
            _save.Flush();
            Announce();
        }

        void Announce()
        {
            _bus.Fire(new ProfileChangedSignal(BestScore, TotalCoins));
        }
    }
}
