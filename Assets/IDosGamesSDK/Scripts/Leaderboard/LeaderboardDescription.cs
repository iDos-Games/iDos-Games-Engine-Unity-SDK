using IDosGames.TitlePublicConfiguration;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class LeaderboardDescription : MonoBehaviour
	{
		[SerializeField] private TMP_Text _title;
		[SerializeField] private TMP_Text _frequency;
		[SerializeField] private TMP_Text _description_1;
		[SerializeField] private TMP_Text _description_2;

		[SerializeField] private Transform _rewardsParent;
		[SerializeField] private LeaderboardRankReward _rankRewardPrefab;

        public void Initialize(Leaderboard leaderboardData)
        {
            if (leaderboardData == null)
            {
                return;
            }

            SetFrequency(leaderboardData.Frequency);
            SetTitle(leaderboardData.Name);
            SetRewards(leaderboardData.RankRewards);
        }

        private void SetTitle(string title)
		{
			_title.text = title;
		}

		private void SetFrequency(StatisticResetFrequency frequency)
		{
			_frequency.text = frequency.ToString();
		}

        private void SetRewards(List<RankReward> rankRewards)
        {
            foreach (Transform child in _rewardsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var rankReward in rankRewards)
            {
                var reward = Instantiate(_rankRewardPrefab, _rewardsParent);
                reward.Set(rankReward.Rank, rankReward.ItemsToGrant);
            }
        }
    }
}