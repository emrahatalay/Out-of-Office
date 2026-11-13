using System;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>Unity'nin trigger mesajını C# olayına çevirir; içinde oyun mantığı yoktur.</summary>
    public sealed class TriggerRelay : MonoBehaviour
    {
        public event Action<Collider> Entered;

        void OnTriggerEnter(Collider other)
        {
            Entered?.Invoke(other);
        }
    }
}
