using System;
using System.Collections.Generic;

namespace IDosGames
{
    [Serializable]
    public class TitleRequest : BaseRequest
    {
        public List<string> Fields { get; set; }
    }

    [Serializable]
    public class CustomTitleDataResponse
    {
        public Dictionary<string, TitlePublicData> PublicData { get; set; }
    }

    [Serializable]
    public class TitlePublicData
    {
        public string Data { get; set; }
        public int SchemaVersion { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum TitleAction
    {
        GetPublicCustomTitleData,
        GetTitlePublicConfiguration,
        GetItemDefinitions,
        GetServerTime,
        GetPlatformSettings,
        GetCurrencyDefinitions,
    }
}
