using System;
using OutOfOffice.Core.Services;
using OutOfOffice.Core.Signals;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VContainer.Unity;

namespace OutOfOffice.Infrastructure
{
    /// <summary>
    /// Unity Localization paketinin ince sarmalayıcısı. Dil durumunu kendisi tutmaz:
    /// seçili dil her zaman LocalizationSettings.SelectedLocale'dir ve paket tarafından kaydedilir.
    /// </summary>
    public sealed class UnityLocalizationService : ILocalizationService, IInitializable, IDisposable
    {
        public const string Table = "Game";

        readonly ISignalBus _bus;
        readonly IDisposable _subscription;

        public UnityLocalizationService(ISignalBus bus)
        {
            _bus = bus;
            _subscription = _bus.Subscribe<LanguageSelectRequestedSignal>(s => SelectLocale(s.Code));
        }

        public string CurrentCode
        {
            get
            {
                var locale = LocalizationSettings.SelectedLocale;
                return locale != null ? locale.Identifier.Code : string.Empty;
            }
        }

        public void Initialize()
        {
            LocalizationSettings.InitializationOperation.WaitForCompletion();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return LocalizationSettings.StringDatabase.GetLocalizedString(Table, key);
        }

        void SelectLocale(string code)
        {
            if (string.IsNullOrEmpty(code)) return;

            var locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(code));
            if (locale != null && locale != LocalizationSettings.SelectedLocale) LocalizationSettings.SelectedLocale = locale;
        }

        void OnLocaleChanged(Locale locale)
        {
            _bus.Fire(new LocaleChangedSignal(locale != null ? locale.Identifier.Code : string.Empty));
        }

        public void Dispose()
        {
            _subscription.Dispose();
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
    }
}
