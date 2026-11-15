using OutOfOffice.Data;
using UnityEngine;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Sahnedeki bir engelin durumu. Prefab'a elle eklenmesi gerekmez; ObstacleField
    /// üretirken ekler. Collider'ı olmayan prefab'a görsel sınırlarına göre kutu eklenir.
    /// </summary>
    public sealed class ObstacleView : MonoBehaviour
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly Color GhostTint = new Color(0.55f, 0.95f, 1f, 1f);
        static MaterialPropertyBlock _block;

        Renderer[] _renderers;
        Collider[] _colliders;
        Transform[] _spinTargets;
        Vector3[] _spinAxes;
        float[] _spinSpeeds;
        bool _initialized;

        public ObstacleDefinition Definition { get; private set; }
        public Vector3 Origin { get; private set; }
        public float Age { get; set; }
        public bool IsGhost { get; private set; }

        public void Bind(ObstacleDefinition definition, Vector3 origin)
        {
            Definition = definition;
            Origin = origin;
            Age = 0f;

            if (_initialized) return;
            _initialized = true;

            CollectSpinParts(definition);
            _renderers = GetComponentsInChildren<Renderer>();
            _colliders = GetComponentsInChildren<Collider>();
            if (_colliders.Length == 0) _colliders = new Collider[] { AddBoundsCollider() };
            foreach (var collider in _colliders) collider.isTrigger = true;
        }

        /// <summary>Dönen parçaları (pervane, tekerlek) bir adım ilerletir. Parça yoksa hiçbir şey yapmaz.</summary>
        public void AdvanceSpin(float deltaTime)
        {
            if (_spinTargets == null) return;

            for (int i = 0; i < _spinTargets.Length; i++)
                _spinTargets[i].Rotate(_spinAxes[i], _spinSpeeds[i] * deltaTime, Space.Self);
        }

        // Tanımdaki adlara uyan çocukları bir kez toplar; koşu sırasında arama ya da bellek ayırma yok.
        void CollectSpinParts(ObstacleDefinition definition)
        {
            var spec = definition.spinParts;
            if (spec == null || spec.Length == 0) return;

            var targets = new System.Collections.Generic.List<Transform>();
            var axes = new System.Collections.Generic.List<Vector3>();
            var speeds = new System.Collections.Generic.List<float>();
            foreach (var child in GetComponentsInChildren<Transform>())
            {
                if (child == transform) continue;
                for (int i = 0; i < spec.Length; i++)
                {
                    if (string.IsNullOrEmpty(spec[i].childName) || !child.name.StartsWith(spec[i].childName)) continue;
                    targets.Add(child);
                    axes.Add(spec[i].axis.normalized);
                    speeds.Add(spec[i].degreesPerSecond);
                    break;
                }
            }

            if (targets.Count == 0) return;
            _spinTargets = targets.ToArray();
            _spinAxes = axes.ToArray();
            _spinSpeeds = speeds.ToArray();
        }

        public void SetGhost(bool ghost)
        {
            IsGhost = ghost;
            foreach (var collider in _colliders) collider.enabled = !ghost;

            if (ghost)
            {
                _block ??= new MaterialPropertyBlock();
                _block.SetColor(BaseColor, GhostTint);
            }

            foreach (var renderer in _renderers) renderer.SetPropertyBlock(ghost ? _block : null);
        }

        BoxCollider AddBoundsCollider()
        {
            var box = gameObject.AddComponent<BoxCollider>();
            if (_renderers.Length == 0) return box;

            var bounds = _renderers[0].bounds;
            for (int i = 1; i < _renderers.Length; i++) bounds.Encapsulate(_renderers[i].bounds);

            box.center = transform.InverseTransformPoint(bounds.center);
            box.size = bounds.size;
            return box;
        }
    }
}
