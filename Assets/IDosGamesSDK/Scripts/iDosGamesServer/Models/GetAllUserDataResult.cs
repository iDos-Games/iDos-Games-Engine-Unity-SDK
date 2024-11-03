using IDosGames.ClientModels;
using Newtonsoft.Json.Linq;

namespace IDosGames
{
    public class GetAllUserDataResult
    {
        public string Message { get; set; }
        public IGSAuthenticationContext AuthContext { get; set; }
        public GetUserInventoryResult UserInventoryResult { get; set; }
        public JObject TitlePublicConfiguration { get; set; }
        public GetUserDataResult UserDataResult { get; set; }
        public GetLeaderboardResult LeaderboardResult { get; set; }
        public JObject GetFriends { get; set; }
        public JObject GetFriendRequests { get; set; }
        public JObject GetRecommendedFriends { get; set; }
        public JObject GetMarketplaceGroupedOffers { get; set; }
        public JObject GetMarketplaceActiveOffers { get; set; }
        public JObject GetMarketplaceHistory { get; set; }
        public Currencies GetCurrencyData { get; set; }
        public PlatformSettingsModel PlatformSettings { get; set; }
    }
}
