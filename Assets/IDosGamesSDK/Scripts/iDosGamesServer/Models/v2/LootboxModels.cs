using IDosGames.TitlePublicConfiguration;
using System;
using System.Collections.Generic;

namespace IDosGames.ServerModels
{
    public enum LootboxAction
    {
        GetDefinitions,
        Open
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class LootboxRequest : IGSRequest
    {
        // Loot box ID (required)
        public string LootboxID;

        // Number to open (default 1)
        public int Count = 1;

        // ID of the selected price option
        public int SelectedOptionID;
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class LootboxDefinitionsResponse
    {
        // List of loot box configurations
        public List<LootboxDefinition> LootboxDefinitions;
    }

    [Serializable]
    public class LootboxOpenResponse
    {
        public string LootboxID;
        public int OpenedCount;
        public int SelectedOptionID;

        // Opening results. This is a list of lists, since multiple items can drop in a single opening
        // and we can open N loot boxes at once.
        public List<List<ItemOrCurrency>> Results;
    }
}
