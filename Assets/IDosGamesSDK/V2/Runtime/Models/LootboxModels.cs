using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Request for all Lootbox actions.
    ///
    /// Field usage by action:
    ///   GetDefinitions — no extra fields required.
    ///   Open           — LootboxID, Count (default 1, clamped 1–100), SelectedOptionID required.
    /// </summary>
    [Serializable]
    public class LootboxRequest : BaseRequest
    {
        /// <summary>ID of the lootbox to open. Required for Open.</summary>
        public string LootboxID;

        /// <summary>Number of boxes to open in one call. Clamped to [1, 100] server-side. Default: 1.</summary>
        public int Count = 1;

        /// <summary>Key of the selected price option from LootboxDefinition.PriceOptions. Required for Open.</summary>
        public int SelectedOptionID;
    }

    // =====================================================================
    // Response models
    // =====================================================================

    [Serializable]
    public class LootboxDefinitionsResponse
    {
        public LootboxDefinitions LootboxDefinitions;
    }

    [Serializable]
    public class LootboxOpenResponse
    {
        public DateTime ServerTimeUtc;
        public string LootboxID;
        public int OpenedCount;
        public int SelectedOptionID;

        /// <summary>
        /// Aggregated result of the entire open operation (all boxes + pity rewards).
        /// EventTokens here are EventTokenOperationApplied — use for balance/cap info.
        /// </summary>
        public ResourceOperation Resources;

        /// <summary>
        /// Per-box breakdown for UI animation. EventTokens inside are plain EventTokenOperation
        /// (no Requested/Applied/NewBalance). Use Resources for authoritative balance data.
        /// </summary>
        public List<ResourceOperation> Results;

        /// <summary>Pity rules that triggered during this open. null if none triggered.</summary>
        public List<LootboxPityTriggerResponse> TriggeredPity;
    }

    [Serializable]
    public class LootboxPityTriggerResponse
    {
        public string RuleID;

        /// <summary>Zero-based index into Results indicating which box received the pity reward.</summary>
        public int BoxIndex;
    }

    // =====================================================================
    // Config models
    // =====================================================================

    [Serializable]
    public class LootboxDefinitions
    {
        public Dictionary<string, LootboxDefinition> Definitions = new();
    }

    [Serializable]
    public class LootboxDefinition
    {
        public string LootboxID;
        public Dictionary<string, string> AssetPaths;

        /// <summary>Available price options keyed by option ID string.</summary>
        public Dictionary<string, LootboxPriceOption> PriceOptions;

        /// <summary>
        /// Reward slots. Each slot independently performs [MinRolls, MaxRolls] rolls per box open.
        /// Use MinRolls=MaxRolls=1 for guaranteed slot, MinRolls=0 for optional slot.
        /// </summary>
        public List<LootboxRewardSlot> RewardSlots;

        /// <summary>Hard-pity rules. Each rule tracks its own counter per player.</summary>
        public List<LootboxPityRule> PityRules;
    }

    [Serializable]
    public class LootboxPriceOption
    {
        public int PriceOptionID;

        /// <summary>
        /// Cost to open. Standard = base cost. PremiumDiscounts = tier-based discounts.
        /// Scaled by Count server-side when opening multiple boxes.
        /// </summary>
        public ResourceConsume RequiredResources;
    }

    [Serializable]
    public class LootboxRewardSlot
    {
        /// <summary>Slot identifier for analytics/UI ("guaranteed_coins", "random_item").</summary>
        public string SlotID;

        public int MinRolls;
        public int MaxRolls;

        /// <summary>Weighted reward pool. Each roll picks one entry by Weight.</summary>
        public List<LootboxRewardRoll> Pool;
    }

    [Serializable]
    public class LootboxRewardRoll
    {
        /// <summary>
        /// Grant-only reward. Standard = items/currencies/tokens for all players.
        /// PremiumBonuses/PremiumTiers = additional rewards for subscribers.
        /// </summary>
        public ResourceGrant Reward;

        public int Weight;

        /// <summary>
        /// Optional random amount range. If set, a value N in [Min, Max] overrides Amount
        /// on ALL items and tokens inside Reward. Best used with single-resource entries.
        /// null = use Amount from Reward as-is.
        /// </summary>
        public LootboxAmountRange AmountRange;
    }

    [Serializable]
    public class LootboxAmountRange
    {
        /// <summary>Inclusive minimum. Must be ≥ 0.</summary>
        public long Min;

        /// <summary>Inclusive maximum. Must be ≥ Min.</summary>
        public long Max;
    }

    [Serializable]
    public class LootboxPityRule
    {
        /// <summary>
        /// Stable rule identifier within the lootbox (e.g. "guaranteed_legendary").
        /// Used as part of the counter key — renaming resets player counters.
        /// </summary>
        public string RuleID;

        /// <summary>Opens between guaranteed triggers. Must be ≥ 1.</summary>
        public int Threshold;

        /// <summary>Weighted reward pool rolled once per trigger.</summary>
        public List<LootboxRewardRoll> Pool;
    }

    // =====================================================================
    // State models
    // =====================================================================

    [Serializable]
    public class UserLootboxState
    {
        /// <summary>
        /// Pity counters keyed by "{LootboxID}:{RuleID}".
        /// Missing key = counter is 0.
        /// </summary>
        public Dictionary<string, UserLootboxPityCounter> Pity = new();
    }

    [Serializable]
    public class UserLootboxPityCounter
    {
        /// <summary>Opens since the last trigger (0..Threshold-1).</summary>
        public int OpensSinceLastTrigger;

        /// <summary>When the rule last triggered. For analytics/debug.</summary>
        public DateTime LastTriggeredAtUtc;
    }

    // =====================================================================
    // Action enum
    // =====================================================================

    public enum LootboxAction
    {
        GetDefinitions,
        Open
    }
}
