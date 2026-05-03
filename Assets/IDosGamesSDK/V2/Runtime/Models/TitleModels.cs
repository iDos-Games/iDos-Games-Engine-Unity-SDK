using System;

namespace IDosGames
{
    [Serializable]
    public class TitleRequest : BaseRequest
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
