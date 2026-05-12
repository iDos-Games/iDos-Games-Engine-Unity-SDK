using System;
using System.Collections.Generic;

namespace IDosGames
{
    // =====================================================================
    // REQUEST
    // =====================================================================

    /// <summary>
    /// Unified request for all CoopEvent actions.
    /// <list type="bullet">
    ///   <item><b>GetDefinitions</b>  — no extra fields required.</item>
    ///   <item><b>GetActiveEvent</b>  — <see cref="CoopChainID"/> required.</item>
    ///   <item><b>GetUserState</b>    — no extra fields required.</item>
    ///   <item><b>GetGroupState</b>   — <see cref="GroupID"/> required.</item>
    ///   <item><b>JoinOrCreateGroup</b> — <see cref="CoopChainID"/> required.</item>
    ///   <item><b>Spin</b>            — <see cref="CoopChainID"/> required; <see cref="RelatedEntityID"/> recommended for idempotency.</item>
    ///   <item><b>ClaimObjectReward</b> — <see cref="GroupID"/> required; <see cref="RelatedEntityID"/> recommended.</item>
    ///   <item><b>ClaimGrandPrize</b> — <see cref="GroupID"/> required; <see cref="RelatedEntityID"/> recommended.</item>
    ///   <item><b>LeaveGroup</b>      — <see cref="GroupID"/> optional (falls back to active group).</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class CoopEventRequest : BaseRequest
    {
        /// <summary>ID of the coop event chain.</summary>
        public string CoopChainID;

        /// <summary>ID of the group (for GetGroupState, ClaimObjectReward, ClaimGrandPrize, LeaveGroup).</summary>
        public string GroupID;
    }

    // =====================================================================
    // ACTION ENUM
    // =====================================================================

    public enum CoopEventAction
    {
        GetDefinitions,
        GetActiveEvent,
        GetUserState,
        GetGroupState,
        JoinOrCreateGroup,
        Spin,
        ClaimObjectReward,
        ClaimGrandPrize,
        LeaveGroup,
    }

    // =====================================================================
    // ENUMS
    // =====================================================================

    public enum CoopGroupStatus
    {
        Forming,
        Active,
        Completed,
        Failed,
        Expired
    }

    public enum CoopMemberStatus
    {
        Active,
        Left,
        Replaced
    }

    public enum CoopEventType
    {
        BuildObjects,
        BossAttack,
    }

    public enum CoopRewardType
    {
        ObjectCompletion,
        GrandPrize
    }

    // =====================================================================
    // RESPONSES
    // =====================================================================

    /// <summary>Active coop event info within a chain (computed window).</summary>
    [Serializable]
    public class ActiveCoopEventInfo
    {
        public string CoopChainID;
        public int CycleIndex;
        public int EventOrder;
        public CoopEventDefinition EventDef;
        public DateTime ComputedStartUtc;
        public DateTime ComputedEndUtc;
        public long SecondsRemaining;
    }

    /// <summary>Personal user state plus brief active group snapshot.</summary>
    [Serializable]
    public class CoopUserStateResponse
    {
        public DateTime ServerTimeUtc;
        public UserCoopEventState UserState;
        /// <summary>null if the player is not in any group.</summary>
        public CoopEventGroupDocument ActiveGroup;
    }

    /// <summary>Full group state plus seconds remaining until the event expires.</summary>
    [Serializable]
    public class CoopGroupStateResponse
    {
        public DateTime ServerTimeUtc;
        public CoopEventGroupDocument Group;
        public long SecondsRemaining;
    }

    /// <summary>Result of a single spinner spin.</summary>
    [Serializable]
    public class CoopSpinResponse
    {
        public DateTime ServerTimeUtc;
        public string GroupID;
        /// <summary>
        /// Resource operation result. Consumed event tokens are in
        /// Resources.Consume.Standard.EventTokens as EventTokenOperationApplied entries.
        /// </summary>
        public ResourceOperation Resources;
        /// <summary>Index of the object that received progress.</summary>
        public int ObjectIndex;
        /// <summary>Spinner sector that was rolled.</summary>
        public CoopSpinnerSector Sector;
        /// <summary>Progress points added by this spin.</summary>
        public int ProgressDelta;
        /// <summary>New CurrentProgress after the spin.</summary>
        public long NewProgress;
        /// <summary>MaxProgress of the object.</summary>
        public long MaxProgress;
        /// <summary>true — the object was completed by this spin.</summary>
        public bool ObjectCompleted;
        /// <summary>true — all objects are completed; Grand Prize is available.</summary>
        public bool AllObjectsCompleted;
    }

