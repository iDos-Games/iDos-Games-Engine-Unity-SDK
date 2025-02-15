using IDosGames.ClientModels;
using IDosGames.CloudScriptModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace IDosGames
{
    public class ShopSystem : MonoBehaviour
    {
        private static ShopSystem _instance;

        private static string PLAYER_PREFS_AFTER_IAP_NOT_GRANTED_PRODUCT { get; set; }
        private const int MAX_ATTEMPTS_COUNT_FOR_GRANT_IAP_ITEMS = 3;
        private static int _attemptsCountForGrantIAPItemsAfterError = 0;

        [SerializeField] private ShopPopUpSystem _popUpSystem;
        public static ShopPopUpSystem PopUpSystem => _instance._popUpSystem;

        public static JArray ProductsForRealMoney { get; private set; }
        public static JArray ProductsForVirtualCurrency { get; private set; }
        public static JArray SpecialOfferProducts { get; private set; }
        public static JObject DailyOfferData { get; private set; }
        public static JArray DailyFreeProducts { get; private set; }

        private static string _iapProcessedProductID;
        private static string _freeProductID;
        public static string _payload;

        private void Awake()
        {
            if (_instance == null || ReferenceEquals(this, _instance))
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            PLAYER_PREFS_AFTER_IAP_NOT_GRANTED_PRODUCT = "NOT_GRANTED_IAP_PRODUCT" + AuthService.UserID;
            CheckForItemsGrantedAfterIAPPurchase();
        }

        private void OnEnable()
        {
            VirtualCurrencyPrices.PricesUpdated += UpdateProductsData;
            IAPValidator.PurchaseValidated += GrantItemsAfterIAPPurchase;
        }

        private void OnDisable()
        {
            VirtualCurrencyPrices.PricesUpdated -= UpdateProductsData;
            IAPValidator.PurchaseValidated -= GrantItemsAfterIAPPurchase;
        }

        private void Start()
        {
            WebFunctionHandler.Instance.OnAdCompleteEvent += WebAdComplete;
        }

        private void OnDestroy()
        {
            WebFunctionHandler.Instance.OnAdCompleteEvent -= WebAdComplete;
        }

        public static void BuyForRealMoney(string ID)
        {
#if IDOSGAMES_MOBILE_IAP
            IAPService.PurchaseProductByID(ID);
#endif
            _iapProcessedProductID = ID;

#if UNITY_WEBGL
            BuyWithPlatformCurrency();
#endif
        }

#if UNITY_WEBGL
        public static async void BuyWithPlatformCurrency()
        {
            if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Telegram)
            {
                Loading.ShowTransparentPanel();

                Product product = UserDataService.GetProductForRealMoney(_iapProcessedProductID);
                float starPrice = (float)IDosGamesSDKSettings.Instance.PlatformCurrencyPriceInCent;

                int priceRM = int.Parse(product.PriceRM);
                int price = (int)Math.Round(priceRM / starPrice);

                _payload = IDosGamesSDKSettings.Instance.TitleID + "-|-" + AuthService.UserID + "-|-" + _iapProcessedProductID + "-|-" + Guid.NewGuid();

                string invoiceLink = await AdditionalIAPService.CreateTelegramInvoice(product.Name, product.Name, _payload, null, "XTR", price);
                Loading.HideAllPanels();
                WebSDK.OpenInvoiceLink(invoiceLink);
            }
        }
#endif

        public static void BuyForVirtualCurrency(string ID, VirtualCurrencyID currencyID, float price)
        {
            if (UserInventory.GetVirtualCurrencyAmount(currencyID.ToString()) < price)
            {
                //Message.Show(MessageCode.NOT_ENOUGH_FUNDS);

                if (currencyID == VirtualCurrencyID.IG)
                {
                    PopUpSystem.ShowTokenPopUp();
                }
                else if (currencyID == VirtualCurrencyID.CO)
                {
                    PopUpSystem.ShowCoinPopUp();
                }

                return;
            }

            FunctionParameters parameter = new()
            {
                ItemID = ID,
            };

            _ = IGSClientAPI.ExecuteFunction
            (
                functionName: ServerFunctionHandlers.BuyItemForVirtualCurrency,
                resultCallback: OnSuccessPurchase,
                notConnectionErrorCallback: OnErrorPurchase,
                functionParameter: parameter
            );
        }

        private static void OnSuccessPurchase(string result)
        {
            if (result == null)
            {
                OnErrorPurchase(null);
            }
            else
            {
                JObject resultData = JsonConvert.DeserializeObject<JObject>(result.ToString());
                if (resultData[JsonProperty.MESSAGE_KEY] != null)
                {
                    Message.Show(resultData[JsonProperty.MESSAGE_KEY].ToString());
                    UserDataService.RequestUserAllData();
                }
            }
        }

        public static void BuySpecialItem(string ID, VirtualCurrencyID currencyID, float price)
        {
            if (UserInventory.GetVirtualCurrencyAmount(currencyID.ToString()) < price)
            {
                //Message.Show(MessageCode.NOT_ENOUGH_FUNDS);

                if (currencyID == VirtualCurrencyID.IG)
                {
                    PopUpSystem.ShowTokenPopUp();
                }
                else if (currencyID == VirtualCurrencyID.CO)
                {
                    PopUpSystem.ShowCoinPopUp();
                }

                return;
            }

            FunctionParameters parameter = new()
            {
                ItemID = ID,
            };

            _ = IGSClientAPI.ExecuteFunction
            (
                functionName: ServerFunctionHandlers.BuyItemSpecialOffer,
                resultCallback: OnSuccessSpecialItemPurchase,
                notConnectionErrorCallback: OnErrorPurchase,
                functionParameter: parameter
            );
        }

        private static void OnSuccessSpecialItemPurchase(string result)
        {
            OnSuccessPurchase(result);
        }

        public static void BuyDailyItem(string ID, VirtualCurrencyID currencyID, float price)
        {
            if (UserInventory.GetVirtualCurrencyAmount(currencyID.ToString()) < price)
            {
                //Message.Show(MessageCode.NOT_ENOUGH_FUNDS);

                if (currencyID == VirtualCurrencyID.IG)
                {
                    PopUpSystem.ShowTokenPopUp();
                }
                else if (currencyID == VirtualCurrencyID.CO)
                {
                    PopUpSystem.ShowCoinPopUp();
                }

                return;
            }

            FunctionParameters parameter = new()
            {
                ItemID = ID,
            };

            _ = IGSClientAPI.ExecuteFunction
           (
               functionName: ServerFunctionHandlers.BuyItemDailyOffer,
               resultCallback: OnSuccessPurchase,
               notConnectionErrorCallback: OnErrorPurchase,
               functionParameter: parameter
           );
        }

        public static void TryGetDailyFreeReward(string ID, bool showAd)
        {
            _freeProductID = ID;

            if (showAd && !UserInventory.HasVIPStatus) //&& AdMediation.Instance != null
            {
                if (AdMediation.Instance != null)
                {
                    if (AdMediation.Instance.ShowRewardedVideo(GetDailyFreeReward))
                    {
                        Debug.Log("Show rewarded video.");
                    }
                    else
                    {
                        ///Message.Show("Ad is not ready.");
                        PopUpSystem.ShowVIPPopUp();
                    }
                }
                else
                {
                    if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
                    {
#if UNITY_WEBGL
                        WebFunctionHandler.Instance.ShowAd(IDosGamesSDKSettings.Instance.AdsGramBlockID.ToString(), ID);
#endif
                    }
                    else
                    {
                        PopUpSystem.ShowVIPPopUp();
                    }
                }
            }
            else
            {
                GetDailyFreeReward();
            }
        }

        private void WebAdComplete(string args)
        {
            if (args == _freeProductID)
            {
                GetDailyFreeReward();
            }

        }

        private static void CheckForItemsGrantedAfterIAPPurchase()
        {
            if (PlayerPrefs.HasKey(PLAYER_PREFS_AFTER_IAP_NOT_GRANTED_PRODUCT))
            {
                _iapProcessedProductID = PlayerPrefs.GetString(PLAYER_PREFS_AFTER_IAP_NOT_GRANTED_PRODUCT);
                GrantItemsAfterIAPPurchase(_iapProcessedProductID);
            }
        }

        private void UpdateProductsData()
        {
            var dataProductsForRealMoney = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ProductsForRealMoney);
            var dataProductsForVirtualCurrency = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ProductsForVirtualCurrency);
            var dataSpecialProducts = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ShopSpecialProducts);
            var dataDailyOffer = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ShopDailyProducts);
            var dataDailyFreeProducts = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ShopDailyFreeProducts);

            ProductsForRealMoney = JsonConvert.DeserializeObject<JArray>(dataProductsForRealMoney);
            ProductsForRealMoney ??= new JArray();

            ProductsForVirtualCurrency = JsonConvert.DeserializeObject<JArray>(dataProductsForVirtualCurrency);
            ProductsForVirtualCurrency ??= new JArray();

            SpecialOfferProducts = JsonConvert.DeserializeObject<JArray>(dataSpecialProducts);
            SpecialOfferProducts ??= new JArray();

            DailyOfferData = JsonConvert.DeserializeObject<JObject>(dataDailyOffer);
            DailyOfferData ??= new JObject();

            DailyFreeProducts = JsonConvert.DeserializeObject<JArray>(dataDailyFreeProducts);
            DailyFreeProducts ??= new JArray();
        }

        private static void GrantItemsAfterIAPPurchase(string ID)
        {
            FunctionParameters parameter = new()
            {
                ItemID = ID,
            };

            _ = IGSClientAPI.ExecuteFunction
          (
              functionName: ServerFunctionHandlers.GrantItemsAfterIAPPurchase,
              resultCallback: OnSuccessGrantItemsAfterIAP,
              notConnectionErrorCallback: OnErrorGrantItemsAfterIAP,
              connectionErrorCallback: () => GrantItemsAfterIAPPurchase(ID),
              functionParameter: parameter
          );
        }

        private static void OnSuccessGrantItemsAfterIAP(string result)
        {
            OnSuccessPurchase(result);

            if (PlayerPrefs.HasKey(PLAYER_PREFS_AFTER_IAP_NOT_GRANTED_PRODUCT))
            {
                PlayerPrefs.DeleteKey(PLAYER_PREFS_AFTER_IAP_NOT_GRANTED_PRODUCT);
                PlayerPrefs.Save();
            }
        }

        private static void OnErrorGrantItemsAfterIAP(string error)
        {
            PlayerPrefs.SetString(PLAYER_PREFS_AFTER_IAP_NOT_GRANTED_PRODUCT, _iapProcessedProductID);

            if (_attemptsCountForGrantIAPItemsAfterError < MAX_ATTEMPTS_COUNT_FOR_GRANT_IAP_ITEMS)
            {
                CheckForItemsGrantedAfterIAPPurchase();
                _attemptsCountForGrantIAPItemsAfterError++;
            }
            else
            {
                OnErrorPurchase(error);
            }
        }

        private static void GetDailyFreeReward(bool adCompleted = true)
        {
            FunctionParameters parameter = new()
            {
                ItemID = _freeProductID,
            };

            _ = IGSClientAPI.ExecuteFunction
           (
               functionName: ServerFunctionHandlers.GetFreeDailyReward,
               resultCallback: OnSuccessSpecialItemPurchase,
               notConnectionErrorCallback: OnErrorPurchase,
               functionParameter: parameter
           );
        }

        private static void OnErrorPurchase(string error)
        {
            Message.Show(MessageCode.SOMETHING_WENT_WRONG.ToString() + " " + error); //LocalizationSystem
        }
    }
}