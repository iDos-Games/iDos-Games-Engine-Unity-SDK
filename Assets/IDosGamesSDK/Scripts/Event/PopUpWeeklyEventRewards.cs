using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class PopUpWeeklyEventRewards : PopUp
	{
		[SerializeField] private RewardItem _rewardItem;
		[SerializeField] private Transform _parent;
		[SerializeField] private Button _buttonContinue;

		private readonly Dictionary<string, int> _virtualCurrencies = new();
		private readonly Dictionary<string, int> _items = new();
		private readonly Dictionary<string, string> _imagePathes = new();

		private void Awake()
		{
			CheckAndResetContinueButton();
		}

		public void ShowRewards(List<JToken> AllRewards)
		{
			ClearDictionaries();
			InitializeRewards(AllRewards);
			InstantiateRewards();

			SetActivatePopUp(true);
		}

		public void OnClickContinue()
		{
			WeeklyEventSystem.UpdateEventForPlayer();
			SetActivatePopUp(false);
		}

		private void CheckAndResetContinueButton()
		{
			if (_buttonContinue != null)
			{
				_buttonContinue.onClick.RemoveAllListeners();
				_buttonContinue.onClick.AddListener(OnClickContinue);
				_buttonContinue.interactable = true;
			}
			else
			{
				OnClickContinue();
			}
		}

		private void SetActivatePopUp(bool active)
		{
			gameObject.SetActive(active);
		}

		private void InitializeRewards(List<JToken> AllRewards)
		{
			var playerRewards = AllRewards.Where(x => int.Parse($"{x[JsonProperty.POINTS]}") <= WeeklyEventSystem.PlayerPoints);

			foreach (var playerReward in playerRewards)
			{
				AddRewardToDictionaries(playerReward[JsonProperty.STANDARD]);

				if (UserInventory.HasVIPStatus)
				{
					AddRewardToDictionaries(playerReward[JsonProperty.PREMIUM]);
				}
			}
		}

		private void ClearDictionaries()
		{
			_virtualCurrencies.Clear();
			_items.Clear();
			_imagePathes.Clear();
		}

		private void InstantiateRewards()
		{
			foreach (Transform child in _parent)
			{
				Destroy(child.gameObject);
			}

			int amount;
			string imagePath;

			foreach (var rewardItem in _items)
			{
				var item = Instantiate(_rewardItem, _parent);

				amount = rewardItem.Value;
				imagePath = _imagePathes[rewardItem.Key];
				item.Set(imagePath, amount);
			}

			foreach (var rewardItem in _virtualCurrencies)
			{
				var item = Instantiate(_rewardItem, _parent);

				amount = rewardItem.Value;
				imagePath = _imagePathes[rewardItem.Key];
				item.Set(imagePath, amount);
			}
		}

		private void AddRewardToDictionaries(JToken reward)
		{
			string itemType = $"{reward[JsonProperty.TYPE]}";
			string currencyID;
			string itemID;
			int itemAmount;

			if (itemType == $"{IGSItemType.VirtualCurrency}")
			{
				currencyID = $"{reward[JsonProperty.CURRENCY_ID]}";
				itemAmount = int.Parse($"{reward[JsonProperty.AMOUNT]}");

                string imagePath = reward[JsonProperty.IMAGE_PATH].ToString();
                string iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;

                if (_imagePathes.ContainsKey(currencyID) == false)
				{
					_imagePathes[currencyID] = iconPath;
				}

				if (_virtualCurrencies.ContainsKey(currencyID))
				{
					_virtualCurrencies[currencyID] += itemAmount;
				}
				else
				{
					_virtualCurrencies[currencyID] = itemAmount;
				}
			}
			else if (itemType == $"{IGSItemType.Item}")
			{
				itemID = $"{reward[JsonProperty.ITEM_ID]}";
				itemAmount = int.Parse($"{reward[JsonProperty.AMOUNT]}");

                string imagePath = reward[JsonProperty.IMAGE_PATH].ToString();
                string iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;

                if (_imagePathes.ContainsKey(itemID) == false)
				{
					_imagePathes[itemID] = iconPath;
				}

				if (_items.ContainsKey(itemID))
				{
					_items[itemID] += itemAmount;
				}
				else
				{
					_items[itemID] = itemAmount;
				}
			}
		}
	}
}
