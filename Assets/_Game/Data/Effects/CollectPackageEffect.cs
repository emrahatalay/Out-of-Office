using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using UnityEngine;

namespace OutOfOffice.Data.Effects
{
    /// <summary>Toplanan şeyi sürpriz paket olarak sayar; paket oyun sonunda açılır.</summary>
    [CreateAssetMenu(menuName = "Out of Office/Effects/Collect Package", fileName = "Effect_CollectPackage")]
    public sealed class CollectPackageEffect : EffectDefinition
    {
        public override void Apply(EffectContext context) { context.Signals.Fire<PackageCollectedSignal>(); }
    }
}
