using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace IDosGames
{
	public class SpecialOfferShopPanel : ShopPanel
	{
		[SerializeField] private ShopSpecialOfferItem _itemPrefab;
		[SerializeField] private Transform _content;

		public static event Action PanelInitialized;

        private bool _initialized;

        public override async void InitializePanel()
        {
            if (_initialized) return;

            var products = IDosGamesData.Config.TitlePublicConfiguration.ShopSpecialProducts;
            if (products == null) return;

            var playerDataSpecialPurchases = DataService.GetCachedCustomUserData(CustomUserDataKey.special_offer_amount_purchases);
            JArray arrayOfSpecialPurchases = JsonConvert.DeserializeObject<JArray>(playerDataSpecialPurchases);
            arrayOfSpecialPurchases ??= new JArray();

            foreach (var product in products)
            {
                var itemID = product.ItemID;
                var specialOfferType = product.Type;
                bool hideItem = false;

                switch (specialOfferType)
                {
                    case SpecialProductType.QuantityLimitedForPlayer:
                        hideItem = IsPlayerQuantityLimitPassed(product, itemID, arrayOfSpecialPurchases);
                        break;
                    case SpecialProductType.TimeQuantityLimitedForPlayer:
                        hideItem = IsTimeLimitPassed(product) || IsPlayerQuantityLimitPassed(product, itemID, arrayOfSpecialPurchases);
                        break;
                    case SpecialProductType.TimeLimited:
                        hideItem = IsTimeLimitPassed(product);
                        break;
                    case SpecialProductType.Unlimited:
                        hideItem = false;
                        break;
                    case SpecialProductType.QuantityLimited:
                        hideItem = true;
                        break;
                }

                if (hideItem) continue;

                var productItem = Instantiate(_itemPrefab, _content);
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
                    () => ShopSystem.BuySpecialItem(itemID, currencyID, price),
                    title, $"{price}", currencyIcon);

                productItem.Fill(onclickCalback, title, $"{price:N0}", icon, currencyIcon);

                var quantityLeftText = GetQuantityForPlayerLeftText(product, itemID, arrayOfSpecialPurchases);
                var endDate = GetProductEndDate(product);
                productItem.SetLimitView(quantityLeftText, endDate, specialOfferType);
            }

            _initialized = true;
            PanelInitialized?.Invoke();
        }

        private bool IsPlayerQuantityLimitPassed(ShopSpecialProduct product, string itemID, JArray arrayOfSpecialPurchases)
        {
            if (arrayOfSpecialPurchases == null) return false;

            foreach (var purchase in arrayOfSpecialPurchases)
            {
                if ($"{purchase[JsonProperty.ITEM_ID]}" != itemID) continue;

                int.TryParse($"{purchase[JsonProperty.AMOUNT]}", out int playerPurchasesAmount);
                return playerPurchasesAmount >= (product.QuantityLimit ?? 0);
            }

            return false;
        }

        private string GetQuantityForPlayerLeftText(ShopSpecialProduct product, string itemID, JArray arrayOfSpecialPurchases)
        {
            int playerPurchasesAmount = 0;
            int quantityLimit = product.QuantityLimit ?? 0;

            foreach (var purchase in arrayOfSpecialPurchases)
            {
                if ($"{purchase[JsonProperty.ITEM_ID]}" != itemID) continue;
                int.TryParse($"{purchase[JsonProperty.AMOUNT]}", out playerPurchasesAmount);
            }

            return $"{playerPurchasesAmount}/{quantityLimit}";
        }

        private bool IsTimeLimitPassed(ShopSpecialProduct product)
        {
            return GetProductEndDate(product) < DateTime.UtcNow;
        }

        private DateTime GetProductEndDate(ShopSpecialProduct product)
        {
            return product.EndDate?.ToUniversalTime() ?? DateTime.MinValue;
        }
    }
}