using System;
using System.Collections.Generic;
using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class CraftRequest : IGSRequest
    {
        public string CraftID { get; set; }
        public int Count { get; set; }
        public int SelectedOptionID { get; set; }

        // List of item IDs from the user's inventory to be "burned".
        // The quantity must be exactly: RequiredItemCount * Count.
        public List<string> InputItemIDs = new List<string>();
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class CraftResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CraftType Type { get; set; }

        public string CraftID { get; set; }
        public int CraftedCount { get; set; }
        public int SelectedOptionID { get; set; } = 0;

        public string InputRarity { get; set; }
        public string OutputRarity { get; set; }

        public List<CraftSingleResult> Results { get; set; } = new();
    }

    [Serializable]
    public class CraftSingleResult
    {
        public int Index { get; set; } // 0..CraftedCount-1
        public List<string> BurnedItemIDs { get; set; }
        public string RolledCollectionID { get; set; }
        public Dictionary<string, int> UsedCollections { get; set; }
        public ItemOrCurrency Output { get; set; }
    }

    [Serializable]
    public class CraftDefinitionsResponse
    {
        public List<CraftDefinition> CraftDefinitions { get; set; }
    }

    // =================================================================================
    // CONFIG & ENUMS
    // =================================================================================

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CraftType
    {
        TradeUpRarity,
        TradeUpCollection,
    }

    [Serializable]
    public class CraftDefinition
    {
        public string CraftID { get; set; }
        public CraftType Type { get; set; }
        public string CatalogVersion { get; set; }
        public string CollectionID { get; set; }
        public string InputRarityID { get; set; }
        public string OutputRarityID { get; set; }
        public int RequiredItemCount { get; set; }
        public List<CraftPriceOption> PriceOptions { get; set; }
    }

    [Serializable]
    public class CraftPriceOption
    {
        public int OptionID { get; set; }
        public List<ItemOrCurrency> RequiredResources { get; set; }
    }

    public enum CraftAction
    {
        GetDefinitions,
        Craft,
    }
}
