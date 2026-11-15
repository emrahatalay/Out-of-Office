using System;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// İlk koşudaki öğretici. Sırayla üç gerçek engel koyar (şerit değiştir, zıpla, kay); her birine
    /// yaklaşınca oyunu yavaşlatır, yönü gösteren 3B oku ve kısa ipucunu açar, oyuncu doğru hareketi
    /// yapınca hızı normale döndürür. Rastgele dizilim öğretici sürerken durur.
    /// Hangi ipucu ve ok gösterilecek sinyalle duyurulur; metni arayüz, oku <see cref="TutorialGuide"/> çizer.
    /// Bitince kayda "tutorial.done" yazılır ve bir daha çıkmaz; yarım kalan koşu sayılmaz.
    /// </summary>
    public sealed class TutorialSystem : ITickable, IDisposable
    {
        const string SaveKey = "tutorial.done";
        const float PassMargin = 3f;

        enum Phase { Idle, Spawn, Approach, Waiting, Passing, Finish }

        // Adım sırası: şerit değiştir, zıpla, kay.
        static readonly AvoidType[] StepAvoid = { AvoidType.ChangeLane, AvoidType.Jump, AvoidType.Slide };

        readonly ISaveService _save;
        readonly GameConfig _config;
        readonly PlayerStatus _player;
        readonly RunSession _session;
        readonly ObstacleField _obstacles;
        readonly IGameStateProvider _state;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        Phase _phase;
        int _step;
        bool _repeat;
        bool _slow;
        float _obstacleZ;
        int _obstacleLane;
        float _finishLeft;

        public TutorialSystem(ISaveService save, GameConfig config, PlayerStatus player, RunSession session,
            ObstacleField obstacles, IGameStateProvider state, ISignalBus bus)
        {
            _save = save;
            _config = config;
            _player = player;
            _session = session;
            _obstacles = obstacles;
            _state = state;
            _bus = bus;

            _bus.Subscribe<RunStartedSignal>(_ => Begin()).AddTo(_subscriptions);
            _bus.Subscribe<LaneChangedSignal>(_ => OnAction(0)).AddTo(_subscriptions);
            _bus.Subscribe<PlayerJumpedSignal>(_ => OnAction(1)).AddTo(_subscriptions);
            _bus.Subscribe<PlayerSlidSignal>(_ => OnAction(2)).AddTo(_subscriptions);
            _bus.Subscribe<PlayerStumbledSignal>(_ => OnAction(_step)).AddTo(_subscriptions);
            _bus.Subscribe<RunEndedSignal>(_ => Stop()).AddTo(_subscriptions);
            _bus.Subscribe<RunClearedSignal>(_ => Stop()).AddTo(_subscriptions);
        }

        void Begin()
        {
            Stop();
            if (_save.GetInt(SaveKey) != 0) return;

            _step = 0;
            _repeat = false;
            _phase = Phase.Spawn;
            _bus.Fire(new TutorialActiveSignal(true));
        }

        public void Tick()
        {
            if (_phase == Phase.Idle || _state.Current != GameState.Running) return;

            switch (_phase)
            {
                case Phase.Spawn:
                    SpawnStepObstacle();
                    break;
                case Phase.Approach:
                    if (_obstacleZ - _player.Position.z <= _config.tutorialTriggerDistance) Trigger();
                    break;
                case Phase.Passing:
                    if (_player.Position.z > _obstacleZ + PassMargin) NextStep();
                    break;
                case Phase.Finish:
                    _finishLeft -= Time.unscaledDeltaTime;
                    if (_finishLeft <= 0f) Finish();
                    break;
            }
        }

        // Oyuncunun önüne, bu adımın hareketini gerektiren gerçek bir engel koyar.
        void SpawnStepObstacle()
        {
            var definition = FindObstacle(StepAvoid[_step]);
            if (definition == null)
            {
                // Dünyada bu türde engel yoksa adımı atla.
                NextStep();
                return;
            }

            _obstacleLane = _player.Lane;
            _obstacleZ = _player.Position.z + _config.tutorialSpawnDistance;
            _obstacles.Spawn(definition, new Vector3(_config.LaneToX(_obstacleLane), 0f, _obstacleZ));
            _phase = Phase.Approach;
        }

        ObstacleDefinition FindObstacle(AvoidType avoidBy)
        {
            var list = _session.World != null ? _session.World.obstacles : null;
            if (list == null) return null;

            for (int i = 0; i < list.Length; i++)
            {
                var candidate = list[i];
                if (candidate != null && candidate.prefab != null && candidate.avoidBy == avoidBy && candidate.movement == null)
                    return candidate;
            }

            return null;
        }

        // Engel yakında: yavaşlat, ok ve ipucunu aç.
        void Trigger()
        {
            // Oyuncu zaten şeridini değiştirdiyse engel onu tehdit etmez; adımı yeniden dene.
            if (_player.Lane != _obstacleLane)
            {
                _repeat = true;
                _phase = Phase.Passing;
                return;
            }

            _phase = Phase.Waiting;
            _slow = true;
            _bus.Fire(new TutorialSlowMotionSignal(true));
            ShowCue();
        }

        void ShowCue()
        {
            switch (StepAvoid[_step])
            {
                case AvoidType.ChangeLane:
                    // Kenar şeritteyse içeri, değilse sola.
                    bool right = _obstacleLane == 0;
                    _bus.Fire(new TutorialHintSignal(right ? "tutorial.right" : "tutorial.left", right ? TutorialCue.Right : TutorialCue.Left));
                    break;
                case AvoidType.Jump:
                    _bus.Fire(new TutorialHintSignal("tutorial.jump", TutorialCue.Up));
                    break;
                default:
                    _bus.Fire(new TutorialHintSignal("tutorial.slide", TutorialCue.Down));
                    break;
            }
        }

        /// <summary>Oyuncu bir hareket yaptı (ya da engele çarptı); sırası gelen adımsa yavaşlatma kalkar.</summary>
        void OnAction(int step)
        {
            if (_step != step || (_phase != Phase.Approach && _phase != Phase.Waiting)) return;

            ReleaseSlow();
            _bus.Fire(new TutorialHintSignal(string.Empty));
            _phase = Phase.Passing;
        }

        void NextStep()
        {
            if (_repeat) _repeat = false;
            else _step++;

            if (_step < StepAvoid.Length)
            {
                _phase = Phase.Spawn;
                return;
            }

            _phase = Phase.Finish;
            _finishLeft = _config.tutorialFinishSeconds;
            _bus.Fire(new TutorialHintSignal("tutorial.go"));
        }

        void Finish()
        {
            _save.SetInt(SaveKey, 1);
            _save.Flush();
            Stop();
        }

        void ReleaseSlow()
        {
            if (!_slow) return;

            _slow = false;
            _bus.Fire(new TutorialSlowMotionSignal(false));
        }

        void Stop()
        {
            if (_phase == Phase.Idle) return;

            ReleaseSlow();
            _phase = Phase.Idle;
            _bus.Fire(new TutorialHintSignal(string.Empty));
            _bus.Fire(new TutorialActiveSignal(false));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
