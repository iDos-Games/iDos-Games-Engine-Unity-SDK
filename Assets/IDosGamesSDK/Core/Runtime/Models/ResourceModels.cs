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

    [Serializable]
    public class ResourceDualPartyResult
    {
        public string FromUserID { get; set; }
        public string ToUserID { get; set; }
        public ResourceOperation FromResult { get; set; }
        public ResourceOperation ToResult { get; set; }
    }

    [Serializable]
    public class ResourceTransferResult
    {
        public string FromUserID { get; set; }
        public string ToUserID { get; set; }
        public ResourceBundle Transferred { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ModifierOperation
    {
        /// <summary>
        /// Multiply the final value.
        /// Example: x2 from the event, x1.5 from the seasonal buff.
        /// Applied as: result *= value
        /// </summary>
        Multiply,

        /// <summary>
        /// Add a percentage to the base value.
        /// Example: +50% = value 0.5 → result += base * 0.5
        /// All AddPercent values ​​are summed BEFORE multiplication (additive stacking).
        /// </summary>
        AddPercent,

        /// <summary>
        /// Fixed bonus to the base value (before multipliers).
        /// Example: +5 spins, +100 coins.
        /// </summary>
        AddFlat,
    }
}
