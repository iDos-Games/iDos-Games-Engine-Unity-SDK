using System;

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

    public enum UserAction
    {
        GetUserAllData,
        GetUserInventory,
        GetCustomUserData,
        UpdateCustomUserData,
        SubtractVirtualCurrency,
        DeleteUserAccount,
    }
}
