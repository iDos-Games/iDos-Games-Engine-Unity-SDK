using IDosGames.TitlePublicConfiguration;
using System;
using System.Collections.Generic;
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
            var RMProducts = IDosGamesData.Config.TitlePublicConfiguration.ProductsForRealMoney;
            var VCProducts = IDosGamesData.Config.TitlePublicConfiguration.ProductsForVirtualCurrency;

            if (RMProducts != null) InitializeRMProduct(RMProducts);
            if (VCProducts != null) InitializeVCProduct(VCProducts);
        }

        private void InitializeRMProduct(List<ProductForRealMoney> RMProducts)
        {
            foreach (var product in RMProducts)
            {
                if (product.ItemClass != PANEL_CLASS) continue;

                var itemID = product.ItemID;
                var price = GetPriceInRealMoney(product.PriceRM.ToString());
                _itemForRM.Fill(() => ShopSystem.BuyForRealMoney(itemID), $"${price}");
                break;
            }
        }

        private async void InitializeVCProduct(List<ProductForVirtualCurrency> VCProducts)
        {
            foreach (var product in VCProducts)
            {
                if (product.ItemClass != PANEL_CLASS) continue;

                var itemID = product.ItemID;
                var price = GetPriceInRealMoney(product.PriceRM.ToString());

                var currencyImagePath = product.CurrencyImagePath;
                var currencyIconPath = (currencyImagePath == JsonProperty.TOKEN_IMAGE_PATH)
                    ? IDosGamesData.Config.Currencies.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH
                    : currencyImagePath;
                var currencyIcon = await ImageLoader.GetSpriteAsync(currencyIconPath);

                var currencyID = GetVirtualCurrencyID(product.CurrencyID);
                price = GetPriceInVirtualCurrency(price, currencyID);

                Action onclickCalback = () => ShopSystem.PopUpSystem.ShowConfirmationPopUp(
                    () => ShopSystem.BuyForVirtualCurrency(itemID, currencyID, price),
                    product.Name, $"{price}", currencyIcon);

                _itemForVirtualCurrency.Fill(onclickCalback, $"{price:N0}");
                break;
            }
        }
    }
}