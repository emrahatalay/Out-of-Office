using System;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Koşuyu başlatır, hız/zorluk/puanı ilerletir, bitince sonucu kaydeder.</summary>
    public sealed class RunSystem : ITickable, IDisposable
    {
        readonly GameConfig _config;
        readonly ContentCatalog _catalog;
        readonly RunSession _session;
        readonly PlayerStatus _player;
        readonly CharacterSelection _selection;
        readonly PlayerProfile _profile;
        readonly IChaser _chaser;
        readonly IObstacleField _obstacles;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        int _lastScore;
        float _scoreExact;
        float _lastDistance;
        float _reviveDeadline;
        bool _awaitingRevive;
        bool _reviving;

        public RunSystem(
            GameConfig config,
            ContentCatalog catalog,
            RunSession session,
            PlayerStatus player,
            CharacterSelection selection,
            PlayerProfile profile,
            IChaser chaser,
            IObstacleField obstacles,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _config = config;
            _catalog = catalog;
            _session = session;
            _player = player;
            _selection = selection;
            _profile = profile;
            _chaser = chaser;
            _obstacles = obstacles;
            _state = state;
            _bus = bus;

            // Menüde de zemin görünsün diye dünya baştan bellidir.
            _session.World = _catalog.worlds[0];
            _bus.Subscribe<GameStateChangedSignal>(OnStateChanged).AddTo(_subscriptions);
            _bus.Subscribe<ReviveRequestedSignal>(_ => Revive()).AddTo(_subscriptions);
            _bus.Subscribe<ReviveDeclinedSignal>(_ => FinalizeIfPending()).AddTo(_subscriptions);
            // Altın puanı da o anki çarpanla (ör. Fazla Mesai) yazılır.
            _bus.Subscribe<CoinCollectedSignal>(_ => _scoreExact += _config.coinScore * _player.ScoreMultiplier).AddTo(_subscriptions);
        }

        void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.To == GameState.Running && signal.From != GameState.Paused)
            {
                // Anahtarla devam: yeni koşu başlatma, kaldığı yerden sürsün.
                if (_reviving)
                {
                    _reviving = false;
                    return;
                }

                FinalizeIfPending();
                _session.Begin(_catalog.worlds[0], _selection.ResolvePlayable(), _config.startSpeed);
                _lastScore = -1;
                _scoreExact = 0f;
                _lastDistance = 0f;
                _bus.Fire<RunStartedSignal>();
            }
            else if (signal.To == GameState.GameOver)
            {
                OnCaught();
            }
            else if (signal.To == GameState.Menu && signal.From != GameState.Boot)
            {
                FinalizeIfPending();
                _bus.Fire<RunClearedSignal>();
            }
        }

        // Aynı koşuda her devam bir öncekinin iki katı anahtar ister: 1, 2, 4...
        int ReviveCost => _config.reviveBaseKeys << Mathf.Min(_session.Revives, 8);

        void OnCaught()
        {
            int cost = ReviveCost;
            if (_profile.Keys < cost)
            {
                FinalizeRun();
                return;
            }

            // Sonuç, oyuncu karar verene (ya da süre dolana) kadar yazılmaz.
            _awaitingRevive = true;
            _reviveDeadline = Time.unscaledTime + _config.reviveOfferSeconds;
            _bus.Fire(new ReviveOfferedSignal(cost, _config.reviveOfferSeconds));
        }

        void Revive()
        {
            if (!_awaitingRevive || !_profile.TrySpendKeys(ReviveCost)) return;

            _awaitingRevive = false;
            _session.Revives++;
            _obstacles.DestroyInRange(_player.Position.z - 5f, _config.reviveClearDistance);
            _chaser.Retreat();

            _reviving = true;
            _bus.Fire<RunRevivedSignal>();
            _reviving = false;
        }

        void FinalizeIfPending()
        {
            if (_awaitingRevive) FinalizeRun();
        }

        public void Tick()
        {
            if (_awaitingRevive && Time.unscaledTime >= _reviveDeadline) FinalizeRun();
            if (_state.Current != GameState.Running) return;

            _session.Elapsed += Time.deltaTime;
            _session.Speed = Mathf.Min(_config.maxSpeed, _config.startSpeed + _config.acceleration * _session.Elapsed);
            _session.Difficulty = Mathf.Clamp01(_session.Elapsed / _config.difficultyRampSeconds);
            float distance = _player.Position.z;
            _scoreExact += (distance - _lastDistance) * _player.ScoreMultiplier;
            _lastDistance = distance;
            _session.Distance = distance;
            _session.Score = Mathf.FloorToInt(_scoreExact);

            if (_session.Score != _lastScore)
            {
                _lastScore = _session.Score;
                _bus.Fire(new ScoreChangedSignal(_session.Score));
            }
        }

        void FinalizeRun()
        {
            _awaitingRevive = false;
            bool isNewBest = _profile.SubmitRun(_session.Score, _session.Coins);
            _bus.Fire(new RunEndedSignal(_session.Score, _session.Coins, _profile.BestScore, isNewBest));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
