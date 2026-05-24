using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    // =========================================================================
    // DEAL OFFERS — Models
    // =========================================================================

    /// <summary>
    /// Request for all DealOffer actions. Inherits from BaseRequest.
    /// <list type="bullet">
    ///   <item><see cref="DealOfferAction.GetDefinition"/> — no extra fields required.</item>
    ///   <item><see cref="DealOfferAction.GetUserState"/> — no extra fields required.</item>
    ///   <item><see cref="DealOfferAction.GetActiveDeals"/> — no extra fields required.</item>
    ///   <item><see cref="DealOfferAction.DismissDeal"/> — requires <see cref="SlotID"/>.</item>
    ///   <item><see cref="DealOfferAction.ExecuteNode"/> — requires <see cref="SlotID"/> and <see cref="NodeID"/>.</item>
    ///   <item><see cref="DealOfferAction.RecordShow"/> — requires <see cref="SlotID"/>.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class DealOfferRequest : BaseRequest
    {
        /// <summary>Slot ID. Required for DismissDeal, ExecuteNode, RecordShow.</summary>
        public string SlotID;

        /// <summary>Node ID. Required for ExecuteNode only.</summary>
        public string NodeID;
    }

    // -------------------------------------------------------------------------
    // Response models
    // -------------------------------------------------------------------------

    [Serializable]
    public class DealOffersDefinitionResponse
    {
        public DealOfferDefinitions DealOfferDefinitions;
    }

    [Serializable]
    public class UserDealOffersStateResponse
    {
        public UserDealOffersState DealOffers;
    }

    [Serializable]
    public class GetActiveDealsResponse
    {
        public List<ActiveDealSlotInfo> Slots = new();
    }

    /// <summary>
    /// Response for ExecuteNode.
    /// Resources.Consume — what was spent (with applied discounts).
    /// Resources.Grant   — what was granted.
    /// </summary>
    [Serializable]
    public class ExecuteNodeResponse
    {
        public DateTime ServerTimeUtc;
        public string SlotID;
        public string NodeID;
        public bool NodeCompleted;
        public bool OfferExhausted;
        public bool Idempotent;
        public ResourceOperation Resources;
    }

    /// <summary>
    /// Response for DismissDeal. Resources is always empty — included for schema consistency.
    /// </summary>
    [Serializable]
    public class DismissDealResponse
    {
        public DateTime ServerTimeUtc;
        public string SlotID;
        public ResourceOperation Resources;
    }

    /// <summary>
    /// Response for RecordShow. Resources is always empty — included for schema consistency.
    /// </summary>
    [Serializable]
    public class RecordShowResponse
    {
        public DateTime ServerTimeUtc;
        public string SlotID;
        public ResourceOperation Resources;
    }

    // -------------------------------------------------------------------------
    // ActiveDealSlotInfo (runtime resolved slot for UI)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolved state of one slot for UI rendering.
    /// Contains definition, runtime state, and cost preview.
    /// </summary>
    [Serializable]
    public class ActiveDealSlotInfo
    {
        public string SlotID;
        public int SortOrder;
        public int QueueIndex;
        public int CycleIndex;
        public DealOfferDefinition OfferDef;
        public UserDealOfferActivationState ActivationState;

        /// <summary>True if the offer hasn't been activated yet — client initializes it with first ExecuteNode.</summary>
        public bool IsNewActivation;

        /// <summary>When the current offer expires. Null = no timer.</summary>
        public DateTime? ComputedExpiresAtUtc;

        /// <summary>
        /// Aggregated preview cost for UI.
        /// Standard = full price across all Purchase nodes without external rewards.
        /// PremiumDiscounts = union of tier discounts across all nodes.
        /// Final discounted price is computed atomically in ExecuteNode via ResourceService.FilterByPremium.
        /// </summary>
        public ResourceConsume Cost;
    }

    // -------------------------------------------------------------------------
    // User State Models
    // -------------------------------------------------------------------------

    /// <summary>Root runtime state for a user's Deal Offers. Stored in UserState.DealOffer.</summary>
    [Serializable]
    public class UserDealOffersState
    {
        /// <summary>Per-slot runtime state. Key = SlotID.</summary>
        public Dictionary<string, UserDealSlotState> Slots = new();

        /// <summary>Lifetime offer history. Key = OfferID.</summary>
        public Dictionary<string, UserDealOfferHistory> OfferHistory = new();

        public DateTime LastUpdatedUtc;
    }

    /// <summary>Runtime state of one slot.</summary>
    [Serializable]
    public class UserDealSlotState
    {
        public string SlotID;
        public int QueueIndex;
        public int CycleIndex;

        /// <summary>Denormalized: matches Queue[QueueIndex].OfferID for convenient reads.</summary>
        public string ActiveOfferID;

        public DateTime ActiveOfferStartedAtUtc;
        public DateTime? ActiveOfferExpiresAtUtc;

        /// <summary>Runtime state of the active offer (nodes, tracks, counters).</summary>
        public UserDealOfferActivationState ActiveOffer;

        /// <summary>When the slot resumes after a cycle cooldown. Null = not paused.</summary>
        public DateTime? NextCycleStartsAtUtc;

        public DateTime LastUpdatedUtc;

        /// <summary>When dismiss-skip takes effect. Null = no active skip.</summary>
        public DateTime? DismissSkipAvailableAtUtc;

        /// <summary>Last time the player dismissed a deal in this slot.</summary>
        public DateTime? LastDismissedAtUtc;
    }

    /// <summary>Runtime state of one offer activation within a slot.</summary>
    [Serializable]
    public class UserDealOfferActivationState
    {
        public string OfferID;
        public int SourceOfferVersion;
        public DealOfferActivationStatus Status;
        public DateTime ActivatedAtUtc;
        public DateTime? ExhaustedAtUtc;
        public DateTime? ExpiredAtUtc;
        public DateTime? DismissedAtUtc;
        public int ShowCount;
        public DateTime? LastShownAtUtc;

        /// <summary>Per-node runtime state. Key = NodeID.</summary>
        public Dictionary<string, UserDealNodeState> Nodes = new();

        /// <summary>Offer-local track values. Key = TrackID.</summary>
        public Dictionary<string, UserDealTrackState> Tracks = new();

        /// <summary>For Choice offers: the chosen NodeID. Null = not chosen yet.</summary>
        public string SelectedChoiceNodeID;
    }

    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DealOfferActivationStatus
    {
        Active,
        Exhausted,
        Expired,
        Dismissed
    }

    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DealNodeRuntimeStatus
    {
        Locked,
        Available,
        InProgress,
        Completed,
        Hidden
    }

    /// <summary>Runtime state of one node within the current offer activation.</summary>
    [Serializable]
    public class UserDealNodeState
    {
        public string NodeID;
        public DealNodeRuntimeStatus Status;

        /// <summary>Number of executions within the current activation.</summary>
        public int ExecutionCount;

        public DateTime? UnlockedAtUtc;
        public DateTime? CompletedAtUtc;
        public DateTime? LastExecutedAtUtc;

        /// <summary>Rewarded video cooldown — when the node becomes available again.</summary>
        public DateTime? NextAvailableAtUtc;

        /// <summary>Last external ref/receipt/ad-verification ID. Used for idempotency.</summary>
        public string LastExecutionRefID;
    }

    /// <summary>Runtime value of an offer-local progress track within the current activation.</summary>
    [Serializable]
    public class UserDealTrackState
    {
        public string TrackID;
        public long CurrentValue;
        public DateTime LastUpdatedUtc;
    }

    /// <summary>Aggregated lifetime history for a specific offer. Accumulates across all activations.</summary>
    [Serializable]
    public class UserDealOfferHistory
    {
        public int TotalActivations;
        public int TotalExhausted;
        public int TotalExpired;
        public int TotalDismissed;
        public int TotalShows;
        public DateTime? LastShownAtUtc;
        public DateTime? LastActivatedAtUtc;
        public DateTime? LastExhaustedAtUtc;

        /// <summary>Lifetime and daily execution counts per node. Key = NodeID.</summary>
        public Dictionary<string, UserDealNodeLifetimeCounts> NodeCounts = new();
    }

    /// <summary>Lifetime + daily execution counters for one node of one offer. Accumulates across activations.</summary>
    [Serializable]
    public class UserDealNodeLifetimeCounts
    {
        public int TotalExecutions;
        public int DailyExecutions;
        public DateTime DailyResetUtc;
    }

    // -------------------------------------------------------------------------
    // Config / Definition Models
    // -------------------------------------------------------------------------

    /// <summary>Root config for the Deal Offers system. Stored in TitlePublicConfigurationModel.DealOffer.</summary>
    [Serializable]
    public class DealOfferDefinitions
    {
        /// <summary>Slots shown simultaneously in UI. Key = SlotID.</summary>
        public Dictionary<string, DealSlotDefinition> Slots = new();

        /// <summary>Library of all offers referenced by slots. Key = OfferID.</summary>
        public Dictionary<string, DealOfferDefinition> Offers = new();
    }

    /// <summary>
    /// A slot — a UI position that cycles through a queue of offers.
    /// When the active offer expires or is exhausted, the slot advances to the next entry.
    /// </summary>
    [Serializable]
    public class DealSlotDefinition
    {
        public string SlotID;
        public bool Enabled = true;
        public int SortOrder;
        public List<DealSlotQueueEntry> Queue = new();

        /// <summary>Cooldown between full queue cycles in seconds. 0 = no delay.</summary>
        public int CooldownBetweenCyclesSec;

        /// <summary>Max full queue cycles. 0 = unlimited.</summary>
        public int MaxCycles;

        /// <summary>If true, Dismiss advances to the next offer after DismissSkipDelaySec.</summary>
        public bool AllowDismissSkip;

        /// <summary>Delay before next offer activates after Dismiss. Used only when AllowDismissSkip = true.</summary>
        public int DismissSkipDelaySec;
    }

    /// <summary>One entry in a slot's offer queue — a reference to an offer with its own timer.</summary>
    [Serializable]
    public class DealSlotQueueEntry
    {
        public int Order;
        public string OfferID;

        /// <summary>How long this offer stays active in the slot in seconds. 0 = until limits exhausted.</summary>
        public int DurationSec;

        /// <summary>Delay before this offer activates after the previous one. 0 = immediate.</summary>
        public int DelayBeforeActivationSec;
    }

    /// <summary>Definition of one offer — a node graph with rewards, costs, and unlock rules.</summary>
    [Serializable]
    public class DealOfferDefinition
    {
        public string OfferID;
        public int Version = 1;
        public bool Enabled = true;
        public DealOfferGraphMode GraphMode;
        public string Name;
        public string Description;
        public List<string> RootNodeIDs = new();
        public List<DealNodeDefinition> Nodes = new();
        public List<DealTrackDefinition> Tracks = new();
        public bool ExhaustedWhenAllTerminalNodesCompleted = true;
        public Dictionary<string, string> AssetPaths;
        public Dictionary<string, string> Metadata = new();
    }

    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DealOfferGraphMode
    {
        Single,
        Chain,
        BranchingChain,
        Choice,
        MeteredChain
    }

    /// <summary>One node in an offer — an atomic action the player can take.</summary>
    [Serializable]
    public class DealNodeDefinition
    {
        public string NodeID;
        public int SortOrder;
        public DealNodeType Type;
        public DealNodeActionDefinition Action;

        /// <summary>Rewards granted on successful node execution.</summary>
        public ResourceGrant Grants;

        /// <summary>Offer-local track changes triggered by this node.</summary>
        public List<DealTrackChange> TrackChanges = new();

        /// <summary>Unlock requirements. Null = root node (available immediately).</summary>
        public DealNodeUnlockRules UnlockRules;

        /// <summary>Nodes unlocked after this node completes.</summary>
        public List<string> NextNodeIDs = new();

        /// <summary>Choice group ID. All nodes sharing this ID are mutually exclusive.</summary>
        public string ChoiceGroupID;

        public DealNodeLimitsDefinition Limits;
        public bool HideWhenCompleted;

        /// <summary>If true, completing this node immediately exhausts the entire offer.</summary>
        public bool ExhaustOfferOnComplete;

        public Dictionary<string, string> AssetPaths;
        public Dictionary<string, string> Metadata = new();
    }

    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DealNodeType
    {
        Purchase,
        FreeClaim,
        RewardedVideo,
        Info
    }

    /// <summary>Container for node action parameters. Fill the relevant nested block based on Type.</summary>
    [Serializable]
    public class DealNodeActionDefinition
    {
        /// <summary>Filled when Type = Purchase.</summary>
        public DealPurchaseActionDefinition Purchase;

        /// <summary>Filled when Type = RewardedVideo.</summary>
        public DealRewardedVideoActionDefinition RewardedVideo;
    }

    [Serializable]
    public class DealPurchaseActionDefinition
    {
        public string StoreOfferID;
        public string BillingProductID;

        /// <summary>
        /// Direct cost in game currencies/items.
        /// Standard = base price. PremiumDiscounts = subscription tier discounts.
        /// </summary>
        public ResourceConsume DirectCost;

        /// <summary>If true, the main reward comes from the external purchase system. Node.Grants = bonus only.</summary>
        public bool UseExternalRewards = true;
    }

    [Serializable]
    public class DealRewardedVideoActionDefinition
    {
        public string AdPlacementID;
        public int ViewsRequiredToComplete = 1;
        public int MaxViewsPerActivation = 1;
        public int CooldownSecondsBetweenViews;
        public bool RequireServerVerification = true;

        /// <summary>If true, rewards are granted per view; otherwise only after completion.</summary>
        public bool GrantRewardsPerView;
    }

    /// <summary>Execution limits for a node. "N/N Available" in UI corresponds to AvailablePerActivation.</summary>
    [Serializable]
    public class DealNodeLimitsDefinition
    {
        /// <summary>Max executions within one offer activation. 0 = unlimited.</summary>
        public int AvailablePerActivation = 1;

        /// <summary>Max lifetime executions per user. 0 = unlimited.</summary>
        public int AvailableTotalPerUser;

        /// <summary>Max daily executions per user. 0 = unlimited.</summary>
        public int AvailableDailyPerUser;
    }

    /// <summary>Conditions that must be met for a node to become available.</summary>
    [Serializable]
    public class DealNodeUnlockRules
    {
        /// <summary>All of these nodes must be completed.</summary>
        public List<string> RequiredCompletedNodeIDs = new();

        /// <summary>At least one of these nodes must be completed.</summary>
        public List<string> RequiredAnyCompletedNodeIDs = new();

        /// <summary>Offer-local track requirements (e.g. aquarium_shells >= 15).</summary>
        public List<DealTrackRequirement> RequiredTracks = new();

        public bool VisibleWhileLocked = true;
    }

    /// <summary>Offer-local progress track definition (e.g. "aquarium shells").</summary>
    [Serializable]
    public class DealTrackDefinition
    {
        public string TrackID;
        public string DisplayName;
        public long StartValue;
        public long MinValue;

        /// <summary>0 = no maximum.</summary>
        public long MaxValue;

        public bool ClampToMin = true;
        public bool ClampToMax = true;
        public bool HiddenFromUI;
    }

    /// <summary>Change applied to an offer-local track when a node executes.</summary>
    [Serializable]
    public class DealTrackChange
    {
        public string TrackID;

        /// <summary>Positive = add, negative = subtract.</summary>
        public long Amount;

        public bool RespectBounds = true;
    }

    /// <summary>Requirement on an offer-local track value for node unlock.</summary>
    [Serializable]
    public class DealTrackRequirement
    {
        public string TrackID;
        public DealComparisonOperator Operator;
        public long Value;
    }

    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DealComparisonOperator
    {
        Eq,
        Gte,
        Lte
    }

    // -------------------------------------------------------------------------
    // Action enum
    // -------------------------------------------------------------------------

    public enum DealOfferAction
    {
        GetDefinition,
        GetUserState,
        GetActiveDeals,
        DismissDeal,
        ExecuteNode,
        RecordShow
    }
}
