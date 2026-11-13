using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Tek çarpışmayı affeden kalkan (ör. Kıdem Kalkanı).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Shield", fileName = "Effect_Shield")]
    public sealed class ShieldEffect : EffectDefinition
    {
        public override void Apply(EffectContext context) { context.Player.HasShield = true; }
        public override void Remove(EffectContext context) { context.Player.HasShield = false; }
    }
}
