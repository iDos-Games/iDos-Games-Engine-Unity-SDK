using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class LeaderboardRankReward : MonoBehaviour
	{
		[SerializeField] private Transform _rewardsParent;
		[SerializeField] private RewardItem _rewardPrefab;
		[SerializeField] private TMP_Text _rankText;

		public void Set(string rank, JArray rewards)
		{
			SetRankText(rank);
			SetRewards(rewards);
		}

		private void SetRankText(string rank)
		{
			_rankText.text = rank;
		}

		private void SetRewards(JArray rewards)
		{
			foreach (Transform child in _rewardsParent)
			{
				Destroy(child.gameObject);
			}

			foreach (var reward in rewards)
			{
				var rewardItem = Instantiate(_rewardPrefab, _rewardsParent);
				int.TryParse($"{reward[JsonProperty.AMOUNT]}", out int amount);
				rewardItem.Set($"{reward[JsonProperty.IMAGE_PATH]}", amount);
			}
		}
	}
}