using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class AuthenticationService
    {
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 100;
        private const string EMAIL_REGEX = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        private static string SAVED_AUTH_TYPE_KEY => "Saved_AuthType" + GetTitleID();
        private static string SAVED_AUTH_EMAIL_KEY => "Saved_Auth_Email" + GetTitleID();
        private static string SAVED_AUTH_PASSWORD_KEY => "Saved_Auth_Password" + GetTitleID();

        public static string UserID { get; private set; }
        public static string ClientSessionTicket { get; private set; }
        public static string EntityToken { get; private set; }
        public static IGSAuthenticationContext AuthContext { get; private set; }

        public static AuthType LastAuthType => (AuthType)PlayerPrefs.GetInt(SAVED_AUTH_TYPE_KEY, (int)AuthType.None);
        public static bool IsLoggedIn => !string.IsNullOrEmpty(ClientSessionTicket) && !string.IsNullOrEmpty(UserID);

        public static string SavedEmail => PlayerPrefs.GetString(SAVED_AUTH_EMAIL_KEY, string.Empty);
        public static string SavedPassword => PlayerPrefs.GetString(SAVED_AUTH_PASSWORD_KEY, string.Empty);

        public static string TelegramInitData { get; set; }

        // Events
        public static event Action OnLoginSuccess;
        public static event Action<string> OnLoginFailed;
        public static event Action PlatformSettingsUpdated;

        // =====================================================================
        // CONTEXT
        // =====================================================================
        public static IGSAuthenticationContext GetAuthContext()
        {
            var context = AuthContext;
            if (context == null) throw new Exception("Not logged in. AuthContext is null.");
            if (string.IsNullOrEmpty(context.UserID)) throw new Exception("Not logged in. UserID is empty.");
            if (string.IsNullOrEmpty(context.ClientSessionTicket)) throw new Exception("Not logged in. ClientSessionTicket is empty.");
            return context;
        }

        // =====================================================================
        // TITLE ID (дл€ PlayerPrefs keys / WEBGL)
        // =====================================================================
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

        // =====================================================================
        // PUBLIC API (Service) Ч теперь дергаем AuthenticationAPI
        // =====================================================================

        public static async Task<OperationResult<AuthenticationResponse>> LoginWithDeviceID()
        {
            try
            {
                OperationResult<AuthenticationResponse> result;

                // Telegram login
                if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Telegram &&
                    !string.IsNullOrEmpty(TelegramInitData))
                {
                    result = await AuthenticationAPI.LoginWithTelegram(TelegramInitData);
                }
                else
                {
                    result = await AuthenticationAPI.LoginWithDeviceID(
                        SystemInfo.deviceUniqueIdentifier,
                        SystemInfo.deviceModel,
                        Application.platform.ToString()
                    );
                }

                if (result.Success)
                {
                    SetCredentials(result.Data);
                    SaveAuthType(AuthType.Device);
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnLoginFailed?.Invoke(result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthService] Login Error: {ex.Message}");
                return OperationResult<AuthenticationResponse>.Fail(ex.Message);
            }
        }

        public static async Task<OperationResult<AuthenticationResponse>> LoginWithEmail(string email, string password)
        {
            try
            {
                var result = await AuthenticationAPI.LoginWithEmail(email, password);

                if (result.Success)
                {
                    SetCredentials(result.Data);
                    SaveAuthType(AuthType.Email);
                    SaveEmailAndPassword(email, password);
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnLoginFailed?.Invoke(result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                return OperationResult<AuthenticationResponse>.Fail(ex.Message);
            }
        }

        public static async Task<OperationResult<AuthenticationResponse>> RegisterUserByEmail(string email, string password)
        {
            try
            {
                var result = await AuthenticationAPI.RegisterUserByEmail(
                    email,
                    password,
                    SystemInfo.deviceUniqueIdentifier,
                    SystemInfo.deviceModel,
                    Application.platform.ToString()
                );

                if (result.Success)
                {
                    SetCredentials(result.Data);
                    SaveAuthType(AuthType.Device);
                    SaveEmailAndPassword(email, password);
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnLoginFailed?.Invoke(result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                return OperationResult<AuthenticationResponse>.Fail(ex.Message);
            }
        }

        public static async Task<OperationResult<SuccessResponse>> AddEmailAndPassword(string email, string password)
        {
            try
            {
                var ctx = GetAuthContext(); // гарантирует, что юзер залогинен

                var result = await AuthenticationAPI.AddEmailAndPassword(
                    ctx.UserID,
                    ctx.ClientSessionTicket,
                    email,
                    password
                );

                if (result.Success)
                {
                    SaveAuthType(AuthType.Email);
                    SaveEmailAndPassword(email, password);
                }

                return result;
            }
            catch (Exception ex)
            {
                return OperationResult<SuccessResponse>.Fail(ex.Message);
            }
        }

        public static Task<OperationResult<SuccessResponse>> ForgotPassword(string email)
            => AuthenticationAPI.ForgotPassword(email);

        public static Task<OperationResult<SuccessResponse>> ResetPassword(string resetToken, string password)
            => AuthenticationAPI.ResetPassword(resetToken, password);

        public static async Task<OperationResult<AuthenticationResponse>> AutoLogin()
        {
            if (LastAuthType == AuthType.Email &&
                !string.IsNullOrEmpty(SavedEmail) &&
                !string.IsNullOrEmpty(SavedPassword))
            {
                return await LoginWithEmail(SavedEmail, SavedPassword);
            }

            return await LoginWithDeviceID();
        }

        public static void Logout()
        {
            UserID = null;
            ClientSessionTicket = null;
            EntityToken = null;
            AuthContext = null;

            IGSUserData.UserInventory = new();
            IGSUserData.Currency = new();

            Loading.SwitchToLoginScene();
        }

        // =====================================================================
        // INTERNAL HELPERS
        // =====================================================================
        private static void SetCredentials(AuthenticationResponse result)
        {
            if (result == null || result.AuthContext == null)
            {
                Debug.LogError("[AuthenticationService] Critical: AuthContext is null.");
                return;
            }

            UserID = result.AuthContext.UserID;
            ClientSessionTicket = result.AuthContext.ClientSessionTicket;
            EntityToken = result.AuthContext.EntityToken;

            AuthContext = new IGSAuthenticationContext(
                result.AuthContext.ClientSessionTicket,
                result.AuthContext.EntityToken,
                result.AuthContext.UserID,
                result.AuthContext.EntityId,
                result.AuthContext.EntityType,
                result.AuthContext.TelemetryKey
            );

            // Global data
            IGSUserData.UserAllDataResult = result;
            IGSUserData.CatalogItemsResult = result.CatalogItemsResult;
            IGSUserData.UserInventory = result.UserInventoryResult;
            IGSUserData.TitlePublicConfiguration = result.TitlePublicConfiguration;
            IGSUserData.CustomUserData = result.CustomUserDataResult;
            IGSUserData.Leaderboard = result.LeaderboardResult;
            IGSUserData.Friends = result.GetFriends;
            IGSUserData.FriendRequests = result.GetFriendRequests;
            IGSUserData.RecommendedFriends = result.GetRecommendedFriends;
            IGSUserData.Currency = result.GetCurrencyData;
            IGSUserData.PlatformSettings = result.PlatformSettings;
            IGSUserData.ImageData = result.ImageData;

            SetPlatformSettings();
            PlatformSettingsUpdated?.Invoke();
        }

        private static void SetPlatformSettings()
        {
            if (IGSUserData.PlatformSettings == null) return;

            var settings = IDosGamesSDKSettings.Instance;
            var ps = IGSUserData.PlatformSettings;

            switch (settings.BuildForPlatform)
            {
                case Platforms.GooglePlay when ps.GooglePlay != null:
                    settings.AndroidBundleID = ps.GooglePlay.BundleID;
                    settings.AdEnabled = ps.GooglePlay.AdSettings.AdEnabled;
                    settings.MediationAppKeyAndroid = ps.GooglePlay.AdSettings.AppKey;
                    settings.BannerEnabled = ps.GooglePlay.AdSettings.BannerEnabled;
                    settings.BannerPosition = ps.GooglePlay.AdSettings.BanerPosition;
                    settings.ReferralTrackerLink = ps.GooglePlay.ReferralSystemSettings.ReferralAppLink;
                    break;

                case Platforms.AppleAppStore when ps.AppleAppStore != null:
                    settings.IosBundleID = ps.AppleAppStore.BundleID;
                    settings.IosAppStoreID = ps.AppleAppStore.AppStoreID;
                    settings.AdEnabled = ps.AppleAppStore.AdSettings.AdEnabled;
                    settings.MediationAppKeyIOS = ps.AppleAppStore.AdSettings.AppKey;
                    settings.BannerEnabled = ps.AppleAppStore.AdSettings.BannerEnabled;
                    settings.BannerPosition = ps.AppleAppStore.AdSettings.BanerPosition;
                    settings.ReferralTrackerLink = ps.AppleAppStore.ReferralSystemSettings.ReferralAppLink;
                    break;

                case Platforms.Telegram when ps.Telegram != null:
                    settings.AdEnabled = ps.Telegram.AdSettings.AdEnabled;
                    settings.AdsGramBlockID = ps.Telegram.AdSettings.BlockID;
                    settings.PlatformCurrencyPriceInCent = ps.Telegram.PlatformCurrencyPriceInCent;
                    settings.TelegramWebAppLink = ps.Telegram.ReferralSystemSettings.ReferralAppLink;
                    break;

                case Platforms.Web when ps.Web != null:
                    settings.AdEnabled = ps.Web.AdSettings.AdEnabled;
                    settings.BannerEnabled = ps.Web.AdSettings.BannerEnabled;
                    settings.BannerPosition = ps.Web.AdSettings.BanerPosition;
                    settings.PlatformCurrencyPriceInCent = ps.Web.PlatformCurrencyPriceInCent;
                    settings.ReferralTrackerLink = ps.Web.ReferralSystemSettings.ReferralAppLink;
                    break;
            }
        }

        private static void SaveAuthType(AuthType authType)
        {
            PlayerPrefs.SetInt(SAVED_AUTH_TYPE_KEY, (int)authType);
            PlayerPrefs.Save();
        }

        private static void SaveEmailAndPassword(string email, string password)
        {
            PlayerPrefs.SetString(SAVED_AUTH_EMAIL_KEY, email);
            PlayerPrefs.SetString(SAVED_AUTH_PASSWORD_KEY, password);
            PlayerPrefs.Save();
        }

        // Helpers
        public static bool CheckEmailAddress(string email) => Regex.IsMatch(email, EMAIL_REGEX, RegexOptions.IgnoreCase);
        public static bool CheckPasswordLenght(string password) => password.Length >= PASSWORD_MIN_LENGTH && password.Length <= PASSWORD_MAX_LENGTH;
    }
}
