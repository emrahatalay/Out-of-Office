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
    }
}
