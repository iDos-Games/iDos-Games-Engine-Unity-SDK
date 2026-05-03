using IDosGames.TitlePublicConfiguration;
using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum LeaderboardAction
    {
        GetLeaderboardDefinitions,
        GetLeaderboard,
        GetMyProgress,
        SubmitScore,
        ClaimCycleReward
    }

    public enum LeaderboardScoreAggregation
    {
        Sum,
        BestScore,
        BestTime,
        LastValue
    }

    public enum LeaderboardCycleReset
    {
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Never
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class LeaderboardRequest : IGSRequest
    {
        public long Score { get; set; }
    }

    // =================================================================================
    // DEFINITIONS (Config)
    // =================================================================================

    [Serializable]
    public class LeaderboardDefinitions
    {
        public List<LeaderboardDefinition> Leaderboards { get; set; }
    }

    [Serializable]
    public class LeaderboardDefinition
    {
        public string LeaderboardID { get; set; }
        public string DisplayName { get; set; }
        public string ScoreDisplayName { get; set; }
        public bool IsEnabled { get; set; }
        public LeaderboardScoreAggregation ScoreAggregation { get; set; }
        public LeaderboardCycleReset CycleReset { get; set; }
        public int TopUsersLimit { get; set; }
        public long MinScoreToEnterTop { get; set; }
        public List<LeaderboardRankReward> RankRewards { get; set; }
        public PremiumRewardsMode DefaultPremiumRewardsMode { get; set; }
        public bool PremiumStackLowerTiers { get; set; }
    }

    [Serializable]
    public class LeaderboardRankReward
    {
        public string Rank { get; set; }
        public List<ItemOrCurrency> Rewards { get; set; }
        public PremiumRewardsMode? PremiumRewardsMode { get; set; }
        //public List<PremiumTierReward> PremiumRewards { get; set; }
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class GetLeaderboardResponse
    {
        public string LeaderboardID { get; set; }
        public int CycleVersion { get; set; }
        public long TotalParticipants { get; set; }
        public DateTime? CycleEndUtc { get; set; }
        public List<LeaderboardUserEntry> TopUsers { get; set; }
    }

    [Serializable]
    public class GetMyProgressResponse
    {
        public string LeaderboardID { get; set; }
        public long CurrentScore { get; set; }
        public int LastKnownRank { get; set; }
        public bool HasUnclaimedReward { get; set; }
    }

    [Serializable]
    public class SubmitScoreResponse
    {
        public string LeaderboardID { get; set; }
        public long NewScore { get; set; }
        public int CycleVersion { get; set; }
    }

    [Serializable]
    public class ClaimCycleRewardResponse
    {
        public string LeaderboardID { get; set; }
        public int Rank { get; set; }
        public List<ItemOrCurrency> BaseRewards { get; set; }
        public List<ItemOrCurrency> PremiumRewards { get; set; }
    }

    // =================================================================================
    // MODELS
    // =================================================================================

    [Serializable]
    public class LeaderboardUserEntry
    {
        public string UserID { get; set; }
        public long Score { get; set; }
        public int Rank { get; set; }
        public DateTime LastScoreUpdateUtc { get; set; }
        public UserPublicDataModel PublicProfile { get; set; }
    }

    [Serializable]
    public class UserLeaderboardsState
    {
        public Dictionary<string, UserLeaderboardProgress> ProgressByCycle { get; set; }
    }

    [Serializable]
    public class UserLeaderboardProgress
    {
        public long CurrentScore { get; set; }
        public int ScoreCycleVersion { get; set; }
        public DateTime LastScoreSubmitUtc { get; set; }
        public int UnclaimedRewardCycleVersion { get; set; }
        public int LastClaimedCycleVersion { get; set; }
        public int LastKnownRank { get; set; }
    }
}
