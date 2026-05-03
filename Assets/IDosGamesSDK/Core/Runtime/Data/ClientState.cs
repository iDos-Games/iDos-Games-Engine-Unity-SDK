//using IDosGames.ClientModels;
using System;

namespace IDosGames
{
    [Serializable]
    public class ClientState
    {
        public TitlePublicConfigurationModel Title { get; set; }
        public UserState User { get; set; }
    }

    [Serializable]
    public class UserState
    {
        public string UserID { get; set; }
        public UserInventoryState InventoryV2 { get; set; }
        public UserEventTokensState EventToken { get; set; }
        //public UserPremiumState Premium { get; set; }
        public UserPublicDataModel PublicData { get; set; }
        public UserSocialState Social { get; set; }
        //public UserQuestState Quest { get; set; }
        //public UserGameLoopsState GameLoop { get; set; }
        //public UserSeasonsState Season { get; set; }
        //public UserCoopEventState CoopEvent { get; set; }
        //public UserCollectionState Collection { get; set; }
        //public UserLootboxState Lootbox { get; set; }
        //public UserStoreState Store { get; set; }
        //public UserDealOffersState DealOffer { get; set; }
        //public UserReferralState Referral { get; set; }
        //public UserLeaderboardsState Leaderboard { get; set; }
        //public PlayerEconomyTuningState EconomyTuning { get; set; }
        public UserUsageState Usage { get; set; }

        //public UserCustomDataState CustomData { get; set; }
        //public UserBlockchainState Blockchain { get; set; }
        //public UserRewardState Reward { get; set; }
        //public UserCharactersState Character { get; set; }
        //public UserMatchState Match { get; set; }
    }

    [Serializable]
    public class UserPublicDataModel
    {
        public string Username { get; set; }
        public string Country { get; set; }
        public string AvatarUrl { get; set; }
        public bool Premium { get; set; }

        // Можно переиспользовать под общий прогресс
        public int Level { get; set; }
        public long Power { get; set; }
        public long NetWorth { get; set; }
        //public Dictionary<string, float> Stats { get; set; }
    }
}
