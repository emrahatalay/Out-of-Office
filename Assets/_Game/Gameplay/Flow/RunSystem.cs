using System;
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
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly IDisposable _subscription;
        int _lastScore;

        public RunSystem(
            GameConfig config,
            ContentCatalog catalog,
            RunSession session,
            PlayerStatus player,
            CharacterSelection selection,
            PlayerProfile profile,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _config = config;
            _catalog = catalog;
            _session = session;
            _player = player;
            _selection = selection;
            _profile = profile;
            _state = state;
            _bus = bus;

            // Menüde de zemin görünsün diye dünya baştan bellidir.
            _session.World = _catalog.worlds[0];
            _subscription = _bus.Subscribe<GameStateChangedSignal>(OnStateChanged);
        }

        void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.To == GameState.Running && signal.From != GameState.Paused)
            {
                _session.Begin(_catalog.worlds[0], _selection.ResolvePlayable(), _config.startSpeed);
                _lastScore = -1;
                _bus.Fire<RunStartedSignal>();
            }
            else if (signal.To == GameState.GameOver)
            {
                EndRun();
            }
            else if (signal.To == GameState.Menu && signal.From != GameState.Boot)
            {
                _bus.Fire<RunClearedSignal>();
            }
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running) return;

            _session.Elapsed += Time.deltaTime;
            _session.Speed = Mathf.Min(_config.maxSpeed, _config.startSpeed + _config.acceleration * _session.Elapsed);
            _session.Difficulty = Mathf.Clamp01(_session.Elapsed / _config.difficultyRampSeconds);
            _session.Distance = _player.Position.z;
            _session.Score = Mathf.FloorToInt(_session.Distance) + _session.Coins * _config.coinScore;

            if (_session.Score != _lastScore)
            {
                _lastScore = _session.Score;
                _bus.Fire(new ScoreChangedSignal(_session.Score));
            }
        }

        void EndRun()
        {
            bool isNewBest = _profile.SubmitRun(_session.Score, _session.Coins);
            _bus.Fire(new RunEndedSignal(_session.Score, _session.Coins, _profile.BestScore, isNewBest));
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}
