using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using System.Collections.Generic;

namespace IDosGames
{
    public static class IGSUserData
    {
        public static GetAllUserDataResult UserAllDataResult { get; set; }
        public static GetUserInventoryResult UserInventory { get; set; }
        public static TitlePublicConfigurationModel TitlePublicConfiguration { get; set; }
        public static GetCustomUserDataResult CustomUserData { get; set; }
        public static GetCatalogItemsResult CatalogItemsResult { get; set; }
        public static GetLeaderboardResult Leaderboard { get; set; }
        public static List<string> Friends { get; set; } 
        public static List<string> FriendRequests { get; set; }
        public static List<string> RecommendedFriends { get; set; }
        public static string MarketplaceGroupedOffers { get; set; }
        public static string MarketplaceActiveOffers { get; set; }
        public static string MarketplaceHistory { get; set; }
        public static Currencies Currency { get; set; }
        public static PlatformSettingsModel PlatformSettings { get; set; }
        public static Dictionary<string, string> ImageData { get; set; }
        public static Dictionary<string, string> AssetBundle { get; set; }
        public static Dictionary<string, PlayerLeaderboardData> LeaderboardData { get; set; }
    }
}
