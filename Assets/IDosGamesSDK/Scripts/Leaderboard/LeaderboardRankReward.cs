using TMPro;
using UnityEngine;
using System.Collections.Generic;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class LeaderboardRankReward : MonoBehaviour
    {
        [SerializeField] private Transform _rewardsParent;
        [SerializeField] private RewardItem _rewardPrefab;
        [SerializeField] private TMP_Text _rankText;

        public void Set(string rank, List<ItemOrCurrency> rewards)
        {
            SetRankText(rank);
            SetRewards(rewards);
        }

        private void SetRankText(string rank)
        {
            _rankText.text = rank;
        }

        private void SetRewards(List<ItemOrCurrency> rewards)
        {
            foreach (Transform child in _rewardsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var reward in rewards)
            {
                var rewardItem = Instantiate(_rewardPrefab, _rewardsParent);
                int amount = reward.Amount ?? 0;
                rewardItem.Set(reward.ImagePath, amount);
            }
        }
    }
}