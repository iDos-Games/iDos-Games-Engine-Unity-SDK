using System.Collections.Generic;
using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace IDosGames
{
    [Serializable]
    public class UserEventTokensState
    {
        public UserTimedEventTokenState TimedEvent { get; set; } = new();
        public Dictionary<string, UserEventTokenProgress> CoopEvent { get; set; } = new();
        public Dictionary<string, UserEventTokenProgress> Leaderboard { get; set; } = new();
        public Dictionary<string, UserEventTokenProgress> Season { get; set; } = new();
    }

    [Serializable]
    public class UserTimedEventTokenState
    {
        public Dictionary<string, UserEventTokenProgress> Scheduled { get; set; } = new();
        public Dictionary<string, UserEventTokenProgress> Chain { get; set; } = new();
    }

    [Serializable]
    public class UserEventTokenProgress
    {
        public EventTokenBalanceData Balance { get; set; }
        public EventTokenDailyData Daily { get; set; }
        public EventTokenMilestoneData Milestone { get; set; }
        public EventTokenMetaData Meta { get; set; }
    }

    [Serializable]
    public class EventTokenBalanceData
    {
        public long Current { get; set; }
        public long TotalEarned { get; set; }
        public long TotalSpent { get; set; }
    }

    [Serializable]
    public class EventTokenDailyData
    {
        public DateTime Date { get; set; }
        public long TotalEarned { get; set; }
        public Dictionary<string, long> EarnedBySource { get; set; }
        public Dictionary<string, int> TriggersBySource { get; set; }
        public Dictionary<string, DateTime> LastTriggerBySource { get; set; }
    }

    [Serializable]
    public class EventTokenMilestoneData
    {
        public List<string> ClaimedIDs { get; set; }
        public List<string> UnlockedIDs { get; set; }
    }

    [Serializable]
    public class EventTokenMetaData
    {
        public DateTime JoinedAtUtc { get; set; }
        public DateTime LastEarnedAtUtc { get; set; }
    }

    [Serializable]
    public class EventTokenOperation
    {
        public EventTokenAddress Address { get; set; }
        public long Amount { get; set; }
        public string Source { get; set; }
    }

    [Serializable]
    public class EventTokenAddress
    {
        public EventTokenType Type { get; set; }
        public string EntityID { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventTokenType
    {
        TimedEventScheduled,
        TimedEventChain,
        Leaderboard,
        CoopEvent,
        Season,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventModifierTarget
    {
        BoardAttackReward,
        BoardRaidReward,
        BoardPassStartReward,
        BoardBuildCost,
        BoardBuildReward,
        BoardTileLandingReward,
        BoardRollSteps,
        QuestReward,
        Custom,
    }
}
