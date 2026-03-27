using IDosGames.TitlePublicConfiguration;
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

        private readonly List<ItemOrCurrency> _standardRewards = new();
        private readonly List<ItemOrCurrency> _premiumRewards = new();

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
            _ = UserService.GetUserInventory();
            Loading.UnblockTouch();
            ShowRewardMessage(currentSectorIndex);
        }

        private void InitializeRewardsData()
        {
            var items = IDosGamesData.Config.TitlePublicConfiguration?.SpinRewards;
            if (items == null) return;

            foreach (var item in items)
            {
                _standardRewards.Add(item.Standard);
                _premiumRewards.Add(item.Premium);
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

            var imagePath = rewards[currentSectorIndex].ImagePath;
            var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH)
                ? IDosGamesData.Config.Currencies.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH
                : imagePath;

            var amount = "x" + (rewards[currentSectorIndex].Amount ?? 0).ToString();
            Message.ShowReward(amount, iconPath);
        }
    }
}
