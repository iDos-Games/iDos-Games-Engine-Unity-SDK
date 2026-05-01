using System;
using System.Collections.Generic;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum PremiumAction
    {
        GetDefinitions,
        GetUserState,
        ActivateTrial,
        PurchaseItemOrCurrency,
        PurchaseRealMoney,
    }

    public enum StoreType
    {
        Apple,
        Google,
    }

    public enum PremiumRewardsMode
    {
        /// <summary>Premium-награды выдаются поверх базовых наград.</summary>
        Additive,
        /// <summary>Premium-награды полностью заменяют базовые награды.</summary>
        Replace,
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class PremiumRequest : IGSRequest
    {
        public string PremiumID { get; set; }
        public int Count { get; set; } = 1;
        public int SelectedOptionID { get; set; }
        public string TransactionID { get; set; }

        public StoreType Store { get; set; }
        public string ProductID { get; set; }
        public string ReceiptData { get; set; }
        public string PurchaseToken { get; set; }
        public string PackageName { get; set; }
        public string AppStoreEnvironment { get; set; }
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class PremiumDefinitionsResponse
    {
        public List<PremiumDefinition> PremiumDefinitions { get; set; }
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
        public List<ItemOrCurrency> ConsumedResources { get; set; }
    }

    // =================================================================================
    // MODELS — Definitions (TitleConfig)
    // =================================================================================

    [Serializable]
    public class PremiumDefinition
    {
        public string PremiumID { get; set; }
        public string DisplayName { get; set; }
        public int Tier { get; set; }
        public int DurationDays { get; set; }
        public int TrialDurationDays { get; set; }
        public List<PremiumPriceOption> PriceOptions { get; set; }
        public int UsdCentPrice { get; set; }
        public string AppleProductID { get; set; }
        public string GoogleProductID { get; set; }

        // Key: benefit name (e.g. "ExpMult", "NoAds"), Value: string-encoded number/bool
        public Dictionary<string, string> Benefits { get; set; }
    }

    [Serializable]
    public class PremiumPriceOption
    {
        public int OptionID { get; set; }
        public string Name { get; set; }
        public List<ItemOrCurrency> RequiredResources { get; set; }
    }

    /// <summary>
    /// Describes a reward tier for Premium users.
    /// Used in Leaderboard, LTE, and other systems that grant bonus rewards based on Premium tier.
    /// </summary>
    [Serializable]
    public class PremiumTierReward
    {
        /// <summary>Display label for UI. Example: "Gold Premium Bonus".</summary>
        public string Label { get; set; }

        /// <summary>
        /// Minimum Premium tier required to receive this reward.
        /// Example: MinPremiumTier=1 → Premium1+, MinPremiumTier=2 → Premium2+.
        /// </summary>
        public int MinPremiumTier { get; set; }

        /// <summary>
        /// Optional. If set — the player must have exactly this subscription active.
        /// null = any player with the required tier qualifies.
        /// </summary>
        public string RequiredPremiumID { get; set; }

        public List<ItemOrCurrency> Rewards { get; set; }
    }

    // =================================================================================
    // MODELS — User state
    // =================================================================================

    [Serializable]
    public class UserPremiumState
    {
        // PremiumID -> subscription
        public Dictionary<string, PremiumSubscription> Subscriptions { get; set; }
        public List<string> ActivatedTrialIds { get; set; }
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
}
