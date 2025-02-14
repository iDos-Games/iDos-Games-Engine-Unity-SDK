using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class SpecialOfferShopPanel : ShopPanel
	{
		[SerializeField] private ShopSpecialOfferItem _itemPrefab;
		[SerializeField] private Transform _content;

		public static event Action PanelInitialized;

        private void OnEnable()
        {
            StartCoroutine(CallInitializePanel());
        }

        IEnumerator CallInitializePanel()
        {
            yield return new WaitForSecondsRealtime(0.3f);
            InitializePanel();
			yield return null;
        }

        public override async void InitializePanel()
		{
			var products = ShopSystem.SpecialOfferProducts;

			if (products == null)
			{
				return;
			}

			foreach (Transform child in _content)
			{
				child.gameObject.SetActive(false);
				Destroy(child.gameObject);
			}

			var playerDataSpecialPurchases = UserDataService.GetCachedCustomUserData(CustomUserDataKey.special_offer_amount_purchases);

			JArray arrayOfSpecialPurchases = JsonConvert.DeserializeObject<JArray>(playerDataSpecialPurchases);
			arrayOfSpecialPurchases ??= new JArray();

			foreach (var product in products)
			{
				var itemID = $"{product[JsonProperty.ITEM_ID]}";

				Enum.TryParse($"{product[JsonProperty.TYPE]}", out SpecialOfferType specialOfferType);

				bool hideItem = false;

				switch (specialOfferType)
				{
					case SpecialOfferType.QuantityLimitedForPlayer:
						hideItem = IsPlayerQuantityLimitPassed(product, itemID, arrayOfSpecialPurchases);
						break;
					case SpecialOfferType.TimeQuantityLimitedForPlayer:
						hideItem = IsTimeLimitPassed(product) || IsPlayerQuantityLimitPassed(product, itemID, arrayOfSpecialPurchases);
						break;
					case SpecialOfferType.TimeLimited:
						hideItem = IsTimeLimitPassed(product);
						break;
					case SpecialOfferType.Unlimited:
						hideItem = false;
						break;
					case SpecialOfferType.QuantityLimited:
						hideItem = true;
						break;
				}

				if (hideItem)
				{
					continue;
				}

				var productItem = Instantiate(_itemPrefab, _content);

				var price = GetPriceInRealMoney($"{product[JsonProperty.PRICE_RM]}");

                string imagePath = product[JsonProperty.IMAGE_PATH].ToString();
                var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;
                var icon = await ImageLoader.GetSpriteAsync(iconPath);

                var title = $"{product[JsonProperty.NAME]}";

                string currencyImagePath = product[JsonProperty.CURRENCY_IMAGE_PATH].ToString();
                var currencyIconPath = (currencyImagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : currencyImagePath;
                var currencyIcon = await ImageLoader.GetSpriteAsync(currencyIconPath);

                var currencyID = GetVirtualCurrencyID($"{product[JsonProperty.CURRENCY_ID]}");

				price = GetPriceInVirtualCurrency(price, currencyID);

				Action onclickCalback = () => ShopSystem.PopUpSystem.ShowConfirmationPopUp(() => ShopSystem.BuySpecialItem(itemID, currencyID, price), title, $"{price}", currencyIcon);

				productItem.Fill(onclickCalback, title, $"{price:N0}", icon, currencyIcon);

				var quantityLeftText = GetQuantityForPlayerLeftText(product, itemID, arrayOfSpecialPurchases);
				var endDate = GetProductEndDate(product);
				productItem.SetLimitView(quantityLeftText, endDate, specialOfferType);
			}

			PanelInitialized?.Invoke();
		}

		private bool IsPlayerQuantityLimitPassed(JToken product, string itemID, JArray arrayOfSpecialPurchases)
		{
			bool isReached = false;

			if (arrayOfSpecialPurchases == null)
			{
				return isReached;
			}

			foreach (var purchase in arrayOfSpecialPurchases)
			{
				if ($"{purchase[JsonProperty.ITEM_ID]}" != itemID)
				{
					continue;
				}

				int.TryParse($"{product[JsonProperty.QUANTITY_LIMIT]}", out int quantityLimit);

				int.TryParse($"{purchase[JsonProperty.AMOUNT]}", out int playerPurchasesAmount);

				isReached = playerPurchasesAmount >= quantityLimit;
			}

			return isReached;
		}

		private string GetQuantityForPlayerLeftText(JToken product, string itemID, JArray arrayOfSpecialPurchases)
		{
			string quantityLeft = string.Empty;

			int playerPurchasesAmount = 0;

			int.TryParse($"{product[JsonProperty.QUANTITY_LIMIT]}", out int quantityLimit);

			foreach (var purchase in arrayOfSpecialPurchases)
			{
				if ($"{purchase[JsonProperty.ITEM_ID]}" != itemID)
				{
					continue;
				}

				int.TryParse($"{purchase[JsonProperty.AMOUNT]}", out playerPurchasesAmount);
			}

			quantityLeft = $"{playerPurchasesAmount}/{quantityLimit}";

			return quantityLeft;
		}

		private bool IsTimeLimitPassed(JToken product)
		{
			DateTime productEndDate = GetProductEndDate(product);

			return productEndDate < DateTime.UtcNow;
		}

		private DateTime GetProductEndDate(JToken product)
		{
			DateTime.TryParse($"{product[JsonProperty.END_DATE]}", out DateTime productEndDate);

            return productEndDate.ToUniversalTime();
        }
	}
}