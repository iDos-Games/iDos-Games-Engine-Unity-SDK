using IDosGames.TitlePublicConfiguration;
using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum DealOfferAction
    {
        GetDefinition,
        GetUserState,
        GetActiveDeals,
        DismissDeal,
        ExecuteNode,
        RecordShow,
    }

    public enum DealOfferActivationStatus
    {
        Active,
        Exhausted,
        Expired,
        Dismissed,
    }

    public enum DealNodeRuntimeStatus
    {
        Locked,
        Available,
        InProgress,
        Completed,
        Hidden,
    }

    public enum DealOfferGraphMode
    {
        Single,
        Chain,
        BranchingChain,
        Choice,
        MeteredChain,
    }

    public enum DealNodeType
    {
        Purchase,
        FreeClaim,
        RewardedVideo,
        Info,
    }

    public enum DealComparisonOperator
    {
        Eq,
        Gte,
        Lte,
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class DealOfferRequest : IGSRequest
    {
        public string SlotID { get; set; }          // Нужен для DismissDeal, ExecuteNode, RecordShow
        public string NodeID { get; set; }          // Нужен для ExecuteNode
        public string ExternalRefID { get; set; }   // Идемпотентность ExecuteNode
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class DealOffersDefinitionResponse
    {
        public DealOffersDefinition DealOfferDefinitions { get; set; }
    }

    [Serializable]
    public class UserDealOffersStateResponse
    {
        public UserDealOffersState DealOffers { get; set; }
    }

    [Serializable]
    public class GetActiveDealsResponse
    {
        public List<ActiveDealSlotInfo> Slots { get; set; } = new();
    }

    [Serializable]
    public class ExecuteNodeResponse
    {
        public string SlotID { get; set; }
        public string NodeID { get; set; }
        public bool NodeCompleted { get; set; }
        public bool OfferExhausted { get; set; }
        public bool Idempotent { get; set; }
        public List<ItemOrCurrency> ConsumedResources { get; set; } = new();
        public List<ItemOrCurrency> GrantedResources { get; set; } = new();
    }

    // =================================================================================
    // DEFINITION MODELS  (config, хранится в TitleConfig)
    // =================================================================================

    [Serializable]
    public class DealOffersDefinition
    {
        public List<DealSlotDefinition> Slots { get; set; } = new();
        public Dictionary<string, DealOfferDefinition> Offers { get; set; } = new();
    }

    [Serializable]
    public class DealSlotDefinition
    {
        public string SlotID { get; set; }
        public bool Enabled { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public List<DealSlotQueueEntry> Queue { get; set; } = new();
        public int CooldownBetweenCyclesSec { get; set; } = 0;
        public int MaxCycles { get; set; } = 0;
        public bool AllowDismissSkip { get; set; } = false;
        public int DismissSkipDelaySec { get; set; } = 0;
    }

    [Serializable]
    public class DealSlotQueueEntry
    {
        public int Order { get; set; }
        public string OfferID { get; set; }
        public int DurationSec { get; set; } = 0;
        public int DelayBeforeActivationSec { get; set; } = 0;
    }

    [Serializable]
    public class DealOfferDefinition
    {
        public string OfferID { get; set; }
        public int Version { get; set; } = 1;
        public bool Enabled { get; set; } = true;
        public DealOfferGraphMode GraphMode { get; set; } = DealOfferGraphMode.Single;
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> RootNodeIDs { get; set; } = new();
        public List<DealNodeDefinition> Nodes { get; set; } = new();
        public List<DealTrackDefinition> Tracks { get; set; } = new();
        public bool ExhaustedWhenAllTerminalNodesCompleted { get; set; } = true;
        public List<string> AssetPaths { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
        public int RequiredPremiumTier { get; set; }
        public string RequiredPremiumID { get; set; }
        public int PremiumDiscountPercent { get; set; }
    }

    [Serializable]
    public class DealNodeDefinition
    {
        public string NodeID { get; set; }
        public int SortOrder { get; set; } = 0;
        public DealNodeType Type { get; set; } = DealNodeType.Purchase;
        public DealNodeActionDefinition Action { get; set; }
        public List<ItemOrCurrency> Grants { get; set; } = new();
        public List<DealTrackChange> TrackChanges { get; set; } = new();
        public DealNodeUnlockRules UnlockRules { get; set; }
        public List<string> NextNodeIDs { get; set; } = new();
        public string ChoiceGroupID { get; set; }
        public DealNodeLimitsDefinition Limits { get; set; }
        public bool HideWhenCompleted { get; set; } = false;
        public bool ExhaustOfferOnComplete { get; set; } = false;
        public List<string> AssetPaths { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    [Serializable]
    public class DealNodeActionDefinition
    {
        public DealPurchaseActionDefinition Purchase { get; set; }
        public DealRewardedVideoActionDefinition RewardedVideo { get; set; }
    }

    [Serializable]
    public class DealPurchaseActionDefinition
    {
        public string StoreOfferID { get; set; }
        public string BillingProductID { get; set; }
        public List<ItemOrCurrency> DirectCost { get; set; } = new();
        public bool UseExternalRewards { get; set; } = true;
    }

    [Serializable]
    public class DealRewardedVideoActionDefinition
    {
        public string AdPlacementID { get; set; }
        public int ViewsRequiredToComplete { get; set; } = 1;
        public int MaxViewsPerActivation { get; set; } = 1;
        public int CooldownSecondsBetweenViews { get; set; } = 0;
        public bool RequireServerVerification { get; set; } = true;
        public bool GrantRewardsPerView { get; set; } = false;
    }

    [Serializable]
    public class DealNodeLimitsDefinition
    {
        public int AvailablePerActivation { get; set; } = 1;
        public int AvailableTotalPerUser { get; set; } = 0;
        public int AvailableDailyPerUser { get; set; } = 0;
    }

    [Serializable]
    public class DealNodeUnlockRules
    {
        public List<string> RequiredCompletedNodeIDs { get; set; } = new();
        public List<string> RequiredAnyCompletedNodeIDs { get; set; } = new();
        public List<DealTrackRequirement> RequiredTracks { get; set; } = new();
        public bool VisibleWhileLocked { get; set; } = true;
    }

    [Serializable]
    public class DealTrackDefinition
    {
        public string TrackID { get; set; }
        public string DisplayName { get; set; }
        public long StartValue { get; set; } = 0;
        public long MinValue { get; set; } = 0;
        public long MaxValue { get; set; } = 0;
        public bool ClampToMin { get; set; } = true;
        public bool ClampToMax { get; set; } = true;
        public bool HiddenFromUI { get; set; } = false;
    }

    [Serializable]
    public class DealTrackChange
    {
        public string TrackID { get; set; }
        public long Amount { get; set; }
        public bool RespectBounds { get; set; } = true;
    }

    [Serializable]
    public class DealTrackRequirement
    {
        public string TrackID { get; set; }
        public DealComparisonOperator Operator { get; set; } = DealComparisonOperator.Gte;
        public long Value { get; set; } = 0;
    }

    // =================================================================================
    // USER STATE MODELS  (runtime, хранится в UserData)
    // =================================================================================

    [Serializable]
    public class UserDealOffersState
    {
        public Dictionary<string, UserDealSlotState> Slots { get; set; } = new();
        public Dictionary<string, UserDealOfferHistory> OfferHistory { get; set; } = new();
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    }

    [Serializable]
    public class UserDealSlotState
    {
        public string SlotID { get; set; }
        public int QueueIndex { get; set; } = 0;
        public int CycleIndex { get; set; } = 0;
        public string ActiveOfferID { get; set; }
        public DateTime ActiveOfferStartedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ActiveOfferExpiresAtUtc { get; set; }
        public UserDealOfferActivationState ActiveOffer { get; set; }
        public DateTime? NextCycleStartsAtUtc { get; set; }
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DismissSkipAvailableAtUtc { get; set; }
        public DateTime? LastDismissedAtUtc { get; set; }
    }

    [Serializable]
    public class UserDealOfferActivationState
    {
        public string OfferID { get; set; }
        public int SourceOfferVersion { get; set; }
        public DealOfferActivationStatus Status { get; set; } = DealOfferActivationStatus.Active;
        public DateTime ActivatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ExhaustedAtUtc { get; set; }
        public DateTime? ExpiredAtUtc { get; set; }
        public DateTime? DismissedAtUtc { get; set; }
        public int ShowCount { get; set; } = 0;
        public DateTime? LastShownAtUtc { get; set; }
        public Dictionary<string, UserDealNodeState> Nodes { get; set; } = new();
        public Dictionary<string, UserDealTrackState> Tracks { get; set; } = new();
        public string SelectedChoiceNodeID { get; set; }
    }

    [Serializable]
    public class UserDealNodeState
    {
        public string NodeID { get; set; }
        public DealNodeRuntimeStatus Status { get; set; } = DealNodeRuntimeStatus.Locked;
        public int ExecutionCount { get; set; } = 0;
        public DateTime? UnlockedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime? LastExecutedAtUtc { get; set; }
        public DateTime? NextAvailableAtUtc { get; set; }
        public string LastExecutionRefID { get; set; }
    }

    [Serializable]
    public class UserDealTrackState
    {
        public string TrackID { get; set; }
        public long CurrentValue { get; set; } = 0;
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    }

    [Serializable]
    public class UserDealOfferHistory
    {
        public int TotalActivations { get; set; } = 0;
        public int TotalExhausted { get; set; } = 0;
        public int TotalExpired { get; set; } = 0;
        public int TotalDismissed { get; set; } = 0;
        public int TotalShows { get; set; } = 0;
        public DateTime? LastShownAtUtc { get; set; }
        public DateTime? LastActivatedAtUtc { get; set; }
        public DateTime? LastExhaustedAtUtc { get; set; }
        public Dictionary<string, UserDealNodeLifetimeCounts> NodeCounts { get; set; } = new();
    }

    [Serializable]
    public class UserDealNodeLifetimeCounts
    {
        public int TotalExecutions { get; set; } = 0;
        public int DailyExecutions { get; set; } = 0;
        public DateTime DailyResetUtc { get; set; }
    }

    // =================================================================================
    // COMPOSITE RESPONSE MODEL  (сервер собирает definition + state вместе)
    // =================================================================================

    [Serializable]
    public class ActiveDealSlotInfo
    {
        public string SlotID { get; set; }
        public int SortOrder { get; set; }
        public int QueueIndex { get; set; }
        public int CycleIndex { get; set; }
        public DealOfferDefinition OfferDef { get; set; }
        public UserDealOfferActivationState ActivationState { get; set; }   // null если IsNewActivation
        public bool IsNewActivation { get; set; }
        public DateTime? ComputedExpiresAtUtc { get; set; }
        public List<ItemOrCurrency> OriginalCost { get; set; }
        public List<ItemOrCurrency> FinalCost { get; set; }
    }
}
