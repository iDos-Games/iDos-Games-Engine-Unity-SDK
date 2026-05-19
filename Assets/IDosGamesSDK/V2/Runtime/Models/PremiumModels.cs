using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    // =====================================================================
    // REQUEST
    // =====================================================================

    /// <summary>
    /// Unified request for all Premium actions.
    ///
    /// Field usage by action:
    ///   GetDefinitions       — no extra fields required.
    ///   GetUserState         — no extra fields required.
    ///   ActivateTrial        — PremiumID, TransactionID.
    ///   PurchaseItemOrCurrency — PremiumID, TransactionID, SelectedOptionID (default "Default"),
    ///                            Count (default 1).
    ///   PurchaseRealMoney    — PremiumID, TransactionID, Store, ProductID, ReceiptData,
    ///                          PurchaseToken, PackageName, AppStoreEnvironment.
    /// </summary>
    [Serializable]
    public class PremiumRequest : BaseRequest
    {
        /// <summary>ID of the premium subscription (e.g. "silver_vip", "battle_pass_premium").</summary>
        public string PremiumID;

        /// <summary>How many subscription periods to purchase at once. Defaults to 1.</summary>
        public int Count = 1;

        /// <summary>
        /// Key of the price option to use from <see cref="PremiumDefinition.PriceOptions"/>.
        /// Defaults to "Default" when not specified.
        /// </summary>
        public string SelectedOptionID;

        /// <summary>
        /// Client-generated unique transaction ID for idempotency.
        /// Required for ActivateTrial, PurchaseItemOrCurrency, PurchaseRealMoney.
        /// </summary>
        public string TransactionID;

        // ---- Real-money fields (PurchaseRealMoney only) ----

        /// <summary>App store platform (Apple / Google). Used only for PurchaseRealMoney.</summary>
        public StoreType Store;

        /// <summary>Store product identifier. Used only for PurchaseRealMoney.</summary>
        public string ProductID;

        /// <summary>Receipt data from the store. Used only for PurchaseRealMoney.</summary>
        public string ReceiptData;

        /// <summary>Purchase token (Google Play). Used only for PurchaseRealMoney.</summary>
        public string PurchaseToken;

        /// <summary>App package name (Google Play). Used only for PurchaseRealMoney.</summary>
        public string PackageName;

        /// <summary>App Store environment ("Sandbox" / "Production"). Used only for PurchaseRealMoney.</summary>
        public string AppStoreEnvironment;
    }

    // =====================================================================
    // ENUMS
    // =====================================================================

    /// <summary>Available actions for the Premium endpoint.</summary>
    public enum PremiumAction
    {
        GetDefinitions,
        GetUserState,
        ActivateTrial,
        PurchaseItemOrCurrency,
        PurchaseRealMoney,
    }

    /// <summary>App store platform for real-money purchases.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StoreType
    {
        Apple,
        Google,
    }

    // =====================================================================
    // RESPONSES
    // =====================================================================

    /// <summary>Response for <see cref="PremiumAction.GetDefinitions"/>.</summary>
    [Serializable]
    public class PremiumDefinitionsResponse
    {
        /// <summary>Full premium definitions from TitlePublicConfiguration.</summary>
        public PremiumDefinitions Premium;
    }

    /// <summary>Response for <see cref="PremiumAction.GetUserState"/>.</summary>
    [Serializable]
    public class PremiumStateResponse
    {
        public DateTime ServerTimeUtc;

        /// <summary>Current premium state of the user. Contains active subscriptions and max tier.</summary>
        public UserPremiumState Premium;
    }

    /// <summary>
    /// Response for <see cref="PremiumAction.ActivateTrial"/> and
    /// <see cref="PremiumAction.PurchaseItemOrCurrency"/> / <see cref="PremiumAction.PurchaseRealMoney"/>.
    /// </summary>
    [Serializable]
    public class PremiumPurchaseResponse
    {
        public DateTime ServerTimeUtc;

        /// <summary>Updated premium state of the user after the purchase or trial activation.</summary>
        public UserPremiumState Premium;

        /// <summary>The subscription that was created or updated.</summary>
        public PremiumSubscription Subscription;

        /// <summary>
        /// Result of the resource operation (consumed cost).
        /// Consume.Standard.Items / EventTokens contain what was spent.
        /// Empty ResourceOperation for ActivateTrial and idempotent replays.
        /// </summary>
        public ResourceOperation Resources;
    }

    // =====================================================================
    // STATE MODELS
    // =====================================================================

    /// <summary>
    /// Player's premium subscription state. Stored in UserDataDocument.Premium.
    /// </summary>
    [Serializable]
    public class UserPremiumState
    {
        /// <summary>
        /// Active (and past) subscriptions keyed by PremiumID.
        /// A subscription is considered active when ExpirationDate > UtcNow.
        /// </summary>
        public Dictionary<string, PremiumSubscription> Subscriptions = new();

        /// <summary>
        /// List of PremiumIDs for which the player has already used a free trial.
        /// Prevents re-activation of the same trial after cancellation.
        /// </summary>
        public List<string> ActivatedTrialIDs = new();

        /// <summary>
        /// Highest active tier among all current subscriptions.
        /// Recomputed server-side on every purchase/query. Use for quick gating checks.
        /// </summary>
        public int MaxActiveTier;
    }

    /// <summary>A single premium subscription entry.</summary>
    [Serializable]
    public class PremiumSubscription
    {
        /// <summary>Matches the key in <see cref="UserPremiumState.Subscriptions"/>.</summary>
        public string PremiumID;

        /// <summary>UTC date of first purchase (or re-purchase after expiry).</summary>
        public DateTime PurchaseDate;

        /// <summary>UTC expiration date. Subscription is active when this is in the future.</summary>
        public DateTime ExpirationDate;

        /// <summary>Unique transaction ID supplied by the client at purchase time.</summary>
        public string TransactionID;

        /// <summary>Whether auto-renewal is enabled (relevant for store subscriptions).</summary>
        public bool IsAutoRenewEnabled;
    }

    // =====================================================================
    // CONFIG MODELS
    // =====================================================================

    /// <summary>
    /// Root premium configuration from TitlePublicConfigurationModel.Premium.
    /// </summary>
    [Serializable]
    public class PremiumDefinitions
    {
        /// <summary>
        /// All premium subscription definitions keyed by PremiumID.
        /// </summary>
        public Dictionary<string, PremiumDefinition> Definitions = new();
    }

    /// <summary>Configuration for a single premium subscription product.</summary>
    [Serializable]
    public class PremiumDefinition
    {
        /// <summary>Matches the key in <see cref="PremiumDefinitions.Definitions"/>.</summary>
        public string PremiumID;

        public string DisplayName;

        /// <summary>Tier level (1 = Silver, 2 = Gold, etc.). Used for gating and reward resolution.</summary>
        public int Tier;

        /// <summary>Subscription duration in days. 0 means "forever" (expires in ~100 years).</summary>
        public int DurationDays = 30;

        /// <summary>Free trial duration in days. 0 means no trial is available.</summary>
        public int TrialDurationDays;

        /// <summary>Available purchase options keyed by option ID (e.g. "Default", "0", "1").</summary>
        public Dictionary<string, PremiumPriceOption> PriceOptions = new();

        /// <summary>Apple IAP product identifier for real-money purchases.</summary>
        public string AppleProductID;

        /// <summary>Google Play product identifier for real-money purchases.</summary>
        public string GoogleProductID;

        /// <summary>
        /// Flexible benefit values keyed by benefit name (e.g. "ExpMult" → "1.5", "NoAds" → "1").
        /// Interpret values as floats: 0 = disabled, 1 = enabled, >1 = multiplier.
        /// </summary>
        public Dictionary<string, string> Benefits = new();
    }

    /// <summary>A single price option for purchasing a premium subscription.</summary>
    [Serializable]
    public class PremiumPriceOption
    {
        /// <summary>Matches the key in <see cref="PremiumDefinition.PriceOptions"/>.</summary>
        public string OptionID;

        /// <summary>Human-readable label (e.g. "For Gold", "For Tickets").</summary>
        public string Name;

        /// <summary>
        /// What the player must spend to activate this option.
        /// Standard.Entries contains currencies/items; Standard.EventTokens contains token costs.
        /// PremiumDiscounts may be present for declarative tier-based discounts.
        /// </summary>
        public ResourceConsume RequiredResources = new();
    }
}
