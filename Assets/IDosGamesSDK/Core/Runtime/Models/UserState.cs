using System;

namespace IDosGames
{
    [Serializable]
    public class UserState
    {
        public string UserID { get; set; }
        public UserInventoryState InventoryV2 { get; set; }
        public UserEventTokensState EventToken { get; set; }

        public UserPublicDataModel PublicData { get; set; }
        public UserCharactersState Character { get; set; }
        public UserCollectionState Collection { get; set; }
        public UserCoopEventState CoopEvent { get; set; }
        public UserDealOffersState DealOffer { get; set; }
        public UserGameLoopsState GameLoop { get; set; }
        public UserLeaderboardsState Leaderboard { get; set; }
        public UserLootboxState Lootbox { get; set; }
        public UserMatchState Match { get; set; }
        public UserPremiumState Premium { get; set; }
        public UserQuestState Quest { get; set; }
        public UserReferralState Referral { get; set; }
        public UserRewardState Reward { get; set; }
        public UserSeasonsState Season { get; set; }
        public UserSocialState Social { get; set; }
        public UserStoreState Store { get; set; }
        public UserTimedBoostsState TimedBoost { get; set; }
        public UserCustomDataState CustomData { get; set; }
    }
}
