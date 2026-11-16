using System;
using System.Collections.Generic;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Günlük görevler. Her gün (yerel takvim) katalogdaki havuzdan farklı türde birkaç görev seçilir;
    /// seçim günün numarasından türetildiği için aynı gün aynı görevler gelir. Sayma yalnızca sinyal
    /// dinleyerek yapılır, koşu döngüsünde iş ve bellek ayırma yoktur. İlerleme kayda koşu bitince,
    /// menüye dönünce ve duraklatınca yazılır (her altında kayıt yazılmaz).
    /// </summary>
    public sealed class MissionSystem : IPostStartable, IDisposable
    {
        const string DayKey = "missions.day";
        const string IdsKey = "missions.ids";
        const string ProgressKey = "missions.progress";
        const string ClaimedKey = "missions.claimed";
        const char Separator = ',';

        readonly GameConfig _config;
        readonly ContentCatalog _catalog;
        readonly RunSession _session;
        readonly ISaveService _save;
        readonly IClock _clock;
        readonly RewardSystem _rewards;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        MissionDefinition[] _missions = new MissionDefinition[0];
        int[] _progress = new int[0];
        bool[] _claimed = new bool[0];
        bool _dirty;

        public MissionSystem(
            GameConfig config,
            ContentCatalog catalog,
            RunSession session,
            ISaveService save,
            IClock clock,
            RewardSystem rewards,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _config = config;
            _catalog = catalog;
            _session = session;
            _save = save;
            _clock = clock;
            _rewards = rewards;
            _state = state;
            _bus = bus;

            _bus.Subscribe<CoinCollectedSignal>(_ => Add(MissionMetric.Coins, 1)).AddTo(_subscriptions);
            _bus.Subscribe<PlayerJumpedSignal>(_ => Add(MissionMetric.Jumps, 1)).AddTo(_subscriptions);
            _bus.Subscribe<PlayerSlidSignal>(_ => Add(MissionMetric.Slides, 1)).AddTo(_subscriptions);
            _bus.Subscribe<LaneChangedSignal>(_ => Add(MissionMetric.LaneChanges, 1)).AddTo(_subscriptions);
            _bus.Subscribe<PowerUpCollectedSignal>(_ => Add(MissionMetric.PowerUps, 1)).AddTo(_subscriptions);
            _bus.Subscribe<ObstacleDestroyedSignal>(_ => Add(MissionMetric.Obstacles, 1)).AddTo(_subscriptions);
            _bus.Subscribe<RunEndedSignal>(OnRunEnded).AddTo(_subscriptions);
            _bus.Subscribe<GameStateChangedSignal>(OnStateChanged).AddTo(_subscriptions);
            _bus.Subscribe<MissionClaimRequestedSignal>(OnClaimRequested).AddTo(_subscriptions);

            Load();
        }

        public void PostStart()
        {
            Announce();
        }

        // ------------------------------------------------------------ Sayma

        void Add(MissionMetric metric, int amount)
        {
            for (int i = 0; i < _missions.Length; i++)
            {
                if (_missions[i].metric != metric || _progress[i] >= _missions[i].target) continue;

                _progress[i] = Math.Min(_missions[i].target, _progress[i] + amount);
                _dirty = true;
            }
        }

        void OnRunEnded(RunEndedSignal signal)
        {
            Add(MissionMetric.Runs, 1);
            Add(MissionMetric.Distance, (int)_session.Distance);

            // Tek koşudaki en iyi puan birikmez: hedefe ulaşan en yüksek değer sayılır.
            for (int i = 0; i < _missions.Length; i++)
            {
                if (_missions[i].metric != MissionMetric.ScoreInRun) continue;

                int best = Math.Min(_missions[i].target, Math.Max(_progress[i], signal.Score));
                if (best == _progress[i]) continue;
                _progress[i] = best;
                _dirty = true;
            }

            Persist();
        }

        void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.To == GameState.Menu)
            {
                // Uygulama gece yarısını aşıp açık kaldıysa menüye dönünce yeni görevler gelir.
                if (_clock.Today != _save.GetInt(DayKey)) Roll();
                Persist();
            }
            else if (signal.To == GameState.Paused)
            {
                Persist();
            }
        }

        // ------------------------------------------------------------ Ödül

        void OnClaimRequested(MissionClaimRequestedSignal signal)
        {
            int slot = signal.Slot;
            if (_state.Current != GameState.Menu || slot < 0 || slot >= _missions.Length) return;
            if (_claimed[slot] || _progress[slot] < _missions[slot].target) return;

            _claimed[slot] = true;
            _rewards.Grant(_missions[slot].rewardKind, _missions[slot].rewardAmount);
            _dirty = true;
            Persist();
        }

        // ------------------------------------------------------------ Günlük seçim ve kayıt

        void Load()
        {
            if (_clock.Today != _save.GetInt(DayKey))
            {
                Roll();
                return;
            }

            // Kayıt bu günün: kimlikleri görevlere çevir, ilerlemeyi geri yükle.
            var ids = _save.GetString(IdsKey).Split(Separator);
            var progress = _save.GetString(ProgressKey).Split(Separator);
            var claimed = _save.GetString(ClaimedKey).Split(Separator);

            var missions = new List<MissionDefinition>();
            var restoredProgress = new List<int>();
            var restoredClaimed = new List<bool>();
            for (int i = 0; i < ids.Length; i++)
            {
                var mission = Find(ids[i]);
                if (mission == null) continue;

                missions.Add(mission);
                restoredProgress.Add(i < progress.Length && int.TryParse(progress[i], out int p) ? Math.Min(p, mission.target) : 0);
                restoredClaimed.Add(i < claimed.Length && claimed[i] == "1");
            }

            // Havuzdan görev silinmiş ya da kayıt bozuksa güne yeniden başla.
            if (missions.Count == 0)
            {
                Roll();
                return;
            }

            _missions = missions.ToArray();
            _progress = restoredProgress.ToArray();
            _claimed = restoredClaimed.ToArray();
        }

        MissionDefinition Find(string id)
        {
            if (_catalog.missions == null) return null;

            foreach (var mission in _catalog.missions)
                if (mission != null && mission.id == id) return mission;
            return null;
        }

        /// <summary>
        /// Bugünün görevlerini seçer: günün numarası rastgeleliğin tohumudur (aynı gün aynı seçim),
        /// havuz karıştırılıp farklı türde ilk görevler alınır.
        /// </summary>
        void Roll()
        {
            var pool = new List<MissionDefinition>();
            if (_catalog.missions != null)
                foreach (var mission in _catalog.missions)
                    if (mission != null) pool.Add(mission);

            var random = new Random(_clock.Today);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            var picked = new List<MissionDefinition>();
            var usedMetrics = new HashSet<MissionMetric>();
            foreach (var mission in pool)
            {
                if (picked.Count >= _config.dailyMissionCount) break;
                if (usedMetrics.Add(mission.metric)) picked.Add(mission);
            }

            _missions = picked.ToArray();
            _progress = new int[_missions.Length];
            _claimed = new bool[_missions.Length];
            _save.SetInt(DayKey, _clock.Today);
            _dirty = true;
            Persist();
        }

        /// <summary>Değişiklik varsa kayda yazar, sonra arayüze güncel durumu duyurur.</summary>
        void Persist()
        {
            Save();
            Announce();
        }

        void Save()
        {
            if (!_dirty) return;

            _save.SetString(IdsKey, Join(_missions.Length, i => _missions[i].id));
            _save.SetString(ProgressKey, Join(_progress.Length, i => _progress[i].ToString()));
            _save.SetString(ClaimedKey, Join(_claimed.Length, i => _claimed[i] ? "1" : "0"));
            _save.Flush();
            _dirty = false;
        }

        static string Join(int count, Func<int, string> item)
        {
            var parts = new string[count];
            for (int i = 0; i < count; i++) parts[i] = item(i);
            return string.Join(Separator.ToString(), parts);
        }

        void Announce()
        {
            int claimable = 0;
            for (int i = 0; i < _missions.Length; i++)
            {
                var mission = _missions[i];
                _bus.Fire(new MissionChangedSignal(i, mission.nameKey, _progress[i], mission.target, mission.rewardKind, mission.rewardAmount, _claimed[i]));
                if (!_claimed[i] && _progress[i] >= mission.target) claimable++;
            }

            _bus.Fire(new MissionsClaimableSignal(claimable));
        }

        public void Dispose()
        {
            Save();
            _subscriptions.Dispose();
        }
    }
}
