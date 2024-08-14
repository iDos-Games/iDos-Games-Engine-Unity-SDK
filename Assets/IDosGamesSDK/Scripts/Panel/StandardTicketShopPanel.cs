using System;
using UnityEngine;


namespace IDosGames
{
	public class StandardTicketShopPanel : ShopPanel
	{
		private const string PANEL_CLASS = ServerItemClass.STANDARD_SPIN_TICKET;

		[SerializeField] private ShopItem _itemPrefab;
		[SerializeField] private Transform _content;

		public override void InitializePanel()
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

				var icon = Resources.Load<Sprite>(product[JsonProperty.IMAGE_PATH].ToString());

				var title = $"{product[JsonProperty.NAME]}";

				var currencyIcon = Resources.Load<Sprite>(product[JsonProperty.CURRENCY_IMAGE_PATH].ToString());

				var currencyID = GetVirtualCurrencyID($"{product[JsonProperty.CURRENCY_ID]}");

				price = GetPriceInVirtualCurrency(price, currencyID);

				Action onclickCalback = () => ShopSystem.PopUpSystem.ShowConfirmationPopUp(() => ShopSystem.BuyForVirtualCurrency(itemID, currencyID, price), title, $"{price}", currencyIcon);

				productItem.Fill(onclickCalback, title, $"{price:N0}", icon, currencyIcon);
			}
		}
	}
}