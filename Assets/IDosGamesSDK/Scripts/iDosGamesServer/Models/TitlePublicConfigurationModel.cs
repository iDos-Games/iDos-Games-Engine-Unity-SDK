using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace IDosGames.TitlePublicConfiguration
{
    public class TitlePublicConfigurationModel
    {
        public CommissionRoyaltyPercentage CommissionRoyaltyPercentage { get; set; }
        public CreativeMarketplace CreativeMarketplace { get; set; }
        public CurrencyPrices CurrencyPrices { get; set; }
        public DefaultAvatarSkin DefaultAvatarSkin { get; set; }
        public EventWeekly EventWeekly { get; set; }
        public List<EventWeeklyRewards> EventWeeklyRewards { get; set; }
        public Friends Friends { get; set; }
        public List<Leaderboard> Leaderboards { get; set; }
        public List<ProductForRealMoney> ProductsForRealMoney { get; set; }
        public List<ProductForVirtualCurrency> ProductsForVirtualCurrency { get; set; }
        public ReferralFirstActivationReward ReferralFirstActivationReward { get; set; }
        public List<ReferralInviteReward> ReferralInviteRewards { get; set; }
        public List<SecondarySpinReward> SecondarySpinRewards { get; set; }
        public List<ShopDailyFreeProduct> ShopDailyFreeProducts { get; set; }
        public ShopDailyProducts ShopDailyProducts { get; set; }
        public List<ShopDailyProductsConstructor> ShopDailyProductsConstructors { get; set; }
        public List<ShopSpecialProduct> ShopSpecialProducts { get; set; }
        public List<SkinCollectionRarity> SkinCollectionRarity { get; set; }
        public List<SpinReward> SpinRewards { get; set; }
        public SystemState SystemState { get; set; }
        public SmartOffers SmartOffers { get; set; }
        public CurrentSmartOffers CurrentSmartOffers { get; set; }
        public List<CryptoWallet> CryptoWallet { get; set; }
        public AiPublicSettings AiSettings { get; set; }
        public List<AiCustomSetting> AiCustomSettings { get; set; }
        public Dictionary<string, string> ImageData { get; set; }
        public Dictionary<string, string> AssetBundle { get; set; }
    }

    public class AiCustomSetting
    {
        public string Name { get; set; }
        public AiPublicSettings AiSettings { get; set; }
    }

    public class AiPublicSettings
    {
        public string SystemInstructions { get; set; }
        public int LastMessages { get; set; }
        public string AiRequestCurrency { get; set; }
        public int AiRequestCurrencyAmount { get; set; }
        public string AiName { get; set; }
        public string AiAvatarUrl { get; set; }
        public string AiWelcomeMessage { get; set; }
    }

    public class CommissionRoyaltyPercentage
    {
        public int Author { get; set; }
        public int Referral { get; set; }
        public int Company { get; set; }
        public int ReferralReward { get; set; }
    }

    public class CreativeMarketplace
    {
        public int PublicationPrice { get; set; }
        public string CurrencyID { get; set; }
    }

    public class CurrencyPrices
    {
        public float Igt { get; set; }
        public float Igc { get; set; }
        public float ExchangeDivider { get; set; }
    }

    public class DefaultAvatarSkin
    {
        public Gender Gender { get; set; }
        public DefaultAvatarSkinData Data { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Gender
    {
        Male,
        Female,
        Neutral
    }

    public class DefaultAvatarSkinData
    {
        public string Body { get; set; }
        public string Glasses { get; set; }
        public string Hands { get; set; }
        public string Hat { get; set; }
        public string Mask { get; set; }
        public string Pants { get; set; }
        public string Shoes { get; set; }
        public string Torso { get; set; }
    }

    public class EventWeekly
    {
        public EventType Type { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class EventWeeklyRewards
    {
        public EventType Type { get; set; }
        public List<Reward> Rewards { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        Weekdays,
        Weekend
    }

    public class Reward
    {
        public int Id { get; set; }
        public int Points { get; set; }
        public ItemOrCurrency Premium { get; set; }
        public ItemOrCurrency Standard { get; set; }
    }

    public class Friends
    {
        public int MaxCount { get; set; }
        public int MaxInactiveDay { get; set; }
    }

    public class Leaderboard
    {
        public string StatisticName { get; set; }
        public string Name { get; set; }
        public string ValueName { get; set; }
        public StatisticResetFrequency Frequency { get; set; }
        public List<RankReward> RankRewards { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatisticType
    {
        Global,
        Country,
        Friends,
        Clan
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatisticResetFrequency
    {
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatisticAggregationMethod
    {
        Last, // Last (always update with the new value)
        Minimum, // Minimum (always use the lowest value)
        Maximum, // Maximum (always use the highest value)
        Sum // Sum (add this value to the existing value)
    }

    public class RankReward
    {
        public string Rank { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    public class ItemOrCurrency
    {
        public ItemType? Type { get; set; }
        public string Catalog { get; set; }
        public int? Amount { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public string CurrencyID { get; set; }
        public string ItemID { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemType
    {
        Item,
        VirtualCurrency
    }

    public class ProductForRealMoney
    {
        public string Name { get; set; }
        public string ItemID { get; set; }
        public string ProductType { get; set; }
        public string ItemClass { get; set; }
        public int PriceRM { get; set; }
        public string ImagePath { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    public class ProductForVirtualCurrency
    {
        public string Name { get; set; }
        public string CurrencyID { get; set; }
        public string CurrencyImagePath { get; set; }
        public int PriceRM { get; set; }
        public string ImagePath { get; set; }
        public string ItemID { get; set; }
        public string ItemClass { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    public class ReferralFirstActivationReward : ItemOrCurrency { }

    public class ReferralInviteReward
    {
        public int FollowersAmount { get; set; }
        public ItemOrCurrency Reward { get; set; }
    }

    public class SecondarySpinReward
    {
        public int Id { get; set; }
        public string ItemID { get; set; }
        public ItemOrCurrency Reward { get; set; }
    }

    public class ShopDailyFreeProduct
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public int Amount { get; set; }
        public string ImagePath { get; set; }
        public string ItemID { get; set; }
        public string ItemClass { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    public class ShopDailyProducts
    {
        public List<ShopDailyProduct> Products { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ShopDailyProduct
    {
        public string ItemID { get; set; }
        public string Name { get; set; }
        public string CurrencyID { get; set; }
        public string CurrencyImagePath { get; set; }
        public string ImagePath { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
        public int PriceRM { get; set; }
    }

    public class CryptoWallet
    {
        public string ChainType { get; set; }
        public int ChainID { get; set; }
        public string RpcUrl { get; set; }
        public decimal GasPrice { get; set; }
        public string BlockchainExplorerUrl { get; set; }
        public string SoftTokenTicker { get; set; }
        public string SoftTokenContractAddress { get; set; }
        public string SoftTokenContractAbi { get; set; }
        public string SoftTokenImagePath { get; set; }
        public string HardTokenTicker { get; set; }
        public string HardTokenContractAddress { get; set; }
        public string HardTokenContractAbi { get; set; }
        public string HardTokenImagePath { get; set; }
        public string NftContractAddress { get; set; }
        public string NftContractAbi { get; set; }
        public string HotWalletAddress { get; set; }
    }

    public class ShopDailyProductsConstructor
    {
        public string ItemID { get; set; }
        public string ItemClass { get; set; }
        public List<ShopDailyProductConstructorItem> Products { get; set; }
    }

    public class ShopDailyProductConstructorItem
    {
        public int Weight { get; set; }
        public int PriceRMFrom { get; set; }
        public int PriceRMTo { get; set; }
        public string ItemID { get; set; }
        public string Name { get; set; }
        public string CurrencyID { get; set; }
        public string CurrencyImagePath { get; set; }
        public string ImagePath { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    public class ShopSpecialProduct
    {
        public string ItemID { get; set; }
        public SpecialProductType Type { get; set; }
        public DateTime? EndDate { get; set; }
        public int? QuantityLimit { get; set; }
        public string Name { get; set; }
        public int PriceRM { get; set; }
        public string CurrencyID { get; set; }
        public string CurrencyImagePath { get; set; }
        public string ImagePath { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SpecialProductType
    {
        TimeLimited,
        QuantityLimited,
        QuantityLimitedForPlayer,
        TimeQuantityLimitedForPlayer,
        Unlimited
    }

    public class SkinCollectionRarity
    {
        public string Rarity { get; set; }
        public int Profit { get; set; }
        public List<string> Collections { get; set; }
    }

    public class SpinReward
    {
        public int Id { get; set; }
        public string ItemID { get; set; }
        public ItemOrCurrency Standard { get; set; }
        public ItemOrCurrency Premium { get; set; }
    }

    public class SystemState
    {
        public bool Leaderboards { get; set; }
        public PlatformSystemState VipFreeTrial { get; set; }
        public PlatformSystemState Wallet { get; set; }
    }

    public class PlatformSystemState
    {
        public bool Ios { get; set; }
        public bool Android { get; set; }
    }

    public class SmartOffers
    {
        public SingleOffers SingleOffers { get; set; }
        public List<ChainOffer> ChainOffers { get; set; }
        public string OneTimeOffer { get; set; }
    }

    public class SingleOffers
    {
        public List<Offer> Cheap { get; set; }
        public List<Offer> Medium { get; set; }
        public List<Offer> Expensive { get; set; }
    }

    public class Offer
    {
        public string OfferID { get; set; }
        public DateTime? EndTime { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public int PriceRM { get; set; }
        public string ProductType { get; set; }
        public string IconImagePath { get; set; }
        public string ImagePath { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    public class ChainOffer
    {
        public string ChainOfferID { get; set; }
        public string IconImagePath { get; set; }
        public DateTime? EndTime { get; set; }
        public string Name { get; set; }
        public List<ChainOfferItem> Offers { get; set; }
    }

    public class ChainOfferItem
    {
        public string OfferID { get; set; }
        public int Level { get; set; }
        public int PriceRM { get; set; }
        public string ImagePath { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }

    public class CurrentSmartOffers
    {
        public List<string> Cheap { get; set; }
        public List<string> Medium { get; set; }
        public List<string> Expensive { get; set; }
        public List<DateTime> EndTime { get; set; }
        public List<DateTime> FirstEndTime { get; set; }
    }

}
