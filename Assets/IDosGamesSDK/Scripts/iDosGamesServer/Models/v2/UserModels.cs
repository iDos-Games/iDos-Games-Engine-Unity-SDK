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
        public int SubtractAmount { get; set; }
    }

    public class CurrencyUpdateResponse
    {
        public string CurrencyID { get; set; }
        public int NewBalance { get; set; }
    }

    public class UsageTimeStats
    {
        public int Today { get; set; }
        public int Yesterday { get; set; }
        public int CurrentWeek { get; set; }
        public int CurrentMonth { get; set; }
        public long Total { get; set; }
        public Dictionary<DateTime, int> History { get; set; }
    }

    public enum UserAction
    {
        GetUserAllData,
        GetUserInventory,
        GetCustomUserData,
        UpdateCustomUserData,
        SubtractVirtualCurrency,
        DeleteUserAccount,
        GetUsageTime,
        AddUsageTime,
    }
}
