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

        // \u041a\u043b\u044e\u0447\u0438 \u0432\u0430\u043b\u044e\u0442 \u2014 \u043d\u0430\u0441\u0442\u0440\u0430\u0438\u0432\u0430\u0439\u0442\u0435 \u043f\u043e\u0434 \u0432\u0430\u0448 \u043f\u0440\u043e\u0435\u043a\u0442
        [Header("Currency Keys")]
        [SerializeField] private string _coinCurrencyKey = "CO";
        [SerializeField] private string _gemCurrencyKey  = "GE";

        private LimitedTimeEventWindowModel _model;
        private bool  _isActive;
        private float _timerTick;

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

        // \u2500\u2500\u2500 Data loading \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

        private async Task LoadDataAsync()
        {
            var result = await LimitedTimeEventService.GetActiveEvents();
            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
                _view.ShowEmpty();
            }
            // \u0412\u044c\u044e \u043e\u0431\u043d\u043e\u0432\u0438\u0442\u0441\u044f \u0447\u0435\u0440\u0435\u0437 HandleActiveEventsUpdated
        }

        // \u2500\u2500\u2500 Service callbacks \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

        private void HandleActiveEventsUpdated(GetActiveEventsResponse response)
        {
            if (!_isActive) return;
            var evt = response?.ActiveEvents?.Count > 0 ? response.ActiveEvents[0] : null;
            RefreshModel(evt);
        }

        private void HandleMilestoneClaimed(EventMilestoneClaimResponse _)
        {
            if (_isActive) _view.Render(_model);
        }

        private void HandleStreakClaimed(EventStreakClaimResponse _)
        {
            if (_isActive) _view.Render(_model);
        }

        private void HandleTokensGranted(EventTokenGrantInfo _)
        {
            if (_isActive) _view.Render(_model);
        }

        private void HandleCurrencyUpdated()
        {
            if (!_isActive) return;
            var (coins, gems) = ReadCurrencies();
            _model.UpdateCurrencies(coins, gems);
            _view.Render(_model);
        }

        // \u2500\u2500\u2500 Helpers \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

        private void RefreshModel(ActiveEventInfo evt)
        {
            bool hasPremium = IDosGamesData.User.Premium != null;
            var (coins, gems) = ReadCurrencies();
            _model.Apply(evt, hasPremium, coins, gems);
            _view.Render(_model);
        }

        private (long coins, long gems) ReadCurrencies()
        {
            var vc    = IDosGamesData.User.VirtualCurrency;
            long coins = vc != null && vc.TryGetValue(_coinCurrencyKey, out var c) ? c : 0;
            long gems  = vc != null && vc.TryGetValue(_gemCurrencyKey,  out var g) ? g : 0;
            return (coins, gems);
        }

        // \u2500\u2500\u2500 Button handlers \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

        private void OnActivateClicked()
        {
            // TODO: \u043e\u0442\u043a\u0440\u044b\u0442\u044c \u044d\u043a\u0440\u0430\u043d \u043f\u043e\u043a\u0443\u043f\u043a\u0438 Premium Pass
            Message.Show("Premium pass activation coming soon!");
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
