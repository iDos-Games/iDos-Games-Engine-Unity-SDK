using System;
using System.Collections.Generic;
using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum QuestAction
    {
        GetQuestDefinitions,
        GetUserQuestState,
        RefreshQuestCycles,
        ClaimQuestReward,
        ClaimMilestoneReward,
        AddProgress,
    }

    /// <summary>
    /// The player's quest status (the server serializes it as a string).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuestStatus
    {
        Active,
        Completed,
        Claimed,
        Expired
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class QuestRequest : IGSRequest
    {
        public bool AutoRefreshCycles { get; set; } = true;

        public string CycleID { get; set; }     // for cyclic quests (daily/weekly/etc)
        public string QuestID { get; set; }     // claim quest
        public string MilestoneID { get; set; } // claim milestone

        // AddProgress payload
        public string MetricID { get; set; }
        public long Value { get; set; } // for AbsoluteValue/MaxValue
    }

    // =================================================================================
    // USER QUEST STATE (in the GetUserQuestState response)
    // =================================================================================

    /// <summary>
    /// The player's quest status.
    /// </summary>
    [Serializable]
    public class UserQuestState
    {
        /// <summary>
        /// Cyclic quests by cycle. Key = CycleID.
        /// </summary>
        public Dictionary<string, UserQuestCycleState> Cycles { get; set; }

        /// <summary>
        /// Lifetime quests (Achievements/Lifetime). Key = QuestID.
        /// </summary>
        public Dictionary<string, UserQuestProgress> PermanentQuests { get; set; }

        /// <summary>
        /// Time of the last quest update by the server (UTC).
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }

    /// <summary>
    /// The state of one cycle for the player (window + quests + milestone).
    /// </summary>
    [Serializable]
    public class UserQuestCycleState
    {
        public string CycleID { get; set; }
        public DateTime CycleStartUtc { get; set; }
        public DateTime CycleEndUtc { get; set; }

        /// <summary>
        /// Quests of the current window. Key = QuestID.
        /// </summary>
        public Dictionary<string, UserQuestProgress> Quests { get; set; } = new Dictionary<string, UserQuestProgress>();

        /// <summary>
        /// How many quests in this window have been completed (Completed or Claimed).
        /// </summary>
        public int CompletedQuestsCount { get; set; } = 0;

        /// <summary>
        /// Which milestone rewards have already been claimed in this window (MilestoneID).
        /// </summary>
        public List<string> ClaimedMilestoneIDs { get; set; } = new List<string>();
    }

    /// <summary>
    /// Player's quest progress.
    /// </summary>
    [Serializable]
    public class UserQuestProgress
    {
        public string QuestID { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Active;

        public DateTime? ActivatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime? ClaimedAtUtc { get; set; }

        /// <summary>
        /// Progress by goals. Key = ObjectiveID.
        /// </summary>
        public Dictionary<string, UserQuestObjectiveProgress> Objectives { get; set; } = new Dictionary<string, UserQuestObjectiveProgress>();
    }

    /// <summary>
    /// Progress of one quest goal.
    /// </summary>
    [Serializable]
    public class UserQuestObjectiveProgress
    {
        public string ObjectiveID { get; set; }
        public long CurrentValue { get; set; } = 0;
        public bool Completed { get; set; } = false;
        public DateTime? CompletedAtUtc { get; set; }
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class GetUserQuestStateResponse
    {
        public UserQuestState State { get; set; } = new UserQuestState();
    }

    [Serializable]
    public class ClaimQuestRewardResponse
    {
        public string QuestID { get; set; }
        public string CycleID { get; set; } // null for permanent
        public QuestStatus NewStatus { get; set; }
        public List<ItemOrCurrency> GrantedBaseRewards { get; set; }
        public List<ItemOrCurrency> GrantedPremiumRewards { get; set; }
    }

    [Serializable]
    public class ClaimMilestoneRewardResponse
    {
        public string CycleID { get; set; }
        public string MilestoneID { get; set; }
        public int CompletedQuestsCount { get; set; }
        public List<ItemOrCurrency> GrantedBaseRewards { get; set; }
        public List<ItemOrCurrency> GrantedPremiumRewards { get; set; }
    }

    [Serializable]
    public class AddQuestProgressResult
    {
        public List<QuestProgressUpdate> Updates { get; set; } = new List<QuestProgressUpdate>();
    }

    [Serializable]
    public class AddProgressResponse
    {
        public string MetricID { get; set; }
        public List<QuestProgressUpdate> Updates { get; set; } = new List<QuestProgressUpdate>();
    }

    [Serializable]
    public class QuestProgressUpdate
    {
        public string QuestID { get; set; }
        public string CycleID { get; set; } // null for permanent
        public string ObjectiveID { get; set; }

        public long NewValue { get; set; }
        public bool ObjectiveCompleted { get; set; }

        public QuestStatus QuestStatus { get; set; }
    }
}
