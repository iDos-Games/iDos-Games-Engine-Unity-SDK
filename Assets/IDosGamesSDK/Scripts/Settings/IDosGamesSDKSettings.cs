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

#if IDOSGAMES_CRYPTO_WALLET
        [Space(5)]
        [SerializeField] private bool _localHotWalletEnabled = true;
        public bool LocalHotWalletEnabled => _localHotWalletEnabled;

        [SerializeField] private string _hotWalletAddress;
        public string HotWalletAddress => _hotWalletAddress;

        [SerializeField] private int _gasPrice = 2;
        public int GasPrice => _gasPrice;

        [Space(5)]
        [SerializeField] private BlockchainNetwork _chainId;
        public BlockchainNetwork ChainID => _chainId;

        [SerializeField] private string _rpcUrl = "https://rpc-testnet.idos.games";
        public string RpcUrl => _rpcUrl;

        [SerializeField] private string _blockchainExplorerUrl = "https://igcscan.com/tx/";
        public string BlockchainExplorerUrl => _blockchainExplorerUrl;

        [Space(10)]
        [SerializeField] private bool _customContractsEnabled;
        public bool CustomContractsEnabled => _customContractsEnabled;

        [Space(5)]
        [SerializeField] private string _firstTokenTicker = "MEM";
        public string FirstTokenTicker => _firstTokenTicker;

        [SerializeField] private string _firstTokenContractAddress = "0x19C86d475bdca14Ff3183D74500A7aD42fbbf515";
        public string FirstTokenContractAddress => _firstTokenContractAddress;

        [SerializeField] private string _firstTokenContractAbi = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"previousAdminRole\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"newAdminRole\",\"type\":\"bytes32\"}],\"name\":\"RoleAdminChanged\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"}],\"name\":\"RoleGranted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"}],\"name\":\"RoleRevoked\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"DEFAULT_ADMIN_ROLE\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"MINTER_ROLE\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"burn\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"burnFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"subtractedValue\",\"type\":\"uint256\"}],\"name\":\"decreaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"}],\"name\":\"getRoleAdmin\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"grantRole\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"hasRole\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"addedValue\",\"type\":\"uint256\"}],\"name\":\"increaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"mint\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"renounceRole\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"revokeRole\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        public string FirstTokenContractAbi => _firstTokenContractAbi;

        [Space(10)]
        [SerializeField] private string _secondTokenTicker = "IGT";
        public string SecondTokenTicker => _secondTokenTicker;

        [SerializeField] private string _secondTokenContractAddress = "0x05C68e2681D1558e9c2D54fEf5c953e27Ee31A62";
        public string SecondTokenContractAddress => _secondTokenContractAddress;

        [SerializeField] private string _secondTokenContractAbi = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"subtractedValue\",\"type\":\"uint256\"}],\"name\":\"decreaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"addedValue\",\"type\":\"uint256\"}],\"name\":\"increaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        public string SecondTokenContractAbi => _secondTokenContractAbi;

        [Space(10)]
        [SerializeField] private string _nftContractAddress = "0x9A76B801E58f6e05B6c1D116b16832090f8B07C3";
        public string NftContractAddress => _nftContractAddress;

        [SerializeField] private string _nftContractAbi = "[{\"inputs\":[{\"internalType\":\"string\",\"name\":\"_name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"_symbol\",\"type\":\"string\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"ApprovalForAll\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"previousOwner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"OwnershipTransferred\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Paused\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"ids\",\"type\":\"uint256[]\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"values\",\"type\":\"uint256[]\"}],\"name\":\"TransferBatch\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"TransferSingle\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"string\",\"name\":\"value\",\"type\":\"string\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"}],\"name\":\"URI\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Unpaused\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address[]\",\"name\":\"accounts\",\"type\":\"address[]\"},{\"internalType\":\"uint256[]\",\"name\":\"ids\",\"type\":\"uint256[]\"}],\"name\":\"balanceOfBatch\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"burn\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint256[]\",\"name\":\"ids\",\"type\":\"uint256[]\"},{\"internalType\":\"uint256[]\",\"name\":\"values\",\"type\":\"uint256[]\"}],\"name\":\"burnBatch\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"}],\"name\":\"exists\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getBalance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_uri\",\"type\":\"string\"},{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"}],\"name\":\"gift_mint\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"}],\"name\":\"isApprovedForAll\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_uri\",\"type\":\"string\"}],\"name\":\"mint\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256[]\",\"name\":\"amounts\",\"type\":\"uint256[]\"},{\"internalType\":\"string[]\",\"name\":\"_uris\",\"type\":\"string[]\"}],\"name\":\"mintBatch\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"pause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"paused\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"renounceOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256[]\",\"name\":\"ids\",\"type\":\"uint256[]\"},{\"internalType\":\"uint256[]\",\"name\":\"amounts\",\"type\":\"uint256[]\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeBatchTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"setApprovalForAll\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"tokenCounts\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"}],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"transferOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"unpause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"name\":\"uri\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"withdrawBalance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        public string NftContractAbi => _nftContractAbi;

#endif

#if UNITY_EDITOR

        [HideInInspector][SerializeField] private string _developerSecretKey;
        public string DeveloperSecretKey
        {
            get => _developerSecretKey;
            set => _developerSecretKey = value;
        }

        [HideInInspector][SerializeField] public string IgsAdminApiLink => $"{_serverLink}/api/{_titleID}/Admin/IGSAdminApi".Trim();

        [HideInInspector] public string WebGLBuildPath = "Assets/WebGLBuild/";
        
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