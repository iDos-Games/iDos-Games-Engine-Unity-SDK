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
        public string TitleID //=> $"{_titleID}".Trim();
        {
            get => $"{_titleID}".Trim();
            set => _titleID = value.Trim();
        }

        [HideInInspector] public bool DevBuild = false;

        [Space(10)]
        [SerializeField] private string _iosBundleID;
        public string IosBundleID => _iosBundleID;

        [SerializeField] private string _iosAppStoreID;
        public string IosAppStoreID => _iosAppStoreID;

        [Space(5)]
        [SerializeField] private string _androidBundleID;
        public string AndroidBundleID => _androidBundleID;

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
        public string CryptoWalletLink => $"{_serverLink}/api/{_titleID}/Client/CryptoWallet".Trim();
        public string MarketplaceActionsLink => $"{_serverLink}/api/{_titleID}/Client/MarketplaceActions".Trim();
        public string MarketplaceDataLink => $"{_serverLink}/api/{_titleID}/Client/MarketplaceData".Trim();
        public string ValidateIAPSubscriptionLink => $"{_serverLink}/api/{_titleID}/Client/ValidateIAPSubscription".Trim();
        public string FriendSystemLink => $"{_serverLink}/api/{_titleID}/Client/FriendSystem".Trim();
        public string LoginSystemLink => $"{_serverLink}/api/{_titleID}/Client/LoginSystem".Trim();
        public string IgsClientApiLink => $"{_serverLink}/api/{_titleID}/Client/IGSClientApi".Trim();
        public string UserDataSystemLink => $"{_serverLink}/api/{_titleID}/Client/UserDataSystem".Trim();
        public string SpinSystemLink => $"{_serverLink}/api/{_titleID}/Client/SpinSystem".Trim();
        public string ChestSystemLink => $"{_serverLink}/api/{_titleID}/Client/ChestSystem".Trim();
        public string RewardAndProfitSystemLink => $"{_serverLink}/api/{_titleID}/Client/RewardAndProfitSystem".Trim();
        public string ReferralSystemLink => $"{_serverLink}/api/{_titleID}/Client/ReferralSystem".Trim();
        public string EventSystemLink => $"{_serverLink}/api/{_titleID}/Client/EventSystem".Trim();
        public string ShopSystemLink => $"{_serverLink}/api/{_titleID}/Client/ShopSystem".Trim();
        public string DealOfferSystemLink => $"{_serverLink}/api/{_titleID}/Client/DealOfferSystem".Trim();
        public string ValidateIAPLink => $"{_serverLink}/api/{_titleID}/Client/ValidateIAP".Trim();
        public string AdditionalIAPValidateLink => $"{_serverLink}/api/{_titleID}/Client/AdditionalIAPValidate".Trim();
        public string TelegramWebhookLink => $"{_serverLink}/api/{_titleID}/Server/TelegramWebhook".Trim();

        [Space(5)]
        [Header("In App Purchasing")]

        [Space(5)]
        [SerializeField] private bool _mobileIAPEnabled;
        private const string MOBILE_IAP_DEFINE = "IDOSGAMES_MOBILE_IAP";

        [Space(5)]
        [Header("Ad Mediation")]

        [Space(5)]
        public string AdsGramBlockID;

        [Space(5)]
        [SerializeField] private AdMediationPlatform _adMediationPlatform;
        public AdMediationPlatform AdMediationPlatform => _adMediationPlatform;

        private const string AD_MEDIATION_DEFINE_PREFIX = "IDOSGAMES_AD_MEDIATION_";
        private const string IRON_SOURCE_AD_QUALITY_DEFINE_POSTFIX = "LEVELPLAY_AD_QUALITY";

        [SerializeField] private string _mediationAppKeyIOS = "";
        public string MediationAppKeyIOS => _mediationAppKeyIOS;

        [SerializeField] private string _mediationAppKeyAndroid = "";
        public string MediationAppKeyAndroid => _mediationAppKeyAndroid;

        [SerializeField] private BannerPosition _banerPosition = BannerPosition.Bottom;
        public BannerPosition BannerPosition => _banerPosition;

        [SerializeField] private bool _bannerEnabled;
        public bool BannerEnabled => _bannerEnabled;

        [SerializeField] private bool _ironSourceAdQualityEnabled;

        [Space(5)]
        [Header("Telegram Settings")]

        [Space(5)]
        [SerializeField] public string TelegramWebAppLink = "https://t.me/iDos_Games_bot/cube2048";

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

        [Header("Referral System")]
        [Space(5)]
        [SerializeField] private string _referralTrackerLink = "https://idosgames.com/games/";
        public string ReferralTrackerLink => _referralTrackerLink;
        [HideInInspector] public string WebGLUrl;

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

        [HideInInspector][SerializeField] public string IgsAdminApiLink => $"{_serverLink}/api/{_titleID}/Admin/IGSAdminApi".Trim();

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