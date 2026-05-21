using System.Collections.Generic;
using System;

namespace IDosGames
{
    /// <summary>
    /// Request payload for every <see cref="CharacterAction"/>. Fields are filled selectively:
    /// <list type="bullet">
    ///   <item><see cref="CharacterAction.UpgradeStatLevel"/> uses <see cref="CharacterID"/> + <see cref="StatID"/>.</item>
    ///   <item><see cref="CharacterAction.UpgradeCharacterLevel"/> uses <see cref="CharacterID"/>.</item>
    ///   <item><see cref="CharacterAction.EquipItems"/> uses <see cref="CharacterID"/> + <see cref="ItemsToEquip"/>.</item>
    ///   <item><see cref="CharacterAction.UnequipItems"/> uses <see cref="CharacterID"/> + <see cref="UnequipSlotIDs"/>.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class CharacterRequest : BaseRequest
    {
        public string CharacterID { get; set; }
        public string StatID { get; set; }
        public List<EquipSlotPair> ItemsToEquip { get; set; }
        public List<string> UnequipSlotIDs { get; set; }
    }

    /// <summary>
    /// Pair {slot → item} for an equip operation.
    /// Either <see cref="ItemInstanceID"/> (precise instance) or <see cref="ItemID"/>
    /// (server picks first free instance) is required.
    /// </summary>
    [Serializable]
    public class EquipSlotPair
    {
        /// <summary>Slot to equip into; must be present in the target character's <c>CharacterEquipment.Slots</c>.</summary>
        public string SlotID { get; set; }

        /// <summary>Concrete instance from <c>InventoryV2.UnstackableItems</c>. Optional if <see cref="ItemID"/> is set.</summary>
        public string ItemInstanceID { get; set; }

        /// <summary>Item catalog key. Used only when <see cref="ItemInstanceID"/> is missing.</summary>
        public string ItemID { get; set; }
    }

    /// <summary>Response for <see cref="CharacterAction.GetUserCharacters"/>.</summary>
    [Serializable]
    public class UserCharactersState
    {
        /// <summary>Map of all player characters keyed by <c>CharacterID</c>. Empty when the player has none yet.</summary>
        public Dictionary<string, CharacterModel> Characters { get; set; } = new();
    }

    /// <summary>Response for a successful <see cref="CharacterAction.UpgradeStatLevel"/>.</summary>
    [Serializable]
    public class UpgradeStatLevelResponse
    {
        public DateTime ServerTimeUtc { get; set; }
        public string CharacterID { get; set; }
        public string StatID { get; set; }
        public int StatLevel { get; set; }

        /// <summary>
        /// V2 unified resource container. Cost lives under
        /// <c>Resources.Consume.Standard.Entries</c> / <c>Resources.Consume.Standard.EventTokens</c>.
        /// </summary>
        public ResourceOperation Resources { get; set; }
    }

    /// <summary>Response for a successful <see cref="CharacterAction.UpgradeCharacterLevel"/>.</summary>
    [Serializable]
    public class UpgradeCharacterLevelResponse
    {
        public DateTime ServerTimeUtc { get; set; }
        public string CharacterID { get; set; }
        public int NewLevel { get; set; }

        /// <summary>V2 unified resource container — see <see cref="UpgradeStatLevelResponse.Resources"/>.</summary>
        public ResourceOperation Resources { get; set; }
    }

    /// <summary>
    /// State of one player character. Stored on Unity side under
    /// <c>UserData.Character[CharacterID]</c>.
    /// </summary>
    [Serializable]
    public class CharacterModel
    {
        /// <summary>
        /// Unique character id. Reserved value <c>"Main"</c> denotes the always-available main character.
        /// Other ids must be present in <see cref="CharacterDefinitions.Definitions"/>.
        /// </summary>
        public string CharacterID { get; set; }

        /// <summary>Optional class label (denormalized from <c>CharacterClassification.ClassID</c>).</summary>
        public string Class { get; set; }

        /// <summary>Display name set by the player or assigned by default.</summary>
        public string Name { get; set; }

        /// <summary>Current rank/level. <c>0</c> means the character is not activated yet.</summary>
        public int Level { get; set; } = 0;

        /// <summary>Accumulated XP for non-manual progression systems (e.g. PvP).</summary>
        public long Experience { get; set; } = 0;

        /// <summary>Total combat power computed from stat levels and weights.</summary>
        public int Power { get; set; } = 0;

        /// <summary>Stat level map. Key — <c>StatID</c>, value — current upgrade level. Missing key == level 0.</summary>
        public Dictionary<string, int> StatLevels { get; set; } = new();

