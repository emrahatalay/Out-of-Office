using System;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>Öğretici ipucunu HUD'da gösterir. Hangi ipucunun ne zaman çıkacağı oyun tarafında belirlenir.</summary>
    public sealed class TutorialPresenter : IStartable, IDisposable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        VisualElement _hint;
        Label _text;

        public TutorialPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var screen = _document.rootVisualElement.Q("hudScreen");
            _hint = screen.Q("tutorialHint");
            _text = screen.Q<Label>("tutorialText");

            _bus.Subscribe<TutorialHintSignal>(OnHint).AddTo(_subscriptions);
        }

        void OnHint(TutorialHintSignal signal)
        {
            bool visible = !string.IsNullOrEmpty(signal.TextKey);
            _text.Localize(signal.TextKey);
            _hint.EnableInClassList(ScreenRouter.HiddenClass, !visible);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
