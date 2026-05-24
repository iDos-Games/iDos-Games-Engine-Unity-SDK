using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Request for all Collection module actions.
    /// Field usage by action:
    ///   GetDefinitions       — no extra fields
    ///   GetUserState         — no extra fields
    ///   OpenPack             — CollectionID, PackTypeID
    ///   OpenCollectionChest  — CollectionID, CollectionChestID
    ///   UseCollectibleJoker  — CollectionID, CollectibleID, CollectibleIsSpecial
    ///   ClaimSetReward       — CollectionID, SetID
    ///   ClaimGrandPrize      — CollectionID
    ///   SendTradeOffer       — CollectionID, CollectibleID, CollectibleIsSpecial,
    ///                          ReceiverUserID, RequestedCollectibleID, RequestedCollectibleIsSpecial
    ///   CancelTradeOffer     — OfferID
    ///   AcceptTradeOffer     — OfferID
    ///   DeclineTradeOffer    — OfferID
    ///   GetMyTradeOffers     — CollectionID
    ///   GetIncomingTradeOffers — CollectionID
    /// </summary>
    [Serializable]
    public class CollectionRequest : BaseRequest
    {
        /// <summary>Collection ID. Required for most actions.</summary>
        public string CollectionID;

        /// <summary>Collectible ID. Used in UseCollectibleJoker, SendTradeOffer.</summary>
        public string CollectibleID;

        /// <summary>Pack type ID. Used in OpenPack.</summary>
        public string PackTypeID;

        /// <summary>CollectionChest ID. Used in OpenCollectionChest.</summary>
        public string CollectionChestID;

        /// <summary>Set ID. Used in ClaimSetReward.</summary>
        public string SetID;

        /// <summary>Whether working with the Special version of a Collectible.</summary>
        public bool CollectibleIsSpecial = false;

        /// <summary>Receiver's UserID. Used in SendTradeOffer.</summary>
        public string ReceiverUserID;

        /// <summary>Requested counter-Collectible ID. Used in SendTradeOffer.</summary>
        public string RequestedCollectibleID;

        /// <summary>Whether the requested counter-Collectible is a Special version.</summary>
        public bool RequestedCollectibleIsSpecial = false;

        /// <summary>Trade offer ID. Used in CancelTradeOffer, AcceptTradeOffer, DeclineTradeOffer.</summary>
        public string OfferID;
    }

    // =====================================================================
    // RESPONSES
    // =====================================================================

    [Serializable]
    public class OpenPackResponse
    {
        /// <summary>Newly obtained Collectibles (non-duplicates).</summary>
        public List<GrantedCollectible> GrantedCollectibles = new();

        /// <summary>Duplicate Collectibles converted to CollectionCurrency.</summary>
        public List<GrantedCollectible> DuplicateCollectibles = new();

        /// <summary>CollectionCurrency earned from duplicates in this pack.</summary>
        public long CollectionCurrencyEarned;

        /// <summary>New CollectionCurrency balance after opening.</summary>
        public long NewCollectionCurrencyBalance;

        /// <summary>Set IDs completed by this pack.</summary>
        public List<string> NewlyCompletedSetIDs = new();

        /// <summary>True if this pack completed the entire collection.</summary>
        public bool CollectionJustCompleted;

        /// <summary>Full resource operation result (cost deducted, bonus rewards granted).</summary>
        public ResourceOperation Resources;

        /// <summary>Triggered pity rules. Null if none triggered.</summary>
        public List<LootboxPityTriggerResponse> TriggeredPity;
    }

    [Serializable]
    public class OpenCollectionChestResponse
    {
        public List<GrantedCollectible> GrantedCollectibles = new();
        public List<GrantedCollectible> DuplicateCollectibles = new();
        public long CollectionCurrencyEarned;
        public long NewCollectionCurrencyBalance;

        /// <summary>Bonus rewards granted. CollectionCurrency deduction is in domain patches, not here.</summary>
        public ResourceOperation Resources;

        /// <summary>Triggered pity rules. Null if none triggered.</summary>
        public List<LootboxPityTriggerResponse> TriggeredPity;
    }

    [Serializable]
    public class UseCollectibleJokerResponse
    {
        public string GrantedCollectibleID;
        public string NewlyCompletedSetID;
        public bool CollectionJustCompleted;

        /// <summary>Resource operation result (Joker item consumed).</summary>
        public ResourceOperation Resources;
    }

    [Serializable]
    public class ClaimSetRewardResponse
    {
        public string SetID;

        /// <summary>Granted set completion rewards.</summary>
        public ResourceOperation Resources;
    }

    [Serializable]
    public class ClaimGrandPrizeResponse
    {
        /// <summary>Granted grand prize rewards.</summary>
        public ResourceOperation Resources;
    }

    [Serializable]
    public class SendTradeOfferResponse
    {
        public string OfferID;
        public DateTime ExpiresAtUtc;

        /// <summary>Resource operation result (empty — changes are domain-only).</summary>
        public ResourceOperation Resources;
    }

    [Serializable]
    public class CancelTradeOfferResponse
    {
        public string OfferID;

        /// <summary>Resource operation result (empty — changes are domain-only).</summary>
        public ResourceOperation Resources;
    }

    [Serializable]
    public class AcceptTradeOfferResponse
    {
        public string OfferID;
        public string ReceivedCollectibleID;
        public bool ReceivedIsSpecial;
        public string SentCollectibleID;
        public bool SentCollectibleIsSpecial;

        /// <summary>
        /// P2P transfer result. Transferred bundle is empty because
        /// Collectibles live outside InventoryV2.
        /// </summary>
        public ResourceTransferResult Transfer;
    }

    [Serializable]
    public class DeclineTradeOfferResponse
    {
        public string OfferID;

        /// <summary>Resource operation result (empty — changes are domain-only).</summary>
        public ResourceOperation Resources;
    }

    [Serializable]
    public class GetTradeOffersResponse
    {
        public List<CollectionTradeOfferDocument> Offers = new();
    }

    // =====================================================================
    // SHARED DTOs
    // =====================================================================

    /// <summary>One Collectible received when opening a pack or chest.</summary>
    [Serializable]
    public class GrantedCollectible
    {
        public string CollectibleID;
        public int Rarity;
        public bool IsSpecial;
        public bool IsDuplicate;

        /// <summary>CollectionCurrency this duplicate was converted into (0 if not a duplicate).</summary>
        public int CollectionCurrencyConverted;
    }

    // =====================================================================
    // USER STATE
    // =====================================================================

    /// <summary>
    /// Player's collection state for the current active season.
    /// Stored in UserDataDocument.Collection. Wiped when season changes.
    /// </summary>
    [Serializable]
    public class UserCollectionState
    {
        /// <summary>Active collection ID.</summary>
        public string CollectionID;

        /// <summary>Season version this state belongs to. Mismatch triggers a server-side wipe.</summary>
        public int SeasonVersion = 0;

        /// <summary>Current CollectionCurrency balance. Earned from duplicates, spent on chests. Resets on wipe.</summary>
        public long CollectionCurrencyBalance = 0;

        /// <summary>Total CollectionCurrency earned this season (monotonically increasing).</summary>
        public long TotalCollectionCurrencyEarned = 0;

        /// <summary>
        /// Owned regular Collectibles. Key = CollectibleID, Value = count.
        /// 0 or missing = not owned. 1 = unique. ≥2 = has duplicates.
        /// </summary>
        public Dictionary<string, int> OwnedCollectibles = new();

        /// <summary>
        /// Owned Special Collectibles. Key = CollectibleID, Value = 0 (none) or 1 (owned).
        /// </summary>
        public Dictionary<string, int> OwnedSpecialCollectibles = new();

        /// <summary>Set IDs whose completion rewards have already been claimed.</summary>
        public List<string> ClaimedSetRewards = new();

        /// <summary>True when all regular and Special Collectibles are collected.</summary>
        public bool IsCollectionCompleted = false;

        /// <summary>True when the Grand Prize has been claimed.</summary>
        public bool GrandPrizeClaimed = false;

        /// <summary>Number of outgoing trades sent today. Resets at 00:00 UTC.</summary>
        public int DailyTradesSent = 0;

        /// <summary>UTC date of the last DailyTradesSent reset.</summary>
        public DateTime DailyTradesResetDate;

        /// <summary>Incoming pending trade offer IDs waiting for the player's response.</summary>
        public List<string> PendingTradeOfferIDs = new();

        /// <summary>Pity counters keyed by "{PackTypeID or ChestID}:{RuleID}". Wiped on season change.</summary>
        public Dictionary<string, UserLootboxPityCounter> PityCounters = new();
    }

    // =====================================================================
    // TRADE DOCUMENTS
    // =====================================================================

    /// <summary>
    /// Active P2P trade offer document. Mirrors the server-side CollectionTradeOfferDocument.
    /// </summary>
    [Serializable]
    public class CollectionTradeOfferDocument
    {
        public string OfferID;
        public string TitleID;
        public string CollectionID;
        public string SenderUserID;
        public UserPublicDataModel SenderPublicData;
        public string OfferedCollectibleID;
        public bool OfferedCollectibleIsSpecial = false;
        public string ReceiverUserID;
        public string RequestedCollectibleID;
        public bool RequestedCollectibleIsSpecial = false;
        public TradeOfferStatus Status = TradeOfferStatus.Pending;
        public DateTime CreatedAtUtc;
        public DateTime ExpiresAtUtc;
        public DateTime? RespondedAtUtc;
        public string DeclineReason;
        public bool IsSpecialTradeEvent = false;
        public string SpecialTradeEventID;
    }

    /// <summary>Trade offer lifecycle status.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradeOfferStatus
    {
        Pending,
        Accepted,
        Declined,
        Cancelled,
        Expired
    }

    // =====================================================================
    // CONFIG MODELS
    // =====================================================================

    /// <summary>Root config for the collection system. Stored in TitlePublicConfigurationModel.Collection.</summary>
    [Serializable]
    public class CollectionDefinitions
    {
        /// <summary>All collections. Key = CollectionID.</summary>
        public Dictionary<string, CollectionDefinition> Collections = new();

        /// <summary>Pack types (lootboxes). Key = PackTypeID.</summary>
        public Dictionary<string, CollectionPackTypeDefinition> PackTypes = new();

        /// <summary>Collection chests (pity sink mechanic).</summary>
        public List<CollectionChestDefinition> CollectionChests = new();

        /// <summary>Duplicate-to-CollectionCurrency conversion rules by rarity.</summary>
        public List<DuplicateCollectionCurrencyConversion> DuplicateConversions = new();

        /// <summary>Max outgoing trades per player per day.</summary>
        public int DailyTradeLimit = 5;

        /// <summary>CatalogID for the CollectibleJoker item.</summary>
        public string CollectibleJokerCatalogID;

        /// <summary>ItemID of the CollectibleJoker wild card.</summary>
        public string CollectibleJokerItemID;

        /// <summary>Time-limited windows allowing Special Collectible trades.</summary>
        public List<SpecialTradeEventDefinition> SpecialTradeEvents = new();
    }

    [Serializable]
    public class CollectionDefinition
    {
        public string CollectionID;
        public string DisplayName;
        public string Description;
        public Dictionary<string, string> AssetPaths;
        public string SeasonChainID;
        public List<CollectionSetDefinition> Sets = new();

        /// <summary>Grand Prize granted when the entire collection is completed.</summary>
        public ResourceGrant GrandPrize = new();
    }

    [Serializable]
    public class CollectionSetDefinition
    {
        public string SetID;
        public string DisplayName;
        public Dictionary<string, string> AssetPaths;
        public int SortOrder;
        public List<CollectibleDefinition> Collectibles = new();

        /// <summary>One-time reward for completing this set.</summary>
        public ResourceGrant SetCompletionReward = new();
    }

    [Serializable]
    public class CollectibleDefinition
    {
        public string CollectibleID;
        public string DisplayName;
        public Dictionary<string, string> AssetPaths;

        /// <summary>Rarity from 1 (common) to 5 (legendary). Affects drop weight and duplicate value.</summary>
        public int Rarity = 1;

        /// <summary>Whether this Collectible has a Special version.</summary>
        public bool HasSpecialVersion = false;

        public int SortOrder;
    }

    [Serializable]
    public class CollectionPackTypeDefinition
    {
        public string PackTypeID;

        /// <summary>Opening cost (consume side).</summary>
        public ResourceConsume Cost;

        /// <summary>Bonus reward slots rolled alongside Collectibles. Uses the same LootboxV2 mechanism.</summary>
        public List<LootboxRewardSlot> BonusRewardSlots;

        /// <summary>Pity rules for this pack type. Counters stored in UserCollectionState.PityCounters.</summary>
        public List<LootboxPityRule> PityRules;

        public string DisplayName;
        public Dictionary<string, string> AssetPaths;

        /// <summary>Total number of Collectibles per pack (e.g. 3 Green, 5 Purple).</summary>
        public int CollectibleCount = 3;

        /// <summary>Minimum guaranteed rarity for at least one Collectible.</summary>
        public int GuaranteedMinRarity = 1;

        /// <summary>If true, at least one Collectible is guaranteed max rarity (5). Used for Purple packs.</summary>
        public bool GuaranteeMaxRarity = false;

        /// <summary>Drop weights by rarity. Key = rarity (1–5), value = arbitrary weight.</summary>
        public Dictionary<int, int> RarityWeights = new();

        /// <summary>Visual tier for sorting: 1=Green, 2=Blue, 3=Orange, 4=Purple.</summary>
        public int ColorTier = 1;
    }

    [Serializable]
    public class DuplicateCollectionCurrencyConversion
    {
        /// <summary>Rarity of the duplicate Collectible (1–5).</summary>
        public int Rarity;

        /// <summary>CollectionCurrency granted when a duplicate of this rarity is auto-converted.</summary>
        public int CollectionCurrencyGranted;
    }

    [Serializable]
    public class CollectionChestDefinition
    {
        public string CollectionChestID;
        public string DisplayName;
        public Dictionary<string, string> AssetPaths;

        /// <summary>CollectionCurrency cost to open.</summary>
        public int CollectionCurrencyCost;

        public int MinCollectibleCount = 1;
        public int MaxCollectibleCount = 2;

        /// <summary>All Collectibles from this chest have rarity >= this value.</summary>
        public int GuaranteedMinRarity = 2;

        /// <summary>Bonus reward slots. Same mechanism as LootboxV2.</summary>
        public List<LootboxRewardSlot> BonusRewardSlots;

        /// <summary>Pity rules for this chest. Counters stored in UserCollectionState.PityCounters.</summary>
        public List<LootboxPityRule> PityRules;

        /// <summary>Visual tier: 1=Bronze, 2=Silver, 3=Gold.</summary>
        public int Tier = 1;
    }

    [Serializable]
    public class SpecialTradeEventDefinition
    {
        public string SpecialTradeEventID;
        public DateTime StartUtc;
        public DateTime EndUtc;

        /// <summary>CollectibleIDs whose Special versions are tradeable during this event.</summary>
        public List<string> AllowedSpecialCollectibleIDs = new();

        /// <summary>Increased daily trade limit during this event. 0 = use global DailyTradeLimit.</summary>
        public int SpecialTradeEventDailyTradeLimit = 0;
    }

    // =====================================================================
    // ACTION ENUM
    // =====================================================================

    public enum CollectionAction
    {
        GetDefinitions,
        GetUserState,
        OpenPack,
        OpenCollectionChest,
        UseCollectibleJoker,
        ClaimSetReward,
        ClaimGrandPrize,
        SendTradeOffer,
        CancelTradeOffer,
        AcceptTradeOffer,
        DeclineTradeOffer,
        GetMyTradeOffers,
        GetIncomingTradeOffers,
    }
}
