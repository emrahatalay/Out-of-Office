using System;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>Yeni bir dünyaya girilince adını HUD'da kısa süre gösterir.</summary>
    public sealed class WorldBannerPresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        VisualElement _banner;
        Label _name;
        IVisualElementScheduledItem _hide;

        public WorldBannerPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var screen = _document.rootVisualElement.Q("hudScreen");
            _banner = screen.Q("worldBanner");
            _name = screen.Q<Label>("worldBannerName");

            _bus.Subscribe<WorldChangedSignal>(OnWorldChanged).AddTo(_subscriptions);
            // Koşu bitince ya da yeniden başlayınca afiş kalmasın.
            _bus.Subscribe<RunStartedSignal>(_ => Hide()).AddTo(_subscriptions);
            _bus.Subscribe<RunClearedSignal>(_ => Hide()).AddTo(_subscriptions);
        }

        void OnWorldChanged(WorldChangedSignal signal)
        {
            _name.Localize(signal.NameKey);
            _banner.RemoveFromClassList(ScreenRouter.HiddenClass);

            _hide?.Pause();
            _hide = _banner.schedule.Execute(Hide).StartingIn((long)(signal.BannerSeconds * 1000f));
        }

        void Hide()
        {
            _hide?.Pause();
            _banner.AddToClassList(ScreenRouter.HiddenClass);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
