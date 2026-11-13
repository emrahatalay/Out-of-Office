using System;
using OutOfOffice.Core.Signals;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    public sealed class HudPresenter : IStartable, ITickable, IDisposable
    {
        const string PopClass = "pop";
        const float ScoreRefreshSeconds = 0.1f;

        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();

        Label _scoreValue;
        Label _coinValue;
        VisualElement _coinChip;
        Button _abilityButton;
        RadialProgress _abilityRing;
        Label _abilityName;
        VisualElement _effectBadge;
        Label _effectName;
        Label _effectTime;
        Action _endCoinPop;

        int _score;
        int _scoreShown = -1;
        float _nextScoreRefresh;
        float _abilityCooldown;
        float _abilityReadyAt;
        bool _abilityReady = true;
        string _effectId;
        float _effectEndsAt;
        int _effectShown = -1;

        public HudPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var screen = _document.rootVisualElement.Q("hudScreen");

            _scoreValue = screen.Q<Label>("scoreValue");
            _coinValue = screen.Q<Label>("coinValue");
            _coinChip = screen.Q("coinChip");
            _abilityButton = screen.Q<Button>("abilityButton");
            _abilityRing = screen.Q<RadialProgress>("abilityRing");
            _abilityName = screen.Q<Label>("abilityName");
            _effectBadge = screen.Q("effectBadge");
            _effectName = screen.Q<Label>("effectName");
            _effectTime = screen.Q<Label>("effectTime");
            _endCoinPop = () => _coinChip.RemoveFromClassList(PopClass);

            screen.Q<Button>("pauseButton").clicked += () => _bus.Fire<PauseRequestedSignal>();
            _abilityButton.clicked += () => _bus.Fire<AbilityRequestedSignal>();

            _bus.Subscribe<RunStartedSignal>(_ => ResetHud()).AddTo(_subscriptions);
            _bus.Subscribe<ScoreChangedSignal>(s => _score = s.Score).AddTo(_subscriptions);
            _bus.Subscribe<CoinCollectedSignal>(OnCoinCollected).AddTo(_subscriptions);
            _bus.Subscribe<CharacterSelectedSignal>(OnCharacterSelected).AddTo(_subscriptions);
            _bus.Subscribe<AbilityUsedSignal>(OnAbilityUsed).AddTo(_subscriptions);
            _bus.Subscribe<TimedEffectStartedSignal>(OnEffectStarted).AddTo(_subscriptions);
            _bus.Subscribe<TimedEffectEndedSignal>(OnEffectEnded).AddTo(_subscriptions);
        }

        void ResetHud()
        {
            _score = 0;
            _scoreShown = -1;
            _nextScoreRefresh = 0f;
            _coinValue.text = "0";
            _abilityReadyAt = 0f;
            SetAbilityReady(true);
            HideEffect();
        }

        void OnCharacterSelected(CharacterSelectedSignal signal)
        {
            _abilityName.Localize(signal.AbilityNameKey);
            _abilityButton.EnableInClassList(ScreenRouter.HiddenClass, string.IsNullOrEmpty(signal.AbilityNameKey));
        }

        void OnCoinCollected(CoinCollectedSignal signal)
        {
            _coinValue.text = signal.RunCoins.ToString();
            _coinChip.AddToClassList(PopClass);
            _coinChip.schedule.Execute(_endCoinPop).StartingIn(110);
        }

        void OnAbilityUsed(AbilityUsedSignal signal)
        {
            _abilityCooldown = Mathf.Max(0.01f, signal.Cooldown);
            _abilityReadyAt = Time.time + signal.Cooldown;
            SetAbilityReady(false);
        }

        void OnEffectStarted(TimedEffectStartedSignal signal)
        {
            _effectId = signal.Id;
            _effectEndsAt = Time.time + signal.Duration;
            _effectShown = -1;
            _effectName.Localize(signal.NameKey);
            _effectBadge.RemoveFromClassList(ScreenRouter.HiddenClass);
        }

        void OnEffectEnded(TimedEffectEndedSignal signal)
        {
            if (signal.Id == _effectId) HideEffect();
        }

        void HideEffect()
        {
            _effectId = null;
            _effectBadge.AddToClassList(ScreenRouter.HiddenClass);
        }

        public void Tick()
        {
            if (_abilityButton == null) return;

            // Skor her karede değişir; metni saniyede en fazla 10 kez yenilemek bellek ayırmayı düşürür.
            if (_score != _scoreShown && Time.unscaledTime >= _nextScoreRefresh)
            {
                _scoreShown = _score;
                _nextScoreRefresh = Time.unscaledTime + ScoreRefreshSeconds;
                _scoreValue.text = _score.ToString();
            }

            if (!_abilityReady)
            {
                float remaining = _abilityReadyAt - Time.time;
                if (remaining <= 0f) SetAbilityReady(true);
                else _abilityRing.progress = 1f - remaining / _abilityCooldown;
            }

            if (_effectId == null) return;

            int seconds = Mathf.CeilToInt(Mathf.Max(0f, _effectEndsAt - Time.time));
            if (seconds == _effectShown) return;
            _effectShown = seconds;
            _effectTime.text = seconds.ToString();
        }

        void SetAbilityReady(bool ready)
        {
            _abilityReady = ready;
            _abilityButton.SetEnabled(ready);
            if (ready) _abilityRing.progress = 1f;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
