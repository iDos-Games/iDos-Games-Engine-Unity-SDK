using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IDosGames
{
    public class AuthService
    {
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 100;
        private const string EMAIL_REGEX = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        private static string SAVED_AUTH_TYPE_KEY => "Saved_AuthType" + GetTitleID();
        private static string SAVED_AUTH_EMAIL_KEY => "Saved_Auth_Email" + GetTitleID();
        private static string SAVED_AUTH_PASSWORD_KEY => "Saved_Auth_Password" + GetTitleID();

        public static WebGLPlatform WebGLPlatform { get; set; }
        public static AuthType LastAuthType => (AuthType)PlayerPrefs.GetInt(SAVED_AUTH_TYPE_KEY, (int)AuthType.None);
        public static bool IsLoggedIn => LastAuthType != AuthType.Device && LastAuthType != AuthType.None;
        public static string SavedEmail => PlayerPrefs.GetString(SAVED_AUTH_EMAIL_KEY, string.Empty);
        public static string SavedPassword => PlayerPrefs.GetString(SAVED_AUTH_PASSWORD_KEY, string.Empty);

        public static string UserID { get; private set; }
        public static string ClientSessionTicket { get; private set; }
        public static string EntityToken { get; private set; }
        public static IGSAuthenticationContext AuthContext { get; private set; }

        public static string TelegramInitData { get; set; }

        private static AuthService _instance;

        public static event Action RequestSent;
        public static event Action LoggedIn;
        public static event Action PlatformSettingsUpdated;

        public static AuthService Instance => _instance;

        private AuthService()
        {
            _instance = this;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new();
        }

        public static string GetTitleID()
        {
            var settings = IDosGamesSDKSettings.Instance;
            if (settings == null)
            {
                Debug.LogWarning("IDosGamesSDKSettings is not initialized properly.");
                return "0";
            }

            string titleID = settings.TitleID;

            if (titleID == "0")
            {
#if UNITY_WEBGL
                string fullUrl = WebSDK.webAppLink;
                if (!string.IsNullOrEmpty(fullUrl))
                {
                    int queryStartIndex = fullUrl.IndexOf('?');
                    if (queryStartIndex != -1 && queryStartIndex < fullUrl.Length - 1)
                    {
                        string queryString = fullUrl.Substring(queryStartIndex + 1);
                        string[] queryParams = queryString.Split('&');

                        foreach (string param in queryParams)
                        {
                            string[] keyValue = param.Split('=');
                            if (keyValue.Length == 2 && keyValue[0] == "titleID")
                            {
                                titleID = keyValue[1];
                                break;
                            }
                        }
                    }
                }
#endif
            }

            return titleID;
        }

        public async void LoginWithDeviceID(Action<GetAllUserDataResult> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            RequestSent?.Invoke();
            try
            {
                GetAllUserDataResult result = null;

                if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Telegram)
                {
                    result = await IGSService.LoginWithTelegram(TelegramInitData);
                }
                else
                {
                    result = await IGSService.LoginWithDeviceID();
                }

                if (result != null) //&& result.AuthContext != null && !string.IsNullOrEmpty(result.AuthContext.ClientSessionTicket)
                {
                    SetCredentials(result);
                    SaveAuthType(AuthType.Device);
                    //ClearEmailAndPassword();

                    resultCallback?.Invoke(result);
                    LoggedIn?.Invoke();
                }
                else
                {
                    IGSClientAPI.OnIGSError("Invalid result: " + result.Message, errorCallback, retryCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback, retryCallback);
            }
        }

        public void LogOut()
        {
            Loading.SwitchToLoginScene(); //LoginWithDeviceID(resultCallback, errorCallback, retryCallback);
        }

        public async void LoginWithEmailAddress(string email, string password, Action<GetAllUserDataResult> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            RequestSent?.Invoke();

            try
            {
                GetAllUserDataResult result = await IGSService.LoginWithEmail(email, password);
                if (result != null && result.AuthContext != null && !string.IsNullOrEmpty(result.AuthContext.ClientSessionTicket))
                {
                    SetCredentials(result);
                    SaveAuthType(AuthType.Device);
                    SaveEmailAndPassword(email, password);

                    resultCallback?.Invoke(result);
                    LoggedIn?.Invoke();
                }
                else
                {
                    IGSClientAPI.OnIGSError("Invalid result: " + result.Message, errorCallback, retryCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback, retryCallback);
            }
        }

        public async void AddUsernamePassword(string email, string password, Action<string> resultCallback = null, Action<string> errorCallback = null)
        {
            RequestSent?.Invoke();

            string result = await IGSService.AddEmailAndPassword(UserID, email, password, AuthContext.ClientSessionTicket);
            if (!string.IsNullOrEmpty(result))
            {
                SaveAuthType(AuthType.Email);
                SaveEmailAndPassword(email, password);
                resultCallback?.Invoke(result);

                LoggedIn?.Invoke();
            }
            else
            {
                IGSClientAPI.OnIGSError(result, errorCallback);
            }

        }

        public async void RegisterUserByEmail(string email, string password, Action<GetAllUserDataResult> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            RequestSent?.Invoke();

            try
            {
                GetAllUserDataResult result = await IGSService.RegisterUserByEmail(email, password);
                if (result != null && result.AuthContext != null && !string.IsNullOrEmpty(result.AuthContext.ClientSessionTicket))
                {
                    SetCredentials(result);
                    SaveAuthType(AuthType.Device);
                    SaveEmailAndPassword(email, password);

                    resultCallback?.Invoke(result);
                    LoggedIn?.Invoke();
                }
                else
                {
                    IGSClientAPI.OnIGSError("Invalid result: " + result.Message, errorCallback, retryCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback, retryCallback);
            }
        }

        public async void SendAccountRecoveryEmail(string email, Action<string> resultCallback = null, Action<string> errorCallback = null)
        {
            RequestSent?.Invoke();

            string result = await IGSService.ForgotPassword(email);
            if (!string.IsNullOrEmpty(result))
            {
                resultCallback?.Invoke(result);
            }
            else
            {
                IGSClientAPI.OnIGSError(result, errorCallback);
            }
        }

        public async void SendResetPassword(string resetToken, string password, Action<string> resultCallback = null, Action<string> errorCallback = null)
        {
            RequestSent?.Invoke();

            string result = await IGSService.ResetPassword(resetToken, password);
            if (!string.IsNullOrEmpty(result))
            {
                resultCallback?.Invoke(result);
            }
            else
            {
                IGSClientAPI.OnIGSError(result, errorCallback);
            }
        }

        public void DeleteTitlePlayerAccount(Action resultCallback = null)
        {
            /*
            IGServerAPI.ExecuteFunction(
                functionName: CloudFunctionName.DELETE_TITLE_PLAYER_ACCOUNT,
                resultCallback: (result) => resultCallback?.Invoke(),
                notConnectionErrorCallback: ShowErrorMessage
                );
            */
        }

        private void SetCredentials(GetAllUserDataResult result)
        {
            UserID = result.AuthContext.UserID;
            ClientSessionTicket = result.AuthContext.ClientSessionTicket;
            EntityToken = result.AuthContext.EntityToken;
            AuthContext = new IGSAuthenticationContext(result.AuthContext.ClientSessionTicket, result.AuthContext.EntityToken, result.AuthContext.UserID, result.AuthContext.EntityId, result.AuthContext.EntityType, result.AuthContext.TelemetryKey);

            IGSUserData.UserAllDataResult = result;

            void UpdateProperty<T>(T resultProperty, Action<T> updateAction) => updateAction?.Invoke(resultProperty);

            UpdateProperty(result.CatalogItemsResult, value => IGSUserData.CatalogItemsResult = value);
            UpdateProperty(result.UserInventoryResult, value => IGSUserData.UserInventory = value);
            UpdateProperty(result.TitlePublicConfiguration, value => IGSUserData.TitlePublicConfiguration = value);
            UpdateProperty(result.CustomUserDataResult, value => IGSUserData.CustomUserData = value);
            UpdateProperty(result.LeaderboardResult, value => IGSUserData.Leaderboard = value);
            UpdateProperty(result.GetFriends, value => IGSUserData.Friends = value);
            UpdateProperty(result.GetFriendRequests, value => IGSUserData.FriendRequests = value);
            UpdateProperty(result.GetRecommendedFriends, value => IGSUserData.RecommendedFriends = value);
            UpdateProperty(result.GetCurrencyData, value => IGSUserData.Currency = value);
            UpdateProperty(result.PlatformSettings, value => IGSUserData.PlatformSettings = value);
            UpdateProperty(result.ImageData, value => IGSUserData.ImageData = value);
            //UpdateProperty(result.GetMarketplaceGroupedOffers, value => IGSUserData.MarketplaceGroupedOffers = value?.ToString());
            //UpdateProperty(result.GetMarketplaceActiveOffers, value => IGSUserData.MarketplaceActiveOffers = value?.ToString());
            //UpdateProperty(result.GetMarketplaceHistory, value => IGSUserData.MarketplaceHistory = value?.ToString());

            SetPlatformSettings();
        }

        public static void SetPlatformSettings()
        {
            if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.GooglePlay)
            {
                IDosGamesSDKSettings.Instance.AndroidBundleID = IGSUserData.PlatformSettings.GooglePlay.BundleID;
                IDosGamesSDKSettings.Instance.AdEnabled = IGSUserData.PlatformSettings.GooglePlay.AdSettings.AdEnabled;
                IDosGamesSDKSettings.Instance.MediationAppKeyAndroid = IGSUserData.PlatformSettings.GooglePlay.AdSettings.AppKey;
                IDosGamesSDKSettings.Instance.BannerEnabled = IGSUserData.PlatformSettings.GooglePlay.AdSettings.BannerEnabled;
                IDosGamesSDKSettings.Instance.BannerPosition = IGSUserData.PlatformSettings.GooglePlay.AdSettings.BanerPosition;
                IDosGamesSDKSettings.Instance.ReferralTrackerLink = IGSUserData.PlatformSettings.GooglePlay.ReferralSystemSettings.ReferralAppLink;
            }
            else if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.AppleAppStore)
            {
                IDosGamesSDKSettings.Instance.IosBundleID = IGSUserData.PlatformSettings.AppleAppStore.BundleID;
                IDosGamesSDKSettings.Instance.IosAppStoreID = IGSUserData.PlatformSettings.AppleAppStore.AppStoreID;
                IDosGamesSDKSettings.Instance.AdEnabled = IGSUserData.PlatformSettings.AppleAppStore.AdSettings.AdEnabled;
                IDosGamesSDKSettings.Instance.MediationAppKeyIOS = IGSUserData.PlatformSettings.AppleAppStore.AdSettings.AppKey;
                IDosGamesSDKSettings.Instance.BannerEnabled = IGSUserData.PlatformSettings.AppleAppStore.AdSettings.BannerEnabled;
                IDosGamesSDKSettings.Instance.BannerPosition = IGSUserData.PlatformSettings.AppleAppStore.AdSettings.BanerPosition;
                IDosGamesSDKSettings.Instance.ReferralTrackerLink = IGSUserData.PlatformSettings.AppleAppStore.ReferralSystemSettings.ReferralAppLink;
            }
            else if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Telegram)
            {
                IDosGamesSDKSettings.Instance.AdEnabled = IGSUserData.PlatformSettings.Telegram.AdSettings.AdEnabled;
                IDosGamesSDKSettings.Instance.AdsGramBlockID = IGSUserData.PlatformSettings.Telegram.AdSettings.BlockID;
                IDosGamesSDKSettings.Instance.PlatformCurrencyPriceInCent = IGSUserData.PlatformSettings.Telegram.PlatformCurrencyPriceInCent;
                IDosGamesSDKSettings.Instance.TelegramWebAppLink = IGSUserData.PlatformSettings.Telegram.ReferralSystemSettings.ReferralAppLink;
            }
            else if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Web)
            {
                IDosGamesSDKSettings.Instance.AdEnabled = IGSUserData.PlatformSettings.Web.AdSettings.AdEnabled;
                IDosGamesSDKSettings.Instance.BannerEnabled = IGSUserData.PlatformSettings.Web.AdSettings.BannerEnabled;
                IDosGamesSDKSettings.Instance.BannerPosition = IGSUserData.PlatformSettings.Web.AdSettings.BanerPosition;
                IDosGamesSDKSettings.Instance.PlatformCurrencyPriceInCent = IGSUserData.PlatformSettings.Web.PlatformCurrencyPriceInCent;
                IDosGamesSDKSettings.Instance.ReferralTrackerLink = IGSUserData.PlatformSettings.Web.ReferralSystemSettings.ReferralAppLink;
            }
            else if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Custom)
            {

            }

            PlatformSettingsUpdated?.Invoke();
        }

        private void SaveAuthType(AuthType authType)
        {
            PlayerPrefs.SetInt(SAVED_AUTH_TYPE_KEY, (int)authType);
            PlayerPrefs.Save();
        }

        private void SaveEmailAndPassword(string email, string password)
        {
            PlayerPrefs.SetString(SAVED_AUTH_EMAIL_KEY, email);
            PlayerPrefs.SetString(SAVED_AUTH_PASSWORD_KEY, password);
            PlayerPrefs.Save();
        }

        private void ClearEmailAndPassword()
        {
            PlayerPrefs.SetString(SAVED_AUTH_EMAIL_KEY, string.Empty);
            PlayerPrefs.SetString(SAVED_AUTH_PASSWORD_KEY, string.Empty);
            PlayerPrefs.Save();
        }

        public static void ShowErrorMessage(string error)
        {
            //var message = GenerateErrorMessage(error);
            Message.Show(error);
        }

        private static string GenerateErrorMessage(MessageCode error)
        {
            string message;

            switch (error)
            {
                case MessageCode.USER_NOT_FOUND:
                    message = "Account not found. You can sign up.";
                    break;

                case MessageCode.INVALID_INPUT_DATA:
                    message = "INVALID INPUT DATA.";
                    break;

                case MessageCode.INCORRECT_PASSWORD:
                    message = "Password is not correct.";
                    break;

                case MessageCode.EMAIL_ALREADY_EXISTS:
                    message = "EMAIL ADDRESS ALREADY EXISTS";
                    break;

                case MessageCode.SESSION_EXPIRED:
                    message = "SESSION EXPIRED";
                    break;

                case MessageCode.INVALID_SESSION_TICKET:
                    message = "INVALID SESSION TICKET";
                    break;

                default:
                    message = $"Error message: {error}";
                    break;
            }

            return message;
        }

        public static bool CheckEmailAddress(string email)
        {
            return Regex.IsMatch(email, EMAIL_REGEX, RegexOptions.IgnoreCase);
        }

        public static bool CheckPasswordLenght(string password)
        {
            var lenght = password.Length;
            return lenght >= PASSWORD_MIN_LENGTH && lenght <= PASSWORD_MAX_LENGTH;
        }

        public void AutoLogin(Action<GetAllUserDataResult> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            switch (LastAuthType)
            {
                case AuthType.Email:
                    LoginWithEmailAddress(SavedEmail, SavedPassword, resultCallback, errorCallback, retryCallback);
                    break;
                case AuthType.Device:
                    LoginWithDeviceID(resultCallback, errorCallback, retryCallback);
                    break;
                default:
                    LoginWithDeviceID(resultCallback, errorCallback, retryCallback);
                    break;
            }
        }
    }
}