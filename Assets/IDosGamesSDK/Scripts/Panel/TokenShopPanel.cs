using System;
using UnityEngine;

namespace IDosGames
{
	public class TokenShopPanel : ShopPanel
	{
		private const string PANEL_CLASS = ServerItemClass.IGT;

		[SerializeField] private ShopItem _itemPrefab;
		[SerializeField] private Transform _content;

		public override void InitializePanel()
		{
			var products = ShopSystem.ProductsForRealMoney;

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

				var title = VirtualCurrencyPrices.ConverRMtoIGTwithDivider(price).ToString("N0") + " Token";

				Action onclickCalback = () => ShopSystem.BuyForRealMoney(itemID);

				productItem.Fill(onclickCalback, title, $"${price}", icon);
			}
		}

	}
}