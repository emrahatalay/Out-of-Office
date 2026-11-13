using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Daha yükseğe zıplatır (ör. Çift Espresso).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Jump Boost", fileName = "Effect_JumpBoost")]
    public sealed class JumpBoostEffect : EffectDefinition
    {
        [Min(1f)] public float multiplier = 1.8f;

        public override void Apply(EffectContext context) { context.Player.JumpMultiplier = multiplier; }
        public override void Remove(EffectContext context) { context.Player.JumpMultiplier = 1f; }
    }
}
