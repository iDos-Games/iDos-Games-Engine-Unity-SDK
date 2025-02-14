using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class SecondarySpinPanelView : MonoBehaviour
	{
		[SerializeField] private SpinWheel _spinWheel;
		public SpinWheel SpinWheel => _spinWheel;

		[SerializeField] private SecondarySpinButton _spinButton;
		[SerializeField] private SpinSectorItem[] _spinSectorItems;

		private readonly List<JObject> _rewards = new();

		private void OnEnable()
		{
			_spinWheel.SpinStarted += OnSpinStarted;
			_spinWheel.SpinEnded += OnSpinEnded;
		}

		private void OnDisable()
		{
			_spinWheel.SpinStarted -= OnSpinStarted;
			_spinWheel.SpinEnded -= OnSpinEnded;
		}

		private void Start()
		{
			InitializeRewardsData();
			SetSpinSectorItems(_rewards);
		}

		private void OnSpinStarted()
		{
			Loading.BlockTouch();
		}

		private void OnSpinEnded(int currentSectorIndex)
		{
			UserDataService.RequestUserAllData();
			Loading.UnblockTouch();
			ShowRewardMessage(currentSectorIndex);
		}

		private void InitializeRewardsData()
		{
			string data = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.SecondarySpinRewards);
			var items = JsonConvert.DeserializeObject<List<JObject>>(data);

			foreach (var item in items)
			{
				_rewards.Add((JObject)item[JsonProperty.REWARD]);
			}
		}

		private void SetSpinSectorItems(List<JObject> rewards)
		{
			for (int i = 0; i < _spinSectorItems.Length; i++)
			{
				int sectorIndex = _spinSectorItems[i].SectorIndex - 1;
				var rewardData = rewards[sectorIndex];

                string imagePath = rewardData[JsonProperty.IMAGE_PATH].ToString();
                var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;

                int amount = (int)rewardData[JsonProperty.AMOUNT];

				_spinSectorItems[i].Set(iconPath, amount);
			}
		}

		public void ResetSpinButton(Action<bool> action)
		{
			var ticketsAmount = UserInventory.GetVirtualCurrencyAmount(VirtualCurrencyID.SS);

			bool showAd = IsNeedToShowAd(ticketsAmount, UserInventory.SecondarySpinTicketRechargeMax);

			_spinButton.Set(() => action?.Invoke(showAd), ticketsAmount, showAd);
		}

		private void ShowRewardMessage(int currentSectorIndex)
		{
            var imagePath = _rewards[currentSectorIndex][JsonProperty.IMAGE_PATH].ToString();
            var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;

			var amount = "x" + _rewards[currentSectorIndex][JsonProperty.AMOUNT].ToString();

			Message.ShowReward(amount, iconPath);
		}

		private bool IsNeedToShowAd(int ticketsAmount, int maxTickets)
		{
			if (UserInventory.HasVIPStatus)
			{
				return false;
			}

			bool isNeed = false;

			if (ticketsAmount <= 1 || maxTickets - ticketsAmount > 0)
			{
				isNeed = true;
			}

			return isNeed;
		}
	}
}
