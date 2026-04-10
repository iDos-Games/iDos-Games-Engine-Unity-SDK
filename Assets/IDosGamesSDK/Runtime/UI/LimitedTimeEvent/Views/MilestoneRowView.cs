using System;
using System.Collections.Generic;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>
    /// One horizontal row in the milestone list.
    /// Layout: [FreeRewardCard] — [MilestoneNumber] — [PassRewardCard]
    /// </summary>
    public class MilestoneRowView : MonoBehaviour
    {
        [Header("Milestone Badge")]
        [SerializeField] private TextMeshProUGUI _milestoneNumberText;
        [SerializeField] private Image           _milestoneBadgeImage;
        [SerializeField] private Sprite          _badgeReached;
        [SerializeField] private Sprite          _badgeDefault;

        [Header("Free Column")]
        [SerializeField] private RewardCardView  _freeCard;

        [Header("Pass Column")]
        [SerializeField] private RewardCardView  _passCard;

        [Header("Progress Line")]
        [SerializeField] private Image           _progressLineImage;
        [SerializeField] private Color           _progressLineReached  = new Color(1f, 0.78f, 0f);
        [SerializeField] private Color           _progressLineDefault  = new Color(0.4f, 0.4f, 0.5f);

        // ─── Public Setup ─────────────────────────────────────────────────────

        public void Setup(
            int milestoneNumber,
            EventMilestoneDefinition definition,
            bool isClaimed,
            bool isReached,
            bool canClaim,
            bool isVip,
            string eventId,
            string chainId,
            Action onClaimSuccess)
        {
            _milestoneNumberText.text = milestoneNumber.ToString();

            bool reached = isReached || isClaimed;
            _milestoneBadgeImage.sprite = reached ? _badgeReached : _badgeDefault;
            _progressLineImage.color    = reached ? _progressLineReached : _progressLineDefault;

            // Free rewards = standard rewards (first item)
            var freeRewards = definition.Rewards;
            if (freeRewards != null && freeRewards.Count > 0)
            {
                _freeCard.gameObject.SetActive(true);
                _freeCard.Setup(
                    reward:   freeRewards[0],
                    isClaimed: isClaimed,
                    canClaim:  canClaim && !isVip,
                    isLocked:  !isReached && !isClaimed,
                    onClaim:   () => OnClaimClicked(definition.MilestoneID, eventId, chainId, onClaimSuccess)
                );
            }
            else
            {
                _freeCard.gameObject.SetActive(false);
            }

            // Pass rewards = VIP rewards (first item if available)
            List<ItemOrCurrency> vipRewards = null;
            if (isVip && definition.VipRewards != null && definition.VipRewards.Count > 0)
                vipRewards = definition.VipRewards[0].Rewards;

            if (vipRewards != null && vipRewards.Count > 0)
            {
                _passCard.gameObject.SetActive(true);
                _passCard.Setup(
                    reward:    vipRewards[0],
                    isClaimed: isClaimed,
                    canClaim:  canClaim && isVip,
                    isLocked:  !isVip || (!isReached && !isClaimed),
                    onClaim:   () => OnClaimClicked(definition.MilestoneID, eventId, chainId, onClaimSuccess)
                );
            }
            else
            {
                // Show locked pass slot
                _passCard.gameObject.SetActive(true);
                _passCard.SetupVipLocked();
            }
        }

        // ─── Claim ────────────────────────────────────────────────────────────

        private async void OnClaimClicked(string milestoneId, string eventId, string chainId, Action onSuccess)
        {
            Loading.ShowTransparentPanel();
            try
            {
                var result = await LimitedTimeEventService.ClaimMilestone(milestoneId, eventId, chainId);
                if (result.Success)
                {
                    onSuccess?.Invoke();
                    // Show reward popup if available
                    var rewards = result.Data?.StandardRewards;
                    if (rewards != null && rewards.Count > 0)
                    {
                        string imagePath = null; // ASSUMPTION: use first reward image if available
                        Message.ShowReward($"Milestone reward claimed!", imagePath ?? "");
                    }
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
