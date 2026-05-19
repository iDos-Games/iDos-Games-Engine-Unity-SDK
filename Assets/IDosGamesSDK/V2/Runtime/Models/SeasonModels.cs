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
    /// Unified request for all Season actions.
    /// <list type="bullet">
    ///   <item><see cref="SeasonAction.GetActiveSeason"/>, <see cref="SeasonAction.GetUserState"/>,
    ///         <see cref="SeasonAction.GrantStatusTokens"/>, <see cref="SeasonAction.ClaimTierReward"/>:
    ///         require <see cref="SeasonChainID"/>.</item>
    ///   <item><see cref="SeasonAction.GrantStatusTokens"/>: requires <see cref="Amount"/> (&gt; 0).</item>
    ///   <item><see cref="SeasonAction.ClaimTierReward"/>: requires <see cref="TierNumber"/> (&gt; 0).</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class SeasonRequest : BaseRequest
    {
        /// <summary>Season chain ID. Required for most actions.</summary>
        public string SeasonChainID;

        /// <summary>Token amount to grant. Used by GrantStatusTokens; must be &gt; 0.</summary>
        public long? Amount;

        /// <summary>Tier number to claim. Used by ClaimTierReward; must be &gt; 0.</summary>
        public int? TierNumber;
    }

    // =====================================================================
    // RESPONSES
    // =====================================================================

    /// <summary>Full information about the currently active season, including player progress.</summary>
    [Serializable]
    public class ActiveSeasonInfo
    {
        /// <summary>Season chain ID.</summary>
        public string SeasonChainID;

        /// <summary>Current cycle index (0-based).</summary>
        public int CycleIndex;

        /// <summary>Definition of the current season from config.</summary>
        public SeasonDefinition Season;

        /// <summary>Computed start time of the season (UTC).</summary>
        public DateTime ComputedStartUtc;

        /// <summary>Computed end time of the season (UTC).</summary>
        public DateTime ComputedEndUtc;

        /// <summary>Seconds remaining until season end.</summary>
        public long SecondsRemaining;

        /// <summary>Player's current progress in this season.</summary>
        public UserSeasonState UserState;

        /// <summary>Next tier not yet reached. null if the player is at the maximum tier.</summary>
        public SeasonTierDefinition NextTier;
    }

    /// <summary>Response for GrantStatusTokens.</summary>
    [Serializable]
    public class GrantStatusTokensResponse
    {
        public string SeasonChainID;
        public string CurrentSeasonID;

        /// <summary>Number of tokens actually granted.</summary>
        public long AmountGranted;

        /// <summary>New token balance after the grant.</summary>
        public long NewStatusTokens;

        /// <summary>Tier before the grant.</summary>
        public int OldTier;

        /// <summary>Tier after the grant.</summary>
        public int NewTier;

        /// <summary>true if the player reached a new tier as a result of this grant.</summary>
        public bool TierUp;
    }

    /// <summary>Response for ClaimTierReward.</summary>
    [Serializable]
    public class ClaimTierRewardResponse
    {
        public string SeasonChainID;
        public string CurrentSeasonID;

        /// <summary>Claimed tier number.</summary>
        public int TierNumber;

        /// <summary>Resources granted for reaching the tier.</summary>
        public ResourceOperation Resources;
    }

    // =====================================================================
    // USER STATE
    // =====================================================================

    /// <summary>Root container for all season chain states of a player. Stored in UserState.Season.</summary>
    [Serializable]
    public class UserSeasonsState
    {
        /// <summary>Season state per chain. Key = SeasonChainID.</summary>
        public Dictionary<string, UserSeasonState> States = new();
    }

    /// <summary>Player's progress within a single season chain.</summary>
    [Serializable]
    public class UserSeasonState
    {
        /// <summary>Season chain ID this state belongs to.</summary>
        public string SeasonChainID;

        /// <summary>ID of the season the player is currently in.</summary>
        public string CurrentSeasonID;

        /// <summary>
        /// Monotonic counter incremented each time the player enters a new season.
        /// Sub-systems (Collection, Coop) compare their own copy to detect stale data and perform a wipe.
        /// </summary>
        public int SeasonVersion;

        /// <summary>Current tier reached in the meta-progression (1..N).</summary>
        public int CurrentTier = 1;

        /// <summary>
        /// Tier numbers for which the one-time TierReachedReward has already been claimed.
        /// Prevents double-claiming across session boundaries.
        /// </summary>
        public List<int> ClaimedTierRewards = new();

        /// <summary>UTC timestamp when the current season started for this player (analytics).</summary>
        public DateTime SeasonStartedAtUtc;
    }

    // =====================================================================
    // CONFIG — SEASON DEFINITIONS
    // =====================================================================

    /// <summary>
    /// Root season config for a title.
    /// Corresponds to TitlePublicConfigurationModel.Season.
    /// </summary>
    [Serializable]
    public class SeasonDefinitions
    {
        /// <summary>All season chains for the title. Key = SeasonChainID.</summary>
        public Dictionary<string, SeasonChainDefinition> Chains = new();
    }

    /// <summary>
    /// A sequential chain of seasons. Seasons run one after another;
    /// after the last one the cycle restarts from the first (up to MaxCycles).
    /// Dates are not stored explicitly — computed from AnchorUtc + DurationSec.
    /// </summary>
    [Serializable]
    public class SeasonChainDefinition
    {
        /// <summary>Unique chain ID within the title. Example: "main", "special_mode".</summary>
        public string SeasonChainID;

        /// <summary>Display name for UI and tools.</summary>
        public string DisplayName;

        /// <summary>
        /// Anchor point (UTC). The first season of the first cycle starts at this moment.
        /// All other season start/end dates are derived from this.
        /// </summary>
        public DateTime AnchorUtc;

        /// <summary>Whether the chain is currently active. false = no season is considered active.</summary>
        public bool IsActive = true;

        /// <summary>Maximum number of full cycles. 0 = infinite.</summary>
        public int MaxCycles = 0;

        /// <summary>Pause between seasons within a cycle (seconds). Usually 0.</summary>
        public int PauseBetweenSeasonsSec = 0;

        /// <summary>Pause between full cycles (seconds).</summary>
        public int PauseBetweenCyclesSec = 0;

        /// <summary>Ordered list of seasons in the chain. Sort by SeasonDefinition.Order ASC when reading.</summary>
        public List<SeasonDefinition> Seasons = new();

        /// <summary>Who may call GrantStatusTokens for this chain.</summary>
        public SeasonGrantAccessMode GrantTokensAccessMode = SeasonGrantAccessMode.ServerOnly;
    }

    /// <summary>
    /// One season within a chain. Dates are computed server-side from AnchorUtc — not stored here.
    /// </summary>
    [Serializable]
    public class SeasonDefinition
    {
        /// <summary>Unique season ID within the chain. Example: "s1", "spring_2026", "halloween".</summary>
        public string SeasonID;

        /// <summary>Order within the chain (0, 1, 2…). Determines the sequence of seasons.</summary>
        public int Order;

        /// <summary>Season duration in seconds. Default ≈ 50 days.</summary>
        public long DurationSec = 4_320_000;

        public string DisplayName;
        public string Description;

        /// <summary>Asset paths for this season (icon, banner, background). Key = purpose, value = path.</summary>
        public Dictionary<string, string> AssetPaths;

        /// <summary>
        /// Tier definitions for the meta-progression. Sort by RequiredTokens ASC.
        /// Empty list = meta-progression disabled for this season.
        /// </summary>
        public List<SeasonTierDefinition> Tiers = new();

        /// <summary>ID of the Collection active during this season. null = no collection.</summary>
        public string LinkedCollectionID;

        /// <summary>Arbitrary key-value params used by client scripts or server logic.</summary>
        public Dictionary<string, string> CustomParams = new();
    }

    /// <summary>One tier in the seasonal meta-progression (Class System).</summary>
    [Serializable]
    public class SeasonTierDefinition
    {
        /// <summary>Tier number. 1 = base level (available with zero tokens).</summary>
        public int Tier;

        /// <summary>Display name. Example: "Bronze", "Silver", "Gold".</summary>
        public string DisplayName;

        /// <summary>Asset paths for this tier (icon, badge, effect). Key = purpose, value = path.</summary>
        public Dictionary<string, string> AssetPaths;

        /// <summary>Minimum cumulative status tokens required to reach this tier.</summary>
        public long RequiredTokens;

        /// <summary>
        /// One-time reward granted on first reach in a season.
        /// Protected against double-claim via UserSeasonState.ClaimedTierRewards.
        /// </summary>
        public ResourceGrant TierReachedReward;
    }

    // =====================================================================
    // CONFIG — SEASON TIER REWARD SCALING
    // =====================================================================

    /// <summary>
    /// Universal container for season-tier-based reward scaling.
    /// Any system (leaderboards, lootboxes, shop, dailies) may attach this to its reward config
    /// to have the server scale a ResourceGrant by the player's current season tier via
    /// SeasonTierRewardResolver.Resolve before calling ResourceService.
    /// </summary>
    [Serializable]
    public class SeasonTierRewardSet
    {
        /// <summary>
        /// Structurally different rewards for specific tiers.
        /// When a bundle matches the player's tier, it fully replaces the base ResourceGrant.
        /// </summary>
        public List<SeasonTierRewardBundle> Bundles;

        /// <summary>
        /// Amount multipliers for the base ResourceGrant.
        /// Applied only if no Bundle matches.
        /// </summary>
        public List<SeasonTierRewardMultiplier> Multipliers;
    }

    /// <summary>
    /// Structurally different rewards for a season tier. Analogous to PremiumTierBundle.
    /// Selection: one best matching entry (highest MinSeasonTier ≤ playerTier). Does not stack.
    /// </summary>
    [Serializable]
    public class SeasonTierRewardBundle
    {
        /// <summary>Season chain ID this bundle applies to.</summary>
        public string SeasonChainID;

        /// <summary>Minimum player season tier for this bundle to apply.</summary>
        public int MinSeasonTier;

        /// <summary>Rewards for this tier. Passed to ResourceService unmodified.</summary>
        public ResourceGrant Rewards;
    }

    /// <summary>
    /// Amount multiplier applied to the base ResourceGrant for a season tier.
    /// Applied as (long)Math.Round(Amount * Multiplier). Entries that scale to ≤ 0 are dropped.
    /// </summary>
    [Serializable]
    public class SeasonTierRewardMultiplier
    {
        /// <summary>Season chain ID this multiplier applies to.</summary>
        public string SeasonChainID;

        /// <summary>Minimum player season tier for this multiplier to apply.</summary>
        public int MinSeasonTier;

        /// <summary>Multiplier value. 1.0 = neutral (no copy made). Applied as Math.Round(Amount * Multiplier).</summary>
        public double Multiplier = 1.0;
    }

    // =====================================================================
    // ENUMS
    // =====================================================================

    /// <summary>Who may call GrantStatusTokens for a given season chain.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SeasonGrantAccessMode
    {
        /// <summary>Server-side code only (tournament, quest). Recommended for production.</summary>
        ServerOnly,

        /// <summary>Client only.</summary>
        ClientOnly,

        /// <summary>Both client and server.</summary>
        Both,
    }

    /// <summary>Season module actions.</summary>
    public enum SeasonAction
    {
        /// <summary>Get the full season chain config for the title.</summary>
        GetDefinitions,

        /// <summary>Get info about the currently active season and player progress.</summary>
        GetActiveSeason,

        /// <summary>Get the player's UserSeasonState for a chain.</summary>
        GetUserState,

        /// <summary>Grant status tokens (used client-side when GrantTokensAccessMode allows).</summary>
        GrantStatusTokens,

        /// <summary>Claim the one-time reward for reaching a tier.</summary>
        ClaimTierReward,
    }
}
