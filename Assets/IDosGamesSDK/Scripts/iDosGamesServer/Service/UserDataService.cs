using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using IDosGames.UserProfile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public class UserDataService
    {
        public const string CURRENCY_ICONS_IMAGE_PATH = "Sprites/Currency/";
        public const string CATALOG_SKIN = "Item";

        public static event Action DataRequested;
        public static event Action DataUpdated;
        public static event Action<string> AllDataRequestError;

        public static event Action FirstTimeDataUpdated;
        public static bool _firstTimeDataUpdated = false;

        public static event Action<GetUserInventoryResult> UserInventoryReceived;
        public static event Action<GetCatalogItemsResult> SkinCatalogReceived;

        public static event Action CustomUserDataUpdated;
        public static event Action SkinCatalogItemsUpdated;
        public static event Action EquippedSkinsUpdated;

        public static IReadOnlyList<SkinCatalogItem> AllSkinsInCatalog => _allSkinsInCatalog.AsReadOnly();

        public static IReadOnlyList<BigInteger> NFTIDs => _nftIDs.AsReadOnly();

        public static IReadOnlyList<string> EquippedSkins => _equippedSkins.AsReadOnly();

        private static readonly Dictionary<string, RarityType> _skinCollectionRarity = new();
        private static readonly Dictionary<string, float> _skinCollectionProfit = new();
        private static readonly Dictionary<string, SkinCatalogItem> _skinItems = new();

        private static readonly List<SkinCatalogItem> _allSkinsInCatalog = new();

        private static readonly List<BigInteger> _nftIDs = new();
        private static List<string> _equippedSkins = new();

        private static UserDataService _instance;

        private static bool _continueRequestAllDataSequence;

        public static TitlePublicConfigurationModel TitlePublicConfiguration;

        public static UserDataService Instance => _instance;

        //avatar
        private static readonly Dictionary<string, AvatarSkinCatalogItem> _avatarSkinItems = new();
        private static readonly List<AvatarSkinCatalogItem> _allAvatarSkinsInCatalog = new();
        public static IReadOnlyList<AvatarSkinCatalogItem> AllAvatarSkinsInCatalog => _allAvatarSkinsInCatalog?.AsReadOnly();

        private UserDataService()
        {
            _instance = this;

            IAPValidator.VIPSubscriptionValidated += OnVIPSubscriptionValidated;
            UserInventory.InventoryUpdated += CheckForEquippedSkinInInventory;
            CustomUserDataUpdated += SetEquippedSkinsList;

            UserInventoryReceived += (result) => _continueRequestAllDataSequence = true;
            SkinCatalogReceived += (result) => _continueRequestAllDataSequence = true;
            AllDataRequestError += (error) => _continueRequestAllDataSequence = true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new();
        }

        public static void ProcessingAllData(ClientStateResponse userDataResult)
        {
            DataRequested?.Invoke();

            UserInventoryReceived?.Invoke(userDataResult.UserInventoryResult);
            IDosGamesData.User.ApplyVirtualCurrency(userDataResult.UserInventoryResult.VirtualCurrency);
            IDosGamesData.User.ApplyVirtualCurrencyRechargeTimes(userDataResult.UserInventoryResult.VirtualCurrencyRechargeTimes);
            IDosGamesData.User.ApplyInventory(userDataResult.UserInventoryResult.Inventory);

            IDosGamesData.Config.ApplyTitlePublicConfiguration(userDataResult.TitlePublicConfiguration);

            OnCatalogItemsReceived(userDataResult.CatalogItemsResult);
            IDosGamesData.Config.ApplyCatalog(CATALOG_SKIN, userDataResult.CatalogItemsResult);

            IDosGamesData.User.ApplyCustomUserData(userDataResult.CustomUserDataResult);

            IDosGamesData.Title.ApplyLeaderboard(userDataResult.LeaderboardResult);

            IDosGamesData.Config.ApplyCurrencies(userDataResult.GetCurrencyData);

            IDosGamesData.Config.TitlePublicConfiguration.ImageData = userDataResult.TitlePublicConfiguration.ImageData;

            IDosGamesData.User.ApplyLeaderboardData(userDataResult.LeaderboardData);
            IDosGamesData.Config.ApplyTitlePublicData(userDataResult.TitlePublicData);

            DataUpdated?.Invoke();
            CustomUserDataUpdated?.Invoke();

            if (!_firstTimeDataUpdated)
            {
                _firstTimeDataUpdated = true;
                FirstTimeDataUpdated?.Invoke();
            }
        }

        public static void RequestUserAllData()
        {
            IGSClientAPI.GetUserAllData(resultCallback: ProcessingAllData, notConnectionErrorCallback: OnAllDataRequestError, connectionErrorCallback: () => { RequestUserAllData(); TryInvokeDataRequestAgain(); });
        }

        // Processing of Received Data
        private static void TryInvokeDataRequestAgain()
        {
            if (!_continueRequestAllDataSequence)
            {
                DataRequested?.Invoke();
            }
        }

        public static string GetCachedTitlePublicConfig(TitleDataKey dataKey)
        {
            var config = IDosGamesData.Config.TitlePublicConfiguration;
            if (config == null) return string.Empty;

            var jObject = JObject.FromObject(config);
            var property = jObject.GetValue(dataKey.ToString(), StringComparison.OrdinalIgnoreCase);

            if (property == null) return string.Empty;

            return property is JValue ? property.ToString() : JsonConvert.SerializeObject(property);
        }

        public static string GetCachedTitlePublicConfig(string dataKey)
        {
            var config = IDosGamesData.Config.TitlePublicConfiguration;
            if (config == null) return string.Empty;

            var jObject = JObject.FromObject(config);
            var property = jObject.GetValue(dataKey, StringComparison.OrdinalIgnoreCase);

            if (property == null) return string.Empty;

            return property is JValue ? property.ToString() : JsonConvert.SerializeObject(property);
        }

        public static string GetCachedCustomUserData(CustomUserDataKey dataKey)
        {
            var cud = IDosGamesData.User.CustomUserData;
            if (cud?.Data == null) return string.Empty;

            cud.Data.TryGetValue(dataKey.ToString(), out var record);
            return record?.Value ?? string.Empty;
        }

        public static string GetCachedCustomUserData(string dataKey)
        {
            var cud = IDosGamesData.User.CustomUserData;
            if (cud?.Data == null) return string.Empty;

            cud.Data.TryGetValue(dataKey, out var record);
            return record?.Value ?? string.Empty;
        }

        public static SkinCatalogItem GetCachedSkinItem(string itemID)
        {
            _skinItems.TryGetValue(itemID, out SkinCatalogItem item);

            if (item == null)
            {

                return GetAvatarSkinItem(itemID);
            }

            return item;
        }

        public static bool IsSkinEquipped(string itemID)
        {
            return _equippedSkins.Contains(itemID);
        }

        public static RarityType GetSkinRarityByCollection(string collection)
        {
            _skinCollectionRarity.TryGetValue(collection, out RarityType rarity);

            return rarity;
        }

        public static float GetSkinProfitByCollection(string collection)
        {
            _skinCollectionProfit.TryGetValue(collection, out float profit);

            return profit;
        }

        public static AvatarSkinCatalogItem GetAvatarSkinItem(string itemID)
        {
            _avatarSkinItems.TryGetValue(itemID, out AvatarSkinCatalogItem item);

            return item;
        }

        public static Product GetProductForRealMoney(string productID)
        {
            var products = ShopSystem.ProductsForRealMoney;

            if (products == null)
            {
                return null;
            }

            foreach (var product in products)
            {
                if (product[JsonProperty.ITEM_ID]?.ToString() == productID)
                {
                    var productObject = new Product
                    {
                        Name = product[JsonProperty.NAME]?.ToString(),
                        ItemID = product[JsonProperty.ITEM_ID]?.ToString(),
                        ProductType = product[JsonProperty.PRODUCT_TYPE]?.ToString(),
                        ItemClass = product[JsonProperty.ITEM_CLASS]?.ToString(),
                        PriceRM = product[JsonProperty.PRICE_RM]?.ToString(),
                        ImagePath = product[JsonProperty.IMAGE_PATH]?.ToString(),
                        ItemsToGrant = product[JsonProperty.ITEMS_TO_GRANT]?.ToObject<List<ItemToGrant>>()
                    };

                    return productObject;
                }
            }

            return null;
        }

        public static float GetTelegramStarPrice()
        {
            string titleData = GetCachedTitlePublicConfig(TitleDataKey.telegram_settings);
            if (string.IsNullOrEmpty(titleData))
            {
                return 2f;
            }

            var starPriceInCent = JsonConvert.DeserializeObject<JObject>(titleData);

            if (starPriceInCent.ContainsKey(JsonProperty.STAR_PRICE_IN_CENT))
            {
                string starPriceInCentString = starPriceInCent[JsonProperty.STAR_PRICE_IN_CENT]?.ToString();
                return float.Parse(starPriceInCentString);
            }
            else
            {
                return 2f;
            }
        }

        public static void UpdateEquippedSkins(List<string> equippedSkins)
        {
            JArray jArray = JArray.FromObject(equippedSkins);

            FunctionParameters parameter = new()
            {
                ItemIDs = jArray
            };

            _ = IGSClientAPI.ExecuteFunction(
                functionName: ServerFunctionHandlers.UpdateEquippedSkins,
                resultCallback: (result) => OnSuccessUpdateEquippedSkins(equippedSkins),
                notConnectionErrorCallback: (error) => OnErrorUpdateEquippedSkins(),
                 connectionErrorCallback: () => UpdateEquippedSkins(equippedSkins),
                  functionParameter: parameter
                );
        }

        public static void UpdateCustomUserData(string key, object data)
        {
            _ = UserService.UpdateCustomUserData(key, data);
        }

        public static async Task ValidateVIPSubscription(string receipt = null)
        {
            Loading.ShowTransparentPanel();

            var result = await IGSService.ValidateIAPSubscription(receipt);

            Loading.HideAllPanels();

            if (string.IsNullOrEmpty(result))
            {
                Message.Show(MessageCode.FAILED_TO_VALIDATE_VIP_SUBSCRIPTION);
                return;
            }

            var validatiuonResult = JsonConvert.DeserializeObject<JObject>(result);

            if (validatiuonResult.ContainsKey("Message") == false)
            {
                Message.Show(MessageCode.FAILED_TO_VALIDATE_VIP_SUBSCRIPTION);
                return;
            }

            Message.Show(validatiuonResult["Message"]?.ToString());
            ShopSystem.PopUpSystem.HideAllPopUps();

            OnVIPSubscriptionValidated();
        }

        private static void SetEquippedSkinsList()
        {
            _equippedSkins.Clear();

            var cud = IDosGamesData.User.CustomUserData;
            if (cud?.Data == null) return;

            if (!cud.Data.TryGetValue(CustomUserDataKey.equipped_skins.ToString(), out var record)
                || string.IsNullOrEmpty(record?.Value)) return;

            try
            {
                _equippedSkins = JsonConvert.DeserializeObject<List<string>>(record.Value);
                _equippedSkins ??= new();
            }
            catch (JsonReaderException)
            {
                Debug.LogError("Incorrect equipped skins format in CustomUserData. JsonReaderException.");
            }

            CheckForEquippedSkinInInventory();
        }

        private static void OnCatalogItemsReceived(GetCatalogItemsResult result)
        {
            SkinCatalogReceived?.Invoke(result);

            UpdateCachedSkinItems(result);

            IDosGamesData.Config.ApplyCatalog(CATALOG_SKIN, result);
        }

        private static void UpdateCachedSkinItems(GetCatalogItemsResult result)
        {
            if (result.Catalog == null)
            {
                return;
            }

            SetSkinCollectionRarityAndProfit();

            _allSkinsInCatalog.Clear();
            _nftIDs.Clear();
            _allAvatarSkinsInCatalog.Clear();

            foreach (var item in result.Catalog)
            {
                var customData = JsonConvert.DeserializeObject<JObject>(item.CustomData);
                if (item.ItemClass == "skin")
                {
                    _skinItems[item.ItemId] = new(item, customData);
                    _allSkinsInCatalog.Add(_skinItems[item.ItemId]);

                    int nftID = _skinItems[item.ItemId].NFTID;

                    if (nftID == 0)
                    {
                        continue;
                    }

                    if (_nftIDs.Contains(nftID))
                    {
                        continue;
                    }

                    _nftIDs.Add(nftID);
                }
                else if (item.ItemClass == "avatar_skin")
                {

                    _avatarSkinItems[item.ItemId] = new(item, customData);
                    _allAvatarSkinsInCatalog.Add(_avatarSkinItems[item.ItemId]);
                    _allSkinsInCatalog.Add(_avatarSkinItems[item.ItemId]);
                }
            }

            SkinCatalogItemsUpdated?.Invoke();
        }

        private static void SetSkinCollectionRarityAndProfit()
        {
            var collectionRarities = IDosGamesData.Config.TitlePublicConfiguration?.SkinCollectionRarity;

            if (collectionRarities == null)
            {
                Debug.LogError("Incorrect SkinCollectionRarity format in TitlePublicConfiguration.");
                return;
            }

            foreach (var collectionRarity in collectionRarities)
            {
                if (collectionRarity.Collections == null) continue;

                Enum.TryParse(collectionRarity.Rarity, true, out RarityType rarity);
                float profit = collectionRarity.Profit;

                foreach (var collection in collectionRarity.Collections)
                {
                    _skinCollectionRarity[collection] = rarity;
                    _skinCollectionProfit[collection] = profit;
                }
            }
        }

        private static void OnVIPSubscriptionValidated()
        {
            RequestUserAllData(); // RequestUserInventory
        }

        private static void CheckForEquippedSkinInInventory()
        {
            List<string> itemsToRemove = new();

            foreach (var itemID in _equippedSkins)
            {
                if (UserInventory.GetItemAmount(itemID) <= 0)
                {
                    itemsToRemove.Add(itemID);
                }
            }

            if (itemsToRemove.Count <= 0)
            {
                return;
            }

            foreach (var itemID in itemsToRemove)
            {
                _equippedSkins.Remove(itemID);
            }

            UpdateEquippedSkins(_equippedSkins);
        }

        private static void OnSuccessUpdateEquippedSkins(List<string> equippedSkins)
        {
            _equippedSkins = equippedSkins;
            EquippedSkinsUpdated?.Invoke();
        }

        private static void OnErrorUpdateEquippedSkins()
        {
            Message.Show(MessageCode.FAILED_TO_UPDATE_EQUIPPED_SKINS);
        }

        private static void OnAllDataRequestError(string error)
        {
            AllDataRequestError?.Invoke(error);
        }
    }
}
