using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    // =========================================================================
    // REQUEST
    // =========================================================================

    /// <summary>
    /// Request for all TimedBoost actions.
    /// <list type="bullet">
    ///   <item><see cref="TimedBoostAction.Activate"/> — requires <see cref="BoostID"/>.</item>
    ///   <item><see cref="TimedBoostAction.GetDefinitions"/>, <see cref="TimedBoostAction.GetActive"/>,
    ///         <see cref="TimedBoostAction.CleanupExpired"/> — no additional fields needed.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class TimedBoostRequest : BaseRequest
    {
        /// <summary>ID of the boost to activate. Required for <see cref="TimedBoostAction.Activate"/>.</summary>
        public string BoostID;
    }

    // =========================================================================
    // RESPONSES
    // =========================================================================

    /// <summary>Response for <see cref="TimedBoostAction.GetActive"/>.</summary>
    [Serializable]
    public class GetActiveTimedBoostsResponse
    {
        /// <summary>Server UTC time at the moment of the read.</summary>
        public DateTime ServerTimeUtc;

        /// <summary>
        /// Live active boosts keyed by <see cref="ActiveTimedBoost.InstanceID"/>.
        /// Expired or charge-exhausted instances are filtered out server-side.
        /// </summary>
        public Dictionary<string, ActiveTimedBoost> Active = new();
    }

    /// <summary>Response for a successful <see cref="TimedBoostAction.Activate"/>.</summary>
    [Serializable]
    public class ActivateTimedBoostResponse
    {
        /// <summary>Server UTC time of the operation.</summary>
        public DateTime ServerTimeUtc;

        /// <summary>ID of the activated boost.</summary>
        public string BoostID;

        /// <summary>
        /// Stacking policy that was applied. Useful to explain why
        /// <see cref="ActivatedBoost"/> may be null (see <see cref="TimedBoostStackingPolicy.KeepBest"/>).
        /// </summary>
        public TimedBoostStackingPolicy StackingPolicy;

        /// <summary>
        /// The active boost instance (new or refreshed). May be null for
        /// <see cref="TimedBoostStackingPolicy.KeepBest"/> when the existing instance is not worse.
        /// </summary>
        public ActiveTimedBoost ActivatedBoost;

        /// <summary>
        /// Resource changes from the activation cost. Consumed resources are in
        /// <c>Resources.Consume.Standard</c>. Premium discounts/tiers already applied.
        /// </summary>
        public ResourceOperation Resources = new();
    }

    // =========================================================================
    // STATE MODELS
    // =========================================================================

    /// <summary>
    /// Root container for all of the player's active boosts. Stored in
    /// <c>UserState.TimedBoost</c> on the client.
    /// </summary>
    [Serializable]
    public class UserTimedBoostsState
    {
        /// <summary>
        /// Active boost instances keyed by <see cref="ActiveTimedBoost.InstanceID"/>.
        /// With <see cref="TimedBoostStackingPolicy.Stack"/> there may be multiple entries
        /// sharing the same <see cref="ActiveTimedBoost.BoostID"/>.
        /// </summary>
        public Dictionary<string, ActiveTimedBoost> Active = new();

        /// <summary>OCC version — used by the server for optimistic concurrency control.</summary>
        public long Version;
    }

    /// <summary>One active boost instance stored inside <see cref="UserTimedBoostsState.Active"/>.</summary>
    [Serializable]
    public class ActiveTimedBoost
    {
        /// <summary>Unique instance ID (dictionary key).</summary>
        public string InstanceID;

        /// <summary>Reference to <see cref="TimedBoostDefinition.BoostID"/>.</summary>
        public string BoostID;

        /// <summary>UTC time the boost was activated.</summary>
        public DateTime ActivatedAtUtc;

        /// <summary>UTC expiry time. The boost is live while <c>ExpiresAtUtc > now</c>.</summary>
        public DateTime ExpiresAtUtc;

        /// <summary>Remaining charge count. null = timer-only (no charge limit).</summary>
        public int? RemainingCharges;

        /// <summary>Snapshot of the effect at activation time. Used for calculations instead of current config.</summary>
        public TimedBoostEffectSpec EffectSnapshot;

        /// <summary>Origin of this instance (for analytics).</summary>
        public TimedBoostSourceType SourceType;

        /// <summary>Optional reference to the source request's RelatedEntityID.</summary>
        public string SourceRef;
    }

    // =========================================================================
    // CONFIG MODELS
    // =========================================================================

    /// <summary>
    /// Top-level config container bound to <c>TitlePublicConfigurationModel.TimedBoost</c>.
    /// </summary>
    [Serializable]
    public class TimedBoostDefinitions
    {
        /// <summary>All boost definitions keyed by <see cref="TimedBoostDefinition.BoostID"/>.</summary>
        public Dictionary<string, TimedBoostDefinition> Definitions;
    }

    /// <summary>Static definition of one boost stored in <c>cfg.TimedBoost.Definitions[BoostID]</c>.</summary>
    [Serializable]
    public class TimedBoostDefinition
    {
        /// <summary>Stable boost ID. Matches the dictionary key.</summary>
        public string BoostID;

        /// <summary>Localised display name.</summary>
        public string DisplayName;

        /// <summary>Tooltip description.</summary>
        public string Description;

        /// <summary>Asset paths keyed by role (e.g. "icon", "banner", "effect_anim").</summary>
        public Dictionary<string, string> AssetPaths;

        /// <summary>
        /// What to deduct on activation. Full <see cref="ResourceConsume"/> — supports
        /// items, currencies, event tokens, premium discounts and premium tier prices.
        /// Premium logic is applied automatically inside ResourceService.
        /// </summary>
        public ResourceConsume ActivationCost;

        /// <summary>Effect specification: what it affects, how and by how much.</summary>
        public TimedBoostEffectSpec Effect;

        /// <summary>
        /// Lifetime in seconds from activation.
        /// <c>ExpiresAtUtc = ActivatedAtUtc + DurationSeconds</c>.
        /// </summary>
        public long DurationSeconds;

        /// <summary>
        /// Optional charge limit. The boost expires when charges reach zero,
        /// even if <see cref="DurationSeconds"/> has not elapsed. null = timer-only.
        /// </summary>
        public int? Charges;

        /// <summary>Behaviour on re-activation when an instance of the same BoostID is already active.</summary>
        public TimedBoostStackingPolicy StackingPolicy;

        /// <summary>
        /// Maximum simultaneous active instances of this BoostID.
        /// Only relevant for <see cref="TimedBoostStackingPolicy.Stack"/>. null = unlimited.
        /// </summary>
        public int? MaxActiveInstances;

        /// <summary>UI tags for grouping ("economy", "pvp", "tournament", …). No server-side effect.</summary>
        public List<string> Tags;
    }

    /// <summary>
    /// Effect specification for one boost. Semantically identical to <c>ModifierEntry</c>,
    /// enabling transparent conversion inside <c>ModifierContext.AddActiveTimedBoosts</c>.
    /// Used both in <see cref="TimedBoostDefinition.Effect"/> (config) and
    /// <see cref="ActiveTimedBoost.EffectSnapshot"/> (player state).
    /// </summary>
    [Serializable]
    public class TimedBoostEffectSpec
    {
        /// <summary>What the boost affects. Matches <c>EventModifierTarget</c>.</summary>
        public EventModifierTarget Target;

        /// <summary>Arithmetic operation type (AddFlat, AddPercent, Multiply).</summary>
        public ModifierOperation Operation;

        /// <summary>
        /// Effect magnitude. Semantics depend on <see cref="Operation"/>:
        /// AddFlat = raw units; AddPercent = fraction (0.5 = +50%); Multiply = factor (2.0 = ×2).
        /// </summary>
        public double Value;
    }

    // =========================================================================
    // ENUMS
    // =========================================================================

    /// <summary>Actions available in the TimedBoost module.</summary>
    public enum TimedBoostAction
    {
        GetDefinitions,
        GetActive,
        Activate,
        CleanupExpired,
    }

    /// <summary>
    /// Behaviour on re-activation when an active instance of the same BoostID already exists.
    /// </summary>
    public enum TimedBoostStackingPolicy
    {
        /// <summary>All existing instances are removed; a fresh instance is created.</summary>
        Replace,

        /// <summary>
        /// The existing instance's expiry is reset to a new full duration from now;
        /// charges are added to the remaining total. Creates a new instance if none exists.
        /// </summary>
        Refresh,

        /// <summary>
        /// Activates only if the new instance is better (higher effect magnitude or longer TTL).
        /// Cost is always deducted; if not better, no new instance is created.
        /// </summary>
        KeepBest,

        /// <summary>
        /// Every activation creates a new instance. Multiple instances of the same BoostID
        /// are allowed. Capped by <see cref="TimedBoostDefinition.MaxActiveInstances"/> if set.
        /// </summary>
        Stack,
    }

    /// <summary>Origin of an <see cref="ActiveTimedBoost"/> instance (for analytics).</summary>
    public enum TimedBoostSourceType
    {
        /// <summary>Player activated the boost via the standard HTTP endpoint.</summary>
        Activation,

        /// <summary>Granted by an admin tool or service script.</summary>
        Admin,

        /// <summary>Granted as a side-effect of an event reward (reserved for future use).</summary>
        EventReward,
    }
}
