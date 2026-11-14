using System;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Ayarlar sekmesinin oyuncu tercihleri: ses ve görüntü kalitesi. Seçimi kayda yazar ve
    /// motora uygular; kalitenin sayısal karşılığı GameConfig'tedir, nasıl uygulanacağını
    /// <see cref="IRenderQuality"/> bilir. Dil seçimi çeviri servisindedir (paket kendisi kaydeder).
    /// </summary>
    public sealed class SettingsSystem : IInitializable, IPostStartable, IDisposable
    {
        const string SoundKey = "sound";
        const string QualityKey = "quality";

        readonly GameConfig _config;
        readonly ISaveService _save;
        readonly IRenderQuality _render;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        public bool SoundOn { get; private set; } = true;
        public GraphicsQuality Quality { get; private set; }

        public SettingsSystem(GameConfig config, ISaveService save, IRenderQuality render, ISignalBus bus)
        {
            _config = config;
            _save = save;
            _render = render;
            _bus = bus;

            _bus.Subscribe<SoundSetRequestedSignal>(s => SetSound(s.On)).AddTo(_subscriptions);
            _bus.Subscribe<QualitySetRequestedSignal>(s => SetQuality(s.Quality)).AddTo(_subscriptions);
            _bus.Subscribe<DeveloperPageRequestedSignal>(_ => OpenDeveloperPage()).AddTo(_subscriptions);
        }

        // Kalite ilk karede uygulansın diye kayıt Initialize'da okunur; arayüz Start'ta
        // abone olduğu için sinyal PostStart'ta atılır.
        public void Initialize()
        {
            SoundOn = _save.GetInt(SoundKey, 1) != 0;
            int quality = _save.GetInt(QualityKey, (int)_config.defaultQuality);
            Quality = (GraphicsQuality)Mathf.Clamp(quality, (int)GraphicsQuality.Low, (int)GraphicsQuality.High);

            ApplySound();
            ApplyQuality();
        }

        public void PostStart()
        {
            Notify();
        }

        void SetSound(bool on)
        {
            if (SoundOn == on) return;
            SoundOn = on;
            _save.SetInt(SoundKey, on ? 1 : 0);
            _save.Flush();
            ApplySound();
            Notify();
        }

        void SetQuality(GraphicsQuality quality)
        {
            if (Quality == quality) return;
            Quality = quality;
            _save.SetInt(QualityKey, (int)quality);
            _save.Flush();
            ApplyQuality();
            Notify();
        }

        void ApplySound()
        {
            AudioListener.volume = SoundOn ? 1f : 0f;
        }

        void ApplyQuality()
        {
            switch (Quality)
            {
                case GraphicsQuality.Low:
                    _render.Apply(_config.renderScaleLow, _config.postProcessingOnLow);
                    break;
                case GraphicsQuality.Medium:
                    _render.Apply(_config.renderScaleMedium, true);
                    break;
                default:
                    _render.Apply(_config.renderScaleHigh, true);
                    break;
            }
        }

        void OpenDeveloperPage()
        {
            if (!string.IsNullOrEmpty(_config.developerUrl)) Application.OpenURL(_config.developerUrl);
        }

        void Notify()
        {
            _bus.Fire(new SettingsChangedSignal(SoundOn, Quality));
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
