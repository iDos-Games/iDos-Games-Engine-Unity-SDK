using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class SpinWindowView : MonoBehaviour
	{
		[SerializeField] private SpinWheel _spinWheel;
		[SerializeField] private SpinViewSwitcher _spinViewSwitcher;
		[SerializeField] private SpinButtonsSwitcher _spinButtonsSwitcher;
		[SerializeField] private SpinSectorItem[] _spinSectorItems;

		private readonly List<JObject> _standardRewards = new();
		private readonly List<JObject> _premiumRewards = new();

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
			SetSpinSectorItems(_standardRewards);
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
			string data = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.SpinRewards);
			var items = JsonConvert.DeserializeObject<List<JObject>>(data);

			foreach (var item in items)
			{
				_standardRewards.Add((JObject)item[JsonProperty.SPIN_STANDARD]);
				_premiumRewards.Add((JObject)item[JsonProperty.SPIN_PREMIUM]);
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

		public void ResetSpinButtonsListener(Action<SpinTicketType> action)
		{
			_spinButtonsSwitcher.ResetListeners(action);
		}

		public void SwitchRewards(SpinTicketType type)
		{
			var rewards = type == SpinTicketType.Standard ? _standardRewards : _premiumRewards;
			SetSpinSectorItems(rewards);
		}

		public void ShowSpinView(SpinTicketType type)
		{
			_spinViewSwitcher.gameObject.SetActive(type != SpinTicketType.Free);
			_spinButtonsSwitcher.Switch(type);
		}

		private void ShowRewardMessage(int currentSectorIndex)
		{
			var currentView = _spinViewSwitcher.CurrentSpinView;
			var rewards = currentView == SpinTicketType.Standard ? _standardRewards : _premiumRewards;
			var imagePath = rewards[currentSectorIndex][JsonProperty.IMAGE_PATH].ToString();
            var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;

            var amount = "x" + rewards[currentSectorIndex][JsonProperty.AMOUNT].ToString();

			Message.ShowReward(amount, iconPath);
		}
	}
}