using OutOfOffice.Data;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>Yoldan toplanan nesne. PowerUp boşsa altındır.</summary>
    public sealed class PickupView : MonoBehaviour
    {
        bool _initialized;

        public PowerUpDefinition PowerUp { get; private set; }

        public void Bind(PowerUpDefinition powerUp)
        {
            PowerUp = powerUp;

            if (_initialized) return;
            _initialized = true;

            var colliders = GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                var sphere = gameObject.AddComponent<SphereCollider>();
                sphere.center = new Vector3(0f, 1f, 0f);
                sphere.radius = 0.7f;
                colliders = new Collider[] { sphere };
            }

            foreach (var collider in colliders) collider.isTrigger = true;
        }
    }
}
