using IDosGames.ClientModels;
using IDosGames.UserProfile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public class DataService
    {
        public const string CURRENCY_ICONS_IMAGE_PATH = "Sprites/Currency/";
        public const string CATALOG_SKIN = "Item";

        public static event Action FirstTimeDataUpdated;
        public static event Action SkinCatalogItemsUpdated;

        public static bool _firstTimeDataUpdated = false;
        public static IReadOnlyList<SkinCatalogItem> AllSkinsInCatalog => _allSkinsInCatalog.AsReadOnly();
        public static IReadOnlyList<BigInteger> NFTIDs => _nftIDs.AsReadOnly();
        public static IReadOnlyList<string> EquippedSkins => _equippedSkins.AsReadOnly();
        public static bool HasVIPStatus { get; private set; }
        public static int SecondarySpinTicketRechargeMax { get; private set; }

        private static readonly Dictionary<string, RarityType> _skinCollectionRarity = new();
        private static readonly Dictionary<string, float> _skinCollectionProfit = new();
        private static readonly Dictionary<string, SkinCatalogItem> _skinItems = new();
        private static readonly List<SkinCatalogItem> _allSkinsInCatalog = new();
        private static readonly List<BigInteger> _nftIDs = new();
        private static List<string> _equippedSkins = new();

        private static readonly Dictionary<string, AvatarSkinCatalogItem> _avatarSkinItems = new();
        private static readonly List<AvatarSkinCatalogItem> _allAvatarSkinsInCatalog = new();
        public static IReadOnlyList<AvatarSkinCatalogItem> AllAvatarSkinsInCatalog
            => _allAvatarSkinsInCatalog?.AsReadOnly();

        private static readonly Dictionary<string, int> _eachItemAmounts = new();
        private static readonly Dictionary<string, long> _virtualCurrencyAmounts = new();
        private static readonly Dictionary<SpinTicketType, int> _spinTickets = new();
        private static readonly Dictionary<ChestKeyFragmentType, int> _chestKeyFragments = new();
        private static readonly List<ItemInstance> _inventoryItemsFull = new();
        private static readonly Dictionary<string, List<ItemInstance>> _inventoryItemsByItemId = new();

        private static DataService _instance;
        public static DataService Instance => _instance;

        private DataService()
        {
            _instance = this;

            IAPValidator.VIPSubscriptionValidated += OnVIPSubscriptionValidated;
            // Когда в UserData обновился инвентарь — синхронизируем производные кэши
            IDosGamesData.User.OnInventoryUpdated += RebuildInventoryCaches;
            IDosGamesData.User.OnCustomUserDataUpdated += SetEquippedSkinsList;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new DataService();
        }

        public static void ProcessingAllData(ClientStateResponse userDataResult)
        {
            IDosGamesData.User.ApplyVirtualCurrency(userDataResult.UserInventoryResult.VirtualCurrency);
            IDosGamesData.User.ApplyVirtualCurrencyRechargeTimes(userDataResult.UserInventoryResult.VirtualCurrencyRechargeTimes);
            IDosGamesData.User.ApplyInventory(userDataResult.UserInventoryResult.Inventory);
            IDosGamesData.User.ApplyCustomUserData(userDataResult.CustomUserDataResult);
            IDosGamesData.User.ApplyLeaderboardData(userDataResult.LeaderboardData);

            IDosGamesData.Config.ApplyTitlePublicConfiguration(userDataResult.TitlePublicConfiguration);
            IDosGamesData.Config.TitlePublicConfiguration.ImageData = userDataResult.TitlePublicConfiguration.ImageData;
            IDosGamesData.Config.ApplyCatalog(CATALOG_SKIN, userDataResult.CatalogItemsResult);
            IDosGamesData.Config.ApplyCurrencies(userDataResult.GetCurrencyData);
            IDosGamesData.Config.ApplyTitlePublicData(userDataResult.TitlePublicData);

            IDosGamesData.Title.ApplyLeaderboard(userDataResult.LeaderboardResult);

            UpdateCachedSkinItems(userDataResult.CatalogItemsResult);
            RebuildInventoryCaches();

            if (!_firstTimeDataUpdated)
            {
                _firstTimeDataUpdated = true;
                FirstTimeDataUpdated?.Invoke();
            }
        }

        private static void RebuildInventoryCaches()
        {
            var user = IDosGamesData.User;

            // Сброс
            _eachItemAmounts.Clear();
            _virtualCurrencyAmounts.Clear();
            _spinTickets.Clear();
            _chestKeyFragments.Clear();
            _inventoryItemsFull.Clear();
            _inventoryItemsByItemId.Clear();

            var inventory = user.Inventory;
            var virtualCurrency = user.VirtualCurrency;
            var rechargeTimes = user.VirtualCurrencyRechargeTimes;

            // Предметы инвентаря
            if (inventory != null)
            {
                foreach (var item in inventory)
                {
                    if (item == null) continue;

                    // _inventoryItemsFull / _inventoryItemsByItemId
                    _inventoryItemsFull.Add(item);
                    if (!_inventoryItemsByItemId.TryGetValue(item.ItemId, out var list))
                    {
                        list = new List<ItemInstance>(4);
                        _inventoryItemsByItemId[item.ItemId] = list;
                    }
                    list.Add(item);

                    // количества
                    _eachItemAmounts.TryGetValue(item.ItemId, out int cur);
                    int add = item.RemainingUses.HasValue ? (int)item.RemainingUses : 1;
                    _eachItemAmounts[item.ItemId] = cur + add;
                }

                UpdateVIPStatus(inventory);
            }

            // Валюты
            if (virtualCurrency != null)
                foreach (var kv in virtualCurrency)
                    _virtualCurrencyAmounts[kv.Key] = kv.Value;

            // Спин-тикеты
            _spinTickets[SpinTicketType.Standard] =
                _eachItemAmounts.TryGetValue(ServerItemID.STANDARD_SPIN_TICKET, out var sv) ? sv : 0;
            _spinTickets[SpinTicketType.Premium] =
                _eachItemAmounts.TryGetValue(ServerItemID.PREMIUM_SPIN_TICKET, out var pv) ? pv : 0;

            // Фрагменты ключей
            SetChestKeyFragments();

            // Перезарядка вторичного тикета
            if (rechargeTimes != null)
            {
                string ticketID = VirtualCurrencyID.SS.ToString();
                SecondarySpinTicketRechargeMax =
                    rechargeTimes.TryGetValue(ticketID, out var rt) ? rt.RechargeMax : 0;
            }

            // Снятие скинов которых больше нет в инвентаре
            CheckForEquippedSkinInInventory();
        }

        private static void SetChestKeyFragments()
        {
            void Set(ChestKeyFragmentType type, string id)
                => _chestKeyFragments[type] =
                    _eachItemAmounts.TryGetValue(id, out var v) ? v : 0;

            Set(ChestKeyFragmentType.Common_1, ServerItemID.COMMON_CHEST_KEY_FRAGMENT_1);
            Set(ChestKeyFragmentType.Common_2, ServerItemID.COMMON_CHEST_KEY_FRAGMENT_2);
            Set(ChestKeyFragmentType.Common_3, ServerItemID.COMMON_CHEST_KEY_FRAGMENT_3);
            Set(ChestKeyFragmentType.Rare_1, ServerItemID.RARE_CHEST_KEY_FRAGMENT_1);
            Set(ChestKeyFragmentType.Rare_2, ServerItemID.RARE_CHEST_KEY_FRAGMENT_2);
            Set(ChestKeyFragmentType.Rare_3, ServerItemID.RARE_CHEST_KEY_FRAGMENT_3);
            Set(ChestKeyFragmentType.Legendary_1, ServerItemID.LEGENDARY_CHEST_KEY_FRAGMENT_1);
            Set(ChestKeyFragmentType.Legendary_2, ServerItemID.LEGENDARY_CHEST_KEY_FRAGMENT_2);
            Set(ChestKeyFragmentType.Legendary_3, ServerItemID.LEGENDARY_CHEST_KEY_FRAGMENT_3);
        }

        private static async void UpdateVIPStatus(List<ItemInstance> inventory)
        {
            HasVIPStatus = inventory.Any(i => i?.ItemClass == ServerItemClass.VIP);

            if (!HasVIPStatus && GetItemAmount(ServerItemID.HAS_VIP_SUBSCRIPTION) > 0)
                await ValidateVIPSubscription();
        }

        public static int GetItemAmount(string itemID)
        {
            _eachItemAmounts.TryGetValue(itemID, out int amount);
            return amount;
        }

        public static long GetVirtualCurrencyAmount(VirtualCurrencyID virtualCurrencyID)
            => GetVirtualCurrencyAmount(virtualCurrencyID.ToString());

        public static long GetVirtualCurrencyAmount(string virtualCurrencyID)
        {
            _virtualCurrencyAmounts.TryGetValue(virtualCurrencyID, out long amount);
            return amount;
        }

        public static int GetSpinTicketAmount(SpinTicketType ticketType)
        {
            _spinTickets.TryGetValue(ticketType, out int amount);
            return amount;
        }

        public static int GetChestKeyFragmentAmount(ChestKeyFragmentType fragmentType)
        {
            _chestKeyFragments.TryGetValue(fragmentType, out int amount);
            return amount;
        }

        public static string GetCachedCustomUserData(CustomUserDataKey dataKey)
            => GetCachedCustomUserData(dataKey.ToString());

        public static string GetCachedCustomUserData(string dataKey)
        {
            var cud = IDosGamesData.User.CustomUserData;
            if (cud?.Data == null) return string.Empty;

            cud.Data.TryGetValue(dataKey, out var record);
            return record?.Value ?? string.Empty;
        }

        public static SkinCatalogItem GetCachedSkinItem(string itemID)
        {
            _skinItems.TryGetValue(itemID, out var item);
            return item ?? (SkinCatalogItem)GetAvatarSkinItem(itemID);
        }

        public static RarityType GetSkinRarityByCollection(string collection)
        {
            _skinCollectionRarity.TryGetValue(collection, out var rarity);
            return rarity;
        }

        public static float GetSkinProfitByCollection(string collection)
        {
            _skinCollectionProfit.TryGetValue(collection, out float profit);
            return profit;
        }

        public static AvatarSkinCatalogItem GetAvatarSkinItem(string itemID)
        {
            _avatarSkinItems.TryGetValue(itemID, out var item);
            return item;
        }

        public static Product GetProductForRealMoney(string productID)
        {
            var products = IDosGamesData.Config.TitlePublicConfiguration.ProductsForRealMoney;
            if (products == null) return null;

            foreach (var product in products)
            {
                if (product.ItemID != productID) continue;

                return new Product
                {
                    Name = product.Name,
                    ItemID = product.ItemID,
                    ProductType = product.ProductType,
                    ItemClass = product.ItemClass,
                    PriceRM = product.PriceRM.ToString(),
                    ImagePath = product.ImagePath,
                    ItemsToGrant = product.ItemsToGrant?.ConvertAll(x => new ItemToGrant
                    {
                        Type = x.Type?.ToString(),
                        CurrencyID = x.CurrencyID,
                        Catalog = x.Catalog,
                        ItemID = x.ItemID,
                        Amount = x.Amount?.ToString(),
                        ImagePath = x.ImagePath
                    })
                };
            }

            return null;
        }

        public static void UpdateEquippedSkins(List<string> equippedSkins)
        {
            JArray jArray = JArray.FromObject(equippedSkins);
            FunctionParameters parameter = new() { ItemIDs = jArray };

            _ = IGSClientAPI.ExecuteFunction(
                functionName: ServerFunctionHandlers.UpdateEquippedSkins,
                resultCallback: _ => OnSuccessUpdateEquippedSkins(equippedSkins),
                notConnectionErrorCallback: _ => OnErrorUpdateEquippedSkins(),
                connectionErrorCallback: () => UpdateEquippedSkins(equippedSkins),
                functionParameter: parameter);
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
                _equippedSkins = JsonConvert.DeserializeObject<List<string>>(record.Value) ?? new();
            }
            catch (JsonReaderException)
            {
                Debug.LogError("Incorrect equipped skins format in CustomUserData. JsonReaderException.");
            }

            CheckForEquippedSkinInInventory();
        }

        private static void CheckForEquippedSkinInInventory()
        {
            var toRemove = _equippedSkins
                .Where(id => GetItemAmount(id) <= 0)
                .ToList();

            if (toRemove.Count == 0) return;

            foreach (var id in toRemove)
                _equippedSkins.Remove(id);

            UpdateEquippedSkins(_equippedSkins);
        }

        private static void OnSuccessUpdateEquippedSkins(List<string> equippedSkins)
        {
            _equippedSkins = equippedSkins;
        }

        private static void OnErrorUpdateEquippedSkins()
            => Message.Show(MessageCode.FAILED_TO_UPDATE_EQUIPPED_SKINS);

        private static void UpdateCachedSkinItems(GetCatalogItemsResult result)
        {
            if (result.Catalog == null) return;

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
                    if (nftID != 0 && !_nftIDs.Contains(nftID))
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

            foreach (var cr in collectionRarities)
            {
                if (cr.Collections == null) continue;
                Enum.TryParse(cr.Rarity, true, out RarityType rarity);

                foreach (var collection in cr.Collections)
                {
                    _skinCollectionRarity[collection] = rarity;
                    _skinCollectionProfit[collection] = cr.Profit;
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

            var validationResult = JsonConvert.DeserializeObject<JObject>(result);
            if (!validationResult.ContainsKey("Message"))
            {
                Message.Show(MessageCode.FAILED_TO_VALIDATE_VIP_SUBSCRIPTION);
                return;
            }

            Message.Show(validationResult["Message"]?.ToString());
            ShopSystem.PopUpSystem.HideAllPopUps();
            OnVIPSubscriptionValidated();
        }

        private static async void OnVIPSubscriptionValidated()
            => await UserService.GetUserInventory();
    }
}
