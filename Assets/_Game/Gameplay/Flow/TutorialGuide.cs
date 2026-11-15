using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Data;
using UnityEngine;
using VContainer.Unity;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Öğreticinin 3B oku. Oyuncunun önünde, gösterdiği yöne doğru gidip gelerek ve nabız gibi
    /// atarak durur. Yalnızca görsel; ne zaman çıkacağını <see cref="TutorialHintSignal"/> söyler.
    /// Ok yükleme anında bir kez üretilir (koşu sırasında Instantiate yok), gerisi aç/kapa.
    /// </summary>
    public sealed class TutorialGuide : IPostStartable, ITickable, IDisposable
    {
        const float BobSpeed = 7f;
        const float BobDistance = 0.35f;
        const float PulseAmount = 0.1f;

        readonly ContentCatalog _catalog;
        readonly GameConfig _config;
        readonly PlayerStatus _player;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        GameObject _arrow;
        TutorialCue _cue;

        public TutorialGuide(ContentCatalog catalog, GameConfig config, PlayerStatus player, ISignalBus bus)
        {
            _catalog = catalog;
            _config = config;
            _player = player;
            _bus = bus;
        }

        public void PostStart()
        {
            if (_catalog.tutorialArrow != null)
            {
                _arrow = UnityEngine.Object.Instantiate(_catalog.tutorialArrow);
                _arrow.name = "TutorialArrow";
                _arrow.SetActive(false);
            }

            _bus.Subscribe<TutorialHintSignal>(OnHint).AddTo(_subscriptions);
        }

        void OnHint(TutorialHintSignal signal)
        {
            _cue = signal.Cue;
            if (_arrow != null) _arrow.SetActive(_cue != TutorialCue.None);
        }

        public void Tick()
        {
            if (_arrow == null || _cue == TutorialCue.None) return;

            // Oyun yavaşlarken de akıcı oynasın diye zaman ölçeğinden bağımsız.
            float time = Time.unscaledTime;
            var player = _player.Position;
            float z = player.z + _config.tutorialArrowAhead;
            float height = _config.tutorialArrowHeight;
            float side = _config.tutorialArrowSide;

            Vector3 position;
            float angle;
            switch (_cue)
            {
                case TutorialCue.Left:
                    position = new Vector3(player.x - side, height, z);
                    angle = 180f;
                    break;
                case TutorialCue.Right:
                    position = new Vector3(player.x + side, height, z);
                    angle = 0f;
                    break;
                case TutorialCue.Up:
                    position = new Vector3(player.x, height, z);
                    angle = 90f;
                    break;
                default:
                    position = new Vector3(player.x, height + 0.9f, z);
                    angle = -90f;
                    break;
            }

            var rotation = Quaternion.Euler(0f, 0f, angle);
            position += rotation * Vector3.right * (Mathf.Sin(time * BobSpeed) * BobDistance);

            var transform = _arrow.transform;
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one * (1f + Mathf.Sin(time * BobSpeed * 0.5f) * PulseAmount);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            if (_arrow != null) UnityEngine.Object.Destroy(_arrow);
        }
    }
}
