using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Unified request for all Store actions.
    /// <list type="bullet">
    ///   <item><b>Purchase</b> — requires <see cref="OfferID"/>; <see cref="Count"/> defaults to 1.</item>
    ///   <item><b>GetDefinitions</b> — no extra fields.</item>
    ///   <item><b>GetUserState</b> — no extra fields.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class StoreRequest : BaseRequest
    {
        /// <summary>Target offer identifier. Required for <see cref="StoreAction.Purchase"/>.</summary>
        public string OfferID { get; set; }

        /// <summary>How many units to buy. Clamped to [1, 100] server-side. Defaults to 1.</summary>
        public int Count { get; set; } = 1;
    }

    // =====================================================================
    // Responses
    // =====================================================================

    /// <summary>Response for <see cref="StoreAction.Purchase"/>.</summary>
    [Serializable]
    public class StorePurchaseResponse
    {
        /// <summary>Server-side UTC timestamp of the purchase.</summary>
        public DateTime ServerTimeUtc { get; set; }

        /// <summary>Purchased offer identifier.</summary>
        public string OfferID { get; set; }

        /// <summary>Number of units purchased.</summary>
        public int Count { get; set; }

        /// <summary>
        /// Full applied resource operation.
        /// <c>Consume.Standard</c> — actually deducted (after discount/tier replacement).
        /// <c>Grant.Standard</c>   — base rewards (after bonus multiplier).
        /// <c>Grant.PremiumTiers</c> — per-tier premium rewards (for UI).
        /// EventTokens inside are <c>EventTokenOperationApplied</c> (Requested/Applied/NewBalance).
        /// </summary>
        public ResourceOperation Resources { get; set; } = new();
    }

    // =====================================================================
    // User State
    // =====================================================================

    /// <summary>Per-user store purchase counters. Used to enforce purchase limits.</summary>
    [Serializable]
    public class UserStoreState
    {
        /// <summary>OfferID → purchase counters for that offer.</summary>
        public Dictionary<string, StorePurchaseState> Purchases { get; set; }
    }

    /// <summary>Purchase counters for a single offer.</summary>
    [Serializable]
    public class StorePurchaseState
    {
        /// <summary>Offer identifier (mirrors the dictionary key).</summary>
        public string OfferID { get; set; }

        /// <summary>Lifetime purchase count. Enforced against <see cref="StoreOfferRules.MaxPurchasesTotal"/>.</summary>
        public int TotalPurchases { get; set; }

        /// <summary>Purchases within the current daily window.</summary>
        public int DailyPurchases { get; set; }

        /// <summary>UTC timestamp when <see cref="DailyPurchases"/> resets.</summary>
        public DateTime DailyResetUtc { get; set; }

        /// <summary>UTC timestamp of the most recent purchase of this offer.</summary>
        public DateTime LastPurchasedAt { get; set; }
    }

    // =====================================================================
    // Config Models
    // =====================================================================

    /// <summary>Root store configuration from <c>TitlePublicConfigurationModel.Store</c>.</summary>
    [Serializable]
    public class StoreDefinitions
    {
        /// <summary>StoreID → store metadata and rules.</summary>
        public Dictionary<string, StoreDefinition> Stores { get; set; }

        /// <summary>OfferID → offer definition (cost, rewards, rules).</summary>
        public Dictionary<string, StoreOfferDefinition> StoreOffers { get; set; }
    }

    /// <summary>Metadata and rules for a named store (e.g. "main", "event_halloween").</summary>
    [Serializable]
    public class StoreDefinition
    {
        /// <summary>Unique store identifier.</summary>
        public string StoreID { get; set; }

        /// <summary>Store type / user segment tag.</summary>
        public string Type { get; set; }

        /// <summary>Display name (optional).</summary>
        public string Name { get; set; }

        /// <summary>Display description (optional).</summary>
        public string Description { get; set; }

        /// <summary>Availability rules for the store.</summary>
        public StoreRules Rules { get; set; }

        /// <summary>UI display settings.</summary>
        public StoreUISettings UI { get; set; }
    }

    /// <summary>Availability rules for a store (time window, feature flags).</summary>
    [Serializable]
    public class StoreRules
    {
        /// <summary>UTC start time. Null = always available from the beginning.</summary>
        public DateTime? StartUtc { get; set; }

        /// <summary>UTC end time. Null = never expires.</summary>
        public DateTime? EndUtc { get; set; }

        /// <summary>Feature flags or segment tags required to access the store.</summary>
        public List<string> RequiredFlags { get; set; }
    }

    /// <summary>UI display settings for a store.</summary>
    [Serializable]
    public class StoreUISettings
    {
        /// <summary>Sort priority (lower = shown first).</summary>
        public int Priority { get; set; }

        /// <summary>Icon resource key.</summary>
        public string Icon { get; set; }
    }

    /// <summary>A single purchasable offer: cost, rewards, limits, and UI metadata.</summary>
    [Serializable]
    public class StoreOfferDefinition
    {
        /// <summary>Unique offer identifier.</summary>
        public string OfferID { get; set; }

        /// <summary>Which stores expose this offer.</summary>
        public List<string> StoreIDs { get; set; }

        /// <summary>Display name (optional).</summary>
        public string Name { get; set; }

        /// <summary>
        /// What the player pays. Supports premium discounts via
        /// <c>Cost.PremiumDiscounts</c> and tier-replacement via <c>Cost.PremiumTiers</c>.
        /// </summary>
        public ResourceConsume Cost { get; set; }

        /// <summary>
        /// What the player receives. Supports bonus multipliers via
        /// <c>Rewards.PremiumBonuses</c> and extra tier rewards via <c>Rewards.PremiumTiers</c>.
        /// </summary>
        public ResourceGrant Rewards { get; set; }

        /// <summary>Purchase limits, time windows, and premium gating.</summary>
        public StoreOfferRules Rules { get; set; }

        /// <summary>UI display settings for this offer.</summary>
        public StoreOfferUISettings UI { get; set; }
    }

    /// <summary>Purchase rules for a single offer.</summary>
    [Serializable]
    public class StoreOfferRules
    {
        /// <summary>UTC start time for this offer. Null = no start restriction.</summary>
        public DateTime? StartUtc { get; set; }

        /// <summary>UTC end time for this offer. Null = no expiry.</summary>
        public DateTime? EndUtc { get; set; }

        /// <summary>Minimum premium tier required to purchase. 0 = available to all.</summary>
        public int RequiredPremiumTier { get; set; }

        /// <summary>Specific premium subscription ID required. Null = any subscription of sufficient tier.</summary>
        public string RequiredPremiumID { get; set; }

        /// <summary>Maximum lifetime purchases per player. 0 = unlimited.</summary>
        public int MaxPurchasesTotal { get; set; }

        /// <summary>Maximum purchases per day per player. 0 = unlimited.</summary>
        public int MaxPurchasesDaily { get; set; }
    }

    /// <summary>UI display settings for an offer.</summary>
    [Serializable]
    public class StoreOfferUISettings
    {
        /// <summary>Sort priority within the store (lower = shown first).</summary>
        public int Priority { get; set; }

        /// <summary>Badge label, e.g. "SALE", "BEST", "NEW".</summary>
        public string Badge { get; set; }
    }

    // =====================================================================
    // Action Enum
    // =====================================================================

    /// <summary>All available Store API actions.</summary>
    public enum StoreAction
    {
        Purchase,
        GetDefinitions,
        GetUserState,
    }
}