    /// <summary>Result of claiming an object completion reward or Grand Prize.</summary>
    [Serializable]
    public class CoopClaimRewardResponse
    {
        public DateTime ServerTimeUtc;
        public string GroupID;
        public CoopRewardType RewardType;
        public ResourceOperation Resources;
    }

    /// <summary>Result of leaving a group.</summary>
    [Serializable]
    public class CoopLeaveGroupResponse
    {
        public DateTime ServerTimeUtc;
        public string GroupID;
        public bool Success;
    }

    // =====================================================================
    // STATE MODELS  (UserDataDocument.CoopEvent)
    // =====================================================================

    /// <summary>
    /// Personal coop event state stored in UserDataDocument.
    /// Contains a reference to the active group and a short history.
    /// </summary>
    [Serializable]
    public class UserCoopEventState
    {
        /// <summary>Active group ID. null — player is not in any group.</summary>
        public string ActiveGroupID;

        /// <summary>Cached active coop event ID. null — no active event.</summary>
        public string ActiveCoopEventID;

        /// <summary>Player's object index in the current group (0–3). -1 — not in a group.</summary>
        public int MyObjectIndex = -1;

        /// <summary>UTC timestamp of the player's last spin.</summary>
        public DateTime LastSpinAtUtc = DateTime.MinValue;

        /// <summary>History of the last ~5 completed coop events (FIFO).</summary>
        public List<CoopEventHistoryEntry> History = new();
    }

    /// <summary>One entry in the player's coop event history.</summary>
    [Serializable]
    public class CoopEventHistoryEntry
    {
        public string GroupID;
        public string CoopEventID;
        public CoopGroupStatus FinalStatus;
        /// <summary>true — Grand Prize was received for this event.</summary>
        public bool GrandPrizeReceived;
        public DateTime FinishedAtUtc;
    }

    // =====================================================================
    // GROUP DOCUMENT  (coop_groups collection)
    // =====================================================================

    /// <summary>
    /// Group document shared by all members of a coop event group.
    /// Received from the server as-is; do not mutate locally.
    /// </summary>
    [Serializable]
    public class CoopEventGroupDocument
    {
        public string GroupID;
        public string TitleID;
        public string CoopChainID;
        public string CoopEventID;
        public int CycleIndex;
        public List<CoopGroupMember> Members = new();
        /// <summary>null for event types other than BuildObjects.</summary>
        public CoopBuildObjectsState BuildObjectsState;
        public CoopGroupStatus Status = CoopGroupStatus.Forming;
        public DateTime CreatedAtUtc;
        public DateTime ExpiresAtUtc;
        public long Version;
    }

    /// <summary>Data for one participant inside a coop group.</summary>
    [Serializable]
    public class CoopGroupMember
    {
        public string UserID;
        public UserPublicDataModel PublicData;
        /// <summary>null for event types other than BuildObjects.</summary>
        public CoopBuildObjectsMemberState BuildObjectsProgress;
        /// <summary>true — this slot is a bot (auto-filled after matchmaking timeout).</summary>
        public bool IsBot;
        public int SpinsCount;
        public long TokensSpent;
        public CoopMemberStatus MemberStatus = CoopMemberStatus.Active;
        public bool GrandPrizeClaimed;
        public DateTime JoinedAtUtc;
        /// <summary>null — member has not left.</summary>
        public DateTime? LeftAtUtc;
    }

    /// <summary>Group-level state for the BuildObjects mechanic.</summary>
    [Serializable]
    public class CoopBuildObjectsState
    {
        public List<CoopPartnerObjectState> Objects = new();
    }

