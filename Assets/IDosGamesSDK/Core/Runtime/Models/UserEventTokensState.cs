using System.Collections.Generic;
using System;

namespace IDosGames
{
    [Serializable]
    public class UserEventTokensState
    {
        public Dictionary<string, UserEventTokenProgress> EventTokens { get; set; }
    }

    [Serializable]
    public class UserEventTokenProgress
    {
        public EventTokenBalanceData EventTokenBalance { get; set; }
        public EventTokenDailyData EventTokenDaily { get; set; }
        public EventTokenMilestoneData EventTokenMilestone { get; set; }
        public EventTokenStreakData EventTokenStreak { get; set; }
        public EventTokenTierData EventTokenTier { get; set; }
        public EventTokenMetaData EventTokenMeta { get; set; }
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
    public class EventTokenStreakData
    {
        public int CurrentDays { get; set; }
        public int MaxDays { get; set; }
        public DateTime LastStreakDate { get; set; }
        public List<int> ClaimedDays { get; set; }
    }

    [Serializable]
    public class EventTokenTierData
    {
        public int TierIndex { get; set; }
    }

    [Serializable]
    public class EventTokenMetaData
    {
        public DateTime JoinedAtUtc { get; set; }
        public DateTime LastEarnedAtUtc { get; set; }
    }
}
