using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames.UI.LimitedTimeEvent
{
    [RequireComponent(typeof(LimitedTimeEventWindowView))]
    public class LimitedTimeEventWindowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LimitedTimeEventWindowView _view;

        [Header("Currency Keys")]
        [SerializeField] private string _coinCurrencyKey = "CO";
        [SerializeField] private string _gemCurrencyKey  = "GE";

        private LimitedTimeEventWindowModel    _model;
        private Func<string, bool, Func<Task>> _claimFactory;
        private bool                           _isActive;
        private float                          _timerTick;

        private void Awake()
        {
            _model = new LimitedTimeEventWindowModel();
            if (_view == null)
                _view = GetComponent<LimitedTimeEventWindowView>();
        }

        private void OnEnable()
        {
            _isActive = true;

            TimedEventService.OnActiveEventsLoaded += HandleActiveEventsLoaded;
            TimedEventService.OnMilestoneClaimed   += HandleMilestoneClaimed;
            TimedEventService.OnTokensGranted      += HandleTokensGranted;
            IDosGamesData.User.OnVirtualCurrencyUpdated += HandleCurrencyUpdated;

            _view.ActivateButton?.onClick.AddListener(OnActivateClicked);
            _view.BackButton?.onClick.AddListener(OnBackClicked);

            _view.ShowLoading();
            _ = LoadDataAsync();
        }

        private void OnDisable()
        {
            _isActive = false;

            TimedEventService.OnActiveEventsLoaded -= HandleActiveEventsLoaded;
            TimedEventService.OnMilestoneClaimed   -= HandleMilestoneClaimed;
            TimedEventService.OnTokensGranted      -= HandleTokensGranted;
            IDosGamesData.User.OnVirtualCurrencyUpdated -= HandleCurrencyUpdated;

            _view.ActivateButton?.onClick.RemoveListener(OnActivateClicked);
            _view.BackButton?.onClick.RemoveListener(OnBackClicked);
        }

        private void Update()
        {
            if (!_isActive || _model.Event == null) return;
            _timerTick += Time.deltaTime;
            if (_timerTick < 0.5f) return;
            _timerTick = 0f;
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            if (_model.Event == null) return;
            var remaining = _model.Event.ComputedEndUtc - DateTime.UtcNow;
            _view.UpdateTimer(remaining.TotalSeconds > 0
                ? FormatCountdown(remaining)
                : "Ended");
        }

        // ─── Data loading ──────────────────────────────────────────────────────

        private async Task LoadDataAsync()
        {
            var result = await TimedEventService.GetActiveEvents();
            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
                _view.ShowEmpty();
            }
            // View обновится через HandleActiveEventsLoaded
        }

        // ─── Service callbacks ─────────────────────────────────────────────────

        private void HandleActiveEventsLoaded(GetActiveEventsResponse response)
        {
            if (!_isActive) return;
            var evt = response?.ActiveEvents?.Count > 0 ? response.ActiveEvents[0] : null;
            RefreshModel(evt);
        }

        private void HandleMilestoneClaimed(EventMilestoneClaimResponse _)
        {
            if (_isActive) _view.Render(_model, _claimFactory);
        }

        private void HandleTokensGranted(ResourceOperation _)
        {
            if (_isActive) _view.Render(_model, _claimFactory);
        }

        private void HandleCurrencyUpdated()
        {
            if (!_isActive) return;
            var (coins, gems) = ReadCurrencies();
            _model.UpdateCurrencies(coins, gems);
            _view.Render(_model, _claimFactory);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private void RefreshModel(ActiveEventInfo evt)
        {
            bool hasPremium = IDosGamesData.User.State?.PublicData?.Premium ?? false;
            var (coins, gems) = ReadCurrencies();
            _model.Apply(evt, hasPremium, coins, gems);
            _claimFactory = BuildClaimFactory(evt);
            _view.Render(_model, _claimFactory);
            _timerTick = 0f;
            UpdateTimerDisplay();
        }

        private static Func<string, bool, Func<Task>> BuildClaimFactory(ActiveEventInfo evt)
        {
            if (evt == null) return null;
            return (milestoneId, isPremium) => async () =>
            {
                var result = await TimedEventService.ClaimMilestone(evt.Type, evt.TimedEventID, milestoneId);
                if (!result.Success)
                    Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
            };
        }

        private (long coins, long gems) ReadCurrencies()
        {
            var vc     = IDosGamesData.User.State?.InventoryV2?.VirtualCurrencies;
            long coins = vc != null && vc.TryGetValue(_coinCurrencyKey, out var c) ? c.Amount : 0;
            long gems  = vc != null && vc.TryGetValue(_gemCurrencyKey,  out var g) ? g.Amount : 0;
            return (coins, gems);
        }

        private static string FormatCountdown(TimeSpan diff)
        {
            if (diff.TotalSeconds <= 0) return "0м";
            if (diff.TotalDays >= 7)
            {
                int weeks = (int)(diff.TotalDays / 7);
                int days  = (int)diff.TotalDays % 7;
                return $"{weeks}н {days}д";
            }
            if (diff.TotalHours >= 24)
                return $"{(int)diff.TotalDays}д {diff.Hours}ч";
            if (diff.TotalMinutes >= 60)
                return $"{(int)diff.TotalHours}ч {diff.Minutes}м";
            return $"{(int)diff.TotalMinutes}м {diff.Seconds}с";
        }

        // ─── Button handlers ──────────────────────────────────────────────────

        private void OnActivateClicked()
        {
            // TODO: открыть экран покупки Premium Pass
            Message.Show("Premium pass activation coming soon!");
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
