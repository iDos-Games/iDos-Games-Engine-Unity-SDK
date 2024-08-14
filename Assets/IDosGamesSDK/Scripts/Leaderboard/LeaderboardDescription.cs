using Newtonsoft.Json.Linq;
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

		public void Initialize(JObject leaderboardData)
		{
			if (leaderboardData == null)
			{
				return;
			}

			SetTitle($"{leaderboardData[JsonProperty.NAME]}");
			SetFrequency($"{leaderboardData[JsonProperty.FREQUENCY]}");
			SetRewards((JArray)leaderboardData[JsonProperty.ITEMS_TO_GRANT]);
		}

		private void SetTitle(string title)
		{
			_title.text = title;
		}

		private void SetFrequency(string frequency)
		{
			_frequency.text = frequency;
		}

		private void SetRewards(JArray rankRewards)
		{
			foreach (Transform child in _rewardsParent)
			{
				Destroy(child.gameObject);
			}

			foreach (var rankReward in rankRewards)
			{
				var reward = Instantiate(_rankRewardPrefab, _rewardsParent);
				reward.Set($"{rankReward[JsonProperty.RANK]}", (JArray)rankReward[JsonProperty.ITEMS_TO_GRANT]);
			}

		}
	}
}