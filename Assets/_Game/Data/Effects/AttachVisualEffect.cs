using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Etki süresince oyuncuya bir görsel takar (ör. sandalye, dron).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Attach Visual", fileName = "Effect_AttachVisual")]
    public sealed class AttachVisualEffect : EffectDefinition
    {
        public GameObject prefab;
        public Vector3 localOffset;

        public override void Apply(EffectContext context) { context.Attachments.Attach(prefab, localOffset); }
        public override void Remove(EffectContext context) { context.Attachments.Detach(prefab); }
    }
}
