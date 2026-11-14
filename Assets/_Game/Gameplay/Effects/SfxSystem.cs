using System;
using OutOfOffice.Core.Signals;
using OutOfOffice.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace OutOfOffice.Gameplay
{
    /// <summary>
    /// Oyun olaylarını ses efektine çevirir. Hangi olayda ne çalacağı SfxLibrary asset'inde
    /// yazar. Sesler açılışta kurulan sabit sayıda kaynaktan sırayla çalar; koşu sırasında nesne
    /// üretilmez. Yalnızca sinyal dinler: çıkarılsa oyun aynen çalışır.
    /// </summary>
    public sealed class SfxSystem : IDisposable
    {
        readonly SfxLibrary _library;
        readonly GameObject _host;
        readonly AudioSource[] _voices;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        int _nextVoice;
        float _lastCoinAt = float.NegativeInfinity;
        float _coinPitch = 1f;

        public SfxSystem(SfxLibrary library, ISignalBus bus)
        {
            _library = library;

            _host = new GameObject("[Sfx]");
            _voices = new AudioSource[Mathf.Max(1, library.voices)];
            for (int i = 0; i < _voices.Length; i++)
            {
                var voice = _host.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.spatialBlend = 0f; // 2B: konumdan bağımsız, hep aynı düzeyde
                _voices[i] = voice;
            }

            bus.Subscribe<PlayerJumpedSignal>(_ => Play(SfxEvent.Jumped)).AddTo(_subscriptions);
            bus.Subscribe<PlayerSlidSignal>(_ => Play(SfxEvent.Slid)).AddTo(_subscriptions);
            bus.Subscribe<LaneChangedSignal>(_ => Play(SfxEvent.LaneChanged)).AddTo(_subscriptions);
            bus.Subscribe<CoinCollectedSignal>(_ => PlayCoin()).AddTo(_subscriptions);
            bus.Subscribe<PowerUpCollectedSignal>(_ => Play(SfxEvent.PowerUpCollected)).AddTo(_subscriptions);
            bus.Subscribe<PackageCollectedSignal>(_ => Play(SfxEvent.PowerUpCollected)).AddTo(_subscriptions);
            bus.Subscribe<PlayerStumbledSignal>(_ => Play(SfxEvent.Stumbled)).AddTo(_subscriptions);
            bus.Subscribe<ShieldBrokenSignal>(_ => Play(SfxEvent.ShieldBroken)).AddTo(_subscriptions);
            bus.Subscribe<PlayerCaughtSignal>(_ => Play(SfxEvent.Caught)).AddTo(_subscriptions);

            // Arayüz Gameplay'i tanımaz; düğmelerin attığı istek sinyalleri "tık" sesini tetikler.
            bus.Subscribe<StartRunRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<PauseRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<PauseToggleRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<ResumeRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<ReturnToMenuRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<LanguageSelectRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<SoundSetRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<QualitySetRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<DeveloperPageRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<CharacterSelectRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<CharacterCycleRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<StorePurchaseRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<AccessoryBuyRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<BuybackRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
            bus.Subscribe<ReviveRequestedSignal>(_ => Play(SfxEvent.UiClicked)).AddTo(_subscriptions);
        }

        /// <summary>Art arda toplanan altınların sesi her seferinde biraz incelir; seri kopunca başa döner.</summary>
        void PlayCoin()
        {
            // Ağır çekimde de seri doğru sayılsın diye ölçeklenmemiş zaman kullanılır.
            float now = Time.unscaledTime;
            bool streak = now - _lastCoinAt <= _library.coinStreakWindow;
            _coinPitch = streak ? Mathf.Min(_coinPitch + _library.coinStreakPitchStep, _library.coinStreakMaxPitch) : 1f;
            _lastCoinAt = now;
            Play(SfxEvent.CoinCollected, _coinPitch);
        }

        void Play(SfxEvent trigger, float pitch = 1f)
        {
            if (!_library.TryGet(trigger, out var entry)) return;

            var voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;

            voice.clip = entry.clip;
            voice.volume = entry.volume * _library.masterVolume;
            voice.pitch = pitch + Random.Range(-entry.pitchVariation, entry.pitchVariation);
            voice.Play();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            if (_host != null) Object.Destroy(_host);
        }
    }
}
