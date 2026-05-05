using System.Collections.Generic;

namespace IDosGames
{
    public class TitlePublicConfigurationModel
    {
        public Dictionary<string, string> ImageData { get; set; }
        public Dictionary<string, string> AssetBundle { get; set; }

        public TitleCustomDataResponse TitleCustomData { get; set; }
        public CurrencyDefinitions Currency { get; set; }
        public ItemDefinitions Item { get; set; }
        //public PremiumDefinitions Premium { get; set; }
        //public RewardDefinitions Reward { get; set; }

        //public UserCustomDataDefinitions UserCustomData { get; set; }
        //public LimitedTimeEventDefinitions LimitedTimeEvent { get; set; }
        //public CoopEventDefinitions CoopEvent { get; set; }
        //public LeaderboardDefinitions Leaderboard { get; set; }
        //public SeasonDefinitions Season { get; set; }

        //public GameLoopDefinitions GameLoop { get; set; }
        //public CharacterDefinitions Character { get; set; }
        //public CollectionDefinitions Collection { get; set; }
        //public CraftDefinitions Craft { get; set; }
        //public LootboxDefinitions Lootbox { get; set; }
        //public StoreDefinitions Store { get; set; }
        //public DealOfferDefinitions DealOffer { get; set; }
        //public QuestDefinitions Quest { get; set; }
        //public ReferralDefinitions Referral { get; set; }
        //public BlockchainDefinitions Blockchain { get; set; }
        //public TimedBoostDefinitions TimedBoost { get; set; }
    }
}