        /// <summary>
        /// Equipment cache: slot id → equipped item.
        /// Source of truth is <c>InventoryV2.UnstackableItems[instanceId].EquippedSlot</c>.
        /// </summary>
        public Dictionary<string, EquippedItem> Equipment { get; set; } = new();

        /// <summary>Last server mutation time (UTC).</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Cache record of an item equipped in a slot. Lives in
    /// <see cref="CharacterModel.Equipment"/>.
    /// </summary>
    [Serializable]
    public class EquippedItem
    {
        public string CatalogID { get; set; }
        public string ItemID { get; set; }
        public string ItemInstanceID { get; set; }
        public DateTime EquippedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Source-of-truth marker stored on an unstackable instance to indicate where it is equipped.
    /// Mirrors <c>UnstackableItemInstanceState.EquippedSlot</c> on the server.
    /// </summary>
    [Serializable]
    public class EquipmentSlot
    {
        public string CharacterID { get; set; }
        public string SlotID { get; set; }
    }

    // ============================================================
    // Title-wide character catalog
    // Mirrors backend IDosGamesSDK/API/Client/v2/Character/Models/CharacterDefinitions.cs
    // Lives under TitlePublicConfiguration.Character.
    // ============================================================

    /// <summary>
    /// Title's character system configuration: character catalog only.
    /// Each <see cref="CharacterDefinition"/> is self-contained (own stats, levels, equipment rules).
    /// No global / fallback dictionaries.
    /// </summary>
    [Serializable]
    public class CharacterDefinitions
    {
        /// <summary>
        /// Character catalog. Key — <c>CharacterID</c>.
        /// The reserved <c>"Main"</c> character must also have an entry here.
        /// </summary>
        public Dictionary<string, CharacterDefinition> Definitions { get; set; }
    }

    /// <summary>
    /// One character template (hero) in the title catalog.
    /// Self-contained: identity, classification, unlock, equipment rules, stats, levels.
    /// </summary>
    [Serializable]
    public class CharacterDefinition
    {
        /// <summary>
        /// Unique character ID. Reserved value <c>"Main"</c> denotes the always-available primary character.
        /// Must not contain <c>'.'</c> or <c>'$'</c> (MongoDB path restriction).
        /// </summary>
        public string CharacterID { get; set; }

        /// <summary>Display identity (name, description, lore, icons/assets).</summary>
        public CharacterIdentity Identity { get; set; }

        /// <summary>Categorical metadata (class, rarity, tags). All fields are string IDs.</summary>
        public CharacterClassification Classification { get; set; }

        /// <summary>Unlock rules (default-available flag and/or cost).</summary>
        public CharacterUnlock Unlock { get; set; }

        /// <summary>Per-slot equipment rules.</summary>
        public CharacterEquipment Equipment { get; set; }

        /// <summary>
        /// Per-character upgradable stat directory. Key — <c>StatID</c>.
        /// null/empty = no upgradable stats. No global fallback.
        /// </summary>
        public Dictionary<string, StatDefinition> Stats { get; set; }

        /// <summary>
        /// Per-character level/rank directory. Key — level number as string (<c>"1"</c>, <c>"2"</c>, ...).
        /// No global fallback.
        /// </summary>
        public Dictionary<string, CharacterLevelDefinition> Levels { get; set; }
    }

    /// <summary>
    /// Identity / display metadata of a character.
    /// All assets live in <see cref="AssetPaths"/> only — there are no separate IconPath fields.
    /// </summary>
    [Serializable]
    public class CharacterIdentity
    {
        /// <summary>Display name shown in UI (localized or base).</summary>
        public string DisplayName { get; set; }

        /// <summary>Short description for UI tooltips.</summary>
        public string Description { get; set; }

        /// <summary>Optional long-form lore / backstory for character detail screens.</summary>
        public string Lore { get; set; }

        /// <summary>Display order in roster UI. Lower values are listed first.</summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Named asset references (e.g. <c>"icon"</c>, <c>"portrait"</c>, <c>"fullArt"</c>,
        /// <c>"sprite"</c>, <c>"voiceIntro"</c>). Only place asset paths live.
        /// </summary>
        public Dictionary<string, string> AssetPaths { get; set; }
    }

    /// <summary>
    /// Classification / categorical metadata. All fields are string IDs referencing external catalogs.
    /// Each field is optional — leave empty for systems the title does not use.
    /// </summary>
    [Serializable]
    public class CharacterClassification
    {
        /// <summary>Character class identifier (e.g. <c>"Mage"</c>, <c>"Warrior"</c>).</summary>
        public string ClassID { get; set; }

