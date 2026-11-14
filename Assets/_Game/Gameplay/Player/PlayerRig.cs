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
    /// Karakter prefab'ı yalnızca görseldir. Prefab'da Animator varsa klipler oynar,
    /// yoksa (geçici küp modeller) görsel elle esnetilir; hareket kodu ikisini ayırt etmez.
    /// </summary>
    public sealed class PlayerRig : IPlayerAttachments, IDisposable
    {
        struct Attachment
        {
            public GameObject Instance;
            public Vector3 Offset;
            public PlayerPose Pose;
        }

        readonly Dictionary<GameObject, Attachment> _attachments = new Dictionary<GameObject, Attachment>();
        readonly GameConfig _config;
        readonly VfxLibrary _vfx;
        readonly CapsuleCollider _collider;
        readonly Transform _shadow;
        readonly Renderer _shadowRenderer;
        readonly MaterialPropertyBlock _shadowBlock;
        readonly float _shadowStrength;
        GameObject _visual;
        GameObject _visualPrefab;
        Animator _animator;
        bool _sliding;
        float _stridePhase;
        float _takeoffSpeed;
        int _seatedCount;
        int _hangingCount;
        bool _caught;

        // Karakter Animator'ının parametreleri. Denetleyici: CharacterModelBuilder üretir.
        static readonly int MovingParam = Animator.StringToHash("Moving");
        static readonly int GroundedParam = Animator.StringToHash("Grounded");
        static readonly int SlidingParam = Animator.StringToHash("Sliding");
        static readonly int RunRateParam = Animator.StringToHash("RunRate");
        static readonly int AirPhaseParam = Animator.StringToHash("AirPhase");
        static readonly int PoseParam = Animator.StringToHash("Pose");
        static readonly int CaughtParam = Animator.StringToHash("Caught");
        static readonly int StumbleParam = Animator.StringToHash("Stumble");
        static readonly int ShadowStrengthId = Shader.PropertyToID("_Strength");

        public Transform Root { get; }
        public Transform VisualRoot { get; }
        public TriggerRelay Relay { get; }

        public PlayerRig(GameConfig config, VfxLibrary vfx)
        {
            _config = config;
            _vfx = vfx;

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

            // Damla gölge: köke bağlıdır ama her karede zemine indirilir; zıplarken küçülüp soluklaşır.
            if (vfx.blobShadowPrefab != null)
            {
                _shadow = Object.Instantiate(vfx.blobShadowPrefab, Root, false).transform;
                _shadow.name = "Shadow";
                _shadowRenderer = _shadow.GetComponentInChildren<Renderer>();
                _shadowBlock = new MaterialPropertyBlock();
                _shadowStrength = _shadowRenderer != null && _shadowRenderer.sharedMaterial != null
                    ? _shadowRenderer.sharedMaterial.GetFloat(ShadowStrengthId)
                    : 1f;
            }

            SetSliding(false);
            UpdateShadow();
        }

        /// <summary>Gölgeyi zemine indirir; görsel yükseldikçe gölge küçülür ve soluklaşır.</summary>
        void UpdateShadow()
        {
            if (_shadow == null) return;

            float height = Mathf.Max(0f, Root.position.y + VisualRoot.localPosition.y);
            float t = Mathf.Clamp01(height / _vfx.shadowFadeHeight);
            float size = _vfx.playerShadowSize * Mathf.Lerp(1f, _vfx.airShadowScale, t);

            _shadow.localPosition = new Vector3(0f, -Root.position.y, 0f);
            _shadow.localScale = new Vector3(size, 1f, size);

            if (_shadowRenderer == null) return;
            _shadowBlock.SetFloat(ShadowStrengthId, _shadowStrength * Mathf.Lerp(1f, _vfx.airShadowStrength, t));
            _shadowRenderer.SetPropertyBlock(_shadowBlock);
        }

        public void SetSliding(bool sliding)
        {
            float height = sliding ? _config.slidingHeight : _config.standingHeight;
            _collider.height = height;
            _collider.center = new Vector3(0f, height * 0.5f, 0f);
            _sliding = sliding;
        }

        /// <summary>
        /// Görseli canlandırır. Modelde Animator varsa durumu ona parametre olarak verir
        /// (koşu, zıplama, kayma klipleri); yoksa geçici görseli elle esnetir: koşma
        /// zıplaması, zıplarken uzama, kayarken basılma. Çağıran kod ikisinde de aynıdır.
        /// </summary>
        public void Animate(float deltaTime, float sideSpeed, float verticalSpeed, bool grounded, float runSpeed)
        {
            // Sandalyede ya da dronda yatmaz: takılı görsel köke bağlıdır, gövde tek başına yatarsa ondan ayrılır.
            float lean = CurrentPose != PlayerPose.None ? 0f : Mathf.Clamp(-sideSpeed * 1.4f, -_config.maxLeanDegrees, _config.maxLeanDegrees);

            if (_animator != null)
            {
                DriveAnimator(deltaTime, verticalSpeed, grounded, runSpeed);
                VisualRoot.localPosition = Vector3.zero;
                VisualRoot.localRotation = Quaternion.Euler(0f, 0f, lean);
                VisualRoot.localScale = Vector3.one;
                UpdateShadow();
                return;
            }

            float squash = _sliding ? _config.slidingHeight / _config.standingHeight : 1f;
            float stretch = grounded ? 1f : 1f + Mathf.Clamp(verticalSpeed * 0.012f, -0.1f, 0.16f);

            float bob = 0f;
            if (grounded && !_sliding && runSpeed > 0f)
            {
                _stridePhase += deltaTime * (7f + runSpeed * 0.3f);
                bob = Mathf.Abs(Mathf.Sin(_stridePhase)) * 0.09f;
            }

            float pitch = _sliding ? -8f : (runSpeed > 0f ? 7f : 0f);

            VisualRoot.localPosition = new Vector3(0f, bob, 0f);
            VisualRoot.localRotation = Quaternion.Euler(pitch, 0f, lean);
            VisualRoot.localScale = new Vector3(1f / Mathf.Sqrt(stretch), squash * stretch, 1f / Mathf.Sqrt(stretch));
            UpdateShadow();
        }

        void DriveAnimator(float deltaTime, float verticalSpeed, bool grounded, float runSpeed)
        {
            // Havadaki klip süreyle değil yükseklik eğrisiyle ilerler: kalkışta 0, tepede 0.5,
            // inişte 1. Böylece zıplama çarpanı, hızlı iniş ve uçuş (tepe pozu) kendiliğinden uyar.
            float airPhase = 0f;
            if (grounded)
            {
                _takeoffSpeed = 0f;
            }
            else
            {
                _takeoffSpeed = Mathf.Max(_takeoffSpeed, Mathf.Abs(verticalSpeed));
                airPhase = _takeoffSpeed > 0f ? Mathf.Clamp01(0.5f - 0.5f * verticalSpeed / _takeoffSpeed) : 0.5f;
            }

            float runRate = Mathf.Clamp(runSpeed / _config.runAnimationReferenceSpeed, _config.runAnimationMinRate, _config.runAnimationMaxRate);

            _animator.SetBool(MovingParam, runSpeed > 0f);
            _animator.SetBool(GroundedParam, grounded);
            _animator.SetBool(SlidingParam, _sliding && grounded);
            _animator.SetFloat(RunRateParam, runRate);
            _animator.SetFloat(AirPhaseParam, airPhase, _config.animationDampTime, deltaTime);
            _animator.SetInteger(PoseParam, (int)CurrentPose);
            _animator.SetBool(CaughtParam, _caught);
        }

        /// <summary>Takılı görsellerin istediği duruş. Asılma oturmayı ezer (dron sandalyeyi de kaldırır).</summary>
        PlayerPose CurrentPose => _hangingCount > 0 ? PlayerPose.Hanging : _seatedCount > 0 ? PlayerPose.Seated : PlayerPose.None;

        /// <summary>Engele çarpıp sendeleme. Animator'ı olmayan görselde etkisizdir.</summary>
        public void Stumble()
        {
            // Otururken ya da asılıyken sendelemez; tetik sonraya kalıp yersiz oynamasın.
            if (_animator != null && !_caught && CurrentPose == PlayerPose.None) _animator.SetTrigger(StumbleParam);
        }

        /// <summary>
        /// Yakalanma duruşu. Havadayken yakalanırsa görsel yere indirilir; koşu sürmediği için
        /// konumu oynanış değil bu çağrı belirler.
        /// </summary>
        public void SetCaught(bool caught)
        {
            _caught = caught;
            if (_animator == null) return;

            _animator.SetBool(CaughtParam, caught);
            if (caught)
            {
                _animator.ResetTrigger(StumbleParam);
                VisualRoot.localPosition = new Vector3(0f, -Root.position.y, 0f);
            }

            UpdateShadow();
        }

        public void SetVisual(GameObject prefab)
        {
            if (prefab == _visualPrefab) return;
            _visualPrefab = prefab;

            if (_visual != null) Object.Destroy(_visual);
            _animator = null;
            if (prefab == null) return;

            _visual = Object.Instantiate(prefab, VisualRoot, false);
            _animator = _visual.GetComponentInChildren<Animator>();
            if (_animator != null)
            {
                _animator.SetInteger(PoseParam, (int)CurrentPose);
                _animator.SetBool(CaughtParam, _caught);
            }

            // Karakter değişti: takılı sandalye ya da dron yeni boya göre yeniden ölçülür.
            foreach (var attachment in _attachments.Values) Fit(attachment);
            foreach (var collider in _visual.GetComponentsInChildren<Collider>()) collider.enabled = false;
        }

        public void Attach(GameObject prefab, Vector3 localOffset, PlayerPose pose = PlayerPose.None)
        {
            if (prefab == null || _attachments.ContainsKey(prefab)) return;

            var instance = Object.Instantiate(prefab, Root, false);
            foreach (var collider in instance.GetComponentsInChildren<Collider>()) collider.enabled = false;

            var attachment = new Attachment { Instance = instance, Offset = localOffset, Pose = pose };
            _attachments.Add(prefab, attachment);
            Fit(attachment);
            CountPose(pose, 1);
        }

        public void Detach(GameObject prefab)
        {
            if (prefab == null || !_attachments.TryGetValue(prefab, out var attachment)) return;

            _attachments.Remove(prefab);
            CountPose(attachment.Pose, -1);
            if (attachment.Instance != null) Object.Destroy(attachment.Instance);
        }

        /// <summary>
        /// Duruş veren görsel (sandalye, dron) karakterin boyuna göre ölçülür: oturma ve asılma
        /// klipleri karakterle birlikte ölçeklendiği için oturağın ve tutamağın da ölçeklenmesi gerekir.
        /// </summary>
        void Fit(Attachment attachment)
        {
            if (attachment.Instance == null) return;

            float scale = attachment.Pose != PlayerPose.None && _animator != null
                ? _animator.humanScale / _config.poseReferenceHumanScale
                : 1f;
            attachment.Instance.transform.localPosition = attachment.Offset * scale;
            attachment.Instance.transform.localScale = Vector3.one * scale;
        }

        void CountPose(PlayerPose pose, int delta)
        {
            if (pose == PlayerPose.Seated) _seatedCount = Mathf.Max(0, _seatedCount + delta);
            else if (pose == PlayerPose.Hanging) _hangingCount = Mathf.Max(0, _hangingCount + delta);
        }

        public void Dispose()
        {
            if (Root != null) Object.Destroy(Root.gameObject);
        }
    }
}
