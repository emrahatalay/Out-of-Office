using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>Oyuncunun paylaşılan durumu. PlayerSystem yazar, diğerleri IPlayerStatus üzerinden okur.</summary>
    public sealed class PlayerStatus : IPlayerStatus
    {
        public Vector3 Position { get; set; }
        public int Lane { get; set; }
        public bool HasShield { get; set; }
        public float JumpMultiplier { get; set; } = 1f;
        public float MagnetRadius { get; set; }
        public float ScoreMultiplier { get; set; } = 1f;
        public float FlightHeight { get; set; }

        public void ResetModifiers()
        {
            HasShield = false;
            JumpMultiplier = 1f;
            MagnetRadius = 0f;
            ScoreMultiplier = 1f;
            FlightHeight = 0f;
        }
    }
}
