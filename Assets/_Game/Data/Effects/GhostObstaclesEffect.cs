using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Engeller süre boyunca çarpmaz (ör. Halüsinasyon).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Ghost Obstacles", fileName = "Effect_GhostObstacles")]
    public sealed class GhostObstaclesEffect : EffectDefinition
    {
        public override void Apply(EffectContext context) { context.Obstacles.BeginGhost(); }
        public override void Remove(EffectContext context) { context.Obstacles.EndGhost(); }
    }
}
