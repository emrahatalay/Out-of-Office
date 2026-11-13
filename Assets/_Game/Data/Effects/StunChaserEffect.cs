using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Kovalayanı süre boyunca durdurur (ör. CAPTCHA).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Stun Chaser", fileName = "Effect_StunChaser")]
    public sealed class StunChaserEffect : EffectDefinition
    {
        public override void Apply(EffectContext context) { context.Chaser.BeginStun(); }
        public override void Remove(EffectContext context) { context.Chaser.EndStun(); }
    }
}
