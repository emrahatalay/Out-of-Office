using OutOfOffice.Core.Run;
using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>
    /// Tek bir oynanış etkisi (Strategy). Güçlendirmeler ve yetenekler bu parçaların
    /// birleşimidir; yeni bir etki türü gerektiğinde bu sınıftan türeyen tek bir sınıf yazılır.
    /// </summary>
    public abstract class EffectDefinition : ScriptableObject
    {
        /// <summary>Etki başladığında bir kez çağrılır.</summary>
        public abstract void Apply(EffectContext context);

        /// <summary>Süre dolunca çağrılır. Anlık etkiler bunu ezmek zorunda değildir.</summary>
        public virtual void Remove(EffectContext context) { }

        /// <summary>Kalkan kırıldığında bu etkiyi içeren süreli etki de biter (ör. sandalye kırılır).</summary>
        public virtual bool EndsWhenShieldBreaks => false;
    }
}
