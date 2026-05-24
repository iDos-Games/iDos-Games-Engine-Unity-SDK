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
    /// Unified request for all Leaderboard actions.
    /// <list type="bullet">
    ///   <item>All actions except <see cref="LeaderboardAction.GetDefinitions"/> require <see cref="LeaderboardID"/>.</item>
    ///   <item><see cref="LeaderboardAction.SubmitScore"/>: requires <see cref="Score"/> (&gt; 0).</item>
    ///   <item><see cref="LeaderboardAction.ClaimMilestone"/>: requires <see cref="MilestoneID"/>.</item>
    ///   <item><see cref="LeaderboardAction.ClaimCycleReward"/>, <see cref="LeaderboardAction.ClaimMilestone"/>:
    ///         benefit from a stable <see cref="BaseRequest.RelatedEntityID"/> for idempotency.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class LeaderboardRequest : BaseRequest
    {
        /// <summary>Leaderboard ID. Required for all actions except GetDefinitions.</summary>
        public string LeaderboardID;

        /// <summary>Score to submit. Used by SubmitScore; must be &gt; 0.</summary>
        public long Score;

        /// <summary>Milestone ID to claim. Used by ClaimMilestone.</summary>
        public string MilestoneID;
    }

    // =====================================================================
    // RESPONSES
    // =====================================================================

    /// <summary>Top users and metadata for a leaderboard (global top or player's bracket).</summary>
    [Serializable]
    public class GetLeaderboardResponse
    {
        public string LeaderboardID;
        public int CycleVersion;
        public long TotalParticipants;
        public DateTime? CycleEndUtc;
        public List<LeaderboardUserEntry> TopUsers = new();

        /// <summary>
        /// Bracket ID the player belongs to. null if brackets are disabled for this leaderboard
        /// or the player has not submitted yet.
        /// </summary>
        public string BracketID;
    }

    /// <summary>Player's personal progress in a leaderboard for the current cycle.</summary>
    [Serializable]
    public class GetMyProgressResponse
    {
        public string LeaderboardID;

        /// <summary>Current aggregated score. 0 if the cycle changed since last submit.</summary>
        public long CurrentScore;

        /// <summary>Last known rank in the top (cached; may lag behind real-time).</summary>
        public int LastKnownRank;

        /// <summary>true if there is an unclaimed end-of-cycle rank reward from a previous cycle.</summary>
        public bool HasUnclaimedReward;

        /// <summary>
        /// Monotonic sum of all submitted scores this cycle (independent of ScoreAggregation mode).
        /// Used for milestone threshold checks.
        /// </summary>
        public long ScoreEarnedThisCycle;

        /// <summary>Next milestone not yet claimed. null if all milestones are claimed or none exist.</summary>
        public LeaderboardMilestoneDefinition NextMilestone;

        /// <summary>true if any milestone threshold is crossed but the reward has not been claimed yet.</summary>
        public bool HasUnclaimedMilestone;
    }

    /// <summary>Response for SubmitScore.</summary>
    [Serializable]
    public class SubmitScoreResponse
    {
        public string LeaderboardID;

        /// <summary>Aggregated score after this submit (respects ScoreAggregation mode).</summary>
        public long NewScore;

        /// <summary>Active cycle version at time of submit.</summary>
        public int CycleVersion;
    }

    /// <summary>Response for ClaimCycleReward.</summary>
    [Serializable]
    public class ClaimCycleRewardResponse
    {
        public DateTime ServerTimeUtc;
        public string LeaderboardID;

        /// <summary>Player's rank in the archived cycle. 0 if the player was not in the top.</summary>
        public int Rank;

        /// <summary>
        /// Resources granted for the rank. Empty ResourceOperation if the rank had no reward
        /// or the player was not found in the archive.
        /// </summary>
        public ResourceOperation Resources;
    }

    /// <summary>Response for ClaimMilestone.</summary>
    [Serializable]
    public class ClaimLeaderboardMilestoneResponse
    {
        public DateTime ServerTimeUtc;
        public string LeaderboardID;
        public string MilestoneID;

        /// <summary>Resources granted for the milestone. Empty ResourceOperation if the milestone had no reward.</summary>
        public ResourceOperation Resources;
    }

    // =====================================================================
    // USER STATE
    // =====================================================================

    /// <summary>Root container for all leaderboard states of a player. Stored in UserState.Leaderboard.</summary>
    [Serializable]
    public class UserLeaderboardsState
    {
        /// <summary>
        /// Progress per leaderboard cycle. Key = cycleDocumentID ("{titleID}_{leaderboardID}").
        /// </summary>
        public Dictionary<string, UserLeaderboardProgress> ProgressByCycle = new();
    }

    /// <summary>Player's progress in a single leaderboard cycle.</summary>
    [Serializable]
    public class UserLeaderboardProgress
    {
        /// <summary>Current aggregated score in the active cycle.</summary>
        public long CurrentScore;

        /// <summary>
        /// Monotonic sum of all submitted scores this cycle.
        /// Always accumulates regardless of ScoreAggregation mode.
        /// Used for milestone threshold checks.
        /// </summary>
        public long ScoreEarnedThisCycle;

        /// <summary>Milestone IDs claimed in the current cycle. Reset when ScoreCycleVersion changes.</summary>
        public List<string> ClaimedMilestoneIDs = new();

        /// <summary>
        /// Cycle version this score belongs to. If less than the server's active CycleVersion,
        /// CurrentScore is stale and treated as 0.
        /// </summary>
        public int ScoreCycleVersion;

        /// <summary>When the player last submitted a score (UTC).</summary>
        public DateTime LastScoreSubmitUtc;

        /// <summary>Cycle version for which there is an unclaimed rank reward. 0 = nothing to claim.</summary>
        public int UnclaimedRewardCycleVersion;

        /// <summary>Last cycle version for which the rank reward was successfully claimed.</summary>
        public int LastClaimedCycleVersion;

        /// <summary>Last known rank in the top (cached for display; may lag).</summary>
        public int LastKnownRank;

        /// <summary>
        /// Bracket ID assigned to the player in the current cycle.
        /// null if brackets are disabled or the player has not submitted yet this cycle.
        /// </summary>
        public string BracketID;
    }

    // =====================================================================
    // DATA MODELS
    // =====================================================================

    /// <summary>One player's entry in a leaderboard top list.</summary>
    [Serializable]
    public class LeaderboardUserEntry
    {
        public string UserID;
        public long Score;

        /// <summary>Rank in the sorted top (1 = first place). Assigned at sort time, not submit time.</summary>
        public int Rank;

        public DateTime LastScoreUpdateUtc;

        /// <summary>Hashed IP for anti-cheat / multi-account detection.</summary>
        public string IpHash;

        /// <summary>Snapshot of the player's public profile at last submit time.</summary>
        public UserPublicDataModel PublicProfile;

        /// <summary>
        /// Snapshot of the player's season tier at cycle end (from UserSeasonState.CurrentTier).
        /// Populated in the archive by ResetCycle via bulk-read; 0 in the active cycle document.
        /// 0 = no season link, no season state, or SeasonTierRewardResolver returns base reward.
        /// </summary>
        public int SeasonTierAtCycleEnd;
    }

    // =====================================================================
    // CONFIG MODELS
    // =====================================================================

    /// <summary>Root leaderboard config for a title. Corresponds to TitlePublicConfigurationModel.Leaderboard.</summary>
    [Serializable]
    public class LeaderboardDefinitions
    {
        /// <summary>All leaderboard definitions for the title. Key = LeaderboardID.</summary>
        public Dictionary<string, LeaderboardDefinition> Definitions = new();
    }

    /// <summary>Configuration for a single leaderboard.</summary>
    [Serializable]
    public class LeaderboardDefinition
    {
        /// <summary>Unique leaderboard ID. Example: "weekly_pvp".</summary>
        public string LeaderboardID;

        public string DisplayName;

        /// <summary>Display name for the score unit shown in UI. Example: "Points", "Wins".</summary>
        public string ScoreDisplayName;

        /// <summary>Whether the leaderboard accepts score submits. false = SubmitScore is rejected.</summary>
        public bool IsEnabled = true;

        /// <summary>How an incoming score is combined with the player's current cycle score.</summary>
        public LeaderboardScoreAggregation ScoreAggregation = LeaderboardScoreAggregation.Sum;

        /// <summary>How often the cycle resets.</summary>
        public LeaderboardCycleReset CycleReset = LeaderboardCycleReset.Daily;

        /// <summary>Maximum number of users tracked in the top. Entries beyond this limit are not stored.</summary>
        public int TopUsersLimit = 100;

        /// <summary>Minimum score required to appear in the top.</summary>
        public long MinScoreToEnterTop = 1;

        /// <summary>
        /// Maximum players per bracket. 0 = brackets disabled, global top is used.
        /// When &gt; 0, each player competes only within their assigned bracket.
        /// </summary>
        public int BracketSize = 0;

        /// <summary>
        /// Season chain ID this leaderboard is linked to (SeasonChainDefinition.SeasonChainID).
        /// Used to scale rank and milestone rewards by the player's season tier via SeasonTierRewardResolver.
        /// null or empty = season scaling disabled; SeasonTierRewards fields are ignored.
        /// </summary>
        public string LinkedSeasonChainID;

        /// <summary>End-of-cycle rewards by rank range.</summary>
        public List<LeaderboardRankReward> RankRewards = new();

        /// <summary>
        /// Milestone rewards for accumulating ScoreEarnedThisCycle within one cycle.
        /// Key = MilestoneID. Independent of final rank.
        /// </summary>
        public Dictionary<string, LeaderboardMilestoneDefinition> Milestones = new();

        /// <summary>When milestones may be claimed relative to cycle end.</summary>
        public EventClaimMode MilestoneClaimMode = EventClaimMode.Instant;
    }

    /// <summary>Reward configuration for one rank range at the end of a cycle.</summary>
    [Serializable]
    public class LeaderboardRankReward
    {
        /// <summary>Rank range string. Formats: "1", "2-5", "6-10", "11-50".</summary>
        public string Rank;

        /// <summary>Base rewards for this rank range (grant only). Supports premium tiers.</summary>
        public ResourceGrant Rewards;

        /// <summary>
        /// Optional season-tier scaling applied before ResourceService via SeasonTierRewardResolver.
        /// null = no seasonal scaling; base Rewards is used as-is.
        /// Only effective when LeaderboardDefinition.LinkedSeasonChainID is set.
        /// </summary>
        public SeasonTierRewardSet SeasonTierRewards;
    }

    /// <summary>Milestone threshold within a leaderboard cycle.</summary>
    [Serializable]
    public class LeaderboardMilestoneDefinition
    {
        public string MilestoneID;
        public string DisplayName;

        /// <summary>Asset paths for this milestone (icon, badge). Key = purpose, value = path.</summary>
        public Dictionary<string, string> AssetPaths;

        /// <summary>ScoreEarnedThisCycle threshold that unlocks this milestone.</summary>
        public long RequiredScore;

        /// <summary>Base rewards for this milestone (grant only). Supports premium tiers.</summary>
        public ResourceGrant Rewards;

        /// <summary>
        /// Optional season-tier scaling applied before ResourceService via SeasonTierRewardResolver.
        /// null = no seasonal scaling; base Rewards is used as-is.
        /// Only effective when LeaderboardDefinition.LinkedSeasonChainID is set.
        /// </summary>
        public SeasonTierRewardSet SeasonTierRewards;

        public int SortOrder;

        /// <summary>
        /// Featured milestones may have a stricter claim window (see LeaderboardDefinition.MilestoneClaimMode
        /// with EventClaimMode.FeaturedAfterEnd).
        /// </summary>
        public bool IsFeatured;
    }

    // =====================================================================
    // ENUMS
    // =====================================================================

    /// <summary>How an incoming score is combined with the player's current cycle value.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaderboardScoreAggregation
    {
        /// <summary>Each submit adds to the running total.</summary>
        Sum,

        /// <summary>Only the highest submitted value is kept.</summary>
        BestScore,

        /// <summary>Only the lowest (fastest) value is kept. Ties broken by earlier submission.</summary>
        BestTime,

        /// <summary>Always replaced by the latest submitted value.</summary>
        LastValue,
    }

    /// <summary>How often the leaderboard cycle resets.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaderboardCycleReset
    {
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,

        /// <summary>No automatic reset — all-time leaderboard.</summary>
        Never,
    }

    public enum LeaderboardAction
    {
        GetDefinitions,
        GetLeaderboard,
        GetMyProgress,
        SubmitScore,
        ClaimCycleReward,
        ClaimMilestone,
        GetFriendsLeaderboard,
    }
}
