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

            string standardImagePath = standardReward[JsonProperty.IMAGE_PATH].ToString();
            string standardIconPath = (standardImagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : standardImagePath;

            _freeRewardItem.Set(standardIconPath, int.Parse($"{standardReward[JsonProperty.AMOUNT]}"));
            _freeRewardCheckMark.gameObject.SetActive(isCompleted);

            string premiumImagePath = premiumReward[JsonProperty.IMAGE_PATH].ToString();
            string premiumIconPath = (premiumImagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : premiumImagePath;

            _vipRewardItem.Set(premiumIconPath, int.Parse($"{premiumReward[JsonProperty.AMOUNT]}"));
            _vipRewardCheckMark.gameObject.SetActive(isCompleted && UserInventory.HasVIPStatus);

            _lockIcon.gameObject.SetActive(!UserInventory.HasVIPStatus);
            _levelSlider.value = isCompleted ? 1 : 0;
            _levelText.text = $"{Reward[JsonProperty.ID]}";
        }
    }
}
