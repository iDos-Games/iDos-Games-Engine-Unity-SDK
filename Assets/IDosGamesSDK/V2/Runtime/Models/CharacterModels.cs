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
        /// <summary>Slot to equip into; must be present in <c>CharacterDefinitions.AllowedEquipmentSlotIDs</c>.</summary>
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
        public ResourceOperation Resources { get; set; } = new(); // ASSUMPTION: ResourceOperation already exists in SDK
    }

    /// <summary>Response for a successful <see cref="CharacterAction.UpgradeCharacterLevel"/>.</summary>
    [Serializable]
    public class UpgradeCharacterLevelResponse
    {
        public DateTime ServerTimeUtc { get; set; }
        public string CharacterID { get; set; }
        public int NewLevel { get; set; }

        /// <summary>V2 unified resource container — see <see cref="UpgradeStatLevelResponse.Resources"/>.</summary>
        public ResourceOperation Resources { get; set; } = new(); // ASSUMPTION: ResourceOperation already exists in SDK
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
        /// Other ids must be present in <see cref="CharacterDefinitions.AllowedCharacterIDs"/>.
        /// </summary>
        public string CharacterID { get; set; }

        /// <summary>Optional class label (e.g. <c>"Mage"</c>, <c>"Warrior"</c>).</summary>
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

    /// <summary>
    /// Title-wide character configuration. Lives under
    /// <c>TitlePublicConfiguration.Character</c> (V2 — singular, no <c>Definitions</c> suffix).
    /// </summary>
    [Serializable]
    public class CharacterDefinitions
    {
        /// <summary>Allowed character ids besides <c>"Main"</c>.</summary>
        public List<string> AllowedCharacterIDs { get; set; }

        /// <summary>Allowed equipment slot ids (e.g. <c>"Head"</c>, <c>"Weapon"</c>).</summary>
        public List<string> AllowedEquipmentSlotIDs { get; set; }

        /// <summary>Global stat dictionary (applies to every character unless overridden).</summary>
        public Dictionary<string, StatDefinition> StatDefinitions { get; set; }

        /// <summary>Per-character stat overrides. Keyed by <c>CharacterID</c>.</summary>
        public Dictionary<string, Dictionary<string, StatDefinition>> CustomStatDefinitions { get; set; }

        /// <summary>Global character-level dictionary keyed by level number (string).</summary>
        public Dictionary<string, CharacterLevelDefinition> LevelDefinitions { get; set; }

        /// <summary>Per-character level overrides. Keyed by <c>CharacterID</c>, then by level (string).</summary>
        public Dictionary<string, Dictionary<string, CharacterLevelDefinition>> CustomLevelDefinitions { get; set; }
    }

    /// <summary>One upgradable stat definition.</summary>
    [Serializable]
    public class StatDefinition
    {
        public string StatID { get; set; }
        public int MaxLevel { get; set; }
        public int Weight { get; set; }

        /// <summary>Base cost for level 1; scales linearly via <see cref="CostScalingFactor"/>.</summary>
        public ResourceConsume BaseCostResource { get; set; } // ASSUMPTION: ResourceConsume already exists in SDK

        public double CostScalingFactor { get; set; }
        public double BaseStatValue { get; set; }
        public double StatScalingFactor { get; set; }

        public List<StatRequirement> Requirements { get; set; }

        public string DisplayName { get; set; }
        public string IconPath { get; set; }
        public string Description { get; set; }
    }

    /// <summary>Prerequisite stat that must reach <see cref="RequiredLevel"/> before upgrading the parent stat.</summary>
    [Serializable]
    public class StatRequirement
    {
        public string RequiredStatID { get; set; }
        public int RequiredLevel { get; set; }
    }

    /// <summary>Configuration of a single character rank/level.</summary>
    [Serializable]
    public class CharacterLevelDefinition
    {
        public int Level { get; set; }

        /// <summary>Cost of stepping into this level (consumed atomically by the server).</summary>
        public ResourceConsume UpgradeCost { get; set; } // ASSUMPTION: ResourceConsume already exists in SDK

        public double GlobalStatMultiplier { get; set; } = 1.0;

        /// <summary>Multiplier applied to <see cref="StatDefinition.MaxLevel"/> at this rank.</summary>
        public float StatMaxLevelMultiplier { get; set; }
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
    }
}
