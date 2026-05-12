using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Request for all RewardV2 actions. Fill only the fields relevant to the chosen action:
    /// <list type="bullet">
    ///   <item><see cref="RewardAction.ClaimDailyReward"/>   — <see cref="CalendarID"/> (optional, null = default).</item>
    ///   <item><see cref="RewardAction.CollectIdleAccrual"/> — <see cref="AccrualID"/> (required).</item>
    ///   <item><see cref="RewardAction.ClaimComebackReward"/>— <see cref="ComebackID"/> (required).</item>
    ///   <item><see cref="RewardAction.ClaimReward"/>         — <see cref="ClaimID"/> (required).</item>
    ///   <item><see cref="RewardAction.GetRewardDefinitions"/>, <see cref="RewardAction.GetUserRewardsState"/> — no extra fields needed.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class RewardRequest : BaseRequest
    {
        /// <summary>Calendar ID for <see cref="RewardAction.ClaimDailyReward"/>. null/empty = server picks the default.</summary>
        public string CalendarID;

        /// <summary>Accrual ID for <see cref="RewardAction.CollectIdleAccrual"/>. Required.</summary>
        public string AccrualID;

        /// <summary>Comeback config ID for <see cref="RewardAction.ClaimComebackReward"/>. Required.</summary>
        public string ComebackID;

        /// <summary>Claim reward ID for <see cref="RewardAction.ClaimReward"/>. Required.</summary>
        public string ClaimID;
    }

    // ===================== Response models =====================

    /// <summary>Response for <see cref="RewardAction.GetRewardDefinitions"/>.</summary>
    [Serializable]
    public class RewardDefinitionsResponse
    {
        /// <summary>Full reward config umbrella (daily calendars, idle accruals, comebacks, claims).</summary>
        public RewardDefinitions RewardDefinitions;
    }

    /// <summary>Response for <see cref="RewardAction.GetUserRewardsState"/>.</summary>
    [Serializable]
    public class UserRewardsStateResponse
    {
        /// <summary>
        /// Player's state across all reward subsystems.
        /// Comeback states may have been presence-ticked (LastSeenAt updated) on this read.
        /// </summary>
        public UserRewardState Rewards = new UserRewardState();
    }

    /// <summary>Response for a successful <see cref="RewardAction.ClaimDailyReward"/>.</summary>
    [Serializable]
    public class ClaimDailyRewardResponse
    {
        public DateTime ServerTimeUtc;

        /// <summary>ID of the calendar that was claimed.</summary>
        public string CalendarID;

        /// <summary>Day number for which rewards were granted.</summary>
        public int DayNumber;

        /// <summary>Updated calendar state after the claim.</summary>
        public UserDailyCalendarState DailyState;

        /// <summary>Resource changes resulting from this claim.</summary>
        public ResourceOperation Resources = new ResourceOperation();
    }

    /// <summary>Response for a successful <see cref="RewardAction.CollectIdleAccrual"/>.</summary>
    [Serializable]
    public class CollectIdleAccrualResponse
    {
        public DateTime ServerTimeUtc;

        /// <summary>ID of the accrual that was collected.</summary>
        public string AccrualID;

        /// <summary>How many seconds of accumulation were applied (after cap).</summary>
        public long AccruedSeconds;

        /// <summary>The final rate per second that was used for the payout calculation.</summary>
        public double AppliedRatePerSecond;

        /// <summary>Updated accrual state after the collect.</summary>
        public UserIdleAccrualState IdleState;

        /// <summary>Resource changes resulting from this collect.</summary>
        public ResourceOperation Resources = new ResourceOperation();
    }

    /// <summary>Response for a successful <see cref="RewardAction.ClaimComebackReward"/>.</summary>
    [Serializable]
    public class ClaimComebackRewardResponse
    {
        public DateTime ServerTimeUtc;

        /// <summary>ID of the comeback config that was claimed.</summary>
        public string ComebackID;

        /// <summary>Index of the tier that was granted (mirrors <see cref="UserComebackState.LastClaimedTierIndex"/>).</summary>
        public int ClaimedTierIndex;

        /// <summary>Updated comeback state after the claim.</summary>
        public UserComebackState ComebackState;

        /// <summary>Resource changes resulting from this claim.</summary>
        public ResourceOperation Resources = new ResourceOperation();
    }

    /// <summary>Response for a successful <see cref="RewardAction.ClaimReward"/>.</summary>
    [Serializable]
    public class ClaimRewardResponse
    {
        public DateTime ServerTimeUtc;

        /// <summary>ID of the reward that was claimed.</summary>
        public string ClaimID;

        /// <summary>Updated claim state after the claim.</summary>
        public UserClaimRewardState ClaimState;

        /// <summary>Resource changes resulting from this claim.</summary>
        public ResourceOperation Resources = new ResourceOperation();
    }

    // ===================== State models =====================

    /// <summary>
    /// Player's state across all reward subsystems. Stored in UserDataDocument.Reward.
    /// All dictionaries may be null/empty — valid state meaning the player has not yet interacted.
    /// </summary>
    [Serializable]
    public class UserRewardState
    {
        /// <summary>Progress per daily login calendar. Key = CalendarID.</summary>
        public Dictionary<string, UserDailyCalendarState> DailyCalendars = new Dictionary<string, UserDailyCalendarState>();

        /// <summary>State per idle accrual. Key = AccrualID.</summary>
        public Dictionary<string, UserIdleAccrualState> IdleAccruals = new Dictionary<string, UserIdleAccrualState>();

        /// <summary>Comeback tracking state. Key = ComebackID.</summary>
        public Dictionary<string, UserComebackState> Comebacks = new Dictionary<string, UserComebackState>();

        /// <summary>Claim reward counters. Key = ClaimID.</summary>
        public Dictionary<string, UserClaimRewardState> Claims = new Dictionary<string, UserClaimRewardState>();
    }

    /// <summary>Player's progress in one daily login calendar.</summary>
    [Serializable]
    public class UserDailyCalendarState
    {
        /// <summary>Mirror of the dictionary key. Identifies which calendar this state belongs to.</summary>
        public string CalendarID;

        /// <summary>
        /// How many days the player has collected in the current calendar run.
        /// Next expected day = CollectedDays + 1 (with looping if IsLooping is true).
        /// May be rolled back by MissBehavior settings on the server.
        /// </summary>
        public int CollectedDays;

        /// <summary>UTC timestamp of the last successful claim. MinValue = never claimed.</summary>
        public DateTime LastClaimAt = DateTime.MinValue;
    }

    /// <summary>
    /// State of one idle accrual for the player. The actual accumulated amount is NOT stored —
    /// it is computed at claim time as min(now - LastCollectAt, MaxAccumulationSeconds) × finalRate.
    /// </summary>
    [Serializable]
    public class UserIdleAccrualState
    {
        /// <summary>Mirror of the dictionary key.</summary>
        public string AccrualID;

        /// <summary>UTC timestamp of the last successful collect. MinValue = never collected.</summary>
        public DateTime LastCollectAt = DateTime.MinValue;

        /// <summary>Denormalized: last payout amount. Useful for UI ("last time you earned X").</summary>
        public long LastClaimedAmount;

        /// <summary>Denormalized: final rate per second used in the last collect. Useful for UI.</summary>
        public double LastClaimedRate;
    }

    /// <summary>
    /// Comeback reward state for the player.
    /// Split into presence-tracking fields (LastSeenAt) and pending-reward fields (PendingReturnedAt / PendingTierIndex).
    /// </summary>
    [Serializable]
    public class UserComebackState
    {
        /// <summary>Mirror of the dictionary key.</summary>
        public string ComebackID;

        /// <summary>Last time the player was seen (any API call). MinValue = first access (will be set to now).</summary>
        public DateTime LastSeenAt = DateTime.MinValue;

        /// <summary>Last successful claim time. MinValue = never claimed.</summary>
        public DateTime LastClaimAt = DateTime.MinValue;

        /// <summary>Index of the last claimed tier in ComebackRewardDefinition.Tiers. -1 = never claimed.</summary>
        public int LastClaimedTierIndex = -1;

        /// <summary>
        /// When the current pending comeback was detected. null = no pending reward.
        /// Used as the idempotency anchor for the claim call.
        /// </summary>
        public DateTime? PendingReturnedAt;

        /// <summary>
        /// Index of the pending tier in ComebackRewardDefinition.Tiers. null = no pending reward.
        /// Locked at detection time, not at claim time — prevents the player from stalling for a better tier.
        /// </summary>
        public int? PendingTierIndex;
    }

    /// <summary>
    /// Player's state for one simple claim reward. Tracks all limit counters.
    /// </summary>
    [Serializable]
    public class UserClaimRewardState
    {
        /// <summary>Mirror of the dictionary key.</summary>
        public string ClaimID;

        /// <summary>Total number of times this reward has been claimed. Monotonically increasing.</summary>
        public int TotalClaims;

        /// <summary>
        /// Recent claim timestamps (UTC, ascending) used for rolling-window enforcement.
        /// The server trims entries older than (now - WindowSeconds) before each check.
        /// null/empty = no claims within the current window.
        /// </summary>
        public List<DateTime> RecentClaimTimestamps;

        /// <summary>UTC timestamp of the last successful claim. MinValue = never claimed.</summary>
        public DateTime LastClaimAt = DateTime.MinValue;
    }

    // ===================== Config models =====================

    /// <summary>
    /// Root reward config umbrella. Stored in TitlePublicConfigurationModel.Reward.
    /// Each subsystem is independent; any can be null/empty if unused.
    /// </summary>
    [Serializable]
    public class RewardDefinitions
    {
        /// <summary>Global tier reward settings (mode, stacking). null = Additive / NoStack defaults.</summary>
        public TierRewardSettings TierRewards = new TierRewardSettings();

        /// <summary>Daily login calendars. Key = CalendarID.</summary>
        public Dictionary<string, DailyCalendarDefinition> DailyCalendars;

        /// <summary>Idle accrual configs. Key = AccrualID.</summary>
        public Dictionary<string, IdleAccrualDefinition> IdleAccruals;

        /// <summary>Comeback reward configs. Key = ComebackID.</summary>
        public Dictionary<string, ComebackRewardDefinition> Comebacks;

        /// <summary>Simple claim reward configs. Key = ClaimID.</summary>
        public Dictionary<string, ClaimRewardDefinition> Claims;
    }

    /// <summary>Global settings for how tier rewards are resolved across the title.</summary>
    [Serializable]
    public class TierRewardSettings
    {
        /// <summary>Additive: tier rewards are added on top of the base. Replace: tier rewards replace the base.</summary>
        public RewardMode RewardMode = RewardMode.Additive;

        /// <summary>If true, all eligible tiers below the player's max are stacked. If false, only the best tier is used.</summary>
        public bool RewardStackLowerTiers = false;
    }

    /// <summary>Reward mode: how premium tier rewards combine with the base reward.</summary>
    public enum RewardMode
    {
        Additive,
        Replace,
    }

    /// <summary>Config for one daily login calendar.</summary>
    [Serializable]
    public class DailyCalendarDefinition
    {
        public string CalendarID;
        public string DisplayName;
        public string Description;
        public Dictionary<string, string> AssetPaths;

        /// <summary>Ordered list of reward days. Each entry has a unique DayNumber starting from 1.</summary>
        public List<DailyRewardDay> Days = new List<DailyRewardDay>();

        /// <summary>If true, restarts from day 1 after the last day is claimed.</summary>
        public bool IsLooping = true;

        /// <summary>What happens to progress when the player misses more than MissThresholdMultiplier claim windows.</summary>
        public DailyMissBehavior MissBehavior = DailyMissBehavior.Forgiving;

        /// <summary>Used only with MissBehavior = ResetBy. Days to roll back CollectedDays.</summary>
        public int ResetByDays;

        /// <summary>
        /// Threshold multiplier for triggering MissBehavior.
        /// CalendarDayUtc: triggers if (today - LastClaimAt.Date).Days > this value.
        /// SlidingWindow: triggers if elapsed seconds > ClaimCooldownSeconds * this value.
        /// Default 2.0.
        /// </summary>
        public double MissThresholdMultiplier = 2.0;

        /// <summary>How the server determines whether a new claim is available.</summary>
        public DailyClaimMode ClaimMode = DailyClaimMode.CalendarDayUtc;

        /// <summary>Used only with ClaimMode = SlidingWindow. Minimum seconds between claims.</summary>
        public int ClaimCooldownSeconds;

        /// <summary>Minimum premium tier required to access this calendar. 0 = available to all.</summary>
        public int RequiredPremiumTier;

        /// <summary>Specific premium subscription ID required. null = any subscription with RequiredPremiumTier.</summary>
        public string RequiredPremiumID;

        public DateTime? AvailableFromUtc;
        public DateTime? AvailableUntilUtc;
    }

    /// <summary>Config for one day within a daily login calendar.</summary>
    [Serializable]
    public class DailyRewardDay
    {
        /// <summary>Day number (1-based, unique within the calendar).</summary>
        public int DayNumber;

        /// <summary>Rewards granted for this day. Supports full premium logic (PremiumBonuses / PremiumTiers).</summary>
        public ResourceGrant Rewards;

        /// <summary>UI hint: mark this day as a milestone (e.g. day 7 / 14 / 30). No server-side effect.</summary>
        public bool IsMilestone;

        /// <summary>Optional per-day assets for the client (icon, artwork). Keys defined by the designer.</summary>
        public Dictionary<string, string> AssetPaths;
    }

    /// <summary>Behavior when the player misses more than MissThresholdMultiplier claim windows.</summary>
    public enum DailyMissBehavior
    {
        /// <summary>Progress is not affected. The next claim continues from where it left off.</summary>
        Forgiving,

        /// <summary>CollectedDays resets to 0 (strict streak calendar).</summary>
        ResetToStart,

        /// <summary>CollectedDays is reduced by DailyCalendarDefinition.ResetByDays (partial penalty).</summary>
        ResetBy,
    }

    /// <summary>Determines what constitutes an available new claim for a daily calendar.</summary>
    public enum DailyClaimMode
    {
        /// <summary>A new claim is available on a new UTC calendar date (LastClaimAt.Date &lt; today).</summary>
        CalendarDayUtc,

        /// <summary>A new claim is available after ClaimCooldownSeconds have elapsed since the last claim.</summary>
        SlidingWindow,
    }

    // ----- Idle Accruals -----

    /// <summary>Config for one idle (passive income) accrual.</summary>
    [Serializable]
    public class IdleAccrualDefinition
    {
        public string AccrualID;
        public string DisplayName;
        public string Description;
        public Dictionary<string, string> AssetPaths;

        /// <summary>
        /// Rate composition. Final rate = (Base + Power*coeff + NetWorth*coeff + equipBonus) * bestPremiumMultiplier.
        /// </summary>
        public IdleRateConfig Rate;

        /// <summary>
        /// Base reward amounts. Each Amount in Standard.Entries and Standard.EventTokens is
        /// multiplied by (accruedSeconds * finalRate) at collect time.
        /// PremiumBonuses / PremiumTiers are passed through to ResourceService as-is.
        /// </summary>
        public ResourceGrant Rewards;

        /// <summary>Maximum seconds that can accumulate before the cap kicks in. 0 = no cap.</summary>
        public long MaxAccumulationSeconds;

        /// <summary>Minimum seconds between two successive collects. 0 = no minimum.</summary>
        public int MinClaimSeconds;

        /// <summary>All conditions must be met at collect time. null = no requirements.</summary>
        public IdleAccrualRequirements Requirements;

        /// <summary>Behavior on the player's very first collect (LastCollectAt == MinValue).</summary>
        public IdleFirstClaimMode FirstClaimMode = IdleFirstClaimMode.EmptyOnFirstClaim;

        /// <summary>
        /// Accrual is unavailable before this UTC time.
        /// Also used as the start of accumulation for AccruedFromConfigStart mode.
        /// null = no lower bound.
        /// </summary>
        public DateTime? AvailableFromUtc;

        public DateTime? AvailableUntilUtc;
    }

    /// <summary>Composition formula for an idle accrual rate.</summary>
    [Serializable]
    public class IdleRateConfig
    {
        /// <summary>Constant base rate per second applied to all players.</summary>
        public double BaseRatePerSecond;

        /// <summary>Additive coefficient multiplied by the player's Power stat. 0 = disabled.</summary>
        public double PowerCoefficient;

        /// <summary>Additive coefficient multiplied by the player's NetWorth. 0 = disabled.</summary>
        public double NetWorthCoefficient;

        /// <summary>Whether to sum IdleRateBonus from all items equipped on EquipmentCharacterID.</summary>
        public bool EquipmentBonusEnabled;

        /// <summary>Character to read equipment bonuses from. null/empty = DefaultData.Main.</summary>
        public string EquipmentCharacterID;

        /// <summary>
        /// Premium rate multipliers. Best matching tier is applied (no stacking).
        /// If no tier matches, effective multiplier is 1.0.
        /// </summary>
        public List<PremiumTierMultiplier> PremiumMultipliers;
    }

    /// <summary>Activation requirements for an idle accrual. All non-zero conditions use logical AND.</summary>
    [Serializable]
    public class IdleAccrualRequirements
    {
        /// <summary>Minimum level of RequirementsCharacterID. 0 = not checked.</summary>
        public int MinCharacterLevel;

        /// <summary>Which character's level and equipment to check. null/empty = DefaultData.Main.</summary>
        public string RequirementsCharacterID;

        /// <summary>Minimum premium tier. 0 = not checked.</summary>
        public int RequiredPremiumTier;

        /// <summary>Specific premium ID required. null = any subscription at RequiredPremiumTier.</summary>
        public string RequiredPremiumID;

        /// <summary>Item IDs that must exist in the player's inventory (TotalAmount > 0). null/empty = not checked.</summary>
        public List<string> RequiredItemIDs;

        /// <summary>Item IDs that must be equipped on RequirementsCharacterID. null/empty = not checked.</summary>
        public List<string> RequiredEquippedItemIDs;
    }

    /// <summary>Behavior on the player's first access to an idle accrual.</summary>
    public enum IdleFirstClaimMode
    {
        /// <summary>First collect pays out 0 and initializes LastCollectAt = now. Accumulation starts from this point.</summary>
        EmptyOnFirstClaim,

        /// <summary>On any first read, LastCollectAt is initialized to now so the player accumulates from first sight.</summary>
        InitOnFirstAccess,

        /// <summary>Accumulation is retroactively counted from AvailableFromUtc (or now if null).</summary>
        AccruedFromConfigStart,
    }

    // ----- Comeback Rewards -----

    /// <summary>Config for one comeback reward (for returning after a long absence).</summary>
    [Serializable]
    public class ComebackRewardDefinition
    {
        public string ComebackID;
        public string DisplayName;
        public string Description;
        public Dictionary<string, string> AssetPaths;

        /// <summary>
        /// Reward tiers by absence duration. The best matching tier
        /// (highest MinAbsenceSeconds that is still &lt;= actual absence) is locked in at detection time.
        /// </summary>
        public List<ComebackTier> Tiers = new List<ComebackTier>();

        /// <summary>Minimum seconds between two comeback claims. 0 = no cooldown.</summary>
        public long ClaimCooldownSeconds;

        /// <summary>
        /// How long the pending reward stays claimable (seconds from PendingReturnedAt).
        /// 0 = stays indefinitely until claimed.
        /// </summary>
        public long ClaimWindowSeconds;

        /// <summary>If true, presence is tracked (LastSeenAt updated) on read-only GetUserRewardsState calls.</summary>
        public bool TrackPresenceOnRead = true;

        /// <summary>Minimum premium tier for access. 0 = available to all.</summary>
        public int RequiredPremiumTier;

        /// <summary>Specific premium ID required. null = any subscription at RequiredPremiumTier.</summary>
        public string RequiredPremiumID;

        public DateTime? AvailableFromUtc;
        public DateTime? AvailableUntilUtc;
    }

    /// <summary>One tier in a comeback reward, keyed by minimum absence duration.</summary>
    [Serializable]
    public class ComebackTier
    {
        /// <summary>Minimum seconds of absence required to qualify for this tier.</summary>
        public long MinAbsenceSeconds;

        /// <summary>Reward for this tier. Supports full premium logic.</summary>
        public ResourceGrant Rewards;

        /// <summary>Optional per-tier assets (e.g. artwork for "one week / one month" variants).</summary>
        public Dictionary<string, string> AssetPaths;
    }

    // ----- Claim Rewards -----

    /// <summary>Config for one simple claim reward with configurable limits.</summary>
    [Serializable]
    public class ClaimRewardDefinition
    {
        public string ClaimID;
        public string DisplayName;
        public string Description;
        public Dictionary<string, string> AssetPaths;

        /// <summary>Manual = player can claim via client API. Auto = server-only trigger.</summary>
        public ClaimRewardMode Mode = ClaimRewardMode.Manual;

        /// <summary>What to grant. Supports full premium logic (PremiumBonuses / PremiumTiers).</summary>
        public ResourceGrant Rewards;

        /// <summary>Minimum seconds between two successive claims. 0 = no cooldown.</summary>
        public int CooldownSeconds;

        /// <summary>Maximum claims allowed within the rolling WindowSeconds window. 0 = no window limit.</summary>
        public int MaxClaimsPerWindow;

        /// <summary>Duration of the rolling window in seconds. Used only when MaxClaimsPerWindow > 0.</summary>
        public int WindowSeconds;

        /// <summary>Total lifetime claim limit. 1 = one-shot reward. 0 = unlimited.</summary>
        public int TotalClaimLimit;

        /// <summary>
        /// Per-premium-tier limit overrides. Best matching tier is applied (no stacking).
        /// Fields left null in an override keep the base definition value.
        /// </summary>
        public List<ClaimLimitOverride> PremiumLimitOverrides;

        /// <summary>Minimum premium tier for access. 0 = available to all.</summary>
        public int RequiredPremiumTier;

        /// <summary>Specific premium ID required. null = any subscription at RequiredPremiumTier.</summary>
        public string RequiredPremiumID;

        public DateTime? AvailableFromUtc;
        public DateTime? AvailableUntilUtc;
    }

    /// <summary>
    /// Per-tier override for one or more limit fields of a ClaimRewardDefinition.
    /// Best matching tier is applied; overrides do NOT stack.
    /// A null field means "keep the base definition value"; 0 means "no limit".
    /// </summary>
    [Serializable]
    public class ClaimLimitOverride
    {
        /// <summary>Minimum tier to which this override applies.</summary>
        public int MinPremiumTier;

        /// <summary>Specific premium ID. null = any subscription at MinPremiumTier.</summary>
        public string RequiredPremiumID;

        public int? CooldownSeconds;
        public int? MaxClaimsPerWindow;
        public int? WindowSeconds;
        public int? TotalClaimLimit;
    }

    /// <summary>Who initiates the claim.</summary>
    public enum ClaimRewardMode
    {
        /// <summary>Player claims via client API.</summary>
        Manual,

        /// <summary>Server-only (background job, GM grant, compensation). Client API will reject.</summary>
        Auto,
    }

    /// <summary>Actions available in the Reward module.</summary>
    public enum RewardAction
    {
        GetRewardDefinitions,
        GetUserRewardsState,
        ClaimDailyReward,
        CollectIdleAccrual,
        ClaimComebackReward,
        ClaimReward,
    }
}
