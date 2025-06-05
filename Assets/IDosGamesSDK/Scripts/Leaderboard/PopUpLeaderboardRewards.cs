using IDosGames.TitlePublicConfiguration;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class PopUpLeaderboardRewards : PopUp
    {
        [SerializeField] private GameObject _popupClaimRewards;
        [SerializeField] private GameObject _popupRewardList;
        [SerializeField] private RewardItem _rewardItem;
        [SerializeField] private Transform _parent;
        [SerializeField] private TMP_Text _title;

        public const string DEFAULT_STATISTIC_NAME = "coin_contest";

        public void SetActivatePopUp(bool active)
        {
            gameObject.SetActive(active);
            _title.text = "Past Tournament Rewards";
            _popupClaimRewards.SetActive(true);
            _popupRewardList.SetActive(false);
        }

        public void RequestClaimTournamentReward()
        {
            IGSClientAPI.ClaimTournamentReward
            (
                statisticName: DEFAULT_STATISTIC_NAME,
                resultCallback: OnClaimTournamentReward,
                connectionErrorCallback: () =>
                {
                    RequestClaimTournamentReward();
                }
            );
        }

        private void OnClaimTournamentReward(UserLeaderboardRewards result)
        {
            // Dictionaries to store reward data
            var virtualCurrencies = new Dictionary<string, int>();
            var items = new Dictionary<string, int>();
            var imagePathes = new Dictionary<string, string>();

            // Process each reward in ItemsToGrant
            foreach (var reward in result.ItemsToGrant)
            {
                if (reward.Type == ItemType.VirtualCurrency)
                {
                    string currencyID = reward.CurrencyID;
                    int amount = reward.Amount ?? 0;
                    string imagePath = reward.ImagePath;

                    if (virtualCurrencies.ContainsKey(currencyID))
                    {
                        virtualCurrencies[currencyID] += amount;
                    }
                    else
                    {
                        virtualCurrencies[currencyID] = amount;
                        imagePathes[currencyID] = imagePath;
                    }
                }
                else if (reward.Type == ItemType.Item)
                {
                    string itemID = reward.ItemID;
                    int amount = reward.Amount ?? 0;
                    string imagePath = reward.ImagePath;

                    if (items.ContainsKey(itemID))
                    {
                        items[itemID] += amount;
                    }
                    else
                    {
                        items[itemID] = amount;
                        imagePathes[itemID] = imagePath;
                    }
                }
            }

            // Clear any existing reward items
            foreach (Transform child in _parent)
            {
                Destroy(child.gameObject);
            }

            // Instantiate rewards for virtual currencies
            foreach (var currency in virtualCurrencies)
            {
                var item = Instantiate(_rewardItem, _parent);
                string imagePath = imagePathes[currency.Key];
                int amount = currency.Value;
                item.Set(imagePath, amount);
            }

            // Instantiate rewards for items
            foreach (var itemEntry in items)
            {
                var item = Instantiate(_rewardItem, _parent);
                string imagePath = imagePathes[itemEntry.Key];
                int amount = itemEntry.Value;
                item.Set(imagePath, amount);
            }

            _title.text = result.Position == 0 ? "Your place in the Tournament: +1000" : $"Your place in the Tournament: {result.Position}";

            // Switch visibility to show the reward list
            _popupClaimRewards.SetActive(false);
            _popupRewardList.SetActive(true);
        }
    }
}
