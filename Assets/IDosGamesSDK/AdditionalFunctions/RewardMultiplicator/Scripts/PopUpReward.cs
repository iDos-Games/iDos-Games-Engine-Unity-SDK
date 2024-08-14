using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class PopUpReward : PopUp
    {
        [SerializeField] private TMP_Text _coinRewardAmount;
        [SerializeField] private TMP_Text _eventRewardAmount;
        [SerializeField] private TMP_Text _skinProfitAmount;

        private void OnEnable()
        {
            UpdateSkinProfitAmountText();
            Show();
        }

        public void Show()
        {
            UpdateCoinRewardAmountText(RewardMultiplicator._currentCoinReward);
            UpdateEventRewardAmountText(RewardMultiplicator._currentEventReward);

            SetActivatePopUp(true);
        }

        public void ShowInterstitial()
        {
            if (AdMediation.Instance != null)
            {
                if (!UserInventory.HasVIPStatus)
                {
                    AdMediation.Instance.ShowInterstitialAd();
                }
            }
        }

        private void UpdateSkinProfitAmountText()
        {
            _skinProfitAmount.text = ClaimRewardSystem.GetSkinProfitAmount().ToString();
        }

        private void UpdateEventRewardAmountText(int amount)
        {
            _eventRewardAmount.text = amount.ToString();
        }

        private void UpdateCoinRewardAmountText(int amount)
        {
            _coinRewardAmount.text = amount.ToString();
        }

        public void SetActivatePopUp(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}