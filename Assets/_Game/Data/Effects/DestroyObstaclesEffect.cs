using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Oyuncunun önündeki engelleri anında siler (ör. Debug).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Destroy Obstacles", fileName = "Effect_DestroyObstacles")]
    public sealed class DestroyObstaclesEffect : EffectDefinition
    {
        [Min(1f)] public float range = 45f;

        public override void Apply(EffectContext context)
        {
            context.Obstacles.DestroyInRange(context.Player.Position.z, range);
        }
    }
}
