using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace IDosGames
{
	public class VIPPanel : ShopPanel
	{
		private const string PANEL_CLASS = ServerItemClass.VIP;

		[SerializeField] private VIPItem _itemForRM;
		[SerializeField] private VIPItem _itemForVirtualCurrency;

		public override void InitializePanel()
		{
			var RMProducts = ShopSystem.ProductsForRealMoney;
			var VCProducts = ShopSystem.ProductsForVirtualCurrency;

			if (RMProducts != null)
			{
				InitializeRMProduct(RMProducts);
			}

			if (VCProducts != null)
			{
				InitializeVCProduct(VCProducts);
			}
		}

		private void InitializeRMProduct(JArray RMProducts)
		{
			foreach (var product in RMProducts)
			{
				var itemClass = $"{product[JsonProperty.ITEM_CLASS]}";

				if (itemClass != PANEL_CLASS)
				{
					continue;
				}

				var itemID = $"{product[JsonProperty.ITEM_ID]}";

				var price = GetPriceInRealMoney($"{product[JsonProperty.PRICE_RM]}");

				_itemForRM.Fill(() => ShopSystem.BuyForRealMoney(itemID), $"${price}");

				break;
			}
		}

		private async void InitializeVCProduct(JArray VCProducts)
		{
			foreach (var product in VCProducts)
			{
				var itemClass = $"{product[JsonProperty.ITEM_CLASS]}";

				if (itemClass != PANEL_CLASS)
				{
					continue;
				}

				var itemID = $"{product[JsonProperty.ITEM_ID]}";

				var price = GetPriceInRealMoney($"{product[JsonProperty.PRICE_RM]}");

                string currencyImagePath = product[JsonProperty.CURRENCY_IMAGE_PATH].ToString();
                var currencyIconPath = (currencyImagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : currencyImagePath;
                var currencyIcon = await ImageLoader.GetSpriteAsync(currencyIconPath);

                var currencyID = GetVirtualCurrencyID($"{product[JsonProperty.CURRENCY_ID]}");

				price = GetPriceInVirtualCurrency(price, currencyID);

				Action onclickCalback = () => ShopSystem.PopUpSystem.ShowConfirmationPopUp(() => ShopSystem.BuyForVirtualCurrency(itemID, currencyID, price), product[JsonProperty.NAME].ToString(), $"{price}", currencyIcon);

				_itemForVirtualCurrency.Fill(onclickCalback, $"{price:N0}");

				break;
			}
		}
	}
}