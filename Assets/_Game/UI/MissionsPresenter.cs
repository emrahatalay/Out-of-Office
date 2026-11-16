using System;
using System.Collections.Generic;
using OutOfOffice.Core.Run;
using OutOfOffice.Core.Signals;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Görevler sekmesi. Satırlar, oyunun duyurduğu günlük görevlerden kendiliğinden oluşur
    /// (<see cref="MissionChangedSignal"/>); arayüz hiçbir kuralı bilmez, ödül almak için yalnızca
    /// istek sinyali atar. Ödülü alınabilir görev varsa sekmede rozet yanar.
    /// </summary>
    public sealed class MissionsPresenter : IStartable, IDisposable
    {
        sealed class Row
        {
            public VisualElement Root;
            public Label Name;
            public VisualElement Fill;
            public Label Progress;
            public VisualElement Reward;
            public VisualElement RewardIcon;
            public Label RewardAmount;
            public Button Claim;
            public VisualElement ClaimIcon;
            public Label ClaimAmount;
            public Label Done;
        }

        readonly UIDocument _document;
        readonly ISignalBus _bus;
        readonly SubscriptionBag _subscriptions = new SubscriptionBag();
        readonly List<Row> _rows = new List<Row>();
        VisualElement _list;
        Label _badge;

        public MissionsPresenter(UIDocument document, ISignalBus bus)
        {
            _document = document;
            _bus = bus;
        }

        public void Start()
        {
            var root = _document.rootVisualElement;
            _list = root.Q("missionList");
            _badge = root.Q<Label>("missionsBadge");

            _bus.Subscribe<MissionChangedSignal>(OnMission).AddTo(_subscriptions);
            _bus.Subscribe<MissionsClaimableSignal>(OnClaimable).AddTo(_subscriptions);
        }

        void OnMission(MissionChangedSignal mission)
        {
            while (_rows.Count <= mission.Slot) _rows.Add(Build(_rows.Count));
            var row = _rows[mission.Slot];

            row.Name.Localize(mission.NameKey);
            row.Progress.text = Math.Min(mission.Progress, mission.Target) + " / " + mission.Target;
            // Genişlik ilerlemeden gelir; bu yüzden satır içi stil kullanılır.
            row.Fill.style.width = Length.Percent(100f * Math.Min(mission.Progress, mission.Target) / Math.Max(1, mission.Target));
            row.Fill.EnableInClassList("mission-fill--done", mission.IsComplete);

            string amount = "x" + mission.RewardAmount;
            row.RewardAmount.text = amount;
            row.ClaimAmount.text = amount;
            SetRewardIcon(row.RewardIcon, mission.RewardKind);
            SetRewardIcon(row.ClaimIcon, mission.RewardKind);

            bool claimable = mission.IsComplete && !mission.Claimed;
            row.Reward.EnableInClassList(ScreenRouter.HiddenClass, mission.IsComplete);
            row.Claim.EnableInClassList(ScreenRouter.HiddenClass, !claimable);
            row.Done.EnableInClassList(ScreenRouter.HiddenClass, !mission.Claimed);
            row.Root.EnableInClassList("mission-row--claimed", mission.Claimed);
        }

        void OnClaimable(MissionsClaimableSignal signal)
        {
            if (_badge == null) return;

            _badge.text = signal.Count.ToString();
            _badge.EnableInClassList(ScreenRouter.HiddenClass, signal.Count == 0);
        }

        Row Build(int slot)
        {
            var row = new Row();
            var root = new VisualElement();
            root.AddToClassList("mission-row");
            row.Root = root;

            var info = new VisualElement();
            info.AddToClassList("mission-info");
            root.Add(info);

            row.Name = new Label();
            row.Name.AddToClassList("mission-name");
            info.Add(row.Name);

            var track = new VisualElement();
            track.AddToClassList("mission-track");
            info.Add(track);

            row.Fill = new VisualElement();
            row.Fill.AddToClassList("mission-fill");
            track.Add(row.Fill);

            row.Progress = new Label();
            row.Progress.AddToClassList("mission-progress");
            info.Add(row.Progress);

            // Sağ taraf: bitmemişse ödül çipi, bitmişse "AL" butonu, alınmışsa "ALINDI".
            row.Reward = new VisualElement();
            row.Reward.AddToClassList("mission-reward");
            row.RewardIcon = new VisualElement { pickingMode = PickingMode.Ignore };
            row.Reward.Add(row.RewardIcon);
            row.RewardAmount = new Label { pickingMode = PickingMode.Ignore };
            row.RewardAmount.AddToClassList("mission-reward-amount");
            row.Reward.Add(row.RewardAmount);
            root.Add(row.Reward);

            row.Claim = new Button(() => _bus.Fire(new MissionClaimRequestedSignal(slot)));
            row.Claim.AddToClassList("buy-button");
            row.Claim.AddToClassList(ScreenRouter.HiddenClass);
            var claimLabel = new Label { pickingMode = PickingMode.Ignore };
            claimLabel.AddToClassList("buy-label");
            claimLabel.Localize("ui.mission.claim");
            row.Claim.Add(claimLabel);
            row.ClaimIcon = new VisualElement { pickingMode = PickingMode.Ignore };
            row.Claim.Add(row.ClaimIcon);
            row.ClaimAmount = new Label { pickingMode = PickingMode.Ignore };
            row.ClaimAmount.AddToClassList("buy-label");
            row.Claim.Add(row.ClaimAmount);
            root.Add(row.Claim);

            row.Done = new Label();
            row.Done.AddToClassList("mission-done");
            row.Done.AddToClassList(ScreenRouter.HiddenClass);
            row.Done.Localize("ui.mission.claimed");
            root.Add(row.Done);

            _list.Add(root);
            return row;
        }

        static void SetRewardIcon(VisualElement icon, RewardKind kind)
        {
            icon.ClearClassList();
            icon.AddToClassList("icon");
            icon.AddToClassList("icon-small");
            icon.AddToClassList("icon-gap");
            icon.AddToClassList(kind == RewardKind.Key ? "icon-key" : kind == RewardKind.Coins ? "icon-coin" : "icon-chair");
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
