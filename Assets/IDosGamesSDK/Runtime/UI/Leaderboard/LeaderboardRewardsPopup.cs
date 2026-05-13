using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Leaderboard
{
    public class LeaderboardRewardsPopup : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button          _closeButton;

        [Header("Tabs")]
        [SerializeField] private LeaderboardTabView _milestonesTabButton;
        [SerializeField] private LeaderboardTabView _rankRewardsTabButton;
        [SerializeField] private GameObject        _milestonesContent;
        [SerializeField] private GameObject        _rankRewardsContent;

        [Header("Milestones")]
        [SerializeField] private LeaderboardMilestoneView _milestoneTemplate;
        [SerializeField] private RectTransform        _milestonesContainer;

        [Header("Rank Rewards")]
        [SerializeField] private LeaderboardRankRewardView _rankRewardTemplate;
        [SerializeField] private RectTransform          _rankRewardsContainer;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            if (_milestonesTabButton != null)
            {
                var btn = _milestonesTabButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => SwitchTab(true));
            }

            if (_rankRewardsTabButton != null)
            {
                var btn = _rankRewardsTabButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => SwitchTab(false));
            }
        }

        // ─────────────────────────────────────────────────────────────────

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void Show(LeaderboardTabData tabData, Func<string, string, Func<Task>> claimMilestoneFactory = null)
        {
            if (tabData == null) return;

            if (_titleText != null)
                _titleText.text = tabData.DisplayName;

            PopulateMilestones(tabData, claimMilestoneFactory);
            PopulateRankRewards(tabData);

            SwitchTab(true);

            Show();
        }

        private void SwitchTab(bool milestones)
        {
            if (_milestonesContent != null) _milestonesContent.SetActive(milestones);
            if (_rankRewardsContent != null) _rankRewardsContent.SetActive(!milestones);

            if (_milestonesTabButton != null) _milestonesTabButton.SetSelected(milestones);
            if (_rankRewardsTabButton != null) _rankRewardsTabButton.SetSelected(!milestones);
        }

        // ─────────────────────────────────────────────────────────────────

        private void PopulateMilestones(LeaderboardTabData tabData, Func<string, string, Func<Task>> claimMilestoneFactory)
        {
            if (_milestoneTemplate == null || _milestonesContainer == null) return;

            ClearContainer(_milestonesContainer);

            bool hasMilestones = tabData.Milestones != null && tabData.Milestones.Count > 0;
            if (!hasMilestones) return;

            long currentScore = tabData.MyProgress?.ScoreEarnedThisCycle ?? 0;
            var  milestones   = tabData.Milestones;

            bool cycleEnded = tabData.CycleEndUtc.HasValue
                ? DateTime.UtcNow >= tabData.CycleEndUtc.Value
                : tabData.CycleReset == LeaderboardCycleReset.Never;

            for (int i = 0; i < milestones.Count; i++)
            {
                var  ms   = milestones[i];
                long prev = i > 0 ? milestones[i - 1].RequiredScore : 0;

                var state = ms.IsClaimed ? LeaderboardMilestoneState.Claimed
                    : !ms.IsReached      ? LeaderboardMilestoneState.Locked
                    : LeaderboardTabData.CanClaimNow(tabData.MilestoneClaimMode, ms.IsFeatured, cycleEnded)
                        ? LeaderboardMilestoneState.Reached
                        : LeaderboardMilestoneState.Pending;

                Func<Task> onClaim = null;
                if (claimMilestoneFactory != null)
                    onClaim = claimMilestoneFactory.Invoke(ms.LeaderboardID, ms.MilestoneID);

                var row = Instantiate(_milestoneTemplate, _milestonesContainer);
                row.Setup(
                    displayName:   ms.DisplayName,
                    requiredScore: ms.RequiredScore,
                    currentScore:  currentScore,
                    previousScore: prev,
                    rewards:       ms.Rewards,
                    state:         state,
                    iconPath:      ms.IconPath,
                    isFeatured:    ms.IsFeatured,
                    onClaim:       onClaim
                );
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_milestonesContainer);
            }

        private void PopulateRankRewards(LeaderboardTabData tabData)
        {
            if (_rankRewardTemplate == null || _rankRewardsContainer == null) return;

            ClearContainer(_rankRewardsContainer);

            bool hasRankRewards = tabData.RankRewards != null && tabData.RankRewards.Count > 0;
            if (!hasRankRewards) return;

            foreach (var item in tabData.RankRewards)
            {
                var row = Instantiate(_rankRewardTemplate, _rankRewardsContainer);
                row.Setup(item);
                row.gameObject.SetActive(true);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rankRewardsContainer);
        }

        private static void ClearContainer(RectTransform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
