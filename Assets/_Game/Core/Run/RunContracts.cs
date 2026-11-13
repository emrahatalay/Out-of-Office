using OutOfOffice.Core.Signals;
using UnityEngine;

namespace OutOfOffice.Core.Run
{
    public enum MoveDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>Oyuncunun o anki durumu. Etkiler ve sistemler oyuncuyu bu arayüzden görür.</summary>
    public interface IPlayerStatus
    {
        Vector3 Position { get; }
        int Lane { get; }
        bool HasShield { get; set; }
    }

    /// <summary>Kovalayan düşman.</summary>
    public interface IChaser
    {
        /// <summary>Ensede mi? Bu sırada bir çarpışma yakalanmak demektir.</summary>
        bool IsThreatening { get; }

        /// <summary>Oyuncu tökezledi; arayı kapat.</summary>
        void CloseIn();

        void BeginStun();
        void EndStun();
    }

    /// <summary>Sahnedeki engellerin tamamı.</summary>
    public interface IObstacleField
    {
        /// <summary>Hayalet moddayken engeller çarpmaz. Begin/End çiftleri iç içe olabilir.</summary>
        void BeginGhost();
        void EndGhost();

        /// <summary>fromZ ile fromZ + range arasındaki engelleri yok eder, sayısını döner.</summary>
        int DestroyInRange(float fromZ, float range);
    }

    /// <summary>
    /// Etkilerin (EffectDefinition) dokunabildiği her şey. Yeni bir etki türü yazarken
    /// başka hiçbir sisteme bağımlılık gerekmez.
    /// </summary>
    public sealed class EffectContext
    {
        public readonly IPlayerStatus Player;
        public readonly IChaser Chaser;
        public readonly IObstacleField Obstacles;
        public readonly ISignalBus Signals;

        public EffectContext(IPlayerStatus player, IChaser chaser, IObstacleField obstacles, ISignalBus signals)
        {
            Player = player;
            Chaser = chaser;
            Obstacles = obstacles;
            Signals = signals;
        }
    }
}
