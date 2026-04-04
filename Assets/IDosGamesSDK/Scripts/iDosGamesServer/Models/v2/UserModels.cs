using IDosGames.ClientModels;
using System;
using System.Collections.Generic;

namespace IDosGames.ServerModels
{
    [Serializable]
    public class UserRequest : IGSRequest
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string CurrencyID { get; set; }
        public long SubtractAmount { get; set; }

        public string FromCurrencyID { get; set; }
        public string ToCurrencyID { get; set; }
        public long TransferAmount { get; set; } = 1;

        public string ItemInstanceID { get; set; }
    }

    [Serializable]
    public class CurrencyUpdateResponse
    {
        public string CurrencyID { get; set; }
        public long NewBalance { get; set; }
    }

    [Serializable]
    public class UsageTimeStats
    {
        public int Today { get; set; }
        public int Yesterday { get; set; }
        public int CurrentWeek { get; set; }
        public int CurrentMonth { get; set; }
        public long Total { get; set; }
        public Dictionary<DateTime, int> History { get; set; }
    }

    [Serializable]
    public class CurrencyTransferResponse
    {
        public string FromCurrencyID { get; set; }
        public string ToCurrencyID { get; set; }
        public long TransferAmount { get; set; }
        public Dictionary<string, long> UpdatedVirtualCurrencies { get; set; }
    }

    [Serializable]
    public class ConsumeItemResponse
    {
        public string ItemID { get; set; }
        public string ItemInstanceID { get; set; }
        public string CatalogVersion { get; set; }
        public long ConsumedAmount { get; set; }
    }


    [Serializable]
    public class VirtualCurrencyResponse
    {
        public string UserID { get; set; }
        public Dictionary<string, long> VirtualCurrency { get; set; }
        public Dictionary<string, VirtualCurrencyRechargeTime> VirtualCurrencyRechargeTimes { get; set; }
    }

    public enum UserAction
    {
        GetClientState,
        GetUserInventory,
        GetCustomUserData,
        UpdateCustomUserData,
        SubtractVirtualCurrency,
        DeleteUserAccount,
        GetUsageTime,
        AddUsageTime,
        TransferVirtualCurrency,
        ConsumeItem,
        GetVirtualCurrency,
    }
}
