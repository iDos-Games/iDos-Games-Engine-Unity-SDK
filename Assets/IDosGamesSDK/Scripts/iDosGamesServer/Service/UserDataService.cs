using IDosGames.ClientModels;
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
        public const string CATALOG_SKIN = "Skin";
        private const int TASK_DELAY_MILLISECONDS_STEP_REQUEST_ALL_DATA = 10;
        private const int MILLISECONDS_BEFORE_BREAK_REQUEST_ALL_DATA_SEQUENCE = 10000;

        public static event Action DataRequested;
        public static event Action DataUpdated;
        public static event Action<string> AllDataRequestError;

        public static event Action<GetUserInventoryResult> UserInventoryReceived;
        public static event Action<JObject> BlobTitleDataReceived;
        public static event Action<GetUserDataResult> UserReadOnlyDataReceived;
        public static event Action<GetCatalogItemsResult> SkinCatalogReceived;

        public static event Action TitleDataUpdated;
        public static event Action UserReadOnlyDataUpdated;
        public static event Action SkinCatalogItemsUpdated;
        public static event Action EquippedSkinsUpdated;

        public static event Action<string, CustomUpdateResult> CustomReadOnlyDataUpdated;

        public static IReadOnlyList<SkinCatalogItem> AllSkinsInCatalog => _allSkinsInCatalog.AsReadOnly();

        public static IReadOnlyList<BigInteger> NFTIDs => _nftIDs.AsReadOnly();

        public static IReadOnlyList<string> EquippedSkins => _equippedSkins.AsReadOnly();

        private static readonly Dictionary<TitleDataKey, string> _titleData = new();
        private static readonly Dictionary<string, string> _titleDataRaw = new();
        private static readonly Dictionary<UserReadOnlyDataKey, string> _playerData = new();
        private static readonly Dictionary<string, string> _playerDataRaw = new();

        private static readonly Dictionary<string, RarityType> _skinCollectionRarity = new();
        private static readonly Dictionary<string, float> _skinCollectionProfit = new();
        private static readonly Dictionary<string, SkinCatalogItem> _skinItems = new();

        private static readonly List<SkinCatalogItem> _allSkinsInCatalog = new();

        private static readonly List<BigInteger> _nftIDs = new();
        private static List<string> _equippedSkins = new();

        private static UserDataService _instance;

        private static bool _continueRequestAllDataSequence;

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
            // UserInventory.InventoryUpdated += CheckForEquippedAvatarSkin;
            UserReadOnlyDataUpdated += SetEquippedSkinsList;

            UserInventoryReceived += (result) => _continueRequestAllDataSequence = true;
            BlobTitleDataReceived += (result) => _continueRequestAllDataSequence = true;
            UserReadOnlyDataReceived += (result) => _continueRequestAllDataSequence = true;
            SkinCatalogReceived += (result) => _continueRequestAllDataSequence = true;
            AllDataRequestError += (error) => _continueRequestAllDataSequence = true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new();
        }

        public static void ProcessingAllData(GetAllUserDataResult userDataResult)
        {
            DataRequested?.Invoke();

            UserInventoryReceived?.Invoke(userDataResult.UserInventoryResult);
            IGSUserData.UserInventory = userDataResult.UserInventoryResult;

            OnBlobTitleDataReceived(userDataResult.BlobTitleDataResult);
            IGSUserData.TitleData = userDataResult.BlobTitleDataResult;

            OnUserReadOnlyDataReceived(userDataResult.UserDataResult);
            IGSUserData.ReadOnlyData = userDataResult.UserDataResult;

            OnSkinCatalogItemsReceived(userDataResult.CatalogItemsResult);
            IGSUserData.SkinCatalogItems = userDataResult.CatalogItemsResult;

            IGSUserData.Leaderboard = userDataResult.LeaderboardResult;

            IGSUserData.Friends = userDataResult.GetFriends.ToString();
            IGSUserData.FriendRequests = userDataResult.GetFriendRequests.ToString();
            IGSUserData.RecommendedFriends = userDataResult.GetRecommendedFriends.ToString();

            DataUpdated?.Invoke();
        }

        public static void RequestUserAllData()
        {
            IGSClientAPI.GetUserAllData(resultCallback: ProcessingAllData, notConnectionErrorCallback: OnAllDataRequestError, connectionErrorCallback: () => { RequestUserAllData(); TryInvokeDataRequestAgain(); });
        }

        private static async Task WaitForNextStepRequestAllDataSequence()
        {
            _continueRequestAllDataSequence = false;

            int elapsedTime = 0;

            while (!_continueRequestAllDataSequence && elapsedTime < MILLISECONDS_BEFORE_BREAK_REQUEST_ALL_DATA_SEQUENCE)
            {
                await Task.Delay(TASK_DELAY_MILLISECONDS_STEP_REQUEST_ALL_DATA);

                elapsedTime += TASK_DELAY_MILLISECONDS_STEP_REQUEST_ALL_DATA;
            }
        }

        public static void RequestUserInventory()
        {
            IGSClientAPI.GetUserInventory
            (
                resultCallback: (result) => UserInventoryReceived?.Invoke(result),
                notConnectionErrorCallback: OnRequestUserInventoryError,
                connectionErrorCallback: () =>
                {
                    RequestUserInventory();
                    TryInvokeDataRequestAgain();
                }
            );
        }

        public static void RequestTitleData()
        {
            IGSClientAPI.GetBlobTitleData(

                resultCallback: OnBlobTitleDataReceived,
                notConnectionErrorCallback: OnRequestTitleDataError,
                connectionErrorCallback: () =>
                {
                    RequestTitleData();
                    TryInvokeDataRequestAgain();
                }
                );
        }
        
        public static void RequestUserReadOnlyData()
        {
            IGSClientAPI.GetUserReadOnlyData
            (
                resultCallback: OnUserReadOnlyDataReceived,
                notConnectionErrorCallback: OnRequestUserReadOnlyDataError,
                connectionErrorCallback: () =>
                {
                    RequestUserReadOnlyData();
                    TryInvokeDataRequestAgain();
                }
            );
        }

        public static void RequestSkinCatalogItems()
        {
            IGSClientAPI.GetCatalogItems
            (
                catalogVersion: CATALOG_SKIN,
                resultCallback: OnSkinCatalogItemsReceived,
                notConnectionErrorCallback: OnRequestSkinCatalogItemsError, //OnRequestSkinCatalogItemsError
                connectionErrorCallback: () =>
                {
                    RequestSkinCatalogItems();
                    TryInvokeDataRequestAgain();
                }
            );
        }

        // Processing of Received Data
        private static void TryInvokeDataRequestAgain()
        {
            if (!_continueRequestAllDataSequence)
            {
                DataRequested?.Invoke();
            }
        }

        public static string GetCachedTitleData(TitleDataKey dataKey)
        {
            _titleData.TryGetValue(dataKey, out string data);

            return $"{data}";
        }

        public static string GetCachedTitleData(string dataKey)
        {
            _titleDataRaw.TryGetValue(dataKey, out string data);

            return $"{data}";
        }

        public static string GetCachedUserReadOnlyData(UserReadOnlyDataKey dataKey)
        {
            _playerData.TryGetValue(dataKey, out string data);

            return $"{data}";
        }

        public static string GetCachedUserReadOnlyData(string dataKey)
        {
            _playerDataRaw.TryGetValue(dataKey, out string data);

            return $"{data}";
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
            string titleData = GetCachedTitleData(TitleDataKey.telegram_settings);
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

        public static void UpdateCustomReadOnlyData(string key, object data)
        {
            FunctionParameters parameter = new()
            {
                Key = key,
                Value = data
            };

            if (IDosGamesSDKSettings.Instance.DebugLogging)
            {
                Debug.Log(JsonConvert.SerializeObject(parameter));
            }
            
            _ = IGSClientAPI.ExecuteFunction(

                functionName: ServerFunctionHandlers.UpdateCustomReadOnlyData,
                resultCallback: (result) => OnUpdateCustomReadOnlyData(result, key),
                notConnectionErrorCallback: (error) => OnErrorUpdateCustomData(),
                connectionErrorCallback: () => UpdateCustomReadOnlyData(key, data),
                functionParameter: parameter
                );
        }

        private static void OnUpdateCustomReadOnlyData(string result, string key)
        {
            if (result != null)
            {

                JObject resultData = JsonConvert.DeserializeObject<JObject>(result);
                if (resultData[JsonProperty.MESSAGE_KEY] != null && resultData[JsonProperty.MESSAGE_KEY].ToString() == "SUCCESS")
                {
                    CustomReadOnlyDataUpdated?.Invoke(key, CustomUpdateResult.SUCCESS);
                }
                else if (resultData[JsonProperty.MESSAGE_KEY] != null && resultData[JsonProperty.MESSAGE_KEY].ToString() == "MESSAGE_CODE_INCORECT_KEY")
                {
                    CustomReadOnlyDataUpdated?.Invoke(key, CustomUpdateResult.INCORECT_KEY);
                }
                else if (resultData[JsonProperty.MESSAGE_KEY] != null && resultData[JsonProperty.MESSAGE_KEY].ToString() == "MESSAGE_CODE_INCORECT_ARGS")
                {
                    CustomReadOnlyDataUpdated?.Invoke(key, CustomUpdateResult.INCIRCT_ARGS);
                }
            }
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

        private static void OnBlobTitleDataReceived(JObject result)
        {
            BlobTitleDataReceived?.Invoke(result);

            UpdateCachedTitleData(result);
        }

        private static void UpdateCachedTitleData(JObject result)
        {
            Dictionary<string, string> dataDictionary = ConvertJObjectToDictionary(result);
            foreach (var data in dataDictionary)
            {
                _titleDataRaw[data.Key] = data.Value;

                if (Enum.TryParse(data.Key, true, out TitleDataKey dataKey))
                {
                    _titleData[dataKey] = data.Value;
                }


            }
            TitleDataUpdated?.Invoke();
        }

        private static Dictionary<string, string> ConvertJObjectToDictionary(JObject jsonObject)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var property in jsonObject.Properties())
            {
                if (property.Value is JValue)
                {
                    // ���� �������� - JValue, ��������� ��� � Dictionary
                    result[property.Name] = ((JValue)property.Value).Value?.ToString();
                }
                else if (property.Value is JObject)
                {
                    result[property.Name] = JsonConvert.SerializeObject(property.Value);
                }
                else if (property.Value is JArray)
                {
                    result[property.Name] = JsonConvert.SerializeObject(property.Value);
                }

            }

            return result;
        }

        private static void OnUserReadOnlyDataReceived(GetUserDataResult result)
        {
            UserReadOnlyDataReceived?.Invoke(result);

            UpdateCachedUserReadOnlyData(result);
        }

        private static void UpdateCachedUserReadOnlyData(GetUserDataResult result)
        {
            foreach (var data in result.Data)
            {
                _playerDataRaw[data.Key] = data.Value.Value;

                if (Enum.TryParse(data.Key, true, out UserReadOnlyDataKey dataKey))
                {
                    _playerData[dataKey] = data.Value.Value;
                }
            }

            UserReadOnlyDataUpdated?.Invoke();
        }

        private static void SetEquippedSkinsList()
        {
            _equippedSkins.Clear();

            var equppedSkinsData = GetCachedUserReadOnlyData(UserReadOnlyDataKey.equipped_skins);

            if (equppedSkinsData == string.Empty)
            {
                return;
            }

            try
            {
                _equippedSkins = JsonConvert.DeserializeObject<List<string>>(equppedSkinsData);
                _equippedSkins ??= new();
            }
            catch (JsonReaderException)
            {
                Debug.LogError("Incorrect equipped skins format in UserReadonlyData. JsonReaderException.");
            }

            CheckForEquippedSkinInInventory();
        }

        private static void OnSkinCatalogItemsReceived(GetCatalogItemsResult result)
        {
            SkinCatalogReceived?.Invoke(result);

            UpdateCachedSkinItems(result);
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
            string collectionRarityData = GetCachedTitleData(TitleDataKey.skin_collection_rarity);

            JArray collectionRarities = new();

            try
            {
                collectionRarities = JsonConvert.DeserializeObject<JArray>(collectionRarityData);
                collectionRarities ??= new();
            }
            catch (JsonReaderException)
            {
                Debug.LogError("Incorrect skin_collection_rarity format in TitleData. JsonReaderException.");
            }

            foreach (var collectionRarity in collectionRarities)
            {
                List<string> collections = new();

                try
                {
                    collections = JsonConvert.DeserializeObject<List<string>>($"{collectionRarity[JsonProperty.COLLECTIONS]}");
                    collections ??= new();
                }
                catch (JsonReaderException)
                {
                    Debug.LogError("Incorrect skin_collection_rarity format in TitleData. JsonReaderException.");
                }

                Enum.TryParse($"{collectionRarity[JsonProperty.RARITY]}", true, out RarityType rarity);
                float.TryParse($"{collectionRarity[JsonProperty.PROFIT]}", out float profit);

                foreach (var collection in collections)
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

        private static void CheckForEquippedAvatarSkin()
        {
            var data = GetCachedUserReadOnlyData(UserReadOnlyDataKey.equipped_avatar_skins);


            if (String.IsNullOrEmpty(data))
            {
                return;
            }
            Dictionary<ClothingType, string> equippedSkins = new Dictionary<ClothingType, string>();
            var titleData = GetCachedTitleData(TitleDataKey.default_avatar_skin);
            Dictionary<ClothingType, string> defaultSkins = new Dictionary<ClothingType, string>();
            Debug.Log(titleData);
            if (!string.IsNullOrEmpty(titleData))
            {
                JObject jsonData = JsonConvert.DeserializeObject<JObject>(titleData);
                JArray titleArray = jsonData.GetValue("Data").Value<JArray>();
                foreach (var jObject in titleArray)
                {
                    var key = jObject.Value<string>("Key");
                    var value = jObject.Value<string>("Value");
                    Enum.TryParse(key, out ClothingType clothingTYpe);
                    if (!defaultSkins.ContainsKey(clothingTYpe))
                    {
                        defaultSkins.Add(clothingTYpe, value);
                    }

                }
            }
            var jarray = JsonConvert.DeserializeObject<JArray>(data);
            foreach (var jObject in jarray)
            {
                var key = jObject.Value<string>("Key");
                var value = jObject.Value<string>("Value");
                Enum.TryParse(key, out ClothingType type); ;
                if (!defaultSkins.ContainsKey(type))
                {
                    equippedSkins.Add(type, value);
                }

            }
            List<ClothingType> removeType = new List<ClothingType>();
            foreach (var item in equippedSkins)
            {
                if (UserInventory.GetItemAmount(item.Value) <= 0)
                {
                    removeType.Add(item.Key);
                }
            }

            foreach (var item in removeType)
            {
                equippedSkins.Remove(item);
            }

            if (equippedSkins.Count <= 0)
            {
                UpdateCustomReadOnlyData(UserReadOnlyDataKey.equipped_avatar_skins.ToString(), defaultSkins);
            }
            else
            {
                foreach (var item in defaultSkins)
                {
                    if (!equippedSkins.ContainsKey(item.Key))
                    {
                        equippedSkins.Add(item.Key, item.Value);
                    }
                }
                UpdateCustomReadOnlyData(UserReadOnlyDataKey.equipped_avatar_skins.ToString(), equippedSkins);
            }
        }

        private static void OnSuccessUpdateEquippedSkins(List<string> equippedSkins)
        {
            _equippedSkins = equippedSkins;
            EquippedSkinsUpdated?.Invoke();
        }

        private static void OnRequestUserInventoryError(string error)
        {
            OnAllDataRequestError(error);
        }

        private static void OnRequestTitleDataError(string error)
        {
            OnAllDataRequestError(error);
        }

        private static void OnRequestUserReadOnlyDataError(string error)
        {
            OnAllDataRequestError(error);
        }

        private static void OnRequestSkinCatalogItemsError(string error)
        {
            OnAllDataRequestError(error);
        }

        private static void OnErrorUpdateEquippedSkins()
        {
            Message.Show(MessageCode.FAILED_TO_UPDATE_EQUIPPED_SKINS);
        }

        private static void OnErrorUpdateCustomData()
        {
            Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
        }

        private static void OnAllDataRequestError(string error)
        {
            AllDataRequestError?.Invoke(error);
        }
    }
}