using System;
using System.Collections.Generic;

namespace IDosGames
{
    [Serializable]
    public class UserState
    {
        public string UserID { get; set; }
        public UserInventoryState InventoryV2 { get; set; }
        public UserEventTokensState EventToken { get; set; }
        public UserPublicDataModel PublicData { get; set; }
        public UserSocialState Social { get; set; }
        public UserCharactersState Character { get; set; }
        public UserQuestState Quest { get; set; }
        public UserSeasonsState Season { get; set; }
        public UserLeaderboardsState Leaderboard { get; set; }
    }

    [Serializable]
    public class UserSocialState
    {
        public List<string> Accepted { get; set; } = new();
        public List<string> IncomingRequests { get; set; } = new();
        public List<string> OutgoingRequests { get; set; } = new();
        public List<string> RecommendedFriends { get; set; } = new();
    }
}
