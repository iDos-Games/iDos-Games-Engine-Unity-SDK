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
        GetStatDefinitions,
        GetCharacters,
        UpgradeStatLevel,
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class CharacterRequest : IGSRequest
    {
        public string CharacterID; // ID персонажа (опционально, если null -> Main)
        public string StatID;      // ID стата дл€ прокачки
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class GetStatDefinitionsResponse
    {
        // StatDefinition Ч класс из вашего SDK
        public List<StatDefinition> StatDefinitions;
    }

    [Serializable]
    public class GetCharactersResponse
    {
        // CharacterModel Ч класс из вашего SDK
        public Dictionary<string, CharacterModel> Characters;
    }

    [Serializable]
    public class UpgradeStatLevelResponse
    {
        public string StatID { get; set; }
        public int StatLevel { get; set; }

        public string CurrencyID { get; set; }
        public int CurrencyBalance { get; set; }
    }

    [Serializable]
    public class CharacterModel
    {
        public string CharacterID { get; set; } // "main" или GUID/что угодно
        public string Class { get; set; } // "Mage", "Warrior" (опционально)
        public string Name { get; set; }

        // ---------- Progress ----------
        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public int Power { get; set; } = 0;

        // ---------- Boosts ----------
        // boostId -> state
        public Dictionary<string, int> StatLevels { get; set; }

        // ---------- Equipment ----------
        // slotId -> equipped item (ссылка на account inventory)
        public Dictionary<string, EquippedItem> Equipment { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Serializable]
    public class EquippedItem
    {
        public string SlotID { get; set; } // —Ћќ“: "weapon", "helmet", "ring1" и т.д.
        public string ItemID { get; set; } // (опционально) дл€ удобства UI/логов можно продублировать ItemId
        public string ItemInstanceID { get; set; } // —сылка на конкретный экземпл€р предмета в account inventory
        public DateTime EquippedAt { get; set; } = DateTime.UtcNow;
    }
}
