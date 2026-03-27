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

        // ── Общие события ─────────────────────────────────────────────────────
        public static event Action DataRequested;
        public static event Action DataUpdated;
        public static event Action<string> AllDataRequestError;
        public static event Action FirstTimeDataUpdated;

        // ── Инвентарь ─────────────────────────────────────────────────────────
        /// <summary>Вызывается после каждого обновления инвентаря пользователя.</summary>
        public static event Action InventoryUpdated;

        // ── Скины / каталог ───────────────────────────────────────────────────
        public static event Action CustomUserDataUpdated;
        public static event Action SkinCatalogItemsUpdated;
        public static event Action EquippedSkinsUpdated;

        // ── Публичные свойства ────────────────────────────────────────────────
        public static bool _firstTimeDataUpdated = false;

        public static IReadOnlyList<SkinCatalogItem> AllSkinsInCatalog => _allSkinsInCatalog.AsReadOnly();
        public static IReadOnlyList<BigInteger> NFTIDs => _nftIDs.AsReadOnly();
        public static IReadOnlyList<string> EquippedSkins => _equippedSkins.AsReadOnly();

        /// <summary>Есть ли у пользователя VIP-статус.</summary>
        public static bool HasVIPStatus { get; private set; }

        /// <summary>Максимум перезарядки вторичного спин-тикета.</summary>
        public static int SecondarySpinTicketRechargeMax { get; private set; }

        // ── Приватные коллекции (скины) ───────────────────────────────────────
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

        // ── Приватные коллекции (инвентарь) ──────────────────────────────────
        // Производные структуры для быстрого доступа; источник данных — UserData.Inventory
        private static readonly Dictionary<string, int> _eachItemAmounts = new();
        private static readonly Dictionary<string, long> _virtualCurrencyAmounts = new();
        private static readonly Dictionary<SpinTicketType, int> _spinTickets = new();
        private static readonly Dictionary<ChestKeyFragmentType, int> _chestKeyFragments = new();
        private static readonly List<ItemInstance> _inventoryItemsFull = new();
        private static readonly Dictionary<string, List<ItemInstance>> _inventoryItemsByItemId = new();

        // ── Singleton / init ──────────────────────────────────────────────────
        private static DataService _instance;
        public static DataService Instance => _instance;

        private static bool _continueRequestAllDataSequence;

        private DataService()
        {
            _instance = this;

            IAPValidator.VIPSubscriptionValidated += OnVIPSubscriptionValidated;
            // Когда в UserData обновился инвентарь — синхронизируем производные кэши
            IDosGamesData.User.OnInventoryUpdated += RebuildInventoryCaches;
            IDosGamesData.User.OnCustomUserDataUpdated += SetEquippedSkinsList;

            AllDataRequestError += _ => _continueRequestAllDataSequence = true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new DataService();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Основной поток данных
        // ═════════════════════════════════════════════════════════════════════

        public static void ProcessingAllData(ClientStateResponse userDataResult)
        {
            DataRequested?.Invoke();

            // — Применяем всё в UserData / TitleConfig (единый источник правды) —
            IDosGamesData.User.ApplyVirtualCurrency(userDataResult.UserInventoryResult.VirtualCurrency);
            IDosGamesData.User.ApplyVirtualCurrencyRechargeTimes(userDataResult.UserInventoryResult.VirtualCurrencyRechargeTimes);
            IDosGamesData.User.ApplyInventory(userDataResult.UserInventoryResult.Inventory);

            IDosGamesData.Config.ApplyTitlePublicConfiguration(userDataResult.TitlePublicConfiguration);
            IDosGamesData.Config.TitlePublicConfiguration.ImageData = userDataResult.TitlePublicConfiguration.ImageData;

            OnCatalogItemsReceived(userDataResult.CatalogItemsResult);
            IDosGamesData.Config.ApplyCatalog(CATALOG_SKIN, userDataResult.CatalogItemsResult);

            IDosGamesData.User.ApplyCustomUserData(userDataResult.CustomUserDataResult);

            IDosGamesData.Title.ApplyLeaderboard(userDataResult.LeaderboardResult);
            IDosGamesData.Config.ApplyCurrencies(userDataResult.GetCurrencyData);
            IDosGamesData.User.ApplyLeaderboardData(userDataResult.LeaderboardData);
            IDosGamesData.Config.ApplyTitlePublicData(userDataResult.TitlePublicData);

            // — Производные кэши инвентаря (строятся из UserData.Inventory) —
            RebuildInventoryCaches();

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
            IGSClientAPI.GetUserAllData(
                resultCallback: ProcessingAllData,
                notConnectionErrorCallback: OnAllDataRequestError,
                connectionErrorCallback: () => { RequestUserAllData(); TryInvokeDataRequestAgain(); });
        }

        private static void TryInvokeDataRequestAgain()
        {
            if (!_continueRequestAllDataSequence)
                DataRequested?.Invoke();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Инвентарь — производные кэши (пересчитываются из UserData.Inventory)
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Перестраивает все производные коллекции инвентаря на основе
        /// актуальных данных из <see cref="IDosGamesData.User"/>.
        /// Вызывается автоматически при каждом изменении UserData.Inventory /
        /// VirtualCurrency / VirtualCurrencyRechargeTimes.
        /// </summary>
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

            InventoryUpdated?.Invoke();
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

        // ═════════════════════════════════════════════════════════════════════
        //  Публичный API — инвентарь (делегирует к кэшам, источник — UserData)
        // ═════════════════════════════════════════════════════════════════════

        public static IReadOnlyList<ItemInstance> GetInventoryItemsFull()
            => _inventoryItemsFull;

        public static IReadOnlyList<ItemInstance> GetInventoryItemsFull(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return Array.Empty<ItemInstance>();
            return _inventoryItemsByItemId.TryGetValue(itemId, out var list)
                ? (IReadOnlyList<ItemInstance>)list
                : Array.Empty<ItemInstance>();
        }

        /// <summary>
        /// Возвращает первый ItemInstanceId для указанного ItemID или null.
        /// </summary>
        public static string GetItemInstanceId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            if (_inventoryItemsByItemId.TryGetValue(itemId, out var list) && list?.Count > 0)
                return list[0].ItemInstanceId;
            return _inventoryItemsFull.FirstOrDefault(x => x?.ItemId == itemId)?.ItemInstanceId;
        }

        /// <summary>
        /// Возвращает список всех ItemInstanceId для указанного ItemID.
        /// </summary>
        public static List<string> GetItemInstanceIds(string itemId)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(itemId)) return result;

            if (_inventoryItemsByItemId.TryGetValue(itemId, out var list) && list?.Count > 0)
            {
                foreach (var it in list)
                    if (!string.IsNullOrEmpty(it?.ItemInstanceId))
                        result.Add(it.ItemInstanceId);
                return result;
            }

            foreach (var it in _inventoryItemsFull)
                if (it?.ItemId == itemId && !string.IsNullOrEmpty(it.ItemInstanceId))
                    result.Add(it.ItemInstanceId);

            return result;
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

        // ═════════════════════════════════════════════════════════════════════
        //  Публичный API — кэш конфигурации и каталога
        // ═════════════════════════════════════════════════════════════════════

        public static string GetCachedTitlePublicConfig(TitleDataKey dataKey)
            => GetCachedTitlePublicConfig(dataKey.ToString());

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

        public static bool IsSkinEquipped(string itemID)
            => _equippedSkins.Contains(itemID);

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
            var products = ShopSystem.ProductsForRealMoney;
            if (products == null) return null;

            foreach (var product in products)
            {
                if (product[JsonProperty.ITEM_ID]?.ToString() != productID) continue;

                return new Product
                {
                    Name = product[JsonProperty.NAME]?.ToString(),
                    ItemID = product[JsonProperty.ITEM_ID]?.ToString(),
                    ProductType = product[JsonProperty.PRODUCT_TYPE]?.ToString(),
                    ItemClass = product[JsonProperty.ITEM_CLASS]?.ToString(),
                    PriceRM = product[JsonProperty.PRICE_RM]?.ToString(),
                    ImagePath = product[JsonProperty.IMAGE_PATH]?.ToString(),
                    ItemsToGrant = product[JsonProperty.ITEMS_TO_GRANT]?.ToObject<List<ItemToGrant>>()
                };
            }

            return null;
        }

        public static float GetTelegramStarPrice()
        {
            string titleData = GetCachedTitlePublicConfig(TitleDataKey.telegram_settings);
            if (string.IsNullOrEmpty(titleData)) return 2f;

            var obj = JsonConvert.DeserializeObject<JObject>(titleData);
            if (obj.ContainsKey(JsonProperty.STAR_PRICE_IN_CENT))
                return float.Parse(obj[JsonProperty.STAR_PRICE_IN_CENT].ToString());

            return 2f;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Экипировка скинов
        // ═════════════════════════════════════════════════════════════════════

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
            EquippedSkinsUpdated?.Invoke();
        }

        private static void OnErrorUpdateEquippedSkins()
            => Message.Show(MessageCode.FAILED_TO_UPDATE_EQUIPPED_SKINS);

        // ═════════════════════════════════════════════════════════════════════
        //  Каталог скинов
        // ═════════════════════════════════════════════════════════════════════

        private static void OnCatalogItemsReceived(GetCatalogItemsResult result)
        {
            UpdateCachedSkinItems(result);
            IDosGamesData.Config.ApplyCatalog(CATALOG_SKIN, result);
        }

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

        // ═════════════════════════════════════════════════════════════════════
        //  VIP / Custom user data
        // ═════════════════════════════════════════════════════════════════════

        public static void UpdateCustomUserData(string key, object data)
            => _ = UserService.UpdateCustomUserData(key, data);

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

        private static void OnVIPSubscriptionValidated()
            => RequestUserAllData();

        private static void OnAllDataRequestError(string error)
            => AllDataRequestError?.Invoke(error);
    }
}
