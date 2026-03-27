using System;
using UnityEngine;

namespace IDosGames
{
	public class WKShopPanel : ShopPanel
	{
		private const string PANEL_CLASS = ServerItemClass.WITHDRAWAL_KEY;

		[SerializeField] private ShopItem _itemPrefab;
		[SerializeField] private Transform _content;

        public override async void InitializePanel()
        {
            var products = IDosGamesData.Config.TitlePublicConfiguration.ProductsForVirtualCurrency;
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

                var title = product.Name;

                var currencyImagePath = product.CurrencyImagePath;
                var currencyIconPath = (currencyImagePath == JsonProperty.TOKEN_IMAGE_PATH)
                    ? IDosGamesData.Config.Currencies.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH
                    : currencyImagePath;
                var currencyIcon = await ImageLoader.GetSpriteAsync(currencyIconPath);

                var currencyID = GetVirtualCurrencyID(product.CurrencyID);
                price = GetPriceInVirtualCurrency(price, currencyID);

                Action onclickCalback = () => ShopSystem.PopUpSystem.ShowConfirmationPopUp(
                    () => ShopSystem.BuyForVirtualCurrency(itemID, currencyID, price),
                    title, $"{price}", currencyIcon);

                productItem.Fill(onclickCalback, title, $"{price:N0}", icon, currencyIcon);
            }
        }
    }
}