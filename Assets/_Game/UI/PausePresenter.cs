using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    public sealed class PausePresenter : IStartable
    {
        readonly UIDocument _document;
        readonly ISignalBus _bus;

        public PausePresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var screen = _document.rootVisualElement.Q("pauseScreen");
            screen.Q<Button>("resumeButton").clicked += () => _bus.Fire<ResumeRequestedSignal>();
            screen.Q<Button>("pauseMenuButton").clicked += () => _bus.Fire<ReturnToMenuRequestedSignal>();
        }
    }
}
