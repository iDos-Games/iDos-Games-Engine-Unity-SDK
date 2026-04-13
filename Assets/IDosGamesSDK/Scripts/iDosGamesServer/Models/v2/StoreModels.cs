using IDosGames.TitlePublicConfiguration;
using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum StoreAction
    {
        Purchase,
        GetDefinitions,
        GetUserState,
    }

    public enum StoreOfferType
    {
        Default,
        Special,
        Event,
        PremiumOnly,
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class StoreRequest : IGSRequest
    {
        public string OfferID { get; set; }
        public int Count { get; set; } = 1;
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class StorePurchaseResponse
    {
        public string OfferID { get; set; }
        public int Count { get; set; }
        public List<ItemOrCurrency> Consumed { get; set; }
        public List<ItemOrCurrency> Granted { get; set; }
    }

    // =================================================================================
    // DEFINITIONS (config — lives in TitleConfig)
    // =================================================================================

    [Serializable]
    public class StoreDefinitions
    {
        public List<StoreDefinition> Stores { get; set; }
        public List<StoreOfferDefinition> StoreOffers { get; set; }
    }

    [Serializable]
    public class StoreDefinition
    {
        public string StoreID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public StoreRules Rules { get; set; }
        public StoreUISettings UI { get; set; }
    }

    [Serializable]
    public class StoreRules
    {
        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
        public int RequiredPremiumTier { get; set; }
        public string RequiredPremiumID { get; set; }
        public List<string> RequiredFlags { get; set; }
    }

    [Serializable]
    public class StoreUISettings
    {
        public int Priority { get; set; }
        public string Icon { get; set; }
    }

    [Serializable]
    public class StoreOfferDefinition
    {
        public string OfferID { get; set; }
        public List<string> StoreIDs { get; set; }
        public StoreOfferType Type { get; set; }
        public string Name { get; set; }
        public EventTokenCost EventTokenCost { get; set; }
        public List<ItemOrCurrency> Cost { get; set; }
        public List<ItemOrCurrency> Rewards { get; set; }
        public StoreOfferRules Rules { get; set; }
        public StoreOfferUISettings UI { get; set; }
    }

    [Serializable]
    public class EventTokenCost
    {
        public string EventID { get; set; }
        public string EventChainID { get; set; }
        public long Amount { get; set; }
    }

    [Serializable]
    public class StoreOfferRules
    {
        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
        public int RequiredPremiumTier { get; set; }
        public string RequiredPremiumID { get; set; }
        public int PremiumDiscountPercent { get; set; }
        public int MaxPurchasesTotal { get; set; }
        public int MaxPurchasesDaily { get; set; }
    }

    [Serializable]
    public class StoreOfferUISettings
    {
        public int Priority { get; set; }
        public string Badge { get; set; }
    }

    // =================================================================================
    // USER STATE (runtime — lives in UserData)
    // =================================================================================

    [Serializable]
    public class UserStoreState
    {
        public Dictionary<string, StorePurchaseState> Purchases { get; set; }
    }

    [Serializable]
    public class StorePurchaseState
    {
        public string OfferID { get; set; }
        public int TotalPurchases { get; set; }
        public int DailyPurchases { get; set; }
        public DateTime DailyResetUtc { get; set; }
        public DateTime LastPurchasedAt { get; set; }
    }
}
