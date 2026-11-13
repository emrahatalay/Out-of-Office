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
            _subscription = _bus.Subscribe<LanguageCycleRequestedSignal>(_ => CycleLocale());
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

        void CycleLocale()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (locales.Count == 0) return;

            int index = locales.IndexOf(LocalizationSettings.SelectedLocale);
            LocalizationSettings.SelectedLocale = locales[(index + 1) % locales.Count];
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
