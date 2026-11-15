using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>
    /// Fişi Çek: süre boyunca bütün robotlar (meslek robotları, süpürge, dron, taksi) durur ve çarpmaz.
    /// Kovalayanı durdurmak için yanına <see cref="StunChaserEffect"/> konur.
    /// </summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Unplug Robots", fileName = "Effect_UnplugRobots")]
    public sealed class UnplugRobotsEffect : EffectDefinition
    {
        public override void Apply(EffectContext context) { context.Obstacles.BeginUnplugRobots(); }
        public override void Remove(EffectContext context) { context.Obstacles.EndUnplugRobots(); }
    }
}
