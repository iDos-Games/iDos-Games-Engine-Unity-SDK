using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    public enum PremiumAction
    {
        GetDefinitions,
        GetUserState,
        ActivateTrial,
        PurchaseItemOrCurrency,
        PurchaseRealMoney,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StoreType
    {
        Apple,
        Google,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PremiumRewardsMode
    {
        Additive,
        Replace,
    }

    [Serializable]
    public class PremiumRequest : IGSRequest
    {
        public string PremiumID { get; set; }
        public int Count { get; set; } = 1;
        public int SelectedOptionID { get; set; }
        public string TransactionID { get; set; }

        // IAP-поля (только для PurchaseRealMoney)
        public StoreType Store { get; set; }
        public string ProductID { get; set; }
        public string ReceiptData { get; set; }
        public string PurchaseToken { get; set; }
        public string PackageName { get; set; }
        public string AppStoreEnvironment { get; set; }
    }

    [Serializable]
    public class PremiumDefinitionsResponse
    {
        public PremiumDefinitions Premium { get; set; }
    }

    [Serializable]
    public class PremiumStateResponse
    {
        public DateTime ServerTimeUtc { get; set; }
        public UserPremiumState Premium { get; set; }
    }

    [Serializable]
    public class PremiumPurchaseResponse
    {
        public DateTime ServerTimeUtc { get; set; }
        public UserPremiumState Premium { get; set; }
        public PremiumSubscription Subscription { get; set; }
        public ResourceOperation Resources { get; set; }
    }

    [Serializable]
    public class UserPremiumState
    {
        public Dictionary<string, PremiumSubscription> Subscriptions { get; set; }
        public List<string> ActivatedTrialIDs { get; set; }
        public int MaxActiveTier { get; set; }
    }

    [Serializable]
    public class PremiumSubscription
    {
        public string PremiumID { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string TransactionID { get; set; }
        public bool IsAutoRenewEnabled { get; set; }
    }

    [Serializable]
    public class PremiumDefinitions
    {
        public List<PremiumDefinition> Definitions { get; set; } = new();
        public PremiumRewardsMode RewardsMode { get; set; } = PremiumRewardsMode.Additive;
        public bool RewardsStackLowerTiers { get; set; }
    }

    [Serializable]
    public class PremiumDefinition
    {
        public string PremiumID { get; set; }
        public string DisplayName { get; set; }
        public int Tier { get; set; }
        public int DurationDays { get; set; }
        public int TrialDurationDays { get; set; }
        public List<PremiumPriceOption> PriceOptions { get; set; }
        public string AppleProductID { get; set; }
        public string GoogleProductID { get; set; }
        public Dictionary<string, string> Benefits { get; set; } = new();
    }

    [Serializable]
    public class PremiumPriceOption
    {
        public int OptionID { get; set; }
        public string Name { get; set; }
        public ResourceConsume RequiredResources { get; set; }
    }
}
