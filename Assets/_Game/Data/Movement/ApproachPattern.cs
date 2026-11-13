using UnityEngine;

namespace OutOfOffice.Data.Movement
{
    /// <summary>Oyuncuya doğru gelir (ör. otonom taksi).</summary>
    [CreateAssetMenu(menuName = "Out of Office/Movement/Approach", fileName = "Movement_Approach")]
    public sealed class ApproachPattern : MovementPattern
    {
        [Tooltip("Oyuncuya doğru hız (m/sn).")]
        public float speed = 8f;

        public override Vector3 Evaluate(Vector3 origin, float age, float laneWidth, float halfTrackWidth)
        {
            origin.z -= speed * age;
            return origin;
        }

        public override float LeadDistance(float secondsUntilPlayerArrives)
        {
            return speed * secondsUntilPlayerArrives;
        }
    }
}
