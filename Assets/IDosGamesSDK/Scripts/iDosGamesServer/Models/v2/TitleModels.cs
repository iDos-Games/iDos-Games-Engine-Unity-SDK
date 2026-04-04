using System;

namespace IDosGames
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
