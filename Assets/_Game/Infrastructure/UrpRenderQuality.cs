using System;
using OutOfOffice.Core.Services;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OutOfOffice.Infrastructure
{
    /// <summary>
    /// <see cref="IRenderQuality"/>'nin URP karşılığı: render ölçeği etkin URP asset'ine,
    /// post-processing oyun kamerasına yazılır. Asset proje dosyası olduğu için (editörde
    /// Play'den sonra değişik kalmasın) özgün ölçek kapanışta geri konur.
    /// </summary>
    public sealed class UrpRenderQuality : IRenderQuality, IDisposable
    {
        readonly Camera _camera;
        UniversalRenderPipelineAsset _asset;
        float _originalScale;

        public UrpRenderQuality(Camera camera)
        {
            _camera = camera;
        }

        public void Apply(float renderScale, bool postProcessing)
        {
            var asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset != null)
            {
                if (asset != _asset)
                {
                    Restore();
                    _asset = asset;
                    _originalScale = asset.renderScale;
                }

                asset.renderScale = renderScale;
            }

            if (_camera != null) _camera.GetUniversalAdditionalCameraData().renderPostProcessing = postProcessing;
        }

        void Restore()
        {
            if (_asset != null) _asset.renderScale = _originalScale;
            _asset = null;
        }

        public void Dispose()
        {
            Restore();
        }
    }
}
