using System;
using System.Collections.Generic;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Oyuncunun önüne engel sıraları, altın dizileri ve güçlendirmeler yerleştirir.
    /// İçeriği WorldDefinition'dan, şerit tercihini ILaneBias'tan alır; hiçbir engeli adıyla bilmez.
    /// Her sırada en az bir şerit geçilebilir kalır.
    /// </summary>
    public sealed class SpawnDirector : ITickable, IDisposable
    {
        readonly GameConfig _config;
        readonly RunSession _session;
        readonly PlayerStatus _player;
        readonly ObstacleField _obstacles;
        readonly PickupField _pickups;
        readonly ILaneBias _bias;
        readonly Workforce _workforce;
        readonly IGameStateProvider _state;
        readonly WorldTimeline _timeline;
        readonly List<ObstacleDefinition>[] _candidatesByWorld;
        List<ObstacleDefinition> _candidates;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly bool[] _laneUsed;
        readonly float[] _laneWeights;
        float _nextRowZ;
        bool _wasFlying;
        bool _tutorialActive;

        const int AirCoinCount = 34;
        const float AirCoinSpacing = 3f;
        const float AirCoinLead = 16f;

        public SpawnDirector(
            GameConfig config,
            RunSession session,
            PlayerStatus player,
            ObstacleField obstacles,
            PickupField pickups,
            ILaneBias bias,
            Workforce workforce,
            WorldTimeline timeline,
            IGameStateProvider state,
            ISignalBus bus)
        {
            _config = config;
            _session = session;
            _player = player;
            _obstacles = obstacles;
            _pickups = pickups;
            _bias = bias;
            _workforce = workforce;
            _timeline = timeline;
            _candidatesByWorld = new List<ObstacleDefinition>[Mathf.Max(1, timeline.Count)];
            for (int i = 0; i < _candidatesByWorld.Length; i++) _candidatesByWorld[i] = new List<ObstacleDefinition>(16);
            _candidates = _candidatesByWorld[0];
            _state = state;
            _laneUsed = new bool[config.laneCount];
            _laneWeights = new float[config.laneCount];

            _subscriptions.Add(bus.Subscribe<RunStartedSignal>(_ => OnRunStarted()));
            _subscriptions.Add(bus.Subscribe<TutorialActiveSignal>(OnTutorialActive));
        }

        // Öğretici kendi engellerini koyarken rastgele dizilim durur; bitince oyuncunun önünden devam eder.
        void OnTutorialActive(TutorialActiveSignal signal)
        {
            _tutorialActive = signal.Active;
            if (!signal.Active) _nextRowZ = _player.Position.z + _config.firstRowDistance;
        }

        void OnRunStarted()
        {
            _tutorialActive = false;
            _nextRowZ = _config.firstRowDistance;
            _wasFlying = false;

            // Bu koşunun engel havuzu, dünya başına: dünyanın engelleri + işini kaptıran mesleklerin robotları.
            for (int i = 0; i < _candidatesByWorld.Length; i++)
            {
                var list = _candidatesByWorld[i];
                list.Clear();
                var world = _timeline.Count > 0 ? _timeline.Get(i) : null;
                if (world != null && world.obstacles != null) list.AddRange(world.obstacles);
                _workforce.CollectRobots(list);
            }
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running || _tutorialActive) return;

            bool flying = _player.FlightHeight > 0f;
            if (flying && !_wasFlying) SpawnAirCoins();
            _wasFlying = flying;

            float horizon = _player.Position.z + _config.spawnAhead;
            while (_nextRowZ < horizon)
            {
                SpawnRow(_nextRowZ);
                float reaction = Mathf.Lerp(_config.reactionTimeEasy, _config.reactionTimeHard, _session.Difficulty);
                float gap = Mathf.Max(6f, _session.Speed * reaction);
                if (Random.value < _config.breatherChance) gap *= _config.breatherMultiplier;
                _nextRowZ += gap;
            }
        }

        void SpawnRow(float z)
        {
            // Sıra hangi dünyaya düşüyorsa onun engelleri, güçlendirmeleri ve altını kullanılır.
            int worldIndex = _timeline.IndexAt(z);
            var world = _timeline.WorldAt(z);
            if (world == null) return;
            _candidates = _candidatesByWorld[Mathf.Min(worldIndex, _candidatesByWorld.Length - 1)];

            int lanes = _config.laneCount;
            float difficulty = _session.Difficulty;

            for (int i = 0; i < lanes; i++)
            {
                _laneUsed[i] = false;
                _laneWeights[i] = _bias.GetWeight(i);
            }

            // Şerit kapatan engeller: en az bir şerit hep açık kalır.
            int blockers = 1;
            if (lanes > 2 && Random.value < Mathf.Lerp(0.15f, 0.75f, difficulty)) blockers = 2;
            blockers = Mathf.Min(blockers, lanes - 1);

            for (int i = 0; i < blockers; i++)
            {
                int lane = PickLane(biased: true);
                var definition = PickObstacle(difficulty, passableOnly: false);
                if (lane < 0 || definition == null) break;

                SpawnObstacle(definition, lane, z);
            }

            // Zorluk arttıkça açık kalan şeride de üstünden atlanan/altından kayılan engel gelir.
            if (Random.value < Mathf.Lerp(0f, 0.45f, difficulty))
            {
                int lane = PickLane(biased: false);
                var definition = PickObstacle(difficulty, passableOnly: true);
                if (lane >= 0 && definition != null) SpawnObstacle(definition, lane, z);
            }

            int freeLane = PickLane(biased: false);
            if (freeLane < 0) return;

            var powerUp = Random.value < _config.powerUpChance ? PickPowerUp(world) : null;
            if (powerUp != null)
            {
                _pickups.Spawn(powerUp.pickupPrefab, powerUp, LanePosition(freeLane, z));
            }
            else if (Random.value < _config.coinTrailChance)
            {
                // Altın dizisi açık şeridi gösterir.
                float start = z - (_config.coinsPerTrail - 1) * 0.5f * _config.coinSpacing;
                for (int i = 0; i < _config.coinsPerTrail; i++)
                    _pickups.Spawn(world.coinPrefab, null, LanePosition(freeLane, start + i * _config.coinSpacing));
            }
        }

        // Uçuş süresince toplanacak, şeritler arasında kıvrılan altın yolu.
        void SpawnAirCoins()
        {
            var world = _timeline.WorldAt(_player.Position.z);
            if (world == null || world.coinPrefab == null) return;

            int lane = _player.Lane;
            float y = _player.FlightHeight - 0.1f;
            float z = _player.Position.z + AirCoinLead;

            for (int i = 0; i < AirCoinCount; i++)
            {
                if (i > 0 && i % 8 == 0)
                    lane = Mathf.Clamp(lane + (Random.value < 0.5f ? -1 : 1), 0, _config.laneCount - 1);

                _pickups.Spawn(world.coinPrefab, null, new Vector3(_config.LaneToX(lane), y, z + i * AirCoinSpacing));
            }
        }

        void SpawnObstacle(ObstacleDefinition definition, int lane, float z)
        {
            _laneUsed[lane] = true;

            var position = LanePosition(lane, z);
            if (definition.movement != null)
            {
                float secondsUntilArrival = (z - _player.Position.z) / Mathf.Max(1f, _session.Speed);
                position.z += definition.movement.LeadDistance(secondsUntilArrival);
            }

            _obstacles.Spawn(definition, position);
        }

        Vector3 LanePosition(int lane, float z)
        {
            return new Vector3(_config.LaneToX(lane), 0f, z);
        }

        /// <summary>Boş şeritlerden birini seçer. biased ise öğrenen yönetmenin ağırlıklarını kullanır.</summary>
        int PickLane(bool biased)
        {
            float total = 0f;
            for (int i = 0; i < _laneUsed.Length; i++)
                if (!_laneUsed[i]) total += biased ? _laneWeights[i] : 1f;

            if (total <= 0f) return -1;

            float roll = Random.value * total;
            int last = -1;
            for (int i = 0; i < _laneUsed.Length; i++)
            {
                if (_laneUsed[i]) continue;
                last = i;
                roll -= biased ? _laneWeights[i] : 1f;
                if (roll <= 0f) return i;
            }

            return last;
        }

        ObstacleDefinition PickObstacle(float difficulty, bool passableOnly)
        {
            float total = 0f;
            for (int i = 0; i < _candidates.Count; i++)
                if (IsCandidate(_candidates[i], difficulty, passableOnly)) total += _candidates[i].spawnWeight;

            if (total <= 0f) return null;

            float roll = Random.value * total;
            ObstacleDefinition last = null;
            for (int i = 0; i < _candidates.Count; i++)
            {
                if (!IsCandidate(_candidates[i], difficulty, passableOnly)) continue;
                last = _candidates[i];
                roll -= last.spawnWeight;
                if (roll <= 0f) return last;
            }

            return last;
        }

        static bool IsCandidate(ObstacleDefinition definition, float difficulty, bool passableOnly)
        {
            if (definition == null || definition.prefab == null) return false;
            if (definition.minDifficulty > difficulty) return false;
            return !passableOnly || definition.avoidBy != AvoidType.ChangeLane;
        }

        static PowerUpDefinition PickPowerUp(WorldDefinition world)
        {
            var powerUps = world.powerUps;
            if (powerUps == null || powerUps.Length == 0) return null;

            float total = 0f;
            for (int i = 0; i < powerUps.Length; i++)
                if (powerUps[i] != null) total += powerUps[i].spawnWeight;

            if (total <= 0f) return null;

            float roll = Random.value * total;
            PowerUpDefinition last = null;
            for (int i = 0; i < powerUps.Length; i++)
            {
                if (powerUps[i] == null) continue;
                last = powerUps[i];
                roll -= last.spawnWeight;
                if (roll <= 0f) return last;
            }

            return last;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
