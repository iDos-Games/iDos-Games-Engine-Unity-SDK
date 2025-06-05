using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using System.Collections.Generic;

namespace IDosGames
{
    public class GetAllUserDataResult
    {
        public string Message { get; set; }
        public IGSAuthenticationContext AuthContext { get; set; }
        public GetUserInventoryResult UserInventoryResult { get; set; }
        public TitlePublicConfigurationModel TitlePublicConfiguration { get; set; }
        public GetCatalogItemsResult CatalogItemsResult { get; set; }
        public GetCustomUserDataResult CustomUserDataResult { get; set; }
        public GetLeaderboardResult LeaderboardResult { get; set; }
        public List<string> GetFriends { get; set; }
        public List<string> GetFriendRequests { get; set; }
        public List<string> GetRecommendedFriends { get; set; }
        public Currencies GetCurrencyData { get; set; }
        public PlatformSettingsModel PlatformSettings { get; set; }
        public Dictionary<string, string> ImageData { get; set; }
        public Dictionary<string, string> AssetBundle { get; set; }
        public Dictionary<string, PlayerLeaderboardData> LeaderboardData { get; set; }
    }
}
