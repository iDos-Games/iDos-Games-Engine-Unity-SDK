using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Leaderboard
{
    public class LeaderboardView : MonoBehaviour
    {
        // ── Header ────────────────────────────────────────────────────────
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _gemText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _participantsText;
        [SerializeField] private TextMeshProUGUI _cycleResetLabel;

        // ── Tabs ──────────────────────────────────────────────────────────
        [Header("Tabs")]
        [SerializeField] private LeaderboardTabView _tabTemplate;
        [SerializeField] private RectTransform      _tabContainer;

        // ── Top 3 ─────────────────────────────────────────────────────────
        [Header("Top 3")]
        [SerializeField] private LeaderboardTopSlot _firstPlace;
        [SerializeField] private LeaderboardTopSlot _secondPlace;
        [SerializeField] private LeaderboardTopSlot _thirdPlace;

        // ── List ──────────────────────────────────────────────────────────
        [Header("List")]
        [SerializeField] private LeaderboardEntryView _entryTemplate;
        [SerializeField] private RectTransform        _entryContainer;

        // ── Current Player ────────────────────────────────────────────────
        [Header("Current Player")]
        [SerializeField] private LeaderboardEntryView  _currentUserRow;

        // ── State Views ───────────────────────────────────────────────────────
        [Header("State Views")]
        [SerializeField] private GameObject _loadingView;
        [SerializeField] private GameObject _contentView;
        [SerializeField] private GameObject _emptyView;

        // ── Rewards Popup ─────────────────────────────────────────────────
        [Header("Rewards Popup")]
        [SerializeField] private Button                 _showRewardsButton;
        [SerializeField] private LeaderboardRewardsPopup _rewardsPopup;

        // ── Navigation ────────────────────────────────────────────────────
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        public Button BackButton => _backButton;

        // ── active tab ID (leaderboardID) ─────────────────────────────────
        public string ActiveTabId { get; private set; } = "";

        // ── cached state ──────────────────────────────────────────────────
        private LeaderboardWindowModel                        _model;
        private Func<string, string, Func<Task>>              _claimMilestoneFactory;
        private readonly List<LeaderboardTabView>             _spawnedTabs = new();

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_showRewardsButton != null)
                _showRewardsButton.onClick.AddListener(OnShowRewardsClicked);
        }

        private void Update() => RefreshTimer();

        // ─────────────────────────────────────────────────────────────────

        public void ShowLoading() => SetState(loading: true,  content: false, empty: false);
        public void ShowEmpty()   => SetState(loading: false, content: false, empty: true);

        /// <summary>
        /// Full render. Call whenever model changes.
        /// <para><paramref name="claimMilestoneFactory"/> — (leaderboardID, milestoneID) → async claim action.</para>
        /// </summary>
        public void Render(
            LeaderboardWindowModel              model,
            Func<string, string, Func<Task>>    claimMilestoneFactory = null)
        {
            _model                 = model;
            _claimMilestoneFactory = claimMilestoneFactory;

            if (!model.IsLoaded)
            {
                ShowEmpty();
                return;
            }

            SetState(loading: false, content: true, empty: false);

            if (_coinText != null) _coinText.text = model.CoinBalance.ToString("N0");
            if (_gemText  != null) _gemText.text  = model.GemBalance.ToString("N0");

            SetupTabs(model);
            RenderActiveTab();
            RefreshRewardsPopupIfOpen();
        }

        private void SetState(bool loading, bool content, bool empty)
        {
            if (_loadingView != null) _loadingView.SetActive(loading);
            if (_contentView != null) _contentView.SetActive(content);
            if (_emptyView   != null) _emptyView.SetActive(empty);
        }

        public void SwitchTab(string tabId)
        {
            if (_model == null) return;
            ActiveTabId = tabId;
            RefreshTabVisuals();
            RenderActiveTab();
        }

        // ─────────────────────────────────────────────────────────────────

        private void SetupTabs(LeaderboardWindowModel model)
        {
            if (_tabTemplate == null || _tabContainer == null) return;

            for (int i = _tabContainer.childCount - 1; i >= 0; i--)
            {
                var child = _tabContainer.GetChild(i);
                Destroy(child.gameObject);
            }
            _spawnedTabs.Clear();

            foreach (var tab in model.Tabs)
            {
                var tabView    = Instantiate(_tabTemplate, _tabContainer);
                var capturedId = tab.LeaderboardID;
                tabView.Setup(capturedId, tab.DisplayName, () => SwitchTab(capturedId));
                tabView.gameObject.SetActive(true);
                _spawnedTabs.Add(tabView);
            }

            // Validate active selection
            if (!_spawnedTabs.Any(t => t.TabId == ActiveTabId))
                ActiveTabId = _spawnedTabs.Count > 0 ? _spawnedTabs[0].TabId : "";

            RefreshTabVisuals();
        }

        private void RefreshTabVisuals()
        {
            if (_model == null) return;

            foreach (var tabView in _spawnedTabs)
            {
                tabView.SetSelected(tabView.TabId == ActiveTabId);

                var data = _model.Tabs.FirstOrDefault(t => t.LeaderboardID == tabView.TabId);
                bool hasNotification = data != null && (data.HasUnclaimedReward || data.HasUnclaimedMilestone);
                tabView.SetHasNotification(hasNotification);
            }
        }

        // ─────────────────────────────────────────────────────────────────

        private void RenderActiveTab()
        {
            if (_model == null) return;

            var data = _model.Tabs.FirstOrDefault(t => t.LeaderboardID == ActiveTabId);

            if (_cycleResetLabel  != null) _cycleResetLabel.text  = data?.CycleResetLabel ?? "";
            if (_participantsText != null) _participantsText.text = data != null ? FormatParticipants(data.TotalParticipants) : "";

            RenderTop3(data);
            RenderList(data);
            RenderCurrentUser(data);
            RefreshTimer();
        }

        // ── Top 3 ─────────────────────────────────────────────────────────

        private void RenderTop3(LeaderboardTabData data)
        {
            string scoreUnit = data?.ScoreDisplayName ?? "";
            _firstPlace?.Setup(data?.Entries.FirstOrDefault(e => e.Rank == 1), scoreUnit);
            _secondPlace?.Setup(data?.Entries.FirstOrDefault(e => e.Rank == 2), scoreUnit);
            _thirdPlace?.Setup(data?.Entries.FirstOrDefault(e => e.Rank == 3), scoreUnit);
        }

        // ── Scrollable list (rank 4+) ─────────────────────────────────────

        private void RenderList(LeaderboardTabData data)
        {
            if (_entryTemplate == null || _entryContainer == null) return;

            ClearContainer(_entryContainer);
            if (data == null) return;

            string scoreUnit = data.ScoreDisplayName;

            foreach (var entry in data.Entries.Where(e => e.Rank > 3).OrderBy(e => e.Rank))
            {
                var row = Instantiate(_entryTemplate, _entryContainer);
                row.Setup(entry, scoreUnit);
                row.gameObject.SetActive(true);
            }
        }

        // ── Current user pinned row ───────────────────────────────────────

        private void RenderCurrentUser(LeaderboardTabData data)
        {
            if (_currentUserRow != null)
            {
                var me = data?.Entries.FirstOrDefault(e => e.IsCurrentUser)
                      ?? data?.MyProgress.AsEntry(data.ScoreDisplayName);

                if (me != null)
                {
                    _currentUserRow.Setup(me, data?.ScoreDisplayName ?? "");
                    _currentUserRow.gameObject.SetActive(true);
                }
                else
                {
                    _currentUserRow.gameObject.SetActive(false);
                }
            }

        }

        // ─────────────────────────────────────────────────────────────────

        private void OnShowRewardsClicked()
        {
            if (_rewardsPopup == null || _model == null) return;

            var data = _model.Tabs.FirstOrDefault(t => t.LeaderboardID == ActiveTabId);
            if (data == null) return;

            _rewardsPopup.Show(data, _claimMilestoneFactory);
        }

        private void RefreshRewardsPopupIfOpen()
        {
            if (_rewardsPopup == null || !_rewardsPopup.gameObject.activeSelf || _model == null) return;

            var data = _model.Tabs.FirstOrDefault(t => t.LeaderboardID == ActiveTabId);
            if (data != null) _rewardsPopup.Show(data, _claimMilestoneFactory);
        }

        // ─────────────────────────────────────────────────────────────────

        private void RefreshTimer()
        {
            if (_timerText == null || _model == null) return;

            var data = _model.Tabs.FirstOrDefault(t => t.LeaderboardID == ActiveTabId);

            if (data?.CycleEndUtc == null || data.CycleReset == LeaderboardCycleReset.Never)
            {
                _timerText.text = "∞";
                return;
            }

            _timerText.text = FormatCountdown(data.CycleEndUtc.Value - DateTime.UtcNow);
        }

        private static string FormatCountdown(TimeSpan diff)
        {
            if (diff.TotalSeconds <= 0) return "0s";
            if (diff.TotalDays   >= 7) { int w = (int)(diff.TotalDays / 7); int d = (int)diff.TotalDays % 7; return $"{w}w {d}d"; }
            if (diff.TotalHours  >= 24){ int d = (int)diff.TotalDays; int h = diff.Hours;    return $"{d}d {h}h";  }
            if (diff.TotalMinutes >= 60){ int h = (int)diff.TotalHours; int m = diff.Minutes; return $"{h}h {m}m"; }
            return $"{(int)diff.TotalMinutes}m {diff.Seconds}s";
        }

        private static string FormatParticipants(long count)
        {
            if (count >= 1_000_000) return $"{count / 1_000_000f:0.#}M players";
            if (count >= 1_000)     return $"{count / 1_000f:0.#}K players";
            return $"{count} players";
        }

        private static void ClearContainer(RectTransform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.GetChild(i).gameObject);
        }
    }

    // ── Compact top-slot (1st / 2nd / 3rd place) ─────────────────────────────

    [Serializable]
    public class LeaderboardTopSlot
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ScoreText;
        public Image           AvatarImage;
        public GameObject      PremiumBadge;
        public GameObject      Root;

        public void Setup(LeaderboardEntryUIItem entry, string scoreDisplayName = "")
        {
            bool hasEntry = entry != null;
            if (Root != null) Root.SetActive(hasEntry);
            if (!hasEntry) return;

            if (NameText  != null) NameText.text  = entry.Name;
            if (ScoreText != null)
            {
                string formatted = entry.Score >= 1_000 ? $"{entry.Score / 1_000f:0.#}K" : entry.Score.ToString("N0");
                ScoreText.text = string.IsNullOrEmpty(scoreDisplayName) ? formatted : $"{formatted} {scoreDisplayName}";
            }
            if (PremiumBadge != null) PremiumBadge.SetActive(entry.IsPremium);
        }
    }

    // ── Extension for MyProgress → entry conversion ───────────────────────────

    internal static class LeaderboardProgressExtensions
    {
        internal static LeaderboardEntryUIItem AsEntry(this LeaderboardMyProgressUIItem progress, string scoreUnit)
        {
            if (progress == null) return null;
            return new LeaderboardEntryUIItem
            {
                UserID        = progress.UserID,
                Name          = "You",
                Score         = progress.CurrentScore,
                Rank          = progress.LastKnownRank,
                IsCurrentUser = true,
            };
        }
    }
}