        /// <summary>Rarity identifier (e.g. <c>"Common"</c>, <c>"Rare"</c>, <c>"Epic"</c>, <c>"Legendary"</c>).</summary>
        public string RarityID { get; set; }

        /// <summary>Free-form gameplay tags (e.g. <c>"ranged"</c>, <c>"flying"</c>, <c>"event-2026"</c>).</summary>
        public List<string> Tags { get; set; }
    }

    /// <summary>
    /// Unlock rules. If <see cref="UnlockedByDefault"/> is true, the character is available
    /// without any unlock action. Otherwise it is obtained either via <see cref="Cost"/>
    /// or through external grants (lootbox, quest reward, event).
    /// </summary>
    [Serializable]
    public class CharacterUnlock
    {
        /// <summary>
        /// If <c>true</c>, the character is available without any unlock action.
        /// <c>"Main"</c> must have this set to <c>true</c>.
        /// </summary>
        public bool UnlockedByDefault { get; set; }

        /// <summary>
        /// Cost to unlock this character. Consumed atomically on the server.
        /// <c>null</c> = cannot be unlocked through cost (must be granted by other systems).
        /// </summary>
        public ResourceConsume Cost { get; set; }
    }

    /// <summary>
    /// Equipment rules for the character. Per-slot configuration with character-level gates,
    /// stat prerequisites and item filters. Validated in addition to item-side <c>ItemEquipment</c>.
    /// </summary>
    [Serializable]
    public class CharacterEquipment
    {
        /// <summary>
        /// Per-slot equipment rules. Key — <c>SlotID</c>.
        /// A character can equip into a slot iff it has an entry here — absence = slot forbidden.
        /// </summary>
        public Dictionary<string, CharacterEquipmentSlot> Slots { get; set; }
    }

    /// <summary>
    /// Equip rules for one slot on one character. All gates must pass: character must satisfy
    /// unlock/stat gates, and the item must satisfy rarity/tag/level filters.
    /// </summary>
    [Serializable]
    public class CharacterEquipmentSlot
    {
        /// <summary>Slot identifier (e.g. <c>"Head"</c>, <c>"Weapon"</c>, <c>"Armor"</c>).</summary>
        public string SlotID { get; set; }

        /// <summary>Minimum character <c>Level</c> required to use this slot. <c>0</c> = always available.</summary>
        public int MinCharacterLevel { get; set; }

        /// <summary>Stat-level prerequisites the character must meet to use the slot.</summary>
        public List<StatRequirement> StatRequirements { get; set; }

        /// <summary>Allowed item rarity IDs. null/empty = any rarity.</summary>
        public List<string> AllowedRarityIDs { get; set; }

        /// <summary>Item must have at least one of these tags. null/empty = no tag filter.</summary>
        public List<string> AllowedItemTags { get; set; }

        /// <summary>Minimum allowed item level/tier in this slot. <c>0</c> = no lower bound.</summary>
        public int MinItemLevel { get; set; }

        /// <summary>Maximum allowed item level/tier in this slot. <c>0</c> = no upper bound.</summary>
        public int MaxItemLevel { get; set; }
    }

    /// <summary>
    /// Definition of one upgradable character stat (boost).
    /// Specifies upgrade rules: max level, cost, value scaling, requirements.
    /// </summary>
    [Serializable]
    public class StatDefinition
    {
        /// <summary>Unique stat ID within the character (e.g. <c>"AttackSpeed"</c>).</summary>
        public string StatID { get; set; }

        /// <summary>
        /// Stat category — free-form string ID referencing an external title-level catalog of stat types.
        /// Recommended (non-binding): <c>"Generic"</c>, <c>"PrimaryAttribute"</c>, <c>"Vital"</c>,
        /// <c>"Combat"</c>, <c>"Resistance"</c>, <c>"AttackType"</c>, <c>"ArmorType"</c>, <c>"Resource"</c>.
        /// </summary>
        public string TypeID { get; set; }

        /// <summary>Stat display name for the UI (localized or base).</summary>
        public string DisplayName { get; set; }

        /// <summary>Text description of the stat and its in-game effect, for UI.</summary>
        public string Description { get; set; }

        /// <summary>
        /// Base maximum stat level. Effective max may be higher if the character's level supplies
        /// <see cref="CharacterLevelDefinition.StatMaxLevelMultiplier"/> &gt; 1.
        /// Effective max = <c>MaxLevel * StatMaxLevelMultiplier</c>.
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>Stat weight when computing the character's overall combat <c>Power</c>.</summary>
        public int Weight { get; set; }

