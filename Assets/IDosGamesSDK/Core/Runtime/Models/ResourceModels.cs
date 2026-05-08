using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;

namespace IDosGames
{
    [Serializable]
    public class ResourceBundle
    {
        public List<ResourceEntry> Entries { get; set; }
        public List<EventTokenOperation> EventTokens { get; set; }
    }

    [Serializable]
    public abstract class PremiumTierModifier
    {
        public int MinPremiumTier { get; set; }
        public string RequiredPremiumID { get; set; }
    }

    [Serializable]
    public class PremiumTierBundle : PremiumTierModifier
    {
        public ResourceBundle Resources { get; set; }
    }

    [Serializable]
    public class PremiumTierDiscount : PremiumTierModifier
    {
        public double DiscountPercent { get; set; }
    }

    [Serializable]
    public class PremiumTierBonus : PremiumTierModifier
    {
        public double BonusPercent { get; set; }
    }

    [Serializable]
    public class PremiumTierMultiplier : PremiumTierModifier
    {
        public double Multiplier { get; set; }
    }

    [Serializable]
    public class ResourceGrant
    {
        public ResourceBundle Standard { get; set; }
        public List<PremiumTierBonus> PremiumBonuses { get; set; }
        public List<PremiumTierBundle> PremiumTiers { get; set; }
    }

    [Serializable]
    public class ResourceConsume
    {
        public ResourceBundle Standard { get; set; }
        public List<PremiumTierDiscount> PremiumDiscounts { get; set; }
        public List<PremiumTierBundle> PremiumTiers { get; set; }
    }

    [Serializable]
    public class ResourceOperation
    {
        public ResourceGrant Grant { get; set; }
        public ResourceConsume Consume { get; set; }
    }

    [Serializable]
    public class ResourceEntry
    {
        public ResourceEntryType? Type { get; set; }
        public string CurrencyID { get; set; }
        public long? Amount { get; set; }
        public string CatalogID { get; set; }
        public string ItemID { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResourceEntryType
    {
        Item,
        VirtualCurrency,
        CryptoCurrency,
        UsdCent,
    }
}