    /// <summary>State of one build object inside a coop group.</summary>
    [Serializable]
    public class CoopPartnerObjectState
    {
        /// <summary>Object index (0–3).</summary>
        public int Index;
        /// <summary>UserID of the member whose spins feed this object.</summary>
        public string OwnerUserID;
        public long CurrentProgress;
        public long MaxProgress = 100;
        public bool IsCompleted;
    }

    /// <summary>Personal BuildObjects progress for one group member.</summary>
    [Serializable]
    public class CoopBuildObjectsMemberState
    {
        /// <summary>Index of the object this member is responsible for (0–3).</summary>
        public int ObjectIndex;
        /// <summary>true — the object completion reward has been claimed.</summary>
        public bool ObjectCompletionRewardClaimed;
    }

    // =====================================================================
    // CONFIG MODELS  (TitlePublicConfigurationModel.CoopEvent)
    // =====================================================================

    /// <summary>Root config for the coop event system.</summary>
    [Serializable]
    public class CoopEventDefinitions
    {
        /// <summary>All coop event chains. Key — CoopChainID.</summary>
        public Dictionary<string, CoopEventChainDefinition> Chains = new();
    }

    /// <summary>A chain of coop events with a cyclic schedule.</summary>
    [Serializable]
    public class CoopEventChainDefinition
    {
        public string CoopChainID;
        public string DisplayName;
        /// <summary>Anchor point (UTC). Cycle 0 starts exactly at this moment.</summary>
        public DateTime AnchorUtc;
        public bool IsActive = true;
        /// <summary>Maximum number of full cycles. 0 — infinite.</summary>
        public int MaxCycles;
        /// <summary>Pause between events within one cycle (seconds).</summary>
        public int PauseBetweenEventsSec;
        /// <summary>Pause between full cycles (seconds).</summary>
        public int PauseBetweenCyclesSec;
        /// <summary>Ordered list of events in this chain (sort by Order ASC).</summary>
        public List<CoopEventDefinition> Events = new();
    }

    /// <summary>One coop event inside a chain.</summary>
    [Serializable]
    public class CoopEventDefinition
    {
        public string CoopEventID;
        public int Order;
        /// <summary>Duration in seconds. Typical: 5–7 days.</summary>
        public long DurationSec = 518400;
        public string DisplayName;
        public string Description;
        public Dictionary<string, string> AssetPaths;
        public CoopEventType EventType = CoopEventType.BuildObjects;
        /// <summary>Number of partners (excluding the initiator). Total group size = 1 + PartnerCount.</summary>
        public int PartnerCount = 4;
        /// <summary>Matchmaking timeout (minutes). After expiry, empty slots are filled with bots.</summary>
        public int MatchmakingTimeoutMinutes = 5;
        /// <summary>Grace period when a member leaves (minutes).</summary>
        public int MemberGracePeriodMinutes = 30;
        /// <summary>BuildObjects mechanic config. Populated only when EventType = BuildObjects.</summary>
        public CoopBuildObjectsDefinition BuildObjects;
        /// <summary>Grand Prize granted to every member when all objects are completed in time.</summary>
        public ResourceGrant GrandPrize = new();
    }

    /// <summary>Config for the BuildObjects mechanic.</summary>
    [Serializable]
    public class CoopBuildObjectsDefinition
    {
        public List<CoopObjectDefinition> Objects = new();
        public ResourceConsume SpinCost = new();
        public List<CoopSpinnerSector> SpinnerTable = new();
    }

    /// <summary>Config for one build object inside a coop event.</summary>
    [Serializable]
    public class CoopObjectDefinition
    {
        /// <summary>Object index (0–3).</summary>
        public int Index;
        public string DisplayName;
        public Dictionary<string, string> AssetPaths;
        public long MaxProgress = 100;
        /// <summary>Reward granted to the member when their object is completed.</summary>
        public ResourceGrant CompletionReward = new();
    }

    /// <summary>One sector of the spinner wheel.</summary>
    [Serializable]
    public class CoopSpinnerSector
    {
        public string DisplayName;
        public Dictionary<string, string> AssetPaths;
        /// <summary>Weight on an arbitrary scale. P(sector) = Weight / sum(all weights).</summary>
        public int Weight;
        public int MinProgress;
        public int MaxProgress;
    }
}
