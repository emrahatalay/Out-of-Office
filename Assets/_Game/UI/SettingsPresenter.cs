using System;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Menünün Ayarlar sekmesi: dil, ses, görüntü kalitesi ve geliştirici kartı. Butonlar yalnızca
    /// istek sinyali atar; seçili seçeneği sonuç sinyallerinden (dil değişti, ayarlar değişti) öğrenir.
    /// Yeni seçenek = UXML'e bir buton, aşağıdaki listeye bir satır.
    /// </summary>
    public sealed class SettingsPresenter : IStartable, IDisposable
    {
        const string ActiveClass = "segment--active";

        static readonly (string button, string code)[] Languages =
        {
            ("languageTr", "tr"),
            ("languageEn", "en"),
        };

        static readonly (string button, GraphicsQuality quality)[] Qualities =
        {
            ("qualityLow", GraphicsQuality.Low),
            ("qualityMedium", GraphicsQuality.Medium),
            ("qualityHigh", GraphicsQuality.High),
        };

        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly ILocalizationService _localization;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly Button[] _languageButtons = new Button[Languages.Length];
        readonly Button[] _qualityButtons = new Button[Qualities.Length];
        Button _soundOn;
        Button _soundOff;

        public SettingsPresenter(UIDocument document, ISignalBus bus, ILocalizationService localization)
        {
            _document = document;
            _bus = bus;
            _localization = localization;
        }

        public void Start()
        {
            var page = _document.rootVisualElement.Q("settingsPage");

            for (int i = 0; i < Languages.Length; i++)
            {
                string code = Languages[i].code;
                _languageButtons[i] = page.Q<Button>(Languages[i].button);
                _languageButtons[i].clicked += () => _bus.Fire(new LanguageSelectRequestedSignal(code));
            }

            for (int i = 0; i < Qualities.Length; i++)
            {
                var quality = Qualities[i].quality;
                _qualityButtons[i] = page.Q<Button>(Qualities[i].button);
                _qualityButtons[i].clicked += () => _bus.Fire(new QualitySetRequestedSignal(quality));
            }

            _soundOn = page.Q<Button>("soundOn");
            _soundOff = page.Q<Button>("soundOff");
            _soundOn.clicked += () => _bus.Fire(new SoundSetRequestedSignal(true));
            _soundOff.clicked += () => _bus.Fire(new SoundSetRequestedSignal(false));
            page.Q<Button>("developerButton").clicked += () => _bus.Fire<DeveloperPageRequestedSignal>();
            page.Q<Label>("versionLabel").text = "v" + Application.version;

            _bus.Subscribe<SettingsChangedSignal>(OnSettingsChanged).AddTo(_subscriptions);
            _bus.Subscribe<LocaleChangedSignal>(s => ShowLanguage(s.Code)).AddTo(_subscriptions);
            ShowLanguage(_localization.CurrentCode);
        }

        void OnSettingsChanged(SettingsChangedSignal signal)
        {
            _soundOn.EnableInClassList(ActiveClass, signal.SoundOn);
            _soundOff.EnableInClassList(ActiveClass, !signal.SoundOn);

            for (int i = 0; i < Qualities.Length; i++)
                _qualityButtons[i].EnableInClassList(ActiveClass, Qualities[i].quality == signal.Quality);
        }

        // Dil kodu bölge içerebilir ("tr-TR"); dilin kendisine bakılır.
        void ShowLanguage(string code)
        {
            for (int i = 0; i < Languages.Length; i++)
            {
                bool active = !string.IsNullOrEmpty(code) && code.StartsWith(Languages[i].code, StringComparison.OrdinalIgnoreCase);
                _languageButtons[i].EnableInClassList(ActiveClass, active);
            }
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
