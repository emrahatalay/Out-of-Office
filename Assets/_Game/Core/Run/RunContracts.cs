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

    /// <summary>Sürpriz paketten çıkabilecek ödül türleri.</summary>
    public enum RewardKind
    {
        Coins,
        Accessory,
        Key
    }

    /// <summary>Oyuncunun o anki durumu. Etkiler ve sistemler oyuncuyu bu arayüzden görür.</summary>
    public interface IPlayerStatus
    {
        Vector3 Position { get; }
        int Lane { get; }
        bool HasShield { get; set; }

        // Etkilerin değiştirdiği değerler. Varsayılanlar: 1, 0, 1, 0.
        float JumpMultiplier { get; set; }
        /// <summary>0'dan büyükse bu yarıçaptaki altınlar oyuncuya çekilir.</summary>
        float MagnetRadius { get; set; }
        float ScoreMultiplier { get; set; }
        /// <summary>0'dan büyükse oyuncu bu yükseklikte uçar.</summary>
        float FlightHeight { get; set; }
    }

    /// <summary>Oyuncuya geçici görsel takar (ör. altındaki sandalye, tepesindeki dron).</summary>
    public interface IPlayerAttachments
    {
        void Attach(GameObject prefab, Vector3 localOffset);
        void Detach(GameObject prefab);
    }

    /// <summary>Kovalayan düşman.</summary>
    public interface IChaser
    {
        /// <summary>Ensede mi? Bu sırada bir çarpışma yakalanmak demektir.</summary>
        bool IsThreatening { get; }

        /// <summary>Oyuncu tökezledi; arayı kapat.</summary>
        void CloseIn();

        /// <summary>Tehdidi bırakıp geriye çekilir (ör. oyuncu anahtarla devam edince).</summary>
        void Retreat();

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
        public readonly IPlayerAttachments Attachments;
        public readonly IChaser Chaser;
        public readonly IObstacleField Obstacles;
        public readonly ISignalBus Signals;

        public EffectContext(IPlayerStatus player, IPlayerAttachments attachments, IChaser chaser, IObstacleField obstacles, ISignalBus signals)
        {
            Player = player;
            Attachments = attachments;
            Chaser = chaser;
            Obstacles = obstacles;
            Signals = signals;
        }
    }
}
