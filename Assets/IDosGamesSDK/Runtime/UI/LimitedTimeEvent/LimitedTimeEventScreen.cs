using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>
    /// Root screen controller for Limited Time Events (Battle Pass style).
    /// Reads data from IDosGamesData, subscribes to SDK events for auto-refresh.
    /// </summary>
    public class LimitedTimeEventScreen : MonoBehaviour
    {
        // ─── Header ───────────────────────────────────────────────────────────
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _eventTitleText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Image           _headerBannerImage;
        [SerializeField] private Button          _activatePassButton;
        [SerializeField] private GameObject      _activatePassBadge;
        [SerializeField] private Button          _helpButton;

        // ─── Progress Bar ──────────────────────────────────────────────────────
        [Header("Progress Bar")]
        [SerializeField] private Slider          _tokenProgressSlider;
        [SerializeField] private TextMeshProUGUI _tokenProgressText;   // "30/200"
        [SerializeField] private TextMeshProUGUI _nextMilestoneLabelText;
        [SerializeField] private Image           _tokenIconImage;

        // ─── Milestone List ───────────────────────────────────────────────────
        [Header("Milestone List")]
        [SerializeField] private Transform       _milestonesContainer;
        [SerializeField] private MilestoneRowView _milestoneRowPrefab;
        [SerializeField] private ScrollRect      _scrollRect;

        // ─── States ───────────────────────────────────────────────────────────
        [Header("State Views")]
        [SerializeField] private GameObject _loadingView;
        [SerializeField] private GameObject _emptyStateView;
        [SerializeField] private GameObject _contentView;
        [SerializeField] private TextMeshProUGUI _emptyStateText;

        // ─── Bonus Window Banner ──────────────────────────────────────────────
        [Header("Bonus Window")]
        [SerializeField] private GameObject      _bonusWindowBanner;
        [SerializeField] private TextMeshProUGUI _bonusWindowText;

        // ─── Streak Widget ────────────────────────────────────────────────────
        [Header("Streak")]
        [SerializeField] private StreakPanelView _streakPanel;

        // ─── Private ──────────────────────────────────────────────────────────
        private bool                _isActive;
        private ActiveEventInfo     _currentEvent;
        private SimpleListPool<MilestoneRowView> _milestonePool;
        private float               _timerUpdateInterval = 1f;
        private float               _timerElapsed;

        // ─── Unity Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            _milestonePool = new SimpleListPool<MilestoneRowView>(_milestoneRowPrefab, _milestonesContainer, 10);
        }

        private void OnEnable()
        {
            _isActive = true;

            LimitedTimeEventService.OnActiveEventsUpdated += OnActiveEventsUpdated;
            LimitedTimeEventService.OnMilestoneClaimed    += OnMilestoneClaimed;
            LimitedTimeEventService.OnStreakRewardClaimed += OnStreakRewardClaimed;
            LimitedTimeEventService.OnTokensGranted       += OnTokensGranted;

            _activatePassButton.onClick.AddListener(OnActivatePassClicked);
            _helpButton.onClick.AddListener(OnHelpClicked);

            RefreshUI();
            _ = FetchDataIfNeeded();
        }

        private void OnDisable()
        {
            _isActive = false;

            LimitedTimeEventService.OnActiveEventsUpdated -= OnActiveEventsUpdated;
            LimitedTimeEventService.OnMilestoneClaimed    -= OnMilestoneClaimed;
            LimitedTimeEventService.OnStreakRewardClaimed -= OnStreakRewardClaimed;
            LimitedTimeEventService.OnTokensGranted       -= OnTokensGranted;

            _activatePassButton.onClick.RemoveListener(OnActivatePassClicked);
            _helpButton.onClick.RemoveListener(OnHelpClicked);
        }

        private void Update()
        {
            if (!_isActive || _currentEvent == null) return;
            _timerElapsed += Time.deltaTime;
            if (_timerElapsed >= _timerUpdateInterval)
            {
                _timerElapsed = 0f;
                UpdateTimer();
            }
        }

        // ─── Data Fetch ───────────────────────────────────────────────────────

        private async Task FetchDataIfNeeded()
        {
            ShowLoading();
            var result = await LimitedTimeEventService.GetActiveEvents();
            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
                ShowEmpty("Failed to load events.");
                return;
            }

            // RefreshUI is called via event OnActiveEventsUpdated
        }

        // ─── SDK Event Callbacks ──────────────────────────────────────────────

        private void OnActiveEventsUpdated(GetActiveEventsResponse response)
        {
            if (!_isActive) return;
            RefreshUI();
        }

        private void OnMilestoneClaimed(EventMilestoneClaimResponse response)
        {
            if (!_isActive) return;
            RefreshUI();
        }

        private void OnStreakRewardClaimed(EventStreakClaimResponse response)
        {
            if (!_isActive) return;
            RefreshUI();
        }

        private void OnTokensGranted(EventTokenGrantInfo info)
        {
            if (!_isActive) return;
            RefreshUI();
        }

        // ─── UI Refresh ───────────────────────────────────────────────────────

        private void RefreshUI()
        {
            // Pick the first active event to display
            // ASSUMPTION: IDosGamesData.User stores active events list; using service response cache
            // We rely on the last data pushed by OnActiveEventsUpdated
            // If no data yet, stay in loading/empty
            _currentEvent = GetCurrentEvent();

            if (_currentEvent == null)
            {
                ShowEmpty("No active events at the moment.");
                return;
            }

            ShowContent();
            RenderHeader();
            RenderProgressBar();
            RenderMilestones();
            RenderBonusWindow();
            RenderStreak();
            UpdateTimer();
        }

        private void RenderHeader()
        {
            var content = _currentEvent.Content;
            _eventTitleText.text = content?.DisplayName ?? "Limited Event";

            // Load banner image
            if (content?.AssetPaths != null && content.AssetPaths.Count > 0)
                _ = LoadHeaderImage(content.AssetPaths[0]);

            // Pass activation badge — ASSUMPTION: VIP check via progress/access
            bool hasPass = false; // ASSUMPTION: check premium tier from UserData
            _activatePassBadge.SetActive(!hasPass);
        }

        private async Task LoadHeaderImage(string path)
        {
            if (!ImageLoader.IsExternalUrl(path))
            {
                _headerBannerImage.sprite = ImageLoader.LoadLocalImage(path);
                return;
            }
            var sprite = await ImageLoader.LoadExternalImageAsync(path);
            if (_isActive && sprite != null) _headerBannerImage.sprite = sprite;
        }

        private void RenderProgressBar()
        {
            var progress = _currentEvent.Progress;
            var nextMilestone = _currentEvent.NextMilestone;

            if (progress == null) return;

            long current = progress.TokenBalance;
            long target  = nextMilestone?.RequiredTokensEarned ?? progress.TokenBalance;

            _tokenProgressText.text = $"{current:N0} / {target:N0}";

            if (target > 0)
                _tokenProgressSlider.value = Mathf.Clamp01((float)current / target);
            else
                _tokenProgressSlider.value = 1f;

            _nextMilestoneLabelText.text = nextMilestone != null
                ? $"Next: {nextMilestone.DisplayName}"
                : "All milestones reached!";

            // Token icon
            var tokenImages = _currentEvent.Content?.Token?.AssetPaths;
            if (tokenImages != null && tokenImages.Count > 0)
                _ = LoadTokenIcon(tokenImages[0]);
        }

        private async Task LoadTokenIcon(string path)
        {
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (_isActive && sprite != null) _tokenIconImage.sprite = sprite;
        }

        private void RenderMilestones()
        {
            var milestones = _currentEvent.Content?.Milestones;
            var progress   = _currentEvent.Progress;

            _milestonePool.ReturnAll();

            if (milestones == null || milestones.Count == 0) return;

            // Sort by SortOrder
            milestones.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

            for (int i = 0; i < milestones.Count; i++)
            {
                var def  = milestones[i];
                var row  = _milestonePool.Get();
                bool claimed = progress?.ClaimedMilestoneIDs?.Contains(def.MilestoneID) ?? false;
                bool reached = (progress?.TokensEarnedTotal ?? 0) >= def.RequiredTokensEarned;
                bool canClaim = reached && !claimed && _currentEvent.CanClaim;

                row.Setup(
                    milestoneNumber: i + 1,
                    definition:      def,
                    isClaimed:       claimed,
                    isReached:       reached,
                    canClaim:        canClaim,
                    isVip:           false,          // ASSUMPTION: check user premium
                    eventId:         _currentEvent.EventType == ActiveEventType.Scheduled ? _currentEvent.ID : null,
                    chainId:         _currentEvent.EventType == ActiveEventType.Chained   ? _currentEvent.ID : null,
                    onClaimSuccess:  RefreshUI
                );
            }
        }

        private void RenderBonusWindow()
        {
            bool active = _currentEvent.BonusWindowActive;
            _bonusWindowBanner.SetActive(active);
            if (active)
            {
                double mult = _currentEvent.BonusWindowMultiplier;
                string end  = _currentEvent.BonusWindowEndUtc.HasValue
                    ? TimeFormatUtil.FormatCountdown(_currentEvent.BonusWindowEndUtc.Value)
                    : "";
                _bonusWindowText.text = $"🔥 Bonus x{mult:F1} active!  Ends in {end}";
            }
        }

        private void RenderStreak()
        {
            var streak = _currentEvent.Content?.Streak;
            if (_streakPanel == null) return;

            if (streak == null || streak.Rewards == null || streak.Rewards.Count == 0)
            {
                _streakPanel.gameObject.SetActive(false);
                return;
            }

            _streakPanel.gameObject.SetActive(true);
            _streakPanel.Setup(
                definition: streak,
                progress:   _currentEvent.Progress,
                eventId:    _currentEvent.EventType == ActiveEventType.Scheduled ? _currentEvent.ID : null,
                chainId:    _currentEvent.EventType == ActiveEventType.Chained   ? _currentEvent.ID : null
            );
        }

        private void UpdateTimer()
        {
            if (_currentEvent == null) return;
            var remaining = _currentEvent.ComputedEndUtc - System.DateTime.UtcNow;
            _timerText.text = remaining.TotalSeconds > 0
                ? TimeFormatUtil.FormatCountdown(_currentEvent.ComputedEndUtc)
                : "Ended";
        }

        // ─── State Helpers ────────────────────────────────────────────────────

        private void ShowLoading()
        {
            _loadingView.SetActive(true);
            _emptyStateView.SetActive(false);
            _contentView.SetActive(false);
        }

        private void ShowEmpty(string msg = "")
        {
            _loadingView.SetActive(false);
            _emptyStateView.SetActive(true);
            _contentView.SetActive(false);
            if (_emptyStateText != null) _emptyStateText.text = msg;
        }

        private void ShowContent()
        {
            _loadingView.SetActive(false);
            _emptyStateView.SetActive(false);
            _contentView.SetActive(true);
        }

        // ─── Cached Event ─────────────────────────────────────────────────────

        // ASSUMPTION: We cache the last received response in a static field for RefreshUI
        // since IDosGamesData may not expose LTE data directly.
        private static GetActiveEventsResponse _cachedResponse;

        private ActiveEventInfo GetCurrentEvent()
        {
            return _cachedResponse?.ActiveEvents != null && _cachedResponse.ActiveEvents.Count > 0
                ? _cachedResponse.ActiveEvents[0]
                : null;
        }

        // Called from service callback to cache
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _cachedResponse = null;
        }

        // Patch: subscribe static cache via separate initializer
        private void Start()
        {
            LimitedTimeEventService.OnActiveEventsUpdated += r => _cachedResponse = r;
        }

        // ─── Button Handlers ──────────────────────────────────────────────────

        private void OnActivatePassClicked()
        {
            // ASSUMPTION: Navigate to shop/premium screen
            Message.Show("Premium pass activation coming soon!");
        }

        private void OnHelpClicked()
        {
            Message.Show("Earn tokens to unlock milestone rewards!");
        }
    }
}
