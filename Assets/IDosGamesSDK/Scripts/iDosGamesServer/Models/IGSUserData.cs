using IDosGames.ClientModels;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace IDosGames
{
    public static class IGSUserData
    {
        public static GetAllUserDataResult UserAllDataResult { get; set; }
        public static GetUserInventoryResult UserInventory { get; set; }
        public static JObject TitlePublicConfiguration { get; set; }
        public static GetCustomUserDataResult CustomUserData { get; set; }
        public static GetCatalogItemsResult SkinCatalogItems { get; set; }
        public static GetLeaderboardResult Leaderboard { get; set; }
        public static string Friends { get; set; } 
        public static string FriendRequests { get; set; }
        public static string RecommendedFriends { get; set; }
        public static string MarketplaceGroupedOffers { get; set; }
        public static string MarketplaceActiveOffers { get; set; }
        public static string MarketplaceHistory { get; set; }
        public static Currencies Currency { get; set; }
        public static PlatformSettingsModel PlatformSettings { get; set; }
        public static Dictionary<string, string> ImageData { get; set; }
        public static Dictionary<string, string> AssetBundle { get; set; }
    }
}
