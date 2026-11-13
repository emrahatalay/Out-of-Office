using UnityEngine;

namespace OutOfOffice.Data.Movement
{
    /// <summary>Şeritler arasında sağa sola süzülür (ör. kargo dronu).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Movement/Side Sweep", fileName = "Movement_SideSweep")]
    public sealed class SideSweepPattern : MovementPattern
    {
        [Tooltip("Kaç şerit genişliğinde salınır.")]
        public float lanes = 1f;
        [Tooltip("Saniyedeki tur sayısı.")]
        public float frequency = 0.35f;

        public override Vector3 Evaluate(Vector3 origin, float age, float laneWidth, float halfTrackWidth)
        {
            float x = origin.x + Mathf.Sin(age * frequency * Mathf.PI * 2f) * lanes * laneWidth;
            origin.x = Mathf.Clamp(x, -halfTrackWidth, halfTrackWidth);
            return origin;
        }
    }
}
