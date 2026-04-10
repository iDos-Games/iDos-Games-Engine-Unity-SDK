using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum LimitedTimeEventAction
    {
        GetActiveEvents,
        GrantTokens,
        SpendTokens,
        ClaimMilestone,
        ClaimStreakReward,
        GetDefinitions,
        GetUserState,
    }

    public enum EventClaimMode
    {
        Instant,
        AfterEventEnd,
        FeaturedAfterEnd,
    }

    public enum EventTokenSourceType
    {
        BoardTileLanding,
        BoardPassStart,
        BoardAttack,
        BoardRaid,
        BoardBuild,
        BoardStageComplete,
        QuestComplete,
        StorePurchase,
        DailyLogin,
        ReferralInvite,
        MilestoneReward,
        CustomAction,
    }

    public enum BonusWindowScheduleType
    {
        OneTime,
        Recurring,
    }

    public enum EventModifierTarget
    {
        BoardAttackReward,
        BoardRaidReward,
        BoardPassStartReward,
        BoardBuildCost,
        BoardTileLandingReward,
        BoardRollSteps,
        QuestReward,
        Custom,
    }

    public enum ModifierOperation
    {
        Multiply,
        AddPercent,
        // TODO: add other operations matching server enum ModifierOperation
    }

    // =================================================================================
    // REQUEST
    // =================================================================================

    [Serializable]
    public class LimitedTimeEventRequest : IGSRequest
    {
        public string EventID { get; set; }
        public string EventChainID { get; set; }

        // GrantTokens
        public string SourceType { get; set; }
        public long? AmountOverride { get; set; }
        public Dictionary<string, string> SourceParams { get; set; }

        // SpendTokens
        public long? SpendAmount { get; set; }

        // ClaimMilestone
        public string MilestoneID { get; set; }

        // ClaimStreakReward
        public int? StreakDay { get; set; }
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class GetActiveEventsResponse
    {
        public List<ActiveEventInfo> ActiveEvents { get; set; }
    }

    [Serializable]
    public class EventTokenGrantInfo
    {
        public string EventID { get; set; }
        public string EventChainID { get; set; }
        public string TokenID { get; set; }
        public string TokenDisplayName { get; set; }
        public List<string> TokenImagesPath { get; set; }
        public long Amount { get; set; }
        public string Source { get; set; }
        public bool VipBonusApplied { get; set; }
        public bool BonusWindowActive { get; set; }
        public double ProgressiveMultiplier { get; set; }
    }

    [Serializable]
    public class EventMilestoneClaimResponse
    {
        public string EventID { get; set; }
        public string EventChainID { get; set; }
        public string MilestoneID { get; set; }
        public List<ItemOrCurrency> StandardRewards { get; set; }
        public List<ItemOrCurrency> VipRewards { get; set; }
        public List<EventTokenGrantInfo> CrossEventTokenGrants { get; set; }
        public bool IsVip { get; set; }
    }

    [Serializable]
    public class EventTokenSpendResponse
    {
        public string EventID { get; set; }
        public string EventChainID { get; set; }
        public long AmountSpent { get; set; }
        public long NewBalance { get; set; }
    }

    [Serializable]
    public class EventStreakClaimResponse
    {
        public string EventID { get; set; }
        public string EventChainID { get; set; }
        public int StreakDay { get; set; }
        public List<ItemOrCurrency> StandardRewards { get; set; }
        public List<ItemOrCurrency> VipRewards { get; set; }
        public List<EventTokenGrantInfo> CrossEventTokenGrants { get; set; }
        public long BonusTokens { get; set; }
    }

    // =================================================================================
    // USER STATE MODELS
    // =================================================================================

    [Serializable]
    public class UserLimitedTimeEventsState
    {
        public Dictionary<string, UserEventProgress> ScheduledEvents { get; set; }
        public Dictionary<string, UserEventChainProgress> EventChains { get; set; }
    }

    [Serializable]
    public class UserEventProgress
    {
        public string EventID { get; set; }
        public long TokenBalance { get; set; }
        public long TokensEarnedTotal { get; set; }
        public long TokensSpentTotal { get; set; }
        public Dictionary<string, long> DailyEarnedBySource { get; set; }
        public Dictionary<string, int> DailyTriggersBySource { get; set; }
        public DateTime DailyCounterDate { get; set; }
        public long DailyEarnedTotal { get; set; }
        public Dictionary<string, DateTime> LastTriggerBySource { get; set; }
        public List<string> ClaimedMilestoneIDs { get; set; }
        public List<string> UnlockedMilestoneIDs { get; set; }
        public int CurrentStreakDays { get; set; }
        public int MaxStreakDays { get; set; }
        public DateTime LastStreakDate { get; set; }
        public List<int> ClaimedStreakDays { get; set; }
        public int CurrentProgressiveTier { get; set; }
        public DateTime JoinedAtUtc { get; set; }
        public DateTime LastEarnedAtUtc { get; set; }
    }

    [Serializable]
    public class UserEventChainProgress
    {
        public string EventChainID { get; set; }
        public int CurrentCycleIndex { get; set; }
        public int CurrentEventOrder { get; set; }
        public UserEventProgress CurrentEventProgress { get; set; }
        public Dictionary<string, UserEventProgressSnapshot> CurrentCycleHistory { get; set; }
        public UserEventChainLifetimeStats LifetimeStats { get; set; }
        public DateTime JoinedAtUtc { get; set; }
        public DateTime LastUpdatedAtUtc { get; set; }
    }

    [Serializable]
    public class UserEventProgressSnapshot
    {
        public string EventID { get; set; }
        public long TokensEarnedTotal { get; set; }
        public long TokensSpentTotal { get; set; }
        public long TokenBalanceAtEnd { get; set; }
        public List<string> ClaimedMilestoneIDs { get; set; }
        public int MaxStreakDays { get; set; }
        public DateTime FinishedAtUtc { get; set; }
    }

    [Serializable]
    public class UserEventChainLifetimeStats
    {
        public int CompletedCycles { get; set; }
        public int CompletedEvents { get; set; }
        public long TotalTokensEarned { get; set; }
        public long TotalTokensSpent { get; set; }
        public int TotalMilestonesClaimed { get; set; }
        public int BestStreakDays { get; set; }
    }

    // =================================================================================
    // CONFIG MODELS
    // =================================================================================

    [Serializable]
    public class LimitedTimeEventsDefinition
    {
        public Dictionary<string, ScheduledEventDefinition> ScheduledEvents { get; set; }
        public Dictionary<string, EventChainDefinition> EventChains { get; set; }
        public LimitedTimeEventsGlobalSettings Settings { get; set; }
    }

    [Serializable]
    public class LimitedTimeEventsGlobalSettings
    {
        public int MaxConcurrentEvents { get; set; }
        public int CleanupAfterDays { get; set; }
        public int GracePeriodHours { get; set; }
    }

    [Serializable]
    public class ScheduledEventDefinition
    {
        public string EventID { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public EventContent Content { get; set; }
        public bool AllowEarningAfterEnd { get; set; }
        public int ClaimGraceHours { get; set; }
        public EventAccessRules Access { get; set; }
        public Dictionary<string, string> CustomParams { get; set; }
    }

    [Serializable]
    public class EventChainDefinition
    {
        public string EventChainID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> AssetPaths { get; set; }
        public DateTime AnchorUtc { get; set; }
        public bool IsActive { get; set; }
        public int MaxCycles { get; set; }
        public List<ChainedEventDefinition> Events { get; set; }
        public int PauseBetweenEventsSec { get; set; }
        public int PauseBetweenCyclesSec { get; set; }
        public EventAccessRules Access { get; set; }
        public Dictionary<string, string> CustomParams { get; set; }
    }

    [Serializable]
    public class ChainedEventDefinition
    {
        public string EventID { get; set; }
        public int Order { get; set; }
        public long DurationSec { get; set; }
        public EventContent Content { get; set; }
        public int ClaimGraceHours { get; set; }
        public EventAccessRules AccessOverride { get; set; }
        public Dictionary<string, string> CustomParams { get; set; }
    }

    [Serializable]
    public class EventContent
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> AssetPaths { get; set; }
        public string Category { get; set; }
        public EventTokenDefinition Token { get; set; }
        public List<EventTokenSource> TokenSources { get; set; }
        public EventClaimMode ClaimMode { get; set; }
        public List<EventMilestoneDefinition> Milestones { get; set; }
        public List<EventRankReward> RankRewards { get; set; }
        public List<EventBonusWindow> BonusWindows { get; set; }
        public List<EventProgressiveMultiplier> ProgressiveMultipliers { get; set; }
        public EventStreakDefinition Streak { get; set; }
        public List<EventModifier> Modifiers { get; set; }
        public string LinkedQuestCycleID { get; set; }
        public string LinkedStoreID { get; set; }
        public string LeaderboardStatisticName { get; set; }
        public EventThemeOverride Theme { get; set; }
        public EventNotificationSettings Notifications { get; set; }
    }

    [Serializable]
    public class EventTokenDefinition
    {
        public string TokenID { get; set; }
        public string DisplayName { get; set; }
        public List<string> AssetPaths { get; set; }
        public long MaxBalance { get; set; }
        public long MaxPerGrant { get; set; }
        public long DailyEarnCap { get; set; }
        public bool BurnOnEventEnd { get; set; }
        public EventTokenConversion BurnConversion { get; set; }
    }

    [Serializable]
    public class EventTokenConversion
    {
        public string TargetCurrencyID { get; set; }
        public double Rate { get; set; }
        public long MaxConvertAmount { get; set; }
    }

    [Serializable]
    public class EventTokenSource
    {
        public EventTokenSourceType SourceType { get; set; }
        public long BaseAmount { get; set; }
        public double VipMultiplier { get; set; }
        public int MinPremiumTierForBonus { get; set; }
        public long DailyCapFromSource { get; set; }
        public int DailyTriggerCap { get; set; }
        public int CooldownSeconds { get; set; }
        public Dictionary<string, string> Params { get; set; }
    }

    [Serializable]
    public class EventBonusWindow
    {
        public string WindowID { get; set; }
        public string DisplayName { get; set; }
        public List<string> AssetPaths { get; set; }
        public double Multiplier { get; set; }
        public BonusWindowScheduleType ScheduleType { get; set; }
        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
        public int RecurringStartHourUtc { get; set; }
        public int RecurringStartMinuteUtc { get; set; }
        public int RecurringDurationMinutes { get; set; }
        public List<int> RecurringDaysOfWeek { get; set; }
        public List<EventTokenSourceType> AffectedSources { get; set; }
    }

    [Serializable]
    public class EventProgressiveMultiplier
    {
        public long MinTokensEarned { get; set; }
        public double Multiplier { get; set; }
        public string TierName { get; set; }
        public List<string> AssetPaths { get; set; }
    }

    [Serializable]
    public class EventStreakDefinition
    {
        public long MinTokensPerDay { get; set; }
        public bool ResetOnMiss { get; set; }
        public List<EventStreakReward> Rewards { get; set; }
    }

    [Serializable]
    public class EventStreakReward
    {
        public int RequiredStreakDays { get; set; }
        public string DisplayName { get; set; }
        public List<string> AssetPaths { get; set; }
        public List<ItemOrCurrency> Rewards { get; set; }
        public List<EventVipReward> VipRewards { get; set; }
        public List<EventTokenReward> TokenRewards { get; set; }
        public long BonusTokens { get; set; }
    }

    [Serializable]
    public class EventModifier
    {
        public string ModifierID { get; set; }
        public string DisplayName { get; set; }
        public List<string> AssetPaths { get; set; }
        public EventModifierTarget Target { get; set; }
        public ModifierOperation Operation { get; set; }
        public double Value { get; set; }
        public int MinPremiumTier { get; set; }
    }

    [Serializable]
    public class EventThemeOverride
    {
        public string ThemeID { get; set; }
        public string BoardAssetID { get; set; }
        public List<string> AssetPaths { get; set; }
        public Dictionary<string, string> ColorPalette { get; set; }
        public string AudioAssetID { get; set; }
        public Dictionary<string, string> CustomParams { get; set; }
    }

    [Serializable]
    public class EventNotificationSettings
    {
        public bool NotifyOnStart { get; set; }
        public int NotifyBeforeEndMinutes { get; set; }
        public bool NotifyOnBonusWindow { get; set; }
        public bool NotifyOnMilestoneReached { get; set; }
        public bool NotifyUnclaimedRewards { get; set; }
        public bool NotifyStreakAtRisk { get; set; }
        public Dictionary<string, string> CustomMessages { get; set; }
    }

    [Serializable]
    public class EventMilestoneDefinition
    {
        public string MilestoneID { get; set; }
        public string DisplayName { get; set; }
        public List<string> AssetPaths { get; set; }
        public long RequiredTokensEarned { get; set; }
        public List<ItemOrCurrency> Rewards { get; set; }
        public List<EventVipReward> VipRewards { get; set; }
        public int SortOrder { get; set; }
        public bool IsFeatured { get; set; }
    }

    [Serializable]
    public class EventVipReward
    {
        public int MinPremiumTier { get; set; }
        public string RequiredPremiumID { get; set; }
        public string Label { get; set; }
        public List<ItemOrCurrency> Rewards { get; set; }
    }

    [Serializable]
    public class EventTokenReward
    {
        public string EventID { get; set; }
        public string EventChainID { get; set; }
        public long Amount { get; set; }
    }

    [Serializable]
    public class EventRankReward
    {
        public string RankRange { get; set; }
        public List<ItemOrCurrency> Rewards { get; set; }
        public List<EventVipReward> VipRewards { get; set; }
    }

    [Serializable]
    public class EventAccessRules
    {
        public int MinPlayerLevel { get; set; }
        public int MinBoardStage { get; set; }
        public int RequiredPremiumTier { get; set; }
        public List<string> RequiredFlags { get; set; }
        public List<string> AllowedCountries { get; set; }
        public List<string> BlockedCountries { get; set; }
        public int MinAccountAgeDays { get; set; }
    }

    // =================================================================================
    // GetActiveEvents response model
    // =================================================================================

    [Serializable]
    public class ActiveEventInfo
    {
        public ActiveEventType EventType { get; set; }
        public string ID { get; set; }
        public string CurrentChainedEventID { get; set; }
        public EventContent Content { get; set; }
        public UserEventProgress Progress { get; set; }
        public DateTime ComputedStartUtc { get; set; }
        public DateTime ComputedEndUtc { get; set; }
        public bool CanEarn { get; set; }
        public bool CanClaim { get; set; }
        public EventMilestoneDefinition NextMilestone { get; set; }
        public double CurrentProgressiveMultiplier { get; set; }
        public string CurrentProgressiveTierName { get; set; }
        public bool BonusWindowActive { get; set; }
        public double BonusWindowMultiplier { get; set; }
        public DateTime? BonusWindowEndUtc { get; set; }
        public DateTime? NextBonusWindowStartUtc { get; set; }
        public List<EventModifier> ActiveModifiers { get; set; }
        public EventThemeOverride Theme { get; set; }
        public int? CurrentCycleIndex { get; set; }
        public int? CurrentEventOrder { get; set; }
        public int? TotalEventsInChain { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActiveEventType
    {
        Scheduled,
        Chained,
    }
}
