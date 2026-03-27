using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class DailyOfferShopPanel : ShopPanel
    {
        [SerializeField] private ShopFreeItem _dailyFreeOfferPrefab;
        [SerializeField] private ShopItem _dailyPaidOfferPrefab;
        [SerializeField] private Transform _content;
        [SerializeField] private DailyOffersTimer _timer;
        [SerializeField] private SpinWindow _spinWindow;
        [SerializeField] private FreeSpinButton _freeSpinButton;

        private DateTime _endDate;

        public override void InitializePanel()
        {
            var freeProducts = IDosGamesData.Config.TitlePublicConfiguration.ShopDailyFreeProducts;
            var offerData = IDosGamesData.Config.TitlePublicConfiguration.ShopDailyProducts;

            if (offerData == null) return;

            if (offerData.EndDate.HasValue)
            {
                _endDate = offerData.EndDate.Value.ToUniversalTime();
            }
            else
            {
                _endDate = DateTime.MinValue;
            }

            UpdateTimer(_endDate);

            var products = offerData.Products;
            if (products == null) return;

            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            InitializeFreeProducts(freeProducts);
            InitializePaidProducts(products);
        }

        private async void InitializeFreeProducts(List<ShopDailyFreeProduct> products)
        {
            var playerData = DataService.GetCachedCustomUserData(CustomUserDataKey.shop_daily_free_products);

            if (IsNeedUpdateDailyFreeProducts(playerData))
            {
                UpdateDailyFreeProducts();
                return;
            }

            foreach (var product in products)
            {
                if (!product.Enabled) continue;

                var productItem = Instantiate(_dailyFreeOfferPrefab, _content);
                var itemID = product.ItemID;

                var imagePath = product.ImagePath;
                var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH)
                    ? IDosGamesData.Config.Currencies.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH
                    : imagePath;
                var icon = await ImageLoader.GetSpriteAsync(iconPath);

                var title = product.Name;
                var itemClass = product.ItemClass;

                int productAmountInPlayer = GetProductAmountInPlayer(itemID, playerData);
                int productAmountInOffer = product.Amount;

                string quantityAmount = $"{productAmountInPlayer}/{productAmountInOffer}";
                productItem.View.SetQuantity(quantityAmount);

                bool isNeedShowAd = IsNeedToShowAd(productAmountInOffer, productAmountInPlayer);
                bool isNeedBlock = productAmountInPlayer < 1;

                if (isNeedBlock)
                    productItem.View.Block();
                else
                {
                    productItem.View.UnBlock();
                    productItem.View.SetActiveAddIcon(isNeedShowAd);
                }

                Action onclickCalback;

                if (itemClass == ServerItemClass.SPIN_TICKET)
                {
                    onclickCalback = () => _spinWindow.OpenFreeSpin();
                    productItem.View.DisableTextAmountToGrant();

                    Action tryToSpinAction = () => _spinWindow.TryToFreeSpin(isNeedShowAd);
                    _freeSpinButton.Set(tryToSpinAction, _endDate, quantityAmount, isNeedShowAd, isNeedBlock);
                }
                else
                {
                    onclickCalback = () => ShopSystem.TryGetDailyFreeReward(itemID, isNeedShowAd);
                    productItem.View.SetAmountToGrant(GetItemAmountToGrant(product));
                }

                productItem.Fill(onclickCalback, title, string.Empty, icon);
            }
        }

        private bool IsNeedToShowAd(int productAmountInOffer, int productAmountInPlayer)
        {
            if (DataService.HasVIPStatus)
            {
                return false;
            }

            bool isNeed = false;

            if (productAmountInOffer <= 1 || productAmountInOffer - productAmountInPlayer > 0)
            {
                isNeed = true;
            }

            return isNeed;
        }

        private string GetItemAmountToGrant(ShopDailyFreeProduct product)
        {
            if (product.ItemsToGrant != null && product.ItemsToGrant.Count > 0)
                return product.ItemsToGrant[0].Amount?.ToString() ?? string.Empty;
            return string.Empty;
        }

        private int GetProductAmountInPlayer(string itemID, string playerData)
        {
            int amount = 0;

            var jsonData = JsonConvert.DeserializeObject<JObject>(playerData);

            if ($"{jsonData}" != string.Empty)
            {
                var playerProducts = jsonData[JsonProperty.PRODUCTS];

                foreach (var product in playerProducts)
                {
                    if ($"{product[JsonProperty.ITEM_ID]}" == itemID)
                    {
                        int.TryParse($"{product[JsonProperty.AMOUNT]}", out amount);
                        break;
                    }
                }
            }

            return amount;
        }

        private void UpdateDailyFreeProducts()
        {
            _ = IGSClientAPI.ExecuteFunction
                (
                functionName: ServerFunctionHandlers.UpdateDailyFreeProducts,
                resultCallback: (result) => OnUpdateDailyFreeProducts(),
                notConnectionErrorCallback: (error) => OnErrorUpdateDailyFreeProducts(),
                connectionErrorCallback: UpdateDailyFreeProducts
                );
        }

        private void OnUpdateDailyFreeProducts()
        {
            _ = UserService.GetUserInventory();
        }

        private void OnErrorUpdateDailyFreeProducts()
        {
            Message.Show(MessageCode.FAILED_TO_UPDATE_DAILY_FREE_PRODUCTS);
        }

        private bool IsNeedUpdateDailyFreeProducts(string playerData)
        {
            if (string.IsNullOrEmpty(playerData)) return true;

            var jsonData = JsonConvert.DeserializeObject<JObject>(playerData);

            string endDateString = $"{jsonData[JsonProperty.END_DATE]}";
            if (string.IsNullOrEmpty(endDateString)) return true;

            DateTimeOffset playerLastUpdateDate = DateTimeOffset.Parse(endDateString, null, System.Globalization.DateTimeStyles.AssumeUniversal);

            return _endDate > playerLastUpdateDate.UtcDateTime;
        }

        private async void InitializePaidProducts(List<ShopDailyProduct> products)
        {
            foreach (var product in products)
            {
                var productItem = Instantiate(_dailyPaidOfferPrefab, _content);
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
                    () => ShopSystem.BuyDailyItem(itemID, currencyID, price),
                    title, $"{price}", currencyIcon);

                productItem.Fill(onclickCalback, title, $"{price:N0}", icon, currencyIcon);
            }
        }

        private void UpdateTimer(DateTime endDate)
        {
            _timer.Set(endDate);
        }
    }
}