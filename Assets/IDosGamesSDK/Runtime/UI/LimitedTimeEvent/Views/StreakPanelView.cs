using System;
using IDosGames.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>
    /// Horizontal strip showing daily streak progress and claimable streak rewards.
    /// </summary>
    public class StreakPanelView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _streakCountText;
        [SerializeField] private TextMeshProUGUI _streakSubtitleText;

        [Header("Streak Day Items")]
        [SerializeField] private Transform           _streakDaysContainer;
        [SerializeField] private StreakDayItemView   _streakDayPrefab;

        private bool                            _isActive;
        private SimpleListPool<StreakDayItemView> _pool;
        private EventStreakDefinition           _definition;
        private UserEventProgress               _progress;
        private string                          _eventId;
        private string                          _chainId;

        private void Awake()
        {
            _pool = new SimpleListPool<StreakDayItemView>(_streakDayPrefab, _streakDaysContainer, 7);
        }

        private void OnEnable()  => _isActive = true;
        private void OnDisable() => _isActive = false;

        public void Setup(
            EventStreakDefinition definition,
            UserEventProgress progress,
            string eventId,
            string chainId)
        {
            _definition = definition;
            _progress   = progress;
            _eventId    = eventId;
            _chainId    = chainId;

            int current = progress?.CurrentStreakDays ?? 0;
            _streakCountText.text    = $"🔥 {current} Day Streak";
            _streakSubtitleText.text = $"Earn {definition.MinTokensPerDay:N0} tokens/day to keep streak";

            RenderDays();
        }

        private void RenderDays()
        {
            _pool.ReturnAll();

            if (_definition.Rewards == null) return;

            foreach (var reward in _definition.Rewards)
            {
                var item   = _pool.Get();
                int day    = reward.RequiredStreakDays;
                bool claimed = _progress?.ClaimedStreakDays?.Contains(day) ?? false;
                bool canClaim = (_progress?.CurrentStreakDays ?? 0) >= day && !claimed;

                item.Setup(
                    streakReward: reward,
                    isClaimed:    claimed,
                    canClaim:     canClaim,
                    onClaim:      () => OnClaimStreak(day)
                );
            }
        }

        private async void OnClaimStreak(int day)
        {
            Loading.ShowTransparentPanel();
            try
            {
                var result = await LimitedTimeEventService.ClaimStreakReward(day, _eventId, _chainId);
                if (!_isActive) return;

                if (result.Success)
                {
                    Message.ShowReward($"Day {day} streak reward claimed!", "");
                    // Parent screen refreshes via event
                }
                else
                {
                    Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
                }
            }
            finally
            {
                Loading.HideAllPanels();
            }
        }
    }
}
