using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>Bir şeride engel konma eğilimi. 1 = nötr, büyük = daha sık.</summary>
    public interface ILaneBias
    {
        float GetWeight(int lane);
    }

    /// <summary>
    /// "Seni öğrenen yapay zekâ". Oyuncunun hangi şeritte ne kadar kaldığını izler ve
    /// engelleri sevdiği şeritlere yöneltir. Hafıza koşular arasında korunur; eski
    /// alışkanlıklar zamanla unutulur.
    /// </summary>
    public sealed class AdaptiveDirector : ILaneBias, ITickable
    {
        const float MinSamplesSeconds = 4f;
        const float MinWeight = 0.05f;

        readonly GameConfig _config;
        readonly PlayerStatus _player;
        readonly IGameStateProvider _state;
        readonly float[] _laneTime;
        float _total;

        public AdaptiveDirector(GameConfig config, PlayerStatus player, IGameStateProvider state)
        {
            _config = config;
            _player = player;
            _state = state;
            _laneTime = new float[config.laneCount];
        }

        public void Tick()
        {
            if (_state.Current != GameState.Running) return;

            float dt = Time.deltaTime;
            float decay = Mathf.Exp(-dt / Mathf.Max(1f, _config.adaptiveMemorySeconds));

            _total = 0f;
            for (int i = 0; i < _laneTime.Length; i++)
            {
                _laneTime[i] *= decay;
                _total += _laneTime[i];
            }

            int lane = Mathf.Clamp(_player.Lane, 0, _laneTime.Length - 1);
            _laneTime[lane] += dt;
            _total += dt;
        }

        public float GetWeight(int lane)
        {
            if (_total < MinSamplesSeconds) return 1f;

            float share = _laneTime[lane] / _total;
            float uniform = 1f / _laneTime.Length;
            return Mathf.Max(MinWeight, Mathf.Lerp(1f, share / uniform, _config.adaptiveStrength));
        }
    }
}
