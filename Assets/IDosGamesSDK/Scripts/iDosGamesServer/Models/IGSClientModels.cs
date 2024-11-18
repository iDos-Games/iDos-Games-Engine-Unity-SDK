using IDosGames.SharedModels;
using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    [Serializable]
    public class GetUserInventoryResult : IGSResultCommon
    {
        public List<ItemInstance> Inventory;
        public Dictionary<string, int> VirtualCurrency;
        public Dictionary<string, VirtualCurrencyRechargeTime> VirtualCurrencyRechargeTimes;
    }

    [Serializable]
    public class ItemInstance : IGSBaseModel
    {
        public string Annotation;
        public List<string> BundleContents;
        public string BundleParent;
        public string CatalogVersion;
        public string CustomData;
        public string DisplayName;
        public DateTime? Expiration;
        public string ItemClass;
        public string ItemId;
        public string ItemInstanceId;
        public DateTime? PurchaseDate;
        public int? RemainingUses;
        public string UnitCurrency;
        public uint UnitPrice;
        public int? UsesIncrementedBy;
    }

    [Serializable]
    public class VirtualCurrencyRechargeTime : IGSBaseModel
    {
        public int RechargeMax;
        public DateTime RechargeTime;
        public int SecondsToRecharge;
    }

    [Serializable]
    public class GetUserInventoryRequest : IGSRequestCommon
    {
        public Dictionary<string, string> CustomTags;
    }

    [Serializable]
    public class GetCustomUserDataResult : IGSResultCommon
    {
        public Dictionary<string, UserDataRecord> Data;
        public uint DataVersion;
    }

    [Serializable]
    public class UserDataRecord : IGSBaseModel
    {
        public DateTime LastUpdated;
        public UserDataPermission? Permission;
        public UserDataType? DataType;
        public string Value;
    }

    public enum UserDataPermission
    {
        Private,
        Public
    }

    public enum UserDataType
    {
        ReadOnly,
        ClientModify,
        Internal
    }

    [Serializable]
    public class GetUserDataRequest : IGSRequestCommon
    {
        public uint? IfChangedFromDataVersion;
        public List<string> Keys;
        public string UserID;
    }

    [Serializable]
    public class GetCatalogItemsRequest : IGSRequestCommon
    {
        public string CatalogVersion;
    }

    [Serializable]
    public class GetCatalogItemsResult : IGSResultCommon
    {
        public List<CatalogItem> Catalog;
    }

    [Serializable]
    public class CatalogItem : IGSBaseModel
    {
        public CatalogItemBundleInfo Bundle;
        public bool CanBecomeCharacter;
        public string CatalogVersion;
        public CatalogItemConsumableInfo Consumable;
        public CatalogItemContainerInfo Container;
        public string CustomData;
        public string Description;
        public string DisplayName;
        public int InitialLimitedEditionCount;
        public bool IsLimitedEdition;
        public bool IsStackable;
        public bool IsTradable;
        public string ItemClass;
        public string ItemId;
        public string ItemImageUrl;
        public Dictionary<string, uint> RealCurrencyPrices;
        public List<string> Tags;
        public Dictionary<string, uint> VirtualCurrencyPrices;
    }

    [Serializable]
    public class CatalogItemBundleInfo : IGSBaseModel
    {
        public List<string> BundledItems;
        public List<string> BundledResultTables;
        public Dictionary<string, uint> BundledVirtualCurrencies;
    }

    [Serializable]
    public class CatalogItemConsumableInfo : IGSBaseModel
    {
        public uint? UsageCount;
        public uint? UsagePeriod;
        public string UsagePeriodGroup;
    }

    [Serializable]
    public class CatalogItemContainerInfo : IGSBaseModel
    {
        public List<string> ItemContents;
        public string KeyItemId;
        public List<string> ResultTableContents;
        public Dictionary<string, uint> VirtualCurrencyContents;
    }

    [Serializable]
    public class ExecuteCloudScriptResult : IGSResultCommon
    {
        public int APIRequestsIssued;
        public ScriptExecutionError Error;
        public double ExecutionTimeSeconds;
        public string FunctionName;
        public object FunctionResult;
        public bool? FunctionResultTooLarge;
        public int HttpRequestsIssued;
        public List<LogStatement> Logs;
        public bool? LogsTooLarge;
        public uint MemoryConsumedBytes;
        public double ProcessorTimeSeconds;
        public int Revision;
    }

    [Serializable]
    public class ScriptExecutionError : IGSBaseModel
    {
        public string Error;
        public string Message;
        public string StackTrace;
    }

    [Serializable]
    public class LogStatement : IGSBaseModel
    {
        public object Data;
        public string Level;
        public string Message;
    }

    [Serializable]
    public class GetLeaderboardResult : IGSResultCommon
    {
        public List<PlayerLeaderboardEntry> Leaderboard;
        public DateTime? NextReset;
        public int Version;
    }

    [Serializable]
    public class PlayerLeaderboardEntry : IGSBaseModel
    {
        public string UserName;
        public string UserID;
        public int Position;
        public PlayerProfileModel Profile;
        public int StatValue;
    }

    [Serializable]
    public class PlayerProfileModel : IGSBaseModel
    {
        public List<AdCampaignAttributionModel> AdCampaignAttributions;
        public string AvatarUrl;
        public DateTime? BannedUntil;
        public List<ContactEmailInfoModel> ContactEmailAddresses;
        public DateTime? Created;
        public string DisplayName;
        public List<string> ExperimentVariants;
        public DateTime? LastLogin;
        public List<LinkedPlatformAccountModel> LinkedAccounts;
        public List<LocationModel> Locations;
        public List<MembershipModel> Memberships;
        public LoginIdentityProvider? Origination;
        public string PlayerId;
        public string PublisherId;
        public List<PushNotificationRegistrationModel> PushNotificationRegistrations;
        public List<StatisticModel> Statistics;
        public List<TagModel> Tags;
        public string TitleId;
        public uint? TotalValueToDateInUSD;
        public List<ValueToDateModel> ValuesToDate;
    }

    [Serializable]
    public class AdCampaignAttributionModel : IGSBaseModel
    {
        public DateTime AttributedAt;
        public string CampaignId;
        public string Platform;
    }

    [Serializable]
    public class ContactEmailInfoModel : IGSBaseModel
    {
        public string EmailAddress;
        public string Name;
        public EmailVerificationStatus? VerificationStatus;
    }

    public enum EmailVerificationStatus
    {
        Unverified,
        Pending,
        Confirmed
    }

    [Serializable]
    public class LinkedPlatformAccountModel : IGSBaseModel
    {
        public string Email;
        public LoginIdentityProvider? Platform;
        public string PlatformUserId;
        public string Username;
    }

    public enum LoginIdentityProvider
    {
        Unknown,
        IGS,
        Custom,
        GameCenter,
        GooglePlay,
        Steam,
        XBoxLive,
        PSN,
        Kongregate,
        Facebook,
        IOSDevice,
        AndroidDevice,
        Twitch,
        WindowsHello,
        GameServer,
        CustomServer,
        NintendoSwitch,
        FacebookInstantGames,
        OpenIdConnect,
        Apple,
        NintendoSwitchAccount,
        GooglePlayGames
    }

    [Serializable]
    public class LocationModel : IGSBaseModel
    {
        public string City;
        public ContinentCode? ContinentCode;
        public CountryCode? CountryCode;
        public double? Latitude;
        public double? Longitude;
    }

    public enum ContinentCode
    {
        AF,
        AN,
        AS,
        EU,
        NA,
        OC,
        SA,
        Unknown
    }

    public enum CountryCode
    {
        AF,
        AX,
        AL,
        DZ,
        AS,
        AD,
        AO,
        AI,
        AQ,
        AG,
        AR,
        AM,
        AW,
        AU,
        AT,
        AZ,
        BS,
        BH,
        BD,
        BB,
        BY,
        BE,
        BZ,
        BJ,
        BM,
        BT,
        BO,
        BQ,
        BA,
        BW,
        BV,
        BR,
        IO,
        BN,
        BG,
        BF,
        BI,
        KH,
        CM,
        CA,
        CV,
        KY,
        CF,
        TD,
        CL,
        CN,
        CX,
        CC,
        CO,
        KM,
        CG,
        CD,
        CK,
        CR,
        CI,
        HR,
        CU,
        CW,
        CY,
        CZ,
        DK,
        DJ,
        DM,
        DO,
        EC,
        EG,
        SV,
        GQ,
        ER,
        EE,
        ET,
        FK,
        FO,
        FJ,
        FI,
        FR,
        GF,
        PF,
        TF,
        GA,
        GM,
        GE,
        DE,
        GH,
        GI,
        GR,
        GL,
        GD,
        GP,
        GU,
        GT,
        GG,
        GN,
        GW,
        GY,
        HT,
        HM,
        VA,
        HN,
        HK,
        HU,
        IS,
        IN,
        ID,
        IR,
        IQ,
        IE,
        IM,
        IL,
        IT,
        JM,
        JP,
        JE,
        JO,
        KZ,
        KE,
        KI,
        KP,
        KR,
        KW,
        KG,
        LA,
        LV,
        LB,
        LS,
        LR,
        LY,
        LI,
        LT,
        LU,
        MO,
        MK,
        MG,
        MW,
        MY,
        MV,
        ML,
        MT,
        MH,
        MQ,
        MR,
        MU,
        YT,
        MX,
        FM,
        MD,
        MC,
        MN,
        ME,
        MS,
        MA,
        MZ,
        MM,
        NA,
        NR,
        NP,
        NL,
        NC,
        NZ,
        NI,
        NE,
        NG,
        NU,
        NF,
        MP,
        NO,
        OM,
        PK,
        PW,
        PS,
        PA,
        PG,
        PY,
        PE,
        PH,
        PN,
        PL,
        PT,
        PR,
        QA,
        RE,
        RO,
        RU,
        RW,
        BL,
        SH,
        KN,
        LC,
        MF,
        PM,
        VC,
        WS,
        SM,
        ST,
        SA,
        SN,
        RS,
        SC,
        SL,
        SG,
        SX,
        SK,
        SI,
        SB,
        SO,
        ZA,
        GS,
        SS,
        ES,
        LK,
        SD,
        SR,
        SJ,
        SZ,
        SE,
        CH,
        SY,
        TW,
        TJ,
        TZ,
        TH,
        TL,
        TG,
        TK,
        TO,
        TT,
        TN,
        TR,
        TM,
        TC,
        TV,
        UG,
        UA,
        AE,
        GB,
        US,
        UM,
        UY,
        UZ,
        VU,
        VE,
        VN,
        VG,
        VI,
        WF,
        EH,
        YE,
        ZM,
        ZW,
        Unknown
    }

    [Serializable]
    public class MembershipModel : IGSBaseModel
    {
        public bool IsActive;
        public DateTime MembershipExpiration;
        public string MembershipId;
        public DateTime? OverrideExpiration;
        public List<SubscriptionModel> Subscriptions;
    }

    [Serializable]
    public class SubscriptionModel : IGSBaseModel
    {
        public DateTime Expiration;
        public DateTime InitialSubscriptionTime;
        public bool IsActive;
        public SubscriptionProviderStatus? Status;
        public string SubscriptionId;
        public string SubscriptionItemId;
        public string SubscriptionProvider;
    }

    public enum SubscriptionProviderStatus
    {
        NoError,
        Cancelled,
        UnknownError,
        BillingError,
        ProductUnavailable,
        CustomerDidNotAcceptPriceChange,
        FreeTrial,
        PaymentPending
    }

    public enum PushNotificationPlatform
    {
        ApplePushNotificationService,
        GoogleCloudMessaging
    }

    [Serializable]
    public class PushNotificationRegistrationModel : IGSBaseModel
    {
        public string NotificationEndpointARN;
        public PushNotificationPlatform? Platform;
    }

    [Serializable]
    public class StatisticModel : IGSBaseModel
    {
        public string Name;
        public int Value;
        public int Version;
    }

    [Serializable]
    public class TagModel : IGSBaseModel
    {
        public string TagValue;
    }

    [Serializable]
    public class ValueToDateModel : IGSBaseModel
    {
        public string Currency;
        public uint TotalValue;
        public string TotalValueAsDecimal;
    }

    [Serializable]
    public class GetLeaderboardRequest : IGSRequestCommon
    {
        public Dictionary<string, string> CustomTags;
        public int? MaxResultsCount;
        public PlayerProfileViewConstraints ProfileConstraints;
        public int StartPosition;
        public string StatisticName;
        public int? Version;
    }

    [Serializable]
    public class PlayerProfileViewConstraints : IGSBaseModel
    {
        public bool ShowAvatarUrl;
        public bool ShowBannedUntil;
        public bool ShowCampaignAttributions;
        public bool ShowContactEmailAddresses;
        public bool ShowCreated;
        public bool ShowDisplayName;
        public bool ShowExperimentVariants;
        public bool ShowLastLogin;
        public bool ShowLinkedAccounts;
        public bool ShowLocations;
        public bool ShowMemberships;
        public bool ShowOrigination;
        public bool ShowPushNotificationRegistrations;
        public bool ShowStatistics;
        public bool ShowTags;
        public bool ShowTotalValueToDateInUsd;
        public bool ShowValuesToDate;
    }
}
