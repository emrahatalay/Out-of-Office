using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Oyuncuyu engellerin üstünde uçurur (ör. Kargo Dronu).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Flight", fileName = "Effect_Flight")]
    public sealed class FlightEffect : EffectDefinition
    {
        [Min(1f)] public float height = 6f;

        public override void Apply(EffectContext context) { context.Player.FlightHeight = height; }
        public override void Remove(EffectContext context) { context.Player.FlightHeight = 0f; }
    }
}
