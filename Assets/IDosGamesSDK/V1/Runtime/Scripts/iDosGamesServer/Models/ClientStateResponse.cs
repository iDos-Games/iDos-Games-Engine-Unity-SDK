using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using System.Collections.Generic;

namespace IDosGames
{
    public class ClientStateResponse
    {
        public IGSAuthenticationContext AuthContext { get; set; }
        public GetUserInventoryResult UserInventoryResult { get; set; }

        public TitlePublicConfigurationModel TitlePublicConfiguration { get; set; }
        public GetCatalogItemsResult CatalogItemsResult { get; set; }
        public GetCustomUserDataResult CustomUserDataResult { get; set; }
        public GetLeaderboardResult LeaderboardResult { get; set; }
        public Currencies GetCurrencyData { get; set; }
        public PlatformSettingsModel PlatformSettings { get; set; }
        public Dictionary<string, PlayerLeaderboardData> LeaderboardData { get; set; }
        public Dictionary<string, object> TitlePublicData { get; set; }
    }
}
