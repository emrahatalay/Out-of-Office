using System;
using System.Collections.Generic;
using OutOfOffice.Core.Run;
using OutOfOffice.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Oyuncunun sahnedeki gövdesi: çarpışma kapsülü + değiştirilebilir görsel.
    /// Karakter prefab'ı yalnızca görseldir; geçici mesh yerine gerçek model
    /// konduğunda bu sınıfta ve hareket kodunda hiçbir şey değişmez.
    /// </summary>
    public sealed class PlayerRig : IPlayerAttachments, IDisposable
    {
        readonly Dictionary<GameObject, GameObject> _attachments = new Dictionary<GameObject, GameObject>();
        readonly GameConfig _config;
        readonly CapsuleCollider _collider;
        GameObject _visual;
        GameObject _visualPrefab;
        bool _sliding;
        float _stridePhase;

        public Transform Root { get; }
        public Transform VisualRoot { get; }
        public TriggerRelay Relay { get; }

        public PlayerRig(GameConfig config)
        {
            _config = config;

            var root = new GameObject("Player");
            Root = root.transform;

            var body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            _collider = root.AddComponent<CapsuleCollider>();
            _collider.isTrigger = true;
            _collider.radius = config.bodyRadius;

            Relay = root.AddComponent<TriggerRelay>();

            VisualRoot = new GameObject("Visual").transform;
            VisualRoot.SetParent(Root, false);

            SetSliding(false);
        }

        public void SetSliding(bool sliding)
        {
            float height = sliding ? _config.slidingHeight : _config.standingHeight;
            _collider.height = height;
            _collider.center = new Vector3(0f, height * 0.5f, 0f);
            _sliding = sliding;
        }

        /// <summary>
        /// Geçici görseli canlandırır: koşma zıplaması, şerit değiştirirken yatma, zıplarken
        /// uzama, kayarken basılma. Gerçek modele geçildiğinde bu değerler Animator
        /// parametrelerine bağlanır; çağıran kod değişmez.
        /// </summary>
        public void Animate(float deltaTime, float sideSpeed, float verticalSpeed, bool grounded, float runSpeed)
        {
            float squash = _sliding ? _config.slidingHeight / _config.standingHeight : 1f;
            float stretch = grounded ? 1f : 1f + Mathf.Clamp(verticalSpeed * 0.012f, -0.1f, 0.16f);

            float bob = 0f;
            if (grounded && !_sliding && runSpeed > 0f)
            {
                _stridePhase += deltaTime * (7f + runSpeed * 0.3f);
                bob = Mathf.Abs(Mathf.Sin(_stridePhase)) * 0.09f;
            }

            float lean = Mathf.Clamp(-sideSpeed * 1.4f, -_config.maxLeanDegrees, _config.maxLeanDegrees);
            float pitch = _sliding ? -8f : (runSpeed > 0f ? 7f : 0f);

            VisualRoot.localPosition = new Vector3(0f, bob, 0f);
            VisualRoot.localRotation = Quaternion.Euler(pitch, 0f, lean);
            VisualRoot.localScale = new Vector3(1f / Mathf.Sqrt(stretch), squash * stretch, 1f / Mathf.Sqrt(stretch));
        }

        public void SetVisual(GameObject prefab)
        {
            if (prefab == _visualPrefab) return;
            _visualPrefab = prefab;

            if (_visual != null) Object.Destroy(_visual);
            if (prefab == null) return;

            _visual = Object.Instantiate(prefab, VisualRoot, false);
            foreach (var collider in _visual.GetComponentsInChildren<Collider>()) collider.enabled = false;
        }

        public void Attach(GameObject prefab, Vector3 localOffset)
        {
            if (prefab == null || _attachments.ContainsKey(prefab)) return;

            var instance = Object.Instantiate(prefab, Root, false);
            instance.transform.localPosition = localOffset;
            foreach (var collider in instance.GetComponentsInChildren<Collider>()) collider.enabled = false;
            _attachments.Add(prefab, instance);
        }

        public void Detach(GameObject prefab)
        {
            if (prefab == null || !_attachments.TryGetValue(prefab, out var instance)) return;

            _attachments.Remove(prefab);
            if (instance != null) Object.Destroy(instance);
        }

        public void Dispose()
        {
            if (Root != null) Object.Destroy(Root.gameObject);
        }
    }
}
