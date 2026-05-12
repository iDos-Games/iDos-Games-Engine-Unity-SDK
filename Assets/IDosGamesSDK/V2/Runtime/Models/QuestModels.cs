using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Unified request for all Quest actions.
    ///
    /// Field usage by action:
    ///   GetQuestDefinitions    — no extra fields
    ///   GetUserQuestState      — AutoRefreshCycles (optional, default true)
    ///   RefreshQuestCycles     — no extra fields
    ///   ClaimQuestReward       — QuestID (required); CycleID (required for cyclic, null for permanent)
    ///   ClaimMilestoneReward   — CycleID (required), MilestoneID (required)
    ///   AddQuestProgress       — MetricID (required), ProgressValue (required, >= 0)
    /// </summary>
    [Serializable]
    public class QuestRequest : BaseRequest
    {
        /// <summary>
        /// When true the server auto-refreshes cycle windows before returning state.
        /// Defaults to true.
        /// </summary>
        public bool AutoRefreshCycles { get; set; } = true;

        /// <summary>Cycle identifier (cyclic quests and milestone claims).</summary>
        public string CycleID { get; set; }

        /// <summary>Quest identifier (ClaimQuestReward).</summary>
        public string QuestID { get; set; }

        /// <summary>Milestone identifier (ClaimMilestoneReward).</summary>
        public string MilestoneID { get; set; }

        /// <summary>Metric key for AddQuestProgress.</summary>
        public string MetricID { get; set; }

        /// <summary>Progress value for AddQuestProgress (must be >= 0).</summary>
        public long ProgressValue { get; set; }
    }

    // ===================== Response DTOs =====================

    /// <summary>Response for GetUserQuestState.</summary>
    [Serializable]
    public class GetUserQuestStateResponse
    {
        public UserQuestState State { get; set; } = new UserQuestState();
    }

    /// <summary>Response for ClaimQuestReward.</summary>
    [Serializable]
    public class ClaimQuestRewardResponse
    {
        public DateTime ServerTimeUtc { get; set; }
        public string QuestID { get; set; }

        /// <summary>null for permanent quests.</summary>
        public string CycleID { get; set; }

        public QuestStatus NewStatus { get; set; }

        /// <summary>
        /// Resource operation with actual granted amounts including premium bonuses and event tokens.
        /// </summary>
        public ResourceOperation Resources { get; set; } = new();
    }

    /// <summary>Response for ClaimMilestoneReward.</summary>
    [Serializable]
    public class ClaimMilestoneRewardResponse
    {
        public DateTime ServerTimeUtc { get; set; }
        public string CycleID { get; set; }
        public string MilestoneID { get; set; }
        public int CompletedQuestsCount { get; set; }

        /// <summary>Resource operation with actual granted amounts.</summary>
        public ResourceOperation Resources { get; set; } = new();
    }

    /// <summary>Response for AddQuestProgress.</summary>
    [Serializable]
    public class AddQuestProgressResponse
    {
        public string MetricID { get; set; }

        /// <summary>Per-objective updates that actually changed.</summary>
        public List<QuestProgressUpdate> Updates { get; set; } = new();
    }

    /// <summary>Describes a single objective change returned by AddQuestProgress.</summary>
    [Serializable]
    public class QuestProgressUpdate
    {
        public string QuestID { get; set; }

        /// <summary>null for permanent quests.</summary>
        public string CycleID { get; set; }

        public string ObjectiveID { get; set; }
        public long NewValue { get; set; }
        public bool ObjectiveCompleted { get; set; }
        public QuestStatus Status { get; set; }
    }

    // ===================== State models =====================

    /// <summary>
    /// Root quest state for a player (UserDataDocument.Quest).
    /// </summary>
    [Serializable]
    public class UserQuestState
    {
        /// <summary>Cyclic quest cycles keyed by CycleID.</summary>
        public Dictionary<string, UserQuestCycleState> Cycles { get; set; } = new();

        /// <summary>Permanent (lifetime/achievement) quests keyed by QuestID.</summary>
        public Dictionary<string, UserQuestProgress> PermanentQuests { get; set; } = new();

        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>State of a single cycle window for a player.</summary>
    [Serializable]
    public class UserQuestCycleState
    {
        public string CycleID { get; set; }
        public DateTime CycleStartUtc { get; set; }
        public DateTime CycleEndUtc { get; set; }

        /// <summary>Quest progress within this window keyed by QuestID.</summary>
        public Dictionary<string, UserQuestProgress> Quests { get; set; } = new();

        /// <summary>
        /// Number of quests in this window that are Completed or Claimed.
        /// Used for milestone threshold checks.
        /// </summary>
        public int CompletedQuestsCount { get; set; } = 0;

        /// <summary>MilestoneIDs already claimed in this window.</summary>
        public List<string> ClaimedMilestoneIDs { get; set; } = new();
    }

    /// <summary>Quest status for a player.</summary>
    public enum QuestStatus
    {
        Active,
        Completed,
        Claimed,
        Expired
    }

    /// <summary>Player's progress on a single quest.</summary>
    [Serializable]
    public class UserQuestProgress
    {
        public string QuestID { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Active;
        public DateTime? ActivatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime? ClaimedAtUtc { get; set; }

        /// <summary>Per-objective progress keyed by ObjectiveID.</summary>
        public Dictionary<string, UserQuestObjectiveProgress> Objectives { get; set; } = new();
    }

    /// <summary>Player's progress on a single quest objective.</summary>
    [Serializable]
    public class UserQuestObjectiveProgress
    {
        public string ObjectiveID { get; set; }
        public long CurrentValue { get; set; } = 0;
        public bool Completed { get; set; } = false;
        public DateTime? CompletedAtUtc { get; set; }
    }

    // ===================== Config models =====================

    /// <summary>Root quest configuration block in TitlePublicConfigurationModel.</summary>
    [Serializable]
    public class QuestDefinitions
    {
        /// <summary>Cycle definitions keyed by CycleID. Resets always happen at 00:00:00 UTC.</summary>
        public Dictionary<string, QuestCycleDefinition> Cycles { get; set; } = new();

        /// <summary>
        /// Quest definitions keyed by QuestID.
        /// Empty/null CycleIDs => permanent quest; otherwise cyclic.
        /// </summary>
        public Dictionary<string, QuestDefinition> Quests { get; set; } = new();
    }

    /// <summary>Cycle reset type. Resets always at 00:00:00 UTC.</summary>
    public enum QuestCycleResetKind
    {
        /// <summary>Every day at 00:00 UTC.</summary>
        Daily,

        /// <summary>Every Monday at 00:00 UTC.</summary>
        Weekly,

        /// <summary>Every 1st of the month at 00:00 UTC.</summary>
        Monthly,

        /// <summary>Every IntervalDays days from AnchorUtc at 00:00 UTC.</summary>
        FixedIntervalDays
    }

    /// <summary>Cycle definition: schedule and milestone rewards.</summary>
    [Serializable]
    public class QuestCycleDefinition
    {
        /// <summary>Cycle identifier.</summary>
        public string CycleID { get; set; }

        public string DisplayName { get; set; }

        public QuestCycleResetKind ResetKind { get; set; } = QuestCycleResetKind.Daily;

        /// <summary>FixedIntervalDays only: window length in days.</summary>
        public int? IntervalDays { get; set; } = 10;

        /// <summary>FixedIntervalDays only: anchor date at 00:00 UTC.</summary>
        public DateTime? AnchorUtc { get; set; }

        /// <summary>Milestone rewards keyed by MilestoneID.</summary>
        public Dictionary<string, QuestCycleMilestoneDefinition> Milestones { get; set; } = new();
    }

    /// <summary>Milestone reward for completing N quests within a cycle window.</summary>
    [Serializable]
    public class QuestCycleMilestoneDefinition
    {
        /// <summary>Milestone identifier.</summary>
        public string MilestoneID { get; set; }

        /// <summary>Number of Completed/Claimed quests required to unlock this milestone.</summary>
        public int RequiredCompletedQuests { get; set; } = 0;

        /// <summary>Rewards granted when this milestone is claimed.</summary>
        public ResourceGrant Reward { get; set; }
    }

    /// <summary>Quest definition.</summary>
    [Serializable]
    public class QuestDefinition
    {
        /// <summary>Quest identifier.</summary>
        public string QuestID { get; set; }

        /// <summary>
        /// CycleIDs this quest belongs to.
        /// Empty/null => permanent quest.
        /// </summary>
        public List<string> CycleIDs { get; set; } = new();

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; } = 0;

        /// <summary>QuestIDs that must be claimed before this quest activates.</summary>
        public List<string> RequiredQuestIDs { get; set; } = new();

        /// <summary>Objectives keyed by ObjectiveID. All must be completed for the quest to complete.</summary>
        public Dictionary<string, QuestObjectiveDefinition> Objectives { get; set; } = new();

        /// <summary>Rewards granted when this quest is claimed.</summary>
        public ResourceGrant Reward { get; set; }
    }

    /// <summary>Quest objective: a universal metric-based counter.</summary>
    [Serializable]
    public class QuestObjectiveDefinition
    {
        /// <summary>Objective identifier within the quest.</summary>
        public string ObjectiveID { get; set; }

        /// <summary>Who is allowed to post progress for this objective.</summary>
        public QuestObjectiveSource Source { get; set; } = QuestObjectiveSource.SystemEvent;

        /// <summary>
        /// Maximum progress delta accepted per single AddQuestProgress call.
        /// 0 means unlimited.
        /// </summary>
        public long MaxProgressPerCall { get; set; }

        /// <summary>Metric key (e.g. "raid_win", "currency_spent:IG").</summary>
        public string MetricID { get; set; }

        public long TargetValue { get; set; } = 1;

        public StatisticAggregationMethod AggregationMethod { get; set; } = StatisticAggregationMethod.Sum;

        /// <summary>Optional filters for metric disambiguation (mode, difficulty, etc.).</summary>
        public Dictionary<string, string> Filters { get; set; } = new();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatisticAggregationMethod
    {
        Last, // Last (always update with the new value)
        Minimum, // Minimum (always use the lowest value)
        Maximum, // Maximum (always use the highest value)
        Sum // Sum (add this value to the existing value)
    }

    /// <summary>Who is permitted to post progress for an objective.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuestObjectiveSource
    {
        /// <summary>Client calls AddQuestProgress directly.</summary>
        ClientApi,

        /// <summary>Another server posts progress with a secret key.</summary>
        ServerApi,

        /// <summary>Internal game logic only — not reachable via HTTP.</summary>
        SystemEvent
    }

    /// <summary>Action enum for the Quest module. Matches the server switch 1:1.</summary>
    public enum QuestAction
    {
        GetQuestDefinitions,
        GetUserQuestState,
        RefreshQuestCycles,
        ClaimQuestReward,
        ClaimMilestoneReward,
        AddQuestProgress,
    }
}
