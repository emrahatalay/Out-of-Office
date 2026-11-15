using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Core.State;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>Menünün alt sekme çubuğu: Ana Sayfa, Koleksiyon, Mağaza, Ayarlar. Yeni sekme = listeye bir satır.</summary>
    public sealed class MenuTabsPresenter : IStartable, IDisposable
    {
        const string ActiveClass = "tab--active";

        static readonly (string tab, string page)[] Tabs =
        {
            ("homeTab", "homePage"),
            ("collectionTab", "collectionPage"),
            ("storeTab", "storePage"),
            ("settingsTab", "settingsPage"),
        };

        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly Button[] _tabs = new Button[Tabs.Length];
        readonly VisualElement[] _pages = new VisualElement[Tabs.Length];
        IDisposable _subscription;

        public MenuTabsPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var screen = _document.rootVisualElement.Q("menuScreen");
            for (int i = 0; i < Tabs.Length; i++)
            {
                int index = i;
                _tabs[i] = screen.Q<Button>(Tabs[i].tab);
                _pages[i] = screen.Q(Tabs[i].page);
                _tabs[i].clicked += () => { Select(index); _bus.Fire<MenuTabClickedSignal>(); };
            }

            Select(0);
            // Menüye her dönüşte ana sayfa açılır.
            _subscription = _bus.Subscribe<GameStateChangedSignal>(s => { if (s.To == GameState.Menu) Select(0); });
        }

        public void Select(int index)
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                _tabs[i].EnableInClassList(ActiveClass, i == index);
                _pages[i].EnableInClassList(ScreenRouter.HiddenClass, i != index);
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
