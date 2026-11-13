using UnityEngine;

namespace OutOfOffice.Data
{
    /// <summary>
    /// Hareketli engellerin davranışı (Strategy). Boş bırakılan engel yerinde durur.
    /// Yeni bir hareket türü = bu sınıftan türeyen tek bir sınıf.
    /// </summary>
    public abstract class MovementPattern : ScriptableObject
    {
        /// <param name="origin">Engelin doğduğu nokta.</param>
        /// <param name="age">Doğduğundan beri geçen saniye.</param>
        /// <param name="laneWidth">Bir şeridin genişliği.</param>
        /// <param name="halfTrackWidth">Orta çizgiden en dış şeridin merkezine uzaklık.</param>
        public abstract Vector3 Evaluate(Vector3 origin, float age, float laneWidth, float halfTrackWidth);

        /// <summary>
        /// Oyuncuya doğru gelen engeller, oyuncuyla tam kendi sırasında buluşsun diye
        /// bu kadar ileride doğar. Yerinde duranlar için 0.
        /// </summary>
        public virtual float LeadDistance(float secondsUntilPlayerArrives) { return 0f; }
    }
}
