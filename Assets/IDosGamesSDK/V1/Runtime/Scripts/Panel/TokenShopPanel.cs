using System;
using UnityEngine;

namespace IDosGames
{
	public class TokenShopPanel : ShopPanel
	{
		private const string PANEL_CLASS = ServerItemClass.IGT;

		[SerializeField] private ShopItem _itemPrefab;
		[SerializeField] private Transform _content;

        public override async void InitializePanel()
        {
            var products = IDosGamesData.Config.TitlePublicConfiguration.ProductsForRealMoney;
            if (products == null) return;

            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            foreach (var product in products)
            {
                if (product.ItemClass != PANEL_CLASS) continue;

                var productItem = Instantiate(_itemPrefab, _content);
                var itemID = product.ItemID;
                var price = GetPriceInRealMoney(product.PriceRM.ToString());

                var imagePath = product.ImagePath;
                var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH)
                    ? IDosGamesData.Config.Currencies.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH
                    : imagePath;
                var icon = await ImageLoader.GetSpriteAsync(iconPath);

                var title = VirtualCurrencyPrices.ConverRMtoIGTwithDivider(price).ToString("N0") + " Token";
                Action onclickCalback = () => ShopSystem.BuyForRealMoney(itemID);
                productItem.Fill(onclickCalback, title, $"${price}", icon);
            }
        }
    }
}