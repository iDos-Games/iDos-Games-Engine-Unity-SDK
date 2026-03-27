using IDosGames.TitlePublicConfiguration;
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

        private readonly List<ItemOrCurrency> _rewards = new();

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
            _ = UserService.GetUserInventory();
            Loading.UnblockTouch();
			ShowRewardMessage(currentSectorIndex);
		}

        private void InitializeRewardsData()
        {
            var items = IDosGamesData.Config.TitlePublicConfiguration?.SecondarySpinRewards;
            if (items == null) return;

            foreach (var item in items)
            {
                _rewards.Add(item.Reward);
            }
        }

        private void SetSpinSectorItems(List<ItemOrCurrency> rewards)
        {
            for (int i = 0; i < _spinSectorItems.Length; i++)
            {
                int sectorIndex = _spinSectorItems[i].SectorIndex - 1;
                var reward = rewards[sectorIndex];

                var imagePath = reward.ImagePath;
                var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH)
                    ? IDosGamesData.Config.Currencies.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH
                    : imagePath;

                int amount = (int)(reward.Amount ?? 0);
                _spinSectorItems[i].Set(iconPath, amount);
            }
        }

        public void ResetSpinButton(Action<bool> action)
		{
            long ticketsAmount = DataService.GetVirtualCurrencyAmount(VirtualCurrencyID.SS);

			bool showAd = IsNeedToShowAd(ticketsAmount, DataService.SecondarySpinTicketRechargeMax);

			_spinButton.Set(() => action?.Invoke(showAd), ticketsAmount, showAd);
		}

        private void ShowRewardMessage(int currentSectorIndex)
        {
            var reward = _rewards[currentSectorIndex];
            var imagePath = reward.ImagePath;
            var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH)
                ? IDosGamesData.Config.Currencies.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH
                : imagePath;
            var amount = "x" + (reward.Amount ?? 0).ToString();
            Message.ShowReward(amount, iconPath);
        }

        private bool IsNeedToShowAd(long ticketsAmount, long maxTickets)
		{
			if (DataService.HasVIPStatus)
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
