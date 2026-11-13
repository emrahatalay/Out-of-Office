using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace OutOfOffice.Infrastructure
{
    /// <summary>
    /// Klavye ve dokunma/fare kaydırmasını oyun sinyallerine çevirir.
    /// Oyunun geri kalanı girdinin nereden geldiğini bilmez.
    /// </summary>
    public sealed class InputReader : ITickable
    {
        const float SwipeScreenFraction = 0.04f;
        const float MinSwipePixels = 24f;

        readonly ISignalBus _bus;
        Vector2 _pressPosition;
        bool _tracking;

        public InputReader(ISignalBus bus)
        {
            _bus = bus;
        }

        public void Tick()
        {
            ReadKeyboard();
            ReadSwipe();
        }

        void ReadKeyboard()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) Move(MoveDirection.Left);
            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) Move(MoveDirection.Right);
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) Move(MoveDirection.Up);
            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) Move(MoveDirection.Down);
            if (keyboard.spaceKey.wasPressedThisFrame) _bus.Fire<AbilityRequestedSignal>();
            if (keyboard.escapeKey.wasPressedThisFrame) _bus.Fire<PauseToggleRequestedSignal>();
        }

        void ReadSwipe()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                _pressPosition = pointer.position.ReadValue();
                _tracking = true;
                return;
            }

            if (!pointer.press.isPressed)
            {
                _tracking = false;
                return;
            }

            if (!_tracking) return;

            var delta = pointer.position.ReadValue() - _pressPosition;
            float threshold = Mathf.Max(MinSwipePixels, Screen.height * SwipeScreenFraction);
            if (delta.magnitude < threshold) return;

            // Bir basışta tek kaydırma sayılır.
            _tracking = false;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) Move(delta.x > 0f ? MoveDirection.Right : MoveDirection.Left);
            else Move(delta.y > 0f ? MoveDirection.Up : MoveDirection.Down);
        }

        void Move(MoveDirection direction)
        {
            _bus.Fire(new MoveInputSignal(direction));
        }
    }
}