        /// <summary>
        /// Resources paid per upgrade level. <c>ResourceEntry.Amount</c> is the base cost for level 1;
        /// level N is scaled via <see cref="CostScalingFactor"/>:
        /// <c>Amount * (1 + CostScalingFactor * (level - 1))</c>.
        /// </summary>
        public ResourceConsume BaseCostResource { get; set; }

        /// <summary>Linear cost-scaling coefficient per level. <c>0.1</c> = +10% over base cost per level above 1.</summary>
        public double CostScalingFactor { get; set; }

        /// <summary>Base stat effect value at level 1.</summary>
        public double BaseStatValue { get; set; }

        /// <summary>
        /// Per-level value scaling coefficient.
        /// Final value at level N: <c>BaseStatValue * (1 + StatScalingFactor * (N - 1))</c>.
        /// </summary>
        public double StatScalingFactor { get; set; }

        public double CharacterLevelScalingFactor { get; set; }

        /// <summary>
        /// Prerequisite list. Before upgrading this stat to any level, each listed stat
        /// must have reached its required level.
        /// </summary>
        public List<StatRequirement> Requirements { get; set; }

        /// <summary>
        /// Named asset references for the stat (e.g. <c>"icon"</c>).
        /// Only place stat asset paths live — there is no separate <c>IconPath</c>.
        /// </summary>
        public Dictionary<string, string> AssetPaths { get; set; }
    }

    /// <summary>
    /// Prerequisite that the referenced stat must reach the given level.
    /// Used in <see cref="StatDefinition.Requirements"/> and <see cref="CharacterEquipmentSlot.StatRequirements"/>.
    /// </summary>
    [Serializable]
    public class StatRequirement
    {
        /// <summary>Dependency stat ID (<see cref="StatDefinition.StatID"/>) that must be upgraded to <see cref="RequiredLevel"/>.</summary>
        public string RequiredStatID { get; set; }

        /// <summary>Minimum level of <see cref="RequiredStatID"/> needed to unlock the dependent action.</summary>
        public int RequiredLevel { get; set; }
    }

    /// <summary>
    /// Configuration of a single character level/rank.
    /// Defines the upgrade cost, the global power multiplier and the stat cap available at this rank.
    /// </summary>
    [Serializable]
    public class CharacterLevelDefinition
    {
        /// <summary>Level/rank this configuration applies to. Numbering starts at <c>1</c>.</summary>
        public int Level { get; set; }

        /// <summary>Cost to upgrade to this level. <c>null</c> = free upgrade.</summary>
        public ResourceConsume UpgradeCost { get; set; }

        /// <summary>Global multiplier applied to the character's total <c>Power</c> at this level. <c>1.0</c> = no bonus.</summary>
        public double GlobalStatMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Multiplier on each stat's max level at this rank. Multiplied against
        /// <see cref="StatDefinition.MaxLevel"/>. <c>1.0</c> = no change; &gt;1.0 raises the cap.
        /// </summary>
        public float StatMaxLevelMultiplier { get; set; }

        /// <summary>
        /// Named asset references for the level/rank (e.g. rank/star icon).
        /// Only place level asset paths live.
        /// </summary>
        public Dictionary<string, string> AssetPaths { get; set; }
    }

    public class UnlockCharacterResponse
    {
        /// <summary>Server time of operation execution (UTC).</summary>
        public DateTime ServerTimeUtc { get; set; }

        /// <summary>
        /// Unlocked character ID (<see cref="CharacterModel.CharacterID"/>).
        /// </summary>
        public string CharacterID { get; set; }

        /// <summary>
        /// Standard container of resource changes.
        /// The debited unlock cost is stored in <c>Resources.Consume.Standard.Entries</c> and/or <c>Resources.Consume.Standard.EventTokens</c>.
        /// When idempotently retried, contains the same result as the first successful call.
        /// </summary>
        public ResourceOperation Resources { get; set; } = new();
    }

    /// <summary>
    /// Server-mirrored action enum for the Character V2 module.
    /// Values must stay 1:1 aligned with backend <c>CharacterAction</c>.
    /// </summary>
    public enum CharacterAction
    {
        GetCharacterDefinitions,
        GetUserCharacters,
        UpgradeStatLevel,
        UpgradeCharacterLevel,
        EquipItems,
        UnequipItems,
        UnequipAllCharacters,
        UnlockCharacter,
    }
}
