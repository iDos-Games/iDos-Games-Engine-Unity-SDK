using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
    public class AlarmSystem : MonoBehaviour
    {
        public static AlarmSystem Instance { get; private set; }

        private readonly List<AlarmObject> _alarmObjects = new();
        private readonly Dictionary<AlarmType, bool> _alarmStates = new();

        public const int DELAY_CHECK_OPENED_WINDOWS = 2;

        private void Awake()
        {
            if (Instance == null || ReferenceEquals(this, Instance))
            {
                Instance = this;
            }

            InitializeAlarmStates();
        }

        private void Start()
        {
            Invoke(nameof(CheckWindowsOpened), DELAY_CHECK_OPENED_WINDOWS);
        }

        private void OnEnable()
        {
            UserInventory.InventoryUpdated += OnInventoryUpdated;
            UserDataService.CustomUserDataUpdated += OnUserReadOnlyDataUpdated;
        }

        private void OnDisable()
        {
            UserInventory.InventoryUpdated -= OnInventoryUpdated;
            UserDataService.CustomUserDataUpdated -= OnUserReadOnlyDataUpdated;
        }

        public void AddAlarmObject(AlarmObject alarmObject)
        {
            _alarmObjects.Add(alarmObject);
        }

        public bool GetAlarmState(AlarmType type)
        {
            return _alarmStates[type];
        }

        public int GetActiveAlarmsAmountOnSideBar()
        {
            int count = 0;

            foreach (AlarmType alarmType in Enum.GetValues(typeof(AlarmType)))
            {
                if (alarmType == AlarmType.OpenedAuthorizationPopUp)
                {
                    continue;
                }

                if (_alarmStates[alarmType])
                {
                    count++;
                }
            }

            return count;
        }

        public void SetAlarmState(AlarmType alarmType, bool active)
        {
            _alarmStates[alarmType] = active;

            foreach (var alarm in _alarmObjects)
            {
                if (alarm.AlarmType == alarmType)
                {
                    alarm.SetActivateRoot(active);
                }
            }
        }

        private void InitializeAlarmStates()
        {
            foreach (AlarmType alarmType in Enum.GetValues(typeof(AlarmType)))
            {
                _alarmStates.Add(alarmType, false);
            }
        }

        private void CheckWindowsOpened()
        {
            SetAlarmState(AlarmType.OpenedInviteFriendsPopUp, PlayerPrefs.GetInt(AlarmType.OpenedInviteFriendsPopUp.ToString(), 0) == 0);
            SetAlarmState(AlarmType.OpenedLeaderboardWindow, PlayerPrefs.GetInt(AlarmType.OpenedLeaderboardWindow.ToString(), 0) == 0);
            SetAlarmState(AlarmType.OpenedAuthorizationPopUp, PlayerPrefs.GetInt(AlarmType.OpenedAuthorizationPopUp.ToString(), 0) == 0);
        }

        private void OnInventoryUpdated()
        {
            UpdateSpinAlarm();
            UpdateChestAlarm();
        }

        private void OnUserReadOnlyDataUpdated()
        {
            UpdateShopAlarms();
            UpdateFriendAlarm();
        }

        private void UpdateFriendAlarm()
        {
            var friend_request = UserDataService.GetCachedCustomUserData(CustomUserDataKey.friend_requests);


            List<string> friends = new List<string>();
            if (!String.IsNullOrEmpty(friend_request))
            {
                friends = JsonConvert.DeserializeObject<List<string>>(friend_request);
                SetAlarmState(AlarmType.AvailableNewFriend, friends.Count() > 0);
            }
            else
            {
                SetAlarmState(AlarmType.AvailableNewFriend, false);
            }



        }

        private void UpdateSpinAlarm()
        {
            var premiumTiketsAmount = UserInventory.GetSpinTicketAmount(SpinTicketType.Premium);
            var standardTiketsAmount = UserInventory.GetSpinTicketAmount(SpinTicketType.Standard);

            SetAlarmState(AlarmType.AvailablePremiumSpin, premiumTiketsAmount > 0);
            SetAlarmState(AlarmType.AvailableStandardSpin, standardTiketsAmount > 0);
        }

        private void UpdateChestAlarm()
        {
            int commonKey1 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Common_1);
            int commonKey2 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Common_2);
            int commonKey3 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Common_3);

            int rareKey1 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Rare_1);
            int rareKey2 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Rare_2);
            int rareKey3 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Rare_3);

            int legendaryKey1 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Legendary_1);
            int legendaryKey2 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Legendary_2);
            int legendaryKey3 = UserInventory.GetChestKeyFragmentAmount(ChestKeyFragmentType.Legendary_3);

            bool hasChest = false;

            if (commonKey1 > 0 && commonKey2 > 0 && commonKey3 > 0)
            {
                hasChest = true;
            }
            else if (rareKey1 > 0 && rareKey2 > 0 && rareKey3 > 0)
            {
                hasChest = true;
            }
            else if (legendaryKey1 > 0 && legendaryKey2 > 0 && legendaryKey3 > 0)
            {
                hasChest = true;
            }

            SetAlarmState(AlarmType.AvailableChest, hasChest);
        }

        private void UpdateShopAlarms()
        {
            var playerData = UserDataService.GetCachedCustomUserData(CustomUserDataKey.shop_daily_free_products);
            var dataDailyFreeProductsData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ShopDailyFreeProducts);

            var freeProducts = JsonConvert.DeserializeObject<JArray>(dataDailyFreeProductsData);
            freeProducts ??= new JArray();

            int availableFreeProducts = 0;

            foreach (var product in freeProducts)
            {
                if ($"{product[JsonProperty.ENABLED]}" != JsonProperty.ENABLED_VALUE)
                {
                    continue;
                };

                var itemID = $"{product[JsonProperty.ITEM_ID]}";

                int productAmountInPlayer = GetAvailabeFreeProductsAmountInPlayer(itemID, playerData);

                if (productAmountInPlayer > 0)
                {
                    availableFreeProducts++;
                    break;
                }
            }

            SetAlarmState(AlarmType.AvailableFreeItemOnShop, availableFreeProducts > 0);
            SetAlarmState(AlarmType.AvailableUpdateDailyFreeProducts, IsNeedUpdateDailyFreeProducts());
        }

        private bool IsNeedUpdateDailyFreeProducts()
        {
            var playerData = UserDataService.GetCachedCustomUserData(CustomUserDataKey.shop_daily_free_products);

            if (playerData == string.Empty)
            {
                return true;
            }

            var jsonData = JsonConvert.DeserializeObject<JObject>(playerData);
            var playerLastUpdateDate = GetEndDateTime(jsonData);

            var dailyOfferData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ShopDailyProducts);
            var offerData = JsonConvert.DeserializeObject<JObject>(dailyOfferData);

            if (GetEndDateTime(offerData) > playerLastUpdateDate)
            {
                return true;
            }

            return false;
        }

        private int GetAvailabeFreeProductsAmountInPlayer(string itemID, string playerData)
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

        private DateTime GetEndDateTime(JToken jToken)
        {
            DateTime.TryParse($"{jToken[JsonProperty.END_DATE]}", out DateTime endDate);

            return endDate.ToUniversalTime();
        }
    }
}