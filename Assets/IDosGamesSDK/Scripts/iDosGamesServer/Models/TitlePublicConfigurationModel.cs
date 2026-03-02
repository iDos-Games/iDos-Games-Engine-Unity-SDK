using IDosGames.ClientModels;
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
        public GameLoopsDefinition GameLoops { get; set; }
        public CharacterDefinitions CharacterDefinitions { get; set; }
        public List<LootboxDefinition> LootboxDefinitions { get; set; }
        public List<CraftDefinition> CraftDefinitions { get; set; }
        public List<DailyRewardsDefinition> DailyRewardsDefinitions { get; set; }
        public List<CurrencyTransferPair> AllowedCurrencyTransferPairs { get; set; }
        public QuestDefinitions QuestDefinitions { get; set; }
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
        public string SoftTokenImagePath { get; set; }
        public string HardTokenTicker { get; set; }
        public string HardTokenContractAddress { get; set; }
        public string HardTokenImagePath { get; set; }
        public string NftContractAddress { get; set; }
        public ExtraChainConfig ChainConfig { get; set; }
    }

    public class ExtraChainConfig
    {
        public int ChainConfigVersion { get; set; }
        public string RewardPoolAddress { get; set; }
        public string VaultDepositAddress { get; set; } // For Solana
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

    public class CharacterDefinitions
    {
        public List<string> AllowedCharacterIDs { get; set; }
        public List<string> AllowedEquipmentSlotIDs { get; set; }
        public List<StatDefinition> StatDefinitions { get; set; }
        public Dictionary<string, List<StatDefinition>> CustomStatDefinitions { get; set; } // List of unique StatDefinitions or global overrides. Key: CharacterID
        public List<CharacterLevelDefinition> LevelDefinitions { get; set; }
        public Dictionary<string, List<CharacterLevelDefinition>> CustomLevelDefinitions { get; set; } // Allows you to set different upgrade costs and different power multipliers for specific heroes. Key: CharacterID
    }

    public class StatDefinition
    {
        public string StatID { get; set; }
        public int MaxLevel { get; set; }
        public int Weight { get; set; }
        public ItemOrCurrency BaseCostResource { get; set; }
        public int BaseCost { get; set; }
        public double CostScalingFactor { get; set; }
        public double BaseStatValue { get; set; }
        public double StatScalingFactor { get; set; }
        public List<StatRequirement> Requirements { get; set; }
        public string DisplayName { get; set; }
        public string IconPath { get; set; }
        public string Description { get; set; }
    }

    public class StatRequirement
    {
        public string RequiredStatID { get; set; }
        public int RequiredLevel { get; set; }
    }

    public class CharacterLevelDefinition
    {
        public int Level { get; set; } // 1, 2, 3...
        public List<ItemOrCurrency> UpgradeCost { get; set; } // The price of transition to this level
        public float GlobalStatMultiplier { get; set; } = 1.0f; // Global Stat Boost (Strength)
        public float StatMaxLevelMultiplier { get; set; } // Multiplied by MaxLevel from StatDefinition. Must be greater than 1 to increase MaxLevel.
    }

    public class LootboxDefinition
    {
        public string LootboxID { get; set; }
        public string LootboxImagePath { get; set; }

        // List of payment options
        public List<LootboxPriceOption> PriceOptions { get; set; }

        // Rewards
        public int MinRewardsPerBox { get; set; }
        public int MaxRewardsPerBox { get; set; }
        public List<LootRewardWeight> PossibleRewards { get; set; }
    }

    public class LootboxPriceOption
    {
        public int OptionID { get; set; }
        public List<ItemOrCurrency> RequiredResources { get; set; }
    }

    public class LootRewardWeight
    {
        public ItemOrCurrency Item { get; set; }
        public int Weight { get; set; }
    }

    public class GameLoopsDefinition
    {
        public BoardLoopDefinition RaidBuild { get; set; }
    }

    public class BoardLoopDefinition
    {
        public string RollCurrencyID { get; set; }
        public string ShieldCurrencyID { get; set; }
        public List<int> AllowedRollMultipliers { get; set; }

        // список уровней/стейджей
        public Dictionary<string, BoardStageDefinition> StagesByLevel { get; set; }
    }

    public class BoardStageDefinition
    {
        public string Name { get; set; }
        public string AssetID { get; set; }
        public string MapImagePath { get; set; }

        /// <summary>Клетки доски. Индекс = position.</summary>
        public List<BoardTileDefinition> Tiles { get; set; }

        /// <summary>Building (обычно 5 шт)</summary>
        public List<BuildingDefinition> Buildings { get; set; }

        /// <summary>Рост стоимости апгрейдов Building’ов по уровню.</summary>
        public double CostGrowthFactor { get; set; }

        /// <summary>Награда за успешную атаку (до множителя).</summary>
        public ItemOrCurrency BaseAttackReward { get; set; }

        /// <summary>“Банк” для рейда (до множителя), используется для генерации raid layout.</summary>
        public ItemOrCurrency BaseRaidReward { get; set; }

        /// <summary>Макс множитель ролла на этой локации (можно повышать по мере прогресса).</summary>
        public int MaxRollMultiplier { get; set; }

        /// <summary>Макс щитов на этой локации.</summary>
        public int MaxShields { get; set; }

        /// <summary>Награда за полное завершение борда.</summary>
        public List<ItemOrCurrency> CompletionReward { get; set; }

        /// <summary>Бонус за проход старта.</summary>
        public List<ItemOrCurrency> PassStartReward { get; set; }
    }

    public enum BoardTileType
    {
        Empty,
        Reward,
        Chance,
        RandomAction,       // триггер Attack/Raid
        Attack,
        Raid,
        Shield,         // pickup shield
        EventToken      // универсальная “ивентовая” клетка
    }

    public class BoardTileDefinition
    {
        public int Index { get; set; }
        public BoardTileType Type { get; set; }

        /// <summary>Что выдавать при приземлении.</summary>
        public List<ItemOrCurrency> TileRewards { get; set; }

        /// <summary>Опциональные параметры под ивенты/AB.</summary>
        public Dictionary<string, string> Params { get; set; }
    }

    public class BuildingDefinition
    {
        public int SlotIndex { get; set; }          // 0..4
        public string Name { get; set; }
        public string AssetID { get; set; }
        public string ImageUrl { get; set; }
        public ItemOrCurrency BaseBuildCost { get; set; }
        public int MaxLevel { get; set; }
    }

    public class DailyRewardsDefinition
    {
        public string CalendarID { get; set; }
        public string Description { get; set; }
        public List<DailyRewardDay> Days { get; set; }
        public bool IsLooping { get; set; } = true;
        public bool IsVipOnly { get; set; } = false;
    }

    public class DailyRewardDay
    {
        public int DayNumber { get; set; }
        public List<ItemOrCurrency> Rewards { get; set; }
        public bool IsMilestone { get; set; } = false;
    }

    public class CurrencyTransferPair
    {
        public string FromCurrencyID { get; set; }
        public string ToCurrencyID { get; set; }
    }

    // =========================
    // QUEST SYSTEM
    // =========================

    /// <summary>
    /// Корневой блок системы квестов в конфиге тайтла.
    /// </summary>
    public class QuestDefinitions
    {
        /// <summary>
        /// Определения циклов (daily/weekly/monthly/каждые N дней).
        /// Время ресета не настраивается: всегда 00:00:00 UTC.
        /// Weekly всегда начинается в Понедельник, Monthly всегда начинается 1 числа.
        /// </summary>
        public List<QuestCycleDefinition> Cycles { get; set; } = new();

        /// <summary>
        /// Определения квестов.
        /// Если CycleID пустой/null => квест вечный (Permanent).
        /// Если CycleID задан => квест цикличный и относится к этому циклу.
        /// </summary>
        public List<QuestDefinition> Quests { get; set; } = new();
    }

    /// <summary>
    /// Тип цикла/ресета. Ресеты всегда 00:00:00 UTC.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuestCycleResetKind
    {
        /// <summary>Каждый день в 00:00 UTC.</summary>
        Daily,

        /// <summary>Каждый Понедельник в 00:00 UTC.</summary>
        Weekly,

        /// <summary>Каждое 1 число месяца в 00:00 UTC.</summary>
        Monthly,

        /// <summary>Каждые IntervalDays дней в 00:00 UTC от AnchorUtc.</summary>
        FixedIntervalDays
    }

    /// <summary>
    /// Определение цикла: расписание + milestone-награды.
    /// Квесты НЕ перечисляются здесь — квесты сами ссылаются на CycleID.
    /// </summary>
    public class QuestCycleDefinition
    {
        /// <summary>
        /// ID цикла (например: "daily", "weekly", "monthly", "cycle_10d").
        /// Mongo-safe (без '.' и '$'), т.к. может быть ключом в state.
        /// </summary>
        public string CycleID { get; set; }

        /// <summary>
        /// Имя цикла для UI/админки.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Тип ресета цикла (всегда 00:00:00 UTC).
        /// Weekly всегда Понедельник, Monthly всегда 1 число.
        /// </summary>
        public QuestCycleResetKind ResetKind { get; set; }

        /// <summary>
        /// Только для FixedIntervalDays: длина окна в днях (например 10 или 15).
        /// Для Daily/Weekly/Monthly игнорируется.
        /// </summary>
        public int IntervalDays { get; set; } = 10;

        /// <summary>
        /// Только для FixedIntervalDays: якорная дата (строго 00:00 UTC),
        /// от которой считаются окна по IntervalDays.
        /// Для Daily/Weekly/Monthly игнорируется.
        /// </summary>
        public DateTime AnchorUtc { get; set; } = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Milestone-награды за N завершённых квестов в текущем окне цикла.
        /// </summary>
        public List<QuestCycleMilestoneDefinition> Milestones { get; set; }
    }

    /// <summary>
    /// Milestone-награда за прогресс цикла.
    /// </summary>
    public class QuestCycleMilestoneDefinition
    {
        /// <summary>
        /// ID милстоуна (например: "m3", "m5"). Mongo-safe.
        /// </summary>
        public string MilestoneID { get; set; }

        /// <summary>
        /// Требуемое количество завершённых квестов (Completed или Claimed) в окне.
        /// </summary>
        public int RequiredCompletedQuests { get; set; } = 0;

        /// <summary>
        /// Награды за milestone (ваш ItemOrCurrency).
        /// </summary>
        public List<ItemOrCurrency> Rewards { get; set; } = new();
    }

    /// <summary>
    /// Определение квеста.
    /// </summary>
    public class QuestDefinition
    {
        /// <summary>
        /// ID квеста (например: "win_3_raids"). Mongo-safe (без '.' и '$').
        /// </summary>
        public string QuestID { get; set; }

        /// <summary>
        /// Список CycleID, к которым относится квест.
        /// - Если null/empty => квест вечный (Permanent), хранится у игрока в PermanentQuests.
        /// - Если содержит элементы => квест цикличный и появляется в каждом из этих циклов.
        ///
        /// Важно: если один QuestID находится в нескольких циклах одновременно,
        /// прогресс хранится ОТДЕЛЬНО для каждого цикла в UserQuestState.Cycles[cycleId].Quests[QuestID].
        /// </summary>
        public List<string> CycleIDs { get; set; }

        /// <summary>Название квеста для UI.</summary>
        public string DisplayName { get; set; }

        /// <summary>Описание квеста для UI.</summary>
        public string Description { get; set; }

        /// <summary>Сортировка в UI (меньше = выше).</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Пререквизиты (опционально): какие QuestID должны быть закрыты ранее.
        /// </summary>
        public List<string> RequiredQuestIDs { get; set; }

        /// <summary>
        /// Цели квеста. Квест выполнен, когда ВСЕ цели выполнены.
        /// </summary>
        public List<QuestObjectiveDefinition> Objectives { get; set; }

        /// <summary>
        /// Награды за выполнение квеста.
        /// </summary>
        public List<ItemOrCurrency> Rewards { get; set; }
    }

    /// <summary>
    /// Цель квеста: универсальный счётчик по MetricID.
    /// </summary>
    public class QuestObjectiveDefinition
    {
        /// <summary>
        /// ID цели внутри квеста (например: "raid_wins"). Mongo-safe.
        /// </summary>
        public string ObjectiveID { get; set; }

        // ✅ НОВОЕ: кто может начислять прогресс
        public QuestObjectiveSource Source { get; set; }

        /// <summary>
        /// Ключ метрики/события, по которому начисляется прогресс
        /// (например: "raid_win", "currency_spent:IG").
        /// </summary>
        public string MetricID { get; set; }

        /// <summary>
        /// Сколько нужно набрать для выполнения цели.
        /// </summary>
        public long TargetValue { get; set; } = 1;

        /// <summary>
        /// Метод агрегации прогресса (обычно Sum). Можно переиспользовать ваш enum.
        /// </summary>
        public StatisticAggregationMethod AggregationMethod { get; set; }

        /// <summary>
        /// Опциональные фильтры для уточнения метрики (режим, сложность и т.п.).
        /// </summary>
        public Dictionary<string, string> Filters { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuestObjectiveSource
    {
        ClientApi,     // клиент сам дергает AddProgress
        ServerApi,     // другой сервер дергает AddProgress с SecretKey
        SystemEvent    // только из внутренней логики игры (без HTTP)
    }
}
