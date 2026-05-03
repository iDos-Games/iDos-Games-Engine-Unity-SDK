using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;

namespace IDosGames
{
    [Serializable]
    public class ResourceBundle
    {
        public List<ItemOrCurrency> Items { get; set; }
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
    public class ItemOrCurrency
    {
        public ItemType? Type { get; set; }
        public string Catalog { get; set; }
        public long? Amount { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public string CurrencyID { get; set; }
        public string ItemID { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemType
    {
        Item,
        VirtualCurrency,
        CryptoCurrency,
        UsdCent,
    }

    [Serializable]
    public class EventTokenOperation
    {
        public EventTokenInfo Token { get; set; }
        public string Source { get; set; }
    }

    [Serializable]
    public class EventTokenInfo
    {
        public EventTokenAddress Target { get; set; }
        public long Amount { get; set; }
    }

    [Serializable]
    public class EventTokenAddress
    {
        public EventTokenSystemType EventTokenSystem { get; set; }
        public string EventTokenEntityID { get; set; }
        public string EventTokenSubContext { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventTokenSystemType
    {
        LimitedTimeEvent,
        Leaderboard,
        CoopEvent,
        Season,
        CustomEvent,
    }
}
