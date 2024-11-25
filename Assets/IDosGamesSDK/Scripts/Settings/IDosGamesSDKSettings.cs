using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
#if UNITY_EDITOR
    [CustomEditor(typeof(IDosGamesSDKSettings))]
    public class IDosGamesSDKSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            IDosGamesSDKSettings settings = (IDosGamesSDKSettings)target;

            if (GUILayout.Button("Save Settings"))
            {
                settings.SaveSettings();
                Debug.Log("Settings Saved! Wait for Script Compilation to complete.");
            }

            GUILayout.Space(10);

            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Save Settings"))
            {
                settings.SaveSettings();
                Debug.Log("Settings Saved! Wait for Script Compilation to complete.");
            }
        }
    }
#endif

    public class IDosGamesSDKSettings : ScriptableObject
    {
        private static IDosGamesSDKSettings _instance;

        public static IDosGamesSDKSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<IDosGamesSDKSettings>("Settings/IDosGamesSDKSettings");
                }
                return _instance;
            }
        }

        [Header("App Settings")]
        [Space(5)]
        [SerializeField] private string _titleID;
        public string TitleID
        {
            get => $"{_titleID}".Trim();
            set => _titleID = value.Trim();
        }

        [Space(5)]
        [SerializeField] private string _titleTemplateID = "default";
        public string TitleTemplateID
        {
            get => _titleTemplateID.Trim();
            set => _titleTemplateID = value.Trim();
        }

        [Space(5)]
        [SerializeField] private Platforms _buildForPlatform = Platforms.GooglePlay;
        public Platforms BuildForPlatform => _buildForPlatform;

        public bool DevBuild { get; set; } = false;

        public string IosBundleID { get; set; }
        public string IosAppStoreID { get; set; }
        public string AndroidBundleID { get; set; }

        public bool? AdEnabled { get; set; }
        public string AdsGramBlockID { get; set; }
        public string MediationAppKeyIOS { get; set; }
        public string MediationAppKeyAndroid { get; set; }
        public BannerPosition? BannerPosition { get; set; }
        public bool? BannerEnabled { get; set; }
        public float? PlatformCurrencyPriceInCent { get; set; }
        public string TelegramWebAppLink { get; set; }
        public string ReferralTrackerLink { get; set; }
        public string WebGLUrl { get; set; }

        public bool IsPlaying { get; set; }
        public int PlayTime { get; set; }

        [Space(5)]
        [SerializeField] private bool _debugLogging;
        public bool DebugLogging => _debugLogging;

        [Space(5)]
        [SerializeField] private bool _showLoadingOnExecuteServerFunction;
        public bool ShowLoadingOnExecuteServerFunction => _showLoadingOnExecuteServerFunction;

        [HideInInspector][SerializeField] private string _serverLink;
        public string ServerLink
        {
            get => _serverLink;
            set => _serverLink = value;
        }

        // [HideInInspector]
        public string CryptoWalletLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/CryptoWallet/Transaction".Trim();
        public string MarketplaceLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Marketplace/".Trim();
        public string MarketplaceDataLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/MarketplaceData/".Trim();
        public string ValidateIAPSubscriptionLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/ValidateIAPSubscription".Trim();
        public string FriendSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Friend/".Trim();
        public string AuthenticationLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Authentication/".Trim();
        public string UserDataSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/UserData/".Trim();
        public string SpinSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Spin/".Trim();
        public string ChestSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Chest/".Trim();
        public string RewardAndProfitSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Reward/".Trim();
        public string ReferralSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Referral/".Trim();
        public string EventSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/TimeLimitedEvent/".Trim();
        public string ShopSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Shop/".Trim();
        public string DealOfferSystemLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/DealOffer/".Trim();
        public string ValidateIAPLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/ValidateIAP".Trim();
        public string PurchaseLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Client/Purchase/".Trim();
        public string TelegramWebhookLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Server/TelegramWebhook".Trim();

        [Space(5)]
        [Header("In App Purchasing")]

        [Space(5)]
        [SerializeField] private bool _mobileIAPEnabled;
        private const string MOBILE_IAP_DEFINE = "IDOSGAMES_MOBILE_IAP";

        [Space(5)]
        [Header("Ad Mediation")]

        [Space(5)]
        [SerializeField] private AdMediationPlatform _adMediationPlatform;
        public AdMediationPlatform AdMediationPlatform => _adMediationPlatform;

        private const string AD_MEDIATION_DEFINE_PREFIX = "IDOSGAMES_AD_MEDIATION_";
        private const string IRON_SOURCE_AD_QUALITY_DEFINE_POSTFIX = "LEVELPLAY_AD_QUALITY";

        [SerializeField] private bool _ironSourceAdQualityEnabled;

        [Space(5)]
        [Header("Analytics")]

        [Space(5)]
        [SerializeField] private bool _firebaseAnalyticsEnabled;
        private const string FIREBASE_ANALYTICS_DEFINE = "IDOSGAMES_FIREBASE_ANALYTICS";

        [Space(5)]
        [SerializeField] private bool _appMetricaEnabled;
        private const string APP_METRICA_DEFINE = "IDOSGAMES_APP_METRICA";

#if IDOSGAMES_APP_METRICA
        [Space(5)]
        [SerializeField] private string _appMetricaApiKey;
        public string AppMetricaApiKey => _appMetricaApiKey;
