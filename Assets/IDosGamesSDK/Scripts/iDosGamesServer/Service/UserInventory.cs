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
        private static readonly Dictionary<string, int> _virtualCurrencyAmounts = new();
        private static readonly Dictionary<SpinTicketType, int> _spinTickets = new();
        private static readonly Dictionary<ChestKeyFragmentType, int> _chestKeyFragments = new();

        public static event Action InventoryUpdated;

        public static event Action SuccessSubtractVirtualCurrency;
        public static event Action ErrorSubtractVirtualCurrency;

        public static bool HasVIPStatus { get; private set; }

        public static int SecondarySpinTicketRechargeMax { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new();
        }

        public static int GetItemAmount(string itemID)
        {
            _eachItemAmounts.TryGetValue(itemID, out int amount);

            return amount;
        }

        public static int GetVirtualCurrencyAmount(VirtualCurrencyID virtualCurrencyID)
        {
            return GetVirtualCurrencyAmount(virtualCurrencyID.ToString());
        }

        public static int GetVirtualCurrencyAmount(string virtualCurrencyID)
        {
            _virtualCurrencyAmounts.TryGetValue(virtualCurrencyID, out int amount);

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

        public static void SubtractVirtualCurrency(VirtualCurrencyID currencyID, int amount)
        {
            SubtractVirtualCurrency(currencyID.ToString(), amount);
        }

        public static void SubtractVirtualCurrency(string currencyID, int amount)
        {
            if (_virtualCurrencyAmounts[currencyID] - amount < 0)
            {
                Instance.OnErrorSubtractVirtualCurrency();
                return;
            }

            FunctionParameters parameter = new()
            {
                CurrencyID = currencyID,
                Amount = amount
            };

            _ = IGSClientAPI.ExecuteFunction
            (
                functionName: ServerFunctionHandlers.SubtractVirtualCurrencyHandler,
                resultCallback: (result) => Instance.OnSuccessSubtractVirtualCurrency(result, currencyID, amount),
                notConnectionErrorCallback: (error) => Instance.OnErrorSubtractVirtualCurrency(),
                connectionErrorCallback: () => SubtractVirtualCurrency(currencyID, amount),
                functionParameter: parameter
            );
        }

        private void OnSuccessSubtractVirtualCurrency(string result, string currencyID, int amount)
        {
            if (result == null)
            {
                OnErrorSubtractVirtualCurrency();
                return;
            }

            _virtualCurrencyAmounts[currencyID] -= amount;

            InventoryUpdated?.Invoke();
            SuccessSubtractVirtualCurrency?.Invoke();
        }

        public static void OnSuccessCoinRewardClaimed(int value)
        {
            _virtualCurrencyAmounts[VirtualCurrencyID.CO.ToString()] += value;

            InventoryUpdated?.Invoke();
        }

        private void OnErrorSubtractVirtualCurrency()
        {
            Debug.Log("Error Subtract Virtual Currency");

            ErrorSubtractVirtualCurrency?.Invoke();
        }

        private static void OnUserInventoryReceived(GetUserInventoryResult result)
        {
            Instance.ResetAllData();

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
        }

        private void SetEachItemAmounts(List<ItemInstance> inventoryItems)
        {
            foreach (var item in inventoryItems)
            {
                int amount = _eachItemAmounts.ContainsKey(item.ItemId) ? _eachItemAmounts[item.ItemId] : 0;
                var remainingUses = item.RemainingUses;
                amount += remainingUses != null ? (int)remainingUses : 1;

                _eachItemAmounts[item.ItemId] = amount;
            }
        }

        private void SetVirtualCurrencyAmounts(Dictionary<string, int> virtualCurrencies)
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

        private async void UpdateVIPStatus(List<ItemInstance> inventoryItems)
        {
            HasVIPStatus = false;

            foreach (var item in inventoryItems)
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