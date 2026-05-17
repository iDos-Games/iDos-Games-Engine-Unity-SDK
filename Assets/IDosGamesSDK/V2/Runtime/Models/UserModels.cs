using System;
using System.Collections.Generic;

namespace IDosGames
{
    [Serializable]
    public class UserRequest : BaseRequest
    {
        public bool IsNewSession { get; set; }
        public int SessionDurationSeconds { get; set; }
        public List<string> Fields { get; set; }
        public List<string> TitleFields { get; set; }
    }

    [Serializable]
    public class UserPublicDataModel
    {
        public string Username { get; set; }
        public string Country { get; set; }
        public string AvatarUrl { get; set; }
        public bool Premium { get; set; }

        // Можно переиспользовать под общий прогресс
        public int Level { get; set; }
        public long Power { get; set; }
        public long NetWorth { get; set; }
        //public Dictionary<string, float> Stats { get; set; }
    }

    [Serializable]
    public class UserUsageState
    {
        public long TotalSeconds { get; set; }
        public long TotalSessions { get; set; }
        public DateTime LastActiveAt { get; set; }
        public Dictionary<string, DailyUsageRecord> Daily { get; set; }
    }

    [Serializable]
    public class UsageTimeStats
    {
        public int Today { get; set; }
        public int Yesterday { get; set; }
        public int CurrentWeek { get; set; }
        public int CurrentMonth { get; set; }
        public long Total { get; set; }
        public long TotalSessions { get; set; }
        public DateTime LastActiveAt { get; set; }
        public Dictionary<string, DailyUsageRecord> History { get; set; }
    }

    [Serializable]
    public class DailyUsageRecord
    {
        public int Seconds { get; set; }
        public int Sessions { get; set; }
        public DateTime LastActiveAt { get; set; }
    }

    [Serializable]
    public class UserInventoryState
    {
        public long Version { get; set; }
        public Dictionary<string, UserVirtualCurrencyState> VirtualCurrencies { get; set; }
        public Dictionary<string, UserCryptoCurrencyState> CryptoCurrencies { get; set; }
        public Dictionary<string, ItemTotals> Items { get; set; }
        public Dictionary<string, UnstackableItemInstanceState> UnstackableItems { get; set; }
    }

    [Serializable]
    public class UserVirtualCurrencyState
    {
        public long Amount { get; set; }
        public UserRechargeState Recharge { get; set; }
        public UserDailyCounters Daily { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [Serializable]
    public class UserRechargeState
    {
        public DateTime LastRechargeAt { get; set; }
        public long PendingSeconds { get; set; }
    }

    [Serializable]
    public class UserDailyCounters
    {
        public DateTime PeriodStartUtc { get; set; }
        public long Earned { get; set; }
        public long Spent { get; set; }
    }

    [Serializable]
    public class UserCryptoCurrencyState
    {
        public decimal Amount { get; set; }
        public decimal Frozen { get; set; }
        public Dictionary<string, UserDepositAddress> DepositAddresses { get; set; }
        public UserCryptoComplianceCounters Compliance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [Serializable]
    public class UserDepositAddress
    {
        public string Address { get; set; }
        public string Memo { get; set; }
        public DateTime AssignedAt { get; set; }
    }

    [Serializable]
    public class UserCryptoComplianceCounters
    {
        public DateTime DailyPeriodStartUtc { get; set; }
        public decimal DailyWithdrawnUsd { get; set; }
        public DateTime MonthlyPeriodStartUtc { get; set; }
        public decimal MonthlyWithdrawnUsd { get; set; }
    }

    [Serializable]
    public class ItemTotals
    {
        public long StackableAmount { get; set; }
        public long UnstackableAmount { get; set; }
        public long TotalAmount { get; set; }
    }

    [Serializable]
    public class UnstackableItemInstanceState
    {
        public string ItemInstanceID { get; set; }
        public string ItemID { get; set; }
        public string CatalogID { get; set; }
        public long RemainingUses { get; set; }
        public DateTime AcquiredAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public EquipmentSlot EquippedSlot { get; set; }
        public string CustomData { get; set; }
    }

    public enum UserAction
    {
        GetClientState,
        GetInventory,
        GetEventTokens,
        GetUsageTime,
        AddUsageTime,
        DeleteUserAccount,
    }
}
