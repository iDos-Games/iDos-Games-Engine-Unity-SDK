using System;
using System.Threading.Tasks;
using IDosGames.ClientModels;
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

            LimitedTimeEventService.OnActiveEventsUpdated += HandleActiveEventsUpdated;
            LimitedTimeEventService.OnMilestoneClaimed    += HandleMilestoneClaimed;
            LimitedTimeEventService.OnStreakRewardClaimed += HandleStreakClaimed;
            LimitedTimeEventService.OnTokensGranted       += HandleTokensGranted;
            IDosGamesData.User.OnVirtualCurrencyUpdated   += HandleCurrencyUpdated;

            _view.ActivateButton?.onClick.AddListener(OnActivateClicked);
            _view.BackButton?.onClick.AddListener(OnBackClicked);

            _view.ShowLoading();
            _ = LoadDataAsync();
        }

        private void OnDisable()
        {
            _isActive = false;

            LimitedTimeEventService.OnActiveEventsUpdated -= HandleActiveEventsUpdated;
            LimitedTimeEventService.OnMilestoneClaimed    -= HandleMilestoneClaimed;
            LimitedTimeEventService.OnStreakRewardClaimed -= HandleStreakClaimed;
            LimitedTimeEventService.OnTokensGranted       -= HandleTokensGranted;
            IDosGamesData.User.OnVirtualCurrencyUpdated   -= HandleCurrencyUpdated;

            _view.ActivateButton?.onClick.RemoveListener(OnActivateClicked);
            _view.BackButton?.onClick.RemoveListener(OnBackClicked);
        }

        private void Update()
        {
            if (!_isActive || _model.Event == null) return;
            _timerTick += Time.deltaTime;
            if (_timerTick < 1f) return;
            _timerTick = 0f;

            var remaining = _model.Event.ComputedEndUtc - DateTime.UtcNow;
            _view.UpdateTimer(remaining.TotalSeconds > 0
                ? TimeFormatUtil.FormatCountdown(_model.Event.ComputedEndUtc)
                : "Ended");
        }

        // ─── Data loading ──────────────────────────────────────────────────────

        private async Task LoadDataAsync()
        {
            var result = await LimitedTimeEventService.GetActiveEvents();
            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
                _view.ShowEmpty();
            }
            // View обновится через HandleActiveEventsUpdated
        }

        // ─── Service callbacks ─────────────────────────────────────────────────

        private void HandleActiveEventsUpdated(GetActiveEventsResponse response)
        {
            if (!_isActive) return;
            var evt = response?.ActiveEvents?.Count > 0 ? response.ActiveEvents[0] : null;
            RefreshModel(evt);
        }

        private void HandleMilestoneClaimed(EventMilestoneClaimResponse _)
        {
            if (_isActive) _view.Render(_model, _claimFactory);
        }

        private void HandleStreakClaimed(EventStreakClaimResponse _)
        {
            if (_isActive) _view.Render(_model, _claimFactory);
        }

        private void HandleTokensGranted(EventTokenGrantInfo _)
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
            bool hasPremium = (IDosGamesData.User.Premium?.MaxActiveTier ?? 0) > 0;
            var (coins, gems) = ReadCurrencies();
            _model.Apply(evt, hasPremium, coins, gems);
            _claimFactory = BuildClaimFactory(evt);
            _view.Render(_model, _claimFactory);
        }

        private static Func<string, bool, Func<Task>> BuildClaimFactory(ActiveEventInfo evt)
        {
            if (evt == null) return null;
            string eventId = evt.EventType == ActiveEventType.Chained ? null    : evt.ID;
            string chainId = evt.EventType == ActiveEventType.Chained ? evt.ID  : null;
            return (milestoneId, isPremium) => async () =>
            {
                var result = await LimitedTimeEventService.ClaimMilestone(milestoneId, eventId, chainId);
                if (!result.Success)
                    Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
            };
        }

        private (long coins, long gems) ReadCurrencies()
        {
            var vc    = IDosGamesData.User.VirtualCurrency;
            long coins = vc != null && vc.TryGetValue(_coinCurrencyKey, out var c) ? c : 0;
            long gems  = vc != null && vc.TryGetValue(_gemCurrencyKey,  out var g) ? g : 0;
            return (coins, gems);
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