#endif

        [Header("Account")]
        [Space(5)]
        [SerializeField] private bool _iOSAccountDeletionEnabled;
        public bool IOSAccountDeletionEnabled => _iOSAccountDeletionEnabled;

        [SerializeField] private bool _AndroidAccountDeletionEnabled;
        public bool AndroidAccountDeletionEnabled => _AndroidAccountDeletionEnabled;

        [Space(5)]
        [Header("Push Notifications")]
        [Space(5)]
        [SerializeField] private bool _pushNotificationsAndroidEnabled;
        [SerializeField] private bool _pushNotificationsIosEnabled;
        private const string PUSH_NOTIFICATIONS_ANDROID_DEFINE = "IDOSGAMES_NOTIFICATIONS_ANDROID";
        private const string PUSH_NOTIFICATIONS_IOS_DEFINE = "IDOSGAMES_NOTIFICATIONS_IOS";

        [Space(5)]
        [Header("MODULES:")]

        [Header("Marketplace")]
        [Space(5)]
        [SerializeField] private bool _marketplaceEnabled;
        private const string MARKETPLACE_DEFINE = "IDOSGAMES_MARKETPLACE";

        [Header("Crypto Wallet")]
        [Space(5)]
        [SerializeField] private bool _cryptoWalletEnabled;
        private const string CRYPTO_WALLET_DEFINE = "IDOSGAMES_CRYPTO_WALLET";


#if UNITY_EDITOR

        [HideInInspector][SerializeField] private string _developerSecretKey;
        public string DeveloperSecretKey
        {
            get => _developerSecretKey;
            set => _developerSecretKey = value;
        }

        [HideInInspector][SerializeField] public string IgsAdminApiLink => $"{_serverLink}/api/{_titleTemplateID}/{_titleID}/Admin/IGSAdminApi".Trim();

        [HideInInspector] public string WebGLBuildPath = "Assets/WebGLBuild/";

        [HideInInspector] public bool ClearDirectory = true;

        public void SaveSettings()
        {
            SaveState(_mobileIAPEnabled, MOBILE_IAP_DEFINE);
            SaveState(_adMediationPlatform, AD_MEDIATION_DEFINE_PREFIX, Enum.GetValues(typeof(AdMediationPlatform)).Cast<AdMediationPlatform>());
            SaveState(_ironSourceAdQualityEnabled, $"{AD_MEDIATION_DEFINE_PREFIX}{IRON_SOURCE_AD_QUALITY_DEFINE_POSTFIX}");
            SaveState(_pushNotificationsAndroidEnabled, PUSH_NOTIFICATIONS_ANDROID_DEFINE);
            SaveState(_pushNotificationsIosEnabled, PUSH_NOTIFICATIONS_IOS_DEFINE);
            SaveState(_marketplaceEnabled, MARKETPLACE_DEFINE);
            SaveState(_firebaseAnalyticsEnabled, FIREBASE_ANALYTICS_DEFINE);
            SaveState(_appMetricaEnabled, APP_METRICA_DEFINE);
            SaveState(_cryptoWalletEnabled, CRYPTO_WALLET_DEFINE);
        }

        private readonly BuildTargetGroup[] platforms = {
          BuildTargetGroup.iOS, BuildTargetGroup.Android, BuildTargetGroup.WebGL, BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.tvOS, BuildTargetGroup.PS4, BuildTargetGroup.PS5, BuildTargetGroup.XboxOne, BuildTargetGroup.Switch, BuildTargetGroup.VisionOS
        };

        private void SaveState<T>(T newState, string definePrefix, IEnumerable<T> enumValues) where T : Enum
        {
            foreach (var platform in platforms)
            {
                var allDefines = GetAllScriptingDefineSymbolsExcept(platform, enumValues.Select(e => $"{definePrefix}{e.ToString().ToUpper()}").ToArray());

                if (!EqualityComparer<T>.Default.Equals(newState, default(T)))
                    allDefines.Add($"{definePrefix}{newState.ToString().ToUpper()}");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, string.Join(";", allDefines.ToArray()));
            }
        }

        private void SaveState(bool newState, string defineSymbol)
        {
            foreach (var platform in platforms)
            {
                var allDefines = GetAllScriptingDefineSymbolsExcept(platform, defineSymbol);

                if (newState)
                    allDefines.Add(defineSymbol);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, string.Join(";", allDefines.ToArray()));
            }
        }

        private List<string> GetAllScriptingDefineSymbolsExcept(BuildTargetGroup targetPlatform, params string[] exceptSymbols)
        {
            string allDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetPlatform);
            var exceptSymbolsSet = new HashSet<string>(exceptSymbols);
            return allDefinesString.Split(';').Where(d => !exceptSymbolsSet.Contains(d)).ToList();
        }

        [MenuItem("Tools/iDos Games SDK/2. General Settings")]
        private static void OpenSettings()
        {
            Selection.activeObject = Instance;
        }

        [MenuItem("Tools/iDos Games SDK/4. Object Inspection Data")]
        private static void SelectObjectInspection()
        {
            string configPath = "Assets/IDosGamesSDK/Resources/Data/ObjectInspection.asset";
            var config = AssetDatabase.LoadAssetAtPath<ObjectInspection>(configPath);

            if (config == null)
            {
                Debug.LogError("ObjectInspection.asset not found.");
                return;
            }

            Selection.activeObject = config;
        }

#endif
    }
}