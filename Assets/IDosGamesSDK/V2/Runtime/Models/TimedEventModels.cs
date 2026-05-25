using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames
{
    // =====================================================================
    // REQUEST
    // =====================================================================

    /// <summary>
    /// Unified request for all TimedEvent actions.
    /// </summary>
    /// <remarks>
    /// Field usage by action:
    ///   GetActiveEvents   — no extra fields required.
    ///   GetDefinitions    — no extra fields required.
    ///   GetUserLteState   — no extra fields required.
    ///   GrantTokens       — Type, LteID, SourceType (required); AmountOverride, SourceParams, Outcome, RollMultiplier (optional).
    ///   SpendTokens       — Type, LteID, SpendAmount (required).
    ///   ClaimMilestone    — Type, LteID, MilestoneID (required).
    /// </remarks>
    [Serializable]
    public class TimedEventRequest : BaseRequest
    {
        /// <summary>LTE event type: Scheduled or Chain.</summary>
        public TimedEventType Type { get; set; }

        /// <summary>
        /// LTE event ID: ScheduledEventDefinition.TimedEventID when Type = Scheduled,
        /// or EventChainDefinition.TimedEventID when Type = Chain.
        /// </summary>
        public string LteID { get; set; }

        // ─── GrantTokens ───

        /// <summary>Token source type string (EventTokenSourceType). Required for GrantTokens.</summary>
        public string SourceType { get; set; }

        /// <summary>Override for BaseAmount. null = use BaseAmount from config.</summary>
        public long? AmountOverride { get; set; }

        /// <summary>Source parameters (ActionName for CustomAction, etc.).</summary>
        public Dictionary<string, string> SourceParams { get; set; }

        // ─── SpendTokens ───

        /// <summary>Number of tokens to spend. Required for SpendTokens.</summary>
        public long? SpendAmount { get; set; }

        // ─── ClaimMilestone ───

        /// <summary>Milestone ID to claim. Required for ClaimMilestone.</summary>
        public string MilestoneID { get; set; }

        /// <summary>
        /// Action outcome or tile type string. Passed from server-side game loop calls.
        /// BoardTileLanding: BoardTileType.ToString(); BoardAttack: "HIT"/"BLOCKED"; etc.
        /// </summary>
        public string Outcome { get; set; }

        /// <summary>
        /// Player's roll multiplier at the time of the action.
        /// Used when EventTokenSource.ScaleWithRollMultiplier = true.
        /// </summary>
        public int RollMultiplier { get; set; } = 1;
    }

    // =====================================================================
    // RESPONSES
    // =====================================================================

    /// <summary>Response for GetActiveEvents: list of currently active events with computed windows.</summary>
    [Serializable]
    public class GetActiveEventsResponse
    {
        public List<ActiveEventInfo> ActiveEvents { get; set; } = new();
    }

    /// <summary>Response for GetUserLteState: player's token progress split by Scheduled and Chain.</summary>
    [Serializable]
    public class UserTimedEventStateResponse
    {
        /// <summary>Progress keyed by ScheduledEventDefinition.TimedEventID.</summary>
        public Dictionary<string, UserEventTokenProgress> Scheduled { get; set; } = new();

        /// <summary>Progress keyed by EventChainDefinition.TimedEventID.</summary>
        public Dictionary<string, UserEventTokenProgress> Chain { get; set; } = new();
    }

    /// <summary>Response for SpendTokens.</summary>
    [Serializable]
    public class EventTokenSpendResponse
    {
        /// <summary>Type of the LTE event against which tokens were spent.</summary>
        public TimedEventType Type { get; set; }

        /// <summary>ID of the LTE event (Scheduled or Chain, depending on Type).</summary>
        public string LteID { get; set; }

        /// <summary>
        /// Full resource operation: Consume.Standard.EventTokens contains the applied spend
        /// with Requested/Applied/NewBalance. Use this as the source of truth on the client.
        /// </summary>
        public ResourceOperation Resources { get; set; } = new();
    }

    /// <summary>Response for ClaimMilestone.</summary>
    [Serializable]
    public class EventMilestoneClaimResponse
    {
        /// <summary>Type of the LTE event in which the milestone was claimed.</summary>
        public TimedEventType Type { get; set; }

        /// <summary>ID of the LTE event (Scheduled or Chain, depending on Type).</summary>
        public string LteID { get; set; }

        /// <summary>ID of the claimed milestone.</summary>
        public string MilestoneID { get; set; }

        /// <summary>
        /// Full resource operation: Grant contains Items and EventTokens (if any).
        /// EventTokens inside are EventTokenOperationApplied with Requested/Applied/NewBalance.
        /// </summary>
        public ResourceOperation Rewards { get; set; }
    }

    // =====================================================================
    // CONFIG MODELS
    // =====================================================================

    /// <summary>Root config for the limited-time event system. Lives in TitlePublicConfiguration.TimedEvent.</summary>
    [Serializable]
    public class TimedEventDefinitions
    {
        /// <summary>Fixed-schedule events. Key = ScheduledEventDefinition.TimedEventID.</summary>
        public Dictionary<string, ScheduledEventDefinition> ScheduledEvents { get; set; } = new();

        /// <summary>Cyclic event chains. Key = EventChainDefinition.TimedEventID.</summary>
        public Dictionary<string, EventChainDefinition> EventChains { get; set; } = new();

        /// <summary>Global settings shared across all events.</summary>
        public LimitedTimeEventsGlobalSettings Settings { get; set; } = new();
    }

    /// <summary>Global settings applied to all events unless overridden per-event.</summary>
    [Serializable]
    public class LimitedTimeEventsGlobalSettings
    {
        /// <summary>Max number of concurrently active events.</summary>
        public int MaxConcurrentEvents { get; set; } = 5;

        /// <summary>Days after event end before player progress is cleaned up.</summary>
        public int CleanupAfterDays { get; set; } = 7;

        /// <summary>Hours after event end during which the finished event is still shown in UI.</summary>
        public int GracePeriodHours { get; set; } = 24;
    }

    /// <summary>A fixed-schedule event with explicit start/end dates.</summary>
    [Serializable]
    public class ScheduledEventDefinition
    {
        /// <summary>Unique event ID, e.g. "halloween_2026". Key in TimedEventDefinitions.ScheduledEvents.</summary>
        public string TimedEventID { get; set; }

        /// <summary>Event start time (UTC). Earning begins at this moment.</summary>
        public DateTime StartUtc { get; set; }

        /// <summary>Event end time (UTC). Earning stops here unless AllowEarningAfterEnd = true.</summary>
        public DateTime EndUtc { get; set; }

        /// <summary>Event content: display, token definition, sources, milestones, etc.</summary>
        public EventContent Content { get; set; } = new();

        /// <summary>Whether tokens can be earned after EndUtc.</summary>
        public bool AllowEarningAfterEnd { get; set; } = false;

        /// <summary>Hours after EndUtc during which milestone claiming is allowed.</summary>
        public int ClaimGraceHours { get; set; } = 48;

        /// <summary>Developer-defined key-value params, e.g. theme, AB-test group.</summary>
        public Dictionary<string, string> CustomParams { get; set; } = new();
    }

    /// <summary>A cyclic chain of events that repeat in sequence.</summary>
    [Serializable]
    public class EventChainDefinition
    {
        /// <summary>Unique chain ID, e.g. "main_rotation". Key in TimedEventDefinitions.EventChains.</summary>
        public string TimedEventID { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> AssetPaths { get; set; }

        /// <summary>
        /// Anchor date (UTC). The first event of the first cycle starts at this moment.
        /// All subsequent dates are derived from this anchor automatically.
        /// </summary>
        public DateTime AnchorUtc { get; set; }

        /// <summary>Whether the chain is active. false = paused, no events running.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Max number of full cycles. 0 = infinite.</summary>
        public int MaxCycles { get; set; } = 0;

        /// <summary>Ordered list of events in the chain, each with its own duration and content.</summary>
        public List<ChainedEventDefinition> Events { get; set; } = new();

        /// <summary>Pause between events within one cycle, in seconds. 0 = no pause.</summary>
        public int PauseBetweenEventsSec { get; set; } = 0;

        /// <summary>Pause between full cycles, in seconds. 0 = no pause.</summary>
        public int PauseBetweenCyclesSec { get; set; } = 0;

        public Dictionary<string, string> CustomParams { get; set; } = new();
    }

    /// <summary>A single event within a cyclic chain. Dates are computed from AnchorUtc, not set manually.</summary>
    [Serializable]
    public class ChainedEventDefinition
    {
        /// <summary>Event ID within the chain, unique per chain, e.g. "chain_event_1".</summary>
        public string ChainedEventID { get; set; }

        /// <summary>Order index within the chain (0, 1, 2...).</summary>
        public int Order { get; set; }

        /// <summary>Event duration in seconds, e.g. 1800 = 30 min, 86400 = 1 day.</summary>
        public long DurationSec { get; set; }

        public EventContent Content { get; set; } = new();

        /// <summary>Hours after this event's end during which claiming is allowed. 0 = only while active.</summary>
        public int ClaimGraceHours { get; set; } = 0;

        public Dictionary<string, string> CustomParams { get; set; } = new();
    }

    /// <summary>Event content shared between ScheduledEventDefinition and ChainedEventDefinition.</summary>
    [Serializable]
    public class EventContent
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> AssetPaths { get; set; }

        /// <summary>Category tag for UI grouping, e.g. "seasonal", "mini", "collab".</summary>
        public string Category { get; set; }

        /// <summary>Event token definition. Token is a separate entity, NOT a VirtualCurrency.</summary>
        public EventTokenDefinition Token { get; set; } = new();

        /// <summary>Whitelist of allowed token sources for this event.</summary>
        public List<EventTokenSource> TokenSources { get; set; } = new();

        /// <summary>When milestone rewards can be claimed: Instant, AfterEventEnd, or FeaturedAfterEnd.</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EventClaimMode ClaimMode { get; set; } = EventClaimMode.Instant;

        /// <summary>Milestone rewards keyed by MilestoneID. Unlock is based on TotalEarned, not current balance.</summary>
        public Dictionary<string, EventMilestoneDefinition> Milestones { get; set; } = new();
    }

    /// <summary>Event token definition. Balance is stored in UserEventProgress, not VirtualCurrency.</summary>
    [Serializable]
    public class EventTokenDefinition
    {
        public string DisplayName { get; set; }
        public Dictionary<string, string> AssetPaths { get; set; }

        /// <summary>Max spendable balance. 0 = unlimited.</summary>
        public long MaxBalance { get; set; } = 0;

        /// <summary>Max tokens per single grant (anti-abuse).</summary>
        public long MaxPerGrant { get; set; } = 1000;

        /// <summary>Max tokens earned per day across all sources. 0 = unlimited.</summary>
        public long DailyEarnCap { get; set; } = 0;

        /// <summary>Whether tokens burn after event end.</summary>
        public bool BurnOnEventEnd { get; set; } = true;

        /// <summary>Burn conversion rule. null = balance is simply zeroed with no compensation.</summary>
        public EventTokenConversion BurnConversion { get; set; }
    }

    /// <summary>Conversion rule for burning leftover event tokens at event end.</summary>
    [Serializable]
    public class EventTokenConversion
    {
        /// <summary>Target VirtualCurrency ID, e.g. "IG" or "DICE".</summary>
        public string TargetCurrencyID { get; set; }

        /// <summary>1 event token = Rate units of target currency. Example: 0.1 means 10 tokens = 1 unit.</summary>
        public double Rate { get; set; } = 0.1;

        /// <summary>Max tokens eligible for conversion (inflation guard).</summary>
        public long MaxConvertAmount { get; set; } = 10000;
    }

    /// <summary>Config for a single token source within an event.</summary>
    [Serializable]
    public class EventTokenSource
    {
        /// <summary>Source system type.</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EventTokenSourceType SourceType { get; set; }

        /// <summary>Base token amount per trigger.</summary>
        public long BaseAmount { get; set; } = 1;

        /// <summary>Max tokens per day from this source. 0 = unlimited.</summary>
        public long DailyCapFromSource { get; set; } = 0;

        /// <summary>Max triggers per day from this source. 0 = unlimited.</summary>
        public int DailyTriggerCap { get; set; } = 0;

        /// <summary>Cooldown between triggers in seconds. 0 = no cooldown.</summary>
        public int CooldownSeconds { get; set; } = 0;

        /// <summary>BoardTileLanding only: whitelist of tile types that grant tokens.</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public List<string> TileTypeFilter { get; set; } = new();

        /// <summary>
        /// BoardAttack/BoardRaid/BoardBuild only: whitelist of outcomes that grant tokens.
        /// Empty = any outcome.
        /// </summary>
        public List<string> OutcomeFilter { get; set; } = new();

        /// <summary>Whether to scale BaseAmount by the player's RollMultiplier.</summary>
        public bool ScaleWithRollMultiplier { get; set; } = true;

        /// <summary>
        /// Context params for source filtering. CustomAction: {"ActionName": "watch_ad"}.
        /// QuestComplete: {"QuestID": "win_5_raids"}.
        /// </summary>
        public Dictionary<string, string> Params { get; set; } = new();
    }

    /// <summary>Milestone reward definition. Unlock is based on TotalEarned, not current balance.</summary>
    [Serializable]
    public class EventMilestoneDefinition
    {
        /// <summary>Unique milestone ID within the event, e.g. "m_100", "m_500".</summary>
        public string MilestoneID { get; set; }

        public string DisplayName { get; set; }
        public Dictionary<string, string> AssetPaths { get; set; }

        /// <summary>TotalEarned threshold required to unlock this milestone.</summary>
        public long RequiredTokensEarned { get; set; }

        public ResourceGrant Rewards { get; set; } = new();

        /// <summary>Sort order in UI (lower = earlier).</summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Whether this is a featured (big) milestone.
        /// When ClaimMode = FeaturedAfterEnd, featured milestones can only be claimed after event ends.
        /// </summary>
        public bool IsFeatured { get; set; } = false;
    }

    /// <summary>
    /// Bonus window config for a single event. Stored in EventContent.BonusWindow,
    /// works the same for both ScheduledEventDefinition and ChainedEventDefinition.
    /// null = bonus window is disabled for this event.
    /// </summary>
    [Serializable]
    public class BonusWindowConfig
    {
        /// <summary>
        /// Sequence of phases from the moment the event starts.
        /// Each element is a single phase with a type (bonus/cooldown) and duration.
        /// Empty list = window is effectively disabled.
        /// </summary>
        public List<BonusWindowPhase> Schedule { get; set; } = new();

        /// <summary>
        /// true  — after the last phase the cycle repeats from the first until the event ends.
        /// false — after all phases are completed no more windows will open.
        /// </summary>
        public bool RepeatCycle { get; set; } = true;

        /// <summary>
        /// Max number of full passes through the entire Schedule (relevant only when RepeatCycle = true).
        /// 0 = infinite (while the event itself is active). Once cycles are exhausted no more windows open.
        /// </summary>
        public int MaxCycles { get; set; } = 0;
    }

    /// <summary>
    /// A single phase in the window schedule.
    /// </summary>
    [Serializable]
    public class BonusWindowPhase
    {
        /// <summary>
        /// Sequential index of the phase within the Schedule (0-based).
        /// Defines the order regardless of the element's position in the list.
        /// Unique within a single Schedule. Phases are sorted ascending by Order
        /// to form a full cycle: bonus → cooldown → ...
        /// </summary>
        public int Order { get; set; }

        /// <summary>Phase type: bonus or cooldown.</summary>
        public BonusWindowPhaseType Type { get; set; }

        /// <summary>Phase duration (seconds). Must be > 0.</summary>
        public long DurationSec { get; set; }

        /// <summary>
        /// Multiplier for BonusRewards. Applied only when Type = MultipliedBonus.
        /// 1.5 = bonus portion ×1.5, 0.5 = bonus portion ×0.5.
        /// Ignored for Bonus (always 1.0) and Cooldown (always 0).
        /// </summary>
        public float BonusMultiplier { get; set; }
    }

    /// <summary>Phase type in the Schedule.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BonusWindowPhaseType
    {
        /// <summary>Cooldown phase: only base Rewards are granted.</summary>
        Cooldown,

        /// <summary>Bonus phase: BonusRewards of milestones are granted.</summary>
        Bonus,

        /// <summary>
        /// Bonus phase: BonusRewards are scaled by BonusMultiplier.
        /// Allows both amplifying (1.5, 2.0) and reducing (0.5) the bonus portion.
        /// </summary>
        MultipliedBonus,
    }

    /// <summary>
    /// Computed state of the bonus window at the time of the request.
    /// </summary>
    [Serializable]
    public class BonusWindowState
    {
        /// <summary>
        /// true  — a BONUS phase is currently active, BonusRewards are being granted;
        /// false — currently a cooldown phase / no more windows ahead, only base Rewards are granted.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>When the CURRENT phase ends (UTC). The UI renders a countdown to this moment.</summary>
        public DateTime CurrentPhaseEndUtc { get; set; }

        /// <summary>
        /// When the NEXT bonus phase starts (UTC).
        /// null = no more bonus windows (Schedule fully traversed or MaxCycles exhausted).
        /// </summary>
        public DateTime? NextBonusStartUtc { get; set; }

        /// <summary>Index of the current full pass through the Schedule (0-based). For UI: "Cycle 3 of 10".</summary>
        public int CurrentCycleIndex { get; set; }

        /// <summary>Index of the current phase within the Schedule (0-based).</summary>
        public int CurrentPhaseIndex { get; set; }

        /// <summary>
        /// BonusRewards multiplier for the currently active phase.
        /// Bonus = 1.0, MultipliedBonus = value from BonusWindowPhase.BonusMultiplier.
        /// 0 if IsActive = false.
        /// </summary>
        public float ActiveBonusMultiplier { get; set; }
    }

    /// <summary>Computed active event info returned by GetActiveEvents.</summary>
    [Serializable]
    public class ActiveEventInfo
    {
        public TimedEventType Type { get; set; }

        /// <summary>LTE event ID. Scheduled ID or Chain ID depending on Type.</summary>
        public string TimedEventID { get; set; }

        /// <summary>Active inner event ID within the chain. null for Scheduled.</summary>
        public string CurrentChainedEventID { get; set; }

        public EventContent Content { get; set; } = new();

        /// <summary>Player's progress for this event. null if no tokens earned yet.</summary>
        public UserEventTokenProgress Progress { get; set; }

        /// <summary>Computed start time of the current event window (derived from AnchorUtc for Chain).</summary>
        public DateTime ComputedStartUtc { get; set; }

        /// <summary>Computed end time of the current event window.</summary>
        public DateTime ComputedEndUtc { get; set; }

        public bool CanEarn { get; set; }
        public bool CanClaim { get; set; }

        /// <summary>Next unclaimed milestone for the progress bar. null if all claimed.</summary>
        public EventMilestoneDefinition NextMilestone { get; set; }

        // ─── Chain-specific ───

        /// <summary>Current cycle index (Chain only). null for Scheduled.</summary>
        public int? CurrentCycleIndex { get; set; }

        /// <summary>Order of the current event within the chain. null for Scheduled.</summary>
        public int? CurrentEventOrder { get; set; }

        /// <summary>Total number of events in the chain. null for Scheduled.</summary>
        public int? TotalEventsInChain { get; set; }
    }

    // =====================================================================
    // ENUMS
    // =====================================================================

    /// <summary>LTE event type: fixed-schedule or cyclic chain.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimedEventType
    {
        Scheduled,
        Chain,
    }

    /// <summary>When milestone rewards can be claimed.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventClaimMode
    {
        /// <summary>Immediately when TotalEarned reaches the threshold.</summary>
        Instant,

        /// <summary>Only after the event ends (within ClaimGraceHours).</summary>
        AfterEventEnd,

        /// <summary>Regular milestones are claimable instantly; IsFeatured milestones only after event ends.</summary>
        FeaturedAfterEnd,
    }

    /// <summary>Type of system that may grant event tokens.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
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
        LeaderboardRankReward,
    }

    /// <summary>Action enum — one entry per backend switch case.</summary>
    public enum TimedEventAction
    {
        GetActiveEvents,
        GrantTokens,
        SpendTokens,
        ClaimMilestone,
        GetDefinitions,
        GetUserLteState,
    }
}
