using System;
using UnityEngine;


namespace IDosGames
{
	public class PremiumTicketShopPanel : ShopPanel
	{
		private const string PANEL_CLASS = ServerItemClass.PREMIUM_SPIN_TICKET;

		[SerializeField] private ShopItem _itemPrefab;
		[SerializeField] private Transform _content;

		public override async void InitializePanel()
		{
			var products = ShopSystem.ProductsForVirtualCurrency;

			if (products == null)
			{
				return;
			}

			foreach (Transform child in _content)
			{
				Destroy(child.gameObject);
			}

			foreach (var product in products)
			{
				var itemClass = $"{product[JsonProperty.ITEM_CLASS]}";

				if (itemClass != PANEL_CLASS)
				{
					continue;
				}

				var productItem = Instantiate(_itemPrefab, _content);

				var itemID = $"{product[JsonProperty.ITEM_ID]}";

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

				Action onclickCalback = () => ShopSystem.PopUpSystem.ShowConfirmationPopUp(() => ShopSystem.BuyForVirtualCurrency(itemID, currencyID, price), title, $"{price}", currencyIcon);

				productItem.Fill(onclickCalback, title, $"{price:N0}", icon, currencyIcon);
			}
		}
	}
}