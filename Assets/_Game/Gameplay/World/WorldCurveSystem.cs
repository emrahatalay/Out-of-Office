using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Kıvrımlı dünya: yol koşu boyunca sağa, sola kıvrılıyor ya da düzleşiyor gibi görünür.
    /// Tamamen görseldir; dünya gerçekte düzdür, şeritler ve çarpışmalar değişmez. Bükmeyi
    /// CurvedWorld.hlsl kullanan shader'lar yapar, bu sınıf yalnızca miktarını belirler.
    /// Çıkarılırsa oyun aynen çalışır, yol düz görünür.
    /// </summary>
    public sealed class WorldCurveSystem : ILateTickable, IDisposable
    {
        static readonly int CurveId = Shader.PropertyToID("_OooCurve");
        static readonly int OriginId = Shader.PropertyToID("_OooCurveOrigin");

        // Bükülen nesneler kameranın gerçek görüş alanının dışından içeri kayar; eleme (culling)
        // alanı bu pay kadar geniş tutulur.
        const float CullingMargin = 1.25f;

        readonly Camera _camera;
        readonly RunSession _session;
        readonly GameConfig _config;
        readonly IGameStateProvider _state;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        float _side;          // -1 (sol) .. +1 (sağ), o anki kıvrım
        float _sideVelocity;
        float _targetSide;
        float _nextChangeDistance;

        public WorldCurveSystem(Camera camera, RunSession session, GameConfig config, IGameStateProvider state, ISignalBus bus)
        {
            _camera = camera;
            _session = session;
            _config = config;
            _state = state;

            bus.Subscribe<RunStartedSignal>(_ => Straighten(_config.curveFirstStraightMeters)).AddTo(_subscriptions);
            bus.Subscribe<RunClearedSignal>(_ => Straighten(0f)).AddTo(_subscriptions);
        }

        void Straighten(float untilDistance)
        {
            _targetSide = 0f;
            _nextChangeDistance = untilDistance;
        }

        public void LateTick()
        {
            if (_camera == null) return;

            if (!_config.curveEnabled)
            {
                Shader.SetGlobalVector(CurveId, Vector4.zero);
                _camera.ResetCullingMatrix();
                return;
            }

            if (_state.Current == GameState.Running && _session.Distance >= _nextChangeDistance) PickNext();

            _side = Mathf.SmoothDamp(_side, _targetSide, ref _sideVelocity, _config.curveBlendSeconds, Mathf.Infinity, Time.deltaTime);

            // Kayma uzaklığın karesiyle büyür: curveDistance'ta tam olarak ayarlanan metre kadar.
            float flat = _config.curveFlatDistance;
            float reach = Mathf.Max(1f, _config.curveDistance - flat);
            float perSquareMeter = 1f / (reach * reach);
            float sidePerSquareMeter = _config.curveSideMeters * perSquareMeter;
            float downPerSquareMeter = _config.curveDownMeters * perSquareMeter;

            Shader.SetGlobalVector(CurveId, new Vector4(_side * sidePerSquareMeter, -downPerSquareMeter, flat, 0f));
            Shader.SetGlobalVector(OriginId, _camera.transform.position);

            WidenCulling(sidePerSquareMeter, downPerSquareMeter, flat);
        }

        void PickNext()
        {
            int current = Mathf.Abs(_targetSide) < 0.01f ? 0 : (int)Mathf.Sign(_targetSide);
            int next;
            if (current == 0) next = UnityEngine.Random.value < 0.5f ? -1 : 1;
            else next = UnityEngine.Random.value < _config.curveStraightChance ? 0 : -current;

            _targetSide = next * UnityEngine.Random.Range(_config.curveMinStrength, 1f);
            _nextChangeDistance = _session.Distance + UnityEngine.Random.Range(_config.curveSegmentMeters.x, _config.curveSegmentMeters.y);
        }

        /// <summary>
        /// Unity nesneleri bükülmemiş konumlarına göre eler. Kıvrım, görüş alanının dışındaki
        /// nesneleri içeri taşıdığı için eleme alanı en büyük kayma kadar genişletilir.
        /// </summary>
        void WidenCulling(float sidePerSquareMeter, float downPerSquareMeter, float flat)
        {
            float near = _camera.nearClipPlane;
            float far = _camera.farClipPlane;
            float farReach = Mathf.Max(0f, far - flat);
            float maxShiftPerMeter = farReach * farReach / far * CullingMargin;

            float tanVertical = Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float tanHorizontal = tanVertical * _camera.aspect;
            float right = near * (tanHorizontal + Mathf.Abs(sidePerSquareMeter) * maxShiftPerMeter);
            float top = near * (tanVertical + Mathf.Abs(downPerSquareMeter) * maxShiftPerMeter);

            _camera.cullingMatrix = Matrix4x4.Frustum(-right, right, -top, top, near, far) * _camera.worldToCameraMatrix;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            Shader.SetGlobalVector(CurveId, Vector4.zero);
            if (_camera != null) _camera.ResetCullingMatrix();
        }
    }
}
