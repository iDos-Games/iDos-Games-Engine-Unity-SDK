using IDosGames.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
    public class UserInventory
    {
        private static UserInventory _instance;

        public static UserInventory Instance => _instance;

        private UserInventory()
        {
            _instance = this;

            UserDataService.UserInventoryReceived += OnUserInventoryReceived;
        }

        private static readonly Dictionary<string, int> _eachItemAmounts = new();
        private static readonly Dictionary<string, long> _virtualCurrencyAmounts = new();
        private static readonly Dictionary<SpinTicketType, int> _spinTickets = new();
        private static readonly Dictionary<ChestKeyFragmentType, int> _chestKeyFragments = new();

        private static readonly List<ItemInstance> _inventoryItemsFull = new();
        private static readonly Dictionary<string, List<ItemInstance>> _inventoryItemsByItemId = new();

        public static event Action InventoryUpdated;

        //public static event Action SuccessSubtractVirtualCurrency;
        //public static event Action ErrorSubtractVirtualCurrency;

        public static bool HasVIPStatus { get; private set; }

        public static int SecondarySpinTicketRechargeMax { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new();
        }

        public static IReadOnlyList<ItemInstance> GetInventoryItemsFull()
        {
            return _inventoryItemsFull;
        }

        public static IReadOnlyList<ItemInstance> GetInventoryItemsFull(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return Array.Empty<ItemInstance>();

            return _inventoryItemsByItemId.TryGetValue(itemId, out var list)
                ? (IReadOnlyList<ItemInstance>)list
                : Array.Empty<ItemInstance>();
        }

        /// <summary>
        /// Возвращает ItemInstanceId для указанного ItemID.
        /// Если у предмета несколько инстансов — вернёт первый найденный.
        /// Если не найдено — вернёт null.
        /// </summary>
        /// <param name="itemId">ItemID (ItemId) предмета.</param>
        /// <returns>ItemInstanceId или null.</returns>
        public static string GetItemInstanceId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            if (_inventoryItemsByItemId.TryGetValue(itemId, out var list) && list != null && list.Count > 0)
            {
                return list[0].ItemInstanceId;
            }

            var inst = _inventoryItemsFull.FirstOrDefault(x => x != null && x.ItemId == itemId);
            return inst != null ? inst.ItemInstanceId : null;
        }

        /// <summary>
        /// Возвращает список всех ItemInstanceId для указанного ItemID.
        /// Если не найдено — вернёт пустой List<string>.
        /// </summary>
        /// <param name="itemId">ItemID (ItemId) предмета.</param>
        /// <returns>Список ItemInstanceId (может быть пустым).</returns>
        public static List<string> GetItemInstanceIds(string itemId)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(itemId))
                return result;

            if (_inventoryItemsByItemId.TryGetValue(itemId, out var list) && list != null && list.Count > 0)
            {
                foreach (var it in list)
                {
                    if (it == null) continue;

                    var id = it.ItemInstanceId;
                    if (!string.IsNullOrEmpty(id))
                        result.Add(id);
                }

                return result;
            }

            foreach (var it in _inventoryItemsFull)
            {
                if (it == null) continue;
                if (it.ItemId != itemId) continue;

                var id = it.ItemInstanceId;
                if (!string.IsNullOrEmpty(id))
                    result.Add(id);
            }

            return result;
        }

        public static int GetItemAmount(string itemID)
        {
            _eachItemAmounts.TryGetValue(itemID, out int amount);

            return amount;
        }

        public static long GetVirtualCurrencyAmount(VirtualCurrencyID virtualCurrencyID)
        {
            return GetVirtualCurrencyAmount(virtualCurrencyID.ToString());
        }

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

        private static void OnUserInventoryReceived(GetUserInventoryResult result)
        {
            Instance.ResetAllData();

            Instance.SetInventoryItemsFull(result.Inventory);
            Instance.SetEachItemAmounts(result.Inventory);
            Instance.SetVirtualCurrencyAmounts(result.VirtualCurrency);
            Instance.SetSpinTickets();
            Instance.SetChestKeyFragments();
            Instance.UpdateVIPStatus(result.Inventory);
            Instance.SetSecondarySpinTicketRechargeMax(result.VirtualCurrencyRechargeTimes);

            InventoryUpdated?.Invoke();
        }

        private void ResetAllData()
        {
            _virtualCurrencyAmounts.Clear();
            _spinTickets.Clear();
            _eachItemAmounts.Clear();
            _inventoryItemsFull.Clear();
            _inventoryItemsByItemId.Clear();
        }

        private void SetInventoryItemsFull(List<ItemInstance> inventoryItems)
        {
            _inventoryItemsFull.Clear();
            _inventoryItemsByItemId.Clear();

            if (inventoryItems == null || inventoryItems.Count == 0) return;

            foreach (var item in inventoryItems)
            {
                if (item == null) continue;

                _inventoryItemsFull.Add(item);

                if (!_inventoryItemsByItemId.TryGetValue(item.ItemId, out var list))
                {
                    list = new List<ItemInstance>(4);
                    _inventoryItemsByItemId[item.ItemId] = list;
                }

                list.Add(item);
            }
        }

        private void SetEachItemAmounts(List<IDosGames.ClientModels.ItemInstance> inventoryItems)
        {
            foreach (IDosGames.ClientModels.ItemInstance item in inventoryItems)
            {
                int amount = _eachItemAmounts.ContainsKey(item.ItemId) ? _eachItemAmounts[item.ItemId] : 0;
                var remainingUses = item.RemainingUses;
                amount += remainingUses != null ? (int)remainingUses : 1;

                _eachItemAmounts[item.ItemId] = amount;
            }
        }

        private void SetVirtualCurrencyAmounts(Dictionary<string, long> virtualCurrencies)
        {
            foreach (var virtualCurency in virtualCurrencies)
            {
                _virtualCurrencyAmounts[virtualCurency.Key] = virtualCurency.Value;
            }
        }

        private void SetSpinTickets()
        {
            _spinTickets[SpinTicketType.Standard] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.STANDARD_SPIN_TICKET).Value;
            _spinTickets[SpinTicketType.Premium] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.PREMIUM_SPIN_TICKET).Value;
        }

        private void SetChestKeyFragments()
        {
            _chestKeyFragments[ChestKeyFragmentType.Common_1] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.COMMON_CHEST_KEY_FRAGMENT_1).Value;
            _chestKeyFragments[ChestKeyFragmentType.Common_2] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.COMMON_CHEST_KEY_FRAGMENT_2).Value;
            _chestKeyFragments[ChestKeyFragmentType.Common_3] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.COMMON_CHEST_KEY_FRAGMENT_3).Value;
            _chestKeyFragments[ChestKeyFragmentType.Rare_1] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.RARE_CHEST_KEY_FRAGMENT_1).Value;
            _chestKeyFragments[ChestKeyFragmentType.Rare_2] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.RARE_CHEST_KEY_FRAGMENT_2).Value;
            _chestKeyFragments[ChestKeyFragmentType.Rare_3] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.RARE_CHEST_KEY_FRAGMENT_3).Value;
            _chestKeyFragments[ChestKeyFragmentType.Legendary_1] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.LEGENDARY_CHEST_KEY_FRAGMENT_1).Value;
            _chestKeyFragments[ChestKeyFragmentType.Legendary_2] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.LEGENDARY_CHEST_KEY_FRAGMENT_2).Value;
            _chestKeyFragments[ChestKeyFragmentType.Legendary_3] = _eachItemAmounts.FirstOrDefault(x => x.Key == ServerItemID.LEGENDARY_CHEST_KEY_FRAGMENT_3).Value;
        }

        private async void UpdateVIPStatus(List<IDosGames.ClientModels.ItemInstance> inventoryItems)
        {
            HasVIPStatus = false;

            foreach (IDosGames.ClientModels.ItemInstance item in inventoryItems)
            {
                if (item.ItemClass == ServerItemClass.VIP)
                {
                    HasVIPStatus = true;
                    break;
                }
            }

            if (HasVIPStatus == false && GetItemAmount(ServerItemID.HAS_VIP_SUBSCRIPTION) > 0)
            {
                await UserDataService.ValidateVIPSubscription();
            }
        }

        private void SetSecondarySpinTicketRechargeMax(Dictionary<string, VirtualCurrencyRechargeTime> rechargeTimes)
        {
            string ticketID = VirtualCurrencyID.SS.ToString();

            SecondarySpinTicketRechargeMax = rechargeTimes.ContainsKey(ticketID) ? rechargeTimes[ticketID].RechargeMax : 0;
        }
    }
}
