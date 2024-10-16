using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames
{
    public class PlatformSettingsModel
    {
        public Settings GooglePlay { get; set; }
        public Settings AppleAppStore { get; set; }
        public Settings Telegram { get; set; }
        public Settings Web { get; set; }
        public Settings Custom { get; set; }
    }

    public class Settings
    {
        public string BundleID { get; set; }
        public string AppStoreID { get; set; }
        public float? PlatformCurrencyPriceInCent { get; set; }
        public AdSettings AdSettings { get; set; }
        public ApplicationUpdate ApplicationUpdate { get; set; }
        public string AnalyticSettings { get; set; }
        public ReferralSystemSettings ReferralSystemSettings { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BannerPosition
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public class AdSettings
    {
        public bool? AdEnabled { get; set; }
        public string AppKey { get; set; }
        public bool? BannerEnabled { get; set; }
        public BannerPosition? BanerPosition { get; set; }
        public string BlockID { get; set; }
    }

    public class ApplicationUpdate
    {
        public string Version { get; set; }
        public UpdateUrgency Urgency { get; set; }
        public string Link { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum UpdateUrgency
    {
        NoUpdates,
        Optional,
        Critical
    }

    public class ReferralSystemSettings
    {
        public string ReferralAppLink { get; set; }
    }
}
