using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class WeeklyEventItem : Item
	{
		[SerializeField] private Slider _levelSlider;
		[SerializeField] private TMP_Text _levelText;

		[Header("Free Reward")]
		[SerializeField] private RewardItem _freeRewardItem;
		[SerializeField] private Image _freeRewardCheckMark;

		[Header("VIP Reward")]
		[SerializeField] private RewardItem _vipRewardItem;
		[SerializeField] private Image _vipRewardCheckMark;
		[SerializeField] private Image _lockIcon;

		public void Set(JToken Reward)
		{
			int rewardPoints = int.Parse($"{Reward[JsonProperty.POINTS]}");
			bool isCompleted = WeeklyEventSystem.PlayerPoints >= rewardPoints;

			JToken standardReward = Reward[JsonProperty.STANDARD];
			JToken premiumReward = Reward[JsonProperty.PREMIUM];

			_freeRewardItem.Set($"{standardReward[JsonProperty.IMAGE_PATH]}",
				int.Parse($"{standardReward[JsonProperty.AMOUNT]}"));

			_freeRewardCheckMark.gameObject.SetActive(isCompleted);

			_vipRewardItem.Set($"{premiumReward[JsonProperty.IMAGE_PATH]}",
				int.Parse($"{premiumReward[JsonProperty.AMOUNT]}"));

			_vipRewardCheckMark.gameObject.SetActive(isCompleted && UserInventory.HasVIPStatus);
			_lockIcon.gameObject.SetActive(UserInventory.HasVIPStatus == false);

			_levelSlider.value = isCompleted ? 1 : 0;
			_levelText.text = $"{Reward[JsonProperty.ID]}";
		}
	}
}
