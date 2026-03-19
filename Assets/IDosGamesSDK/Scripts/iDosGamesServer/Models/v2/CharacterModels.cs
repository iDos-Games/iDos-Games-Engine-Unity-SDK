using System;
using System.Collections.Generic;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

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

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class CharacterRequest : IGSRequest
    {
        public string CharacterID { get; set; } // Character ID (optional if null -> Main)
        public string StatID { get; set; }      // Stat ID for leveling
        public List<EquipSlotPair> ItemsToEquip { get; set; }
        public List<string> UnequipSlotIDs { get; set; }
    }

    [Serializable]
    public class EquipSlotPair
    {
        public string SlotID { get; set; }
        public string ItemInstanceID { get; set; }
        public string ItemID { get; set; } // Can use ItemID instead of ItemInstanceID, but ItemInstanceID must be null
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class GetStatDefinitionsResponse
    {
        public List<StatDefinition> StatDefinitions { get; set; }
    }

    [Serializable]
    public class GetCharactersResponse
    {
        public Dictionary<string, CharacterModel> Characters { get; set; }
    }

    [Serializable]
    public class UpgradeStatLevelResponse
    {
        public string StatID { get; set; }
        public int StatLevel { get; set; }

        public ItemOrCurrency ConsumedResource { get; set; }
        public int ConsumedResourceBalance { get; set; }
    }

    [Serializable]
    public class CharacterModel
    {
        public string CharacterID { get; set; }
        public string Class { get; set; } // "Mage", "Warrior" (optional)
        public string Name { get; set; }

        // ---------- Progress ----------
        public int Level { get; set; } // If Level = 0 then the character is not yet open (If CharacterID = Main it is open by default and has Level 1)
        public long Experience { get; set; }
        public int Power { get; set; }

        // ---------- Boosts ----------
        // boostId -> state
        public Dictionary<string, int> StatLevels { get; set; }

        // ---------- Equipment ----------
        // slotId -> equipped item (ссылка на account inventory)
        public Dictionary<string, EquippedItem> Equipment { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [Serializable]
    public class EquippedItem
    {
        public string SlotID { get; set; } // —Ћќ“: "weapon", "helmet", "ring1" и т.д.
        public string ItemID { get; set; } // (опционально) дл€ удобства UI/логов можно продублировать ItemId
        public string ItemInstanceID { get; set; } // —сылка на конкретный экземпл€р предмета в account inventory
        public DateTime EquippedAt { get; set; } = DateTime.UtcNow;
    }

    [Serializable]
    public class UpgradeCharacterLevelResponse
    {
        public string CharacterID { get; set; }
        public int NewLevel { get; set; }
        public List<ItemOrCurrency> ConsumedResources { get; set; }
    }
}
