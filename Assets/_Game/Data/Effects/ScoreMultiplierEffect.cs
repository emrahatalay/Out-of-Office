using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Kazanılan puanı çarpar (ör. Fazla Mesai).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Score Multiplier", fileName = "Effect_ScoreMultiplier")]
    public sealed class ScoreMultiplierEffect : EffectDefinition
    {
        [Min(1f)] public float multiplier = 2f;

        public override void Apply(EffectContext context) { context.Player.ScoreMultiplier = multiplier; }
        public override void Remove(EffectContext context) { context.Player.ScoreMultiplier = 1f; }
    }
}
