using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>
    /// Prompt Injection: ilerideki altınlar, hangi şeritte olursa olsun oyuncu yerine toplanır.
    /// Mıknatıstan farkı menzilin uzun ve şerit sınırının olmamasıdır.
    /// </summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Auto Collect", fileName = "Effect_AutoCollect")]
    public sealed class AutoCollectEffect : EffectDefinition
    {
        [Min(0f), Tooltip("Oyuncunun önünde bu mesafeye kadar olan altınlar toplanır (metre).")]
        public float range = 30f;

        public override void Apply(EffectContext context) { context.Player.AutoCollectRange = range; }
        public override void Remove(EffectContext context) { context.Player.AutoCollectRange = 0f; }
    }
}
