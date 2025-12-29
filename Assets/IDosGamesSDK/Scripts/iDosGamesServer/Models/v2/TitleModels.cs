using System;

namespace IDosGames.ServerModels
{
    [Serializable]
    public class TitleRequest : IGSRequest
    {

    }

    public enum TitleAction
    {
        GetTitlePublicData,
        GetTitlePublicConfiguration,
        GetCatalogItems,
        GetLeaderboard,
        GetServerTime,
        GetPlatformSettings,
        GetCurrencyData,
    }
}
