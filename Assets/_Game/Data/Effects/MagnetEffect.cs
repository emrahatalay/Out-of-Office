using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Yakındaki altınları oyuncuya çeker (ör. Maaş Mıknatısı).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Magnet", fileName = "Effect_Magnet")]
    public sealed class MagnetEffect : EffectDefinition
    {
        [Min(0f)] public float radius = 7f;

        public override void Apply(EffectContext context) { context.Player.MagnetRadius = radius; }
        public override void Remove(EffectContext context) { context.Player.MagnetRadius = 0f; }
    }
}
