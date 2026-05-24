using System;
using System.Collections.Generic;

namespace IDosGames
{
    [Serializable]
    public class ItemDefinitions
    {
        public Dictionary<string, ItemCatalog> Catalogs { get; set; }
    }

    [Serializable]
    public class ItemCatalog
    {
        public Dictionary<string, ItemDefinition> Items { get; set; }
    }

    [Serializable]
    public class ItemDefinition
    {
        public string CatalogID { get; set; }
        public string ItemID { get; set; }
        public string ItemClass { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string CustomData { get; set; }
        public bool IsStackable { get; set; }
        public bool IsTradable { get; set; }
        public int Weight { get; set; }
        public Dictionary<string, string> AssetPaths { get; set; }

        public NFTModel NFT { get; set; }
        public ItemStats Stats { get; set; }
        public ItemEquipment Equipment { get; set; }
        public ItemUpgrade Upgrade { get; set; }
        public ItemMetadata Metadata { get; set; }
    }

    [Serializable]
    public class NFTModel
    {
        public Dictionary<string, NFTNetworkBinding> Networks { get; set; } = new();
        public string MetadataUrl { get; set; }
    }

    [Serializable]
    public class NFTNetworkBinding
    {
        public string ContractAddress { get; set; }
        public string TokenID { get; set; }
        public string TokenStandard { get; set; }
    }

    [Serializable]
    public class ItemStats
    {
        public int Power { get; set; }
        public Dictionary<string, double> FlatBonuses { get; set; } = new();
        public Dictionary<string, double> PercentBonuses { get; set; } = new();
    }

    [Serializable]
    public class ItemEquipment
    {
        public int MinCharacterLevel { get; set; }
        public Dictionary<string, int> UseRequirements { get; set; } = new();
        public List<string> AllowedCharacterIDs { get; set; } = new();
        public List<string> AllowedSlotIDs { get; set; } = new();
    }

    public class ItemUpgrade
    {
        public int MaxLevel { get; set; }
        public ResourceConsume BaseCostResource { get; set; }
        public double CostScalingFactor { get; set; }
        public double FlatScalingFactor { get; set; }
        public double PercentScalingFactor { get; set; }
    }

    [Serializable]
    public class ItemMetadata
    {
        public string RarityID { get; set; }
        public string CollectionID { get; set; }
        public string AuthorID { get; set; }
    }
}
