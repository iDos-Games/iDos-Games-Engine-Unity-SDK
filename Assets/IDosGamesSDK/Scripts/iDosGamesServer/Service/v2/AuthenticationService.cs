using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class AuthenticationService
    {
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 100;

        private static string SAVED_AUTH_TYPE_KEY => "Saved_AuthType_" + AuthenticationAPI.GetTitleID();
        private static string SAVED_AUTH_EMAIL_KEY => "Saved_Auth_Email_" + AuthenticationAPI.GetTitleID();
        private static string SAVED_AUTH_PASSWORD_KEY => "Saved_Auth_Password_" + AuthenticationAPI.GetTitleID();

        public static AuthType LastAuthType => (AuthType)PlayerPrefs.GetInt(SAVED_AUTH_TYPE_KEY, (int)AuthType.None);
        public static bool IsLoggedIn => LastAuthType != AuthType.Device && LastAuthType != AuthType.None;
        public static string SavedEmail => PlayerPrefs.GetString(SAVED_AUTH_EMAIL_KEY, string.Empty);
        public static string SavedPassword => PlayerPrefs.GetString(SAVED_AUTH_PASSWORD_KEY, string.Empty);

        public static string TelegramInitData { get; set; }
        public static WebGLPlatform WebGLPlatform { get; set; }
        public static PlatformUser PlatformUser { get; set; }

        public static event Action OnLoggedIn;
        public static event Action OnLoggedOut;
        public static event Action OnRequestSent;

        public static IGSAuthenticationContext AuthContext => _authContext;
        private static IGSAuthenticationContext _authContext;

        // ─── Auth context ─────────────────────────────────────────────────────────

        public static IGSAuthenticationContext GetAuthContext()
        {
            if (_authContext == null)
                throw new Exception("Not logged in. AuthContext is null.");
            if (string.IsNullOrEmpty(_authContext.UserID))
                throw new Exception("Not logged in. UserID is empty.");
            if (string.IsNullOrEmpty(_authContext.ClientSessionTicket))
                throw new Exception("Not logged in. ClientSessionTicket is empty.");
            return _authContext;
        }

        // ─── Base request ─────────────────────────────────────────────────────────

        private static AuthenticationRequest CreateBaseRequest() => new()
        {
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
        };

        // ─── Login methods ────────────────────────────────────────────────────────

        public static async Task<OperationResult<ClientStateResponse>> LoginWithPlatformToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                const string err = "AuthToken is null or empty.";
                Debug.LogWarning(err);
                Message.Show(err);
                return OperationResult<ClientStateResponse>.Fail(err);
            }

            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.PlatformAuthToken = authToken;
            request.DeviceID = SystemInfo.deviceUniqueIdentifier;
            request.Device = SystemInfo.deviceModel;
            request.Platform = Application.platform.ToString();

            var tokenResult = await AuthenticationAPI.LoginTokensWithPlatformToken(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientStateResponse>.Fail(tokenResult.Error);
            }

            ApplyTokenContext(tokenResult.Data);
            return await FetchAndApplyClientState(AuthType.iDosGames);
        }

        public static async Task<OperationResult<ClientStateResponse>> LoginWithDeviceID()
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            OperationResult<PlatformLoginResponse> tokenResult;

            if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Telegram)
            {
                var request = CreateBaseRequest();
                request.TelegramInitData = TelegramInitData;
                tokenResult = await AuthenticationAPI.LoginTokensWithTelegram(request);
            }
            else
            {
                var request = CreateBaseRequest();
                request.DeviceID = SystemInfo.deviceUniqueIdentifier;
                request.Device = SystemInfo.deviceModel;
                request.Platform = Application.platform.ToString();
                tokenResult = await AuthenticationAPI.LoginTokensWithDeviceID(request);
            }

            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientStateResponse>.Fail(tokenResult.Error);
            }

            ApplyTokenContext(tokenResult.Data);
            return await FetchAndApplyClientState(AuthType.Device);
        }

        public static async Task<OperationResult<ClientStateResponse>> LoginWithEmail(string email, string password)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.Email = email;
            request.Password = password;

            var tokenResult = await AuthenticationAPI.LoginTokensWithEmail(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientStateResponse>.Fail(tokenResult.Error);
            }

            ApplyTokenContext(tokenResult.Data);
            var result = await FetchAndApplyClientState(AuthType.Email);

            if (result.Success)
                SaveEmailAndPassword(email, password);

            return result;
        }

        public static async Task<OperationResult<ClientStateResponse>> RegisterWithEmail(string email, string password)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.Email = email;
            request.Password = password;
            request.DeviceID = SystemInfo.deviceUniqueIdentifier;
            request.Device = SystemInfo.deviceModel;
            request.Platform = Application.platform.ToString();

            var tokenResult = await AuthenticationAPI.RegisterTokensWithEmail(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientStateResponse>.Fail(tokenResult.Error);
            }

            ApplyTokenContext(tokenResult.Data);
            var result = await FetchAndApplyClientState(AuthType.Email);

            if (result.Success)
                SaveEmailAndPassword(email, password);

            return result;
        }

        public static async Task<OperationResult<ClientStateResponse>> LoginWithGoogle(string googleAuthToken)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.PlatformAuthToken = googleAuthToken;

            var tokenResult = await AuthenticationAPI.LoginTokensWithGoogle(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientStateResponse>.Fail(tokenResult.Error);
            }

            ApplyTokenContext(tokenResult.Data);
            return await FetchAndApplyClientState(AuthType.iDosGames);
        }

        // ─── Password recovery ────────────────────────────────────────────────────

        public static async Task<OperationResult<SuccessResponse>> ForgotPassword(string email)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.Email = email;

            var result = await AuthenticationAPI.ForgotPassword(request);

            if (!result.Success)
                Message.Show(result.Error);

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> ResetPassword(string email, string resetToken, string password)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.Email = email;
            request.ResetToken = resetToken;
            request.Password = password;

            var result = await AuthenticationAPI.ResetPassword(request);

            if (!result.Success)
                Message.Show(result.Error);

            return result;
        }

        // ─── Auto-login ───────────────────────────────────────────────────────────

        public static Task<OperationResult<ClientStateResponse>> AutoLogin() => LastAuthType switch
        {
            AuthType.Email => LoginWithEmail(SavedEmail, SavedPassword),
            _ => LoginWithDeviceID(),
        };

        // ─── Logout ───────────────────────────────────────────────────────────────

        public static void LogOut()
        {
            _authContext = null;
            IDosGamesData.OnUserLoggedOut();
            OnLoggedOut?.Invoke();
            Loading.SwitchToLoginScene();
        }

        // ─── Validation ───────────────────────────────────────────────────────────

        public static bool IsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) &&
            System.Text.RegularExpressions.Regex.IsMatch(
                email,
                @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public static bool IsValidPasswordLength(string password) =>
            !string.IsNullOrEmpty(password) &&
            password.Length >= PASSWORD_MIN_LENGTH &&
            password.Length <= PASSWORD_MAX_LENGTH;

        // ─── Private helpers ──────────────────────────────────────────────────────

        private static void ApplyTokenContext(PlatformLoginResponse tokenData)
        {
            _authContext = new IGSAuthenticationContext
            {
                UserID = tokenData.TitleUserID,
                ClientSessionTicket = tokenData.TitleClientSessionTicket,
                ClientSessionTicketExpiration = tokenData.TitleClientSessionTicketExpiration,
            };

            // Синхронизация со старым AuthService пока старый код его использует
            //AuthService.AuthContext = _authContext;
            //AuthService.UserID = _authContext.UserID;
            //AuthService.ClientSessionTicket = _authContext.ClientSessionTicket;
        }

        private static async Task<OperationResult<ClientStateResponse>> FetchAndApplyClientState(AuthType authType)
        {
            var result = await UserService.GetClientState();

            if (!result.Success
                || result.Data?.AuthContext == null
                || string.IsNullOrEmpty(result.Data.AuthContext.ClientSessionTicket))
            {
                var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Invalid ClientState response.";
                Message.Show(err);
                return OperationResult<ClientStateResponse>.Fail(err);
            }

            ApplyFullSession(result.Data, authType);
            OnLoggedIn?.Invoke();

            return result;
        }

        private static void ApplyFullSession(ClientStateResponse data, AuthType authType)
        {
            var ctx = data.AuthContext;
            _authContext = new IGSAuthenticationContext(
                ctx.ClientSessionTicket,
                ctx.EntityToken,
                ctx.UserID,
                ctx.EntityId,
                ctx.EntityType,
                ctx.TelemetryKey);

            var user = IDosGamesData.User;
            if (data.UserInventoryResult != null)
            {
                user.ApplyInventory(data.UserInventoryResult.Inventory);
                if (data.UserInventoryResult.VirtualCurrency != null) user.ApplyVirtualCurrency(data.UserInventoryResult.VirtualCurrency);
                if (data.UserInventoryResult.VirtualCurrencyRechargeTimes != null) user.ApplyVirtualCurrencyRechargeTimes(data.UserInventoryResult.VirtualCurrencyRechargeTimes);
            }

            if (data.CustomUserDataResult != null) user.ApplyCustomUserData(data.CustomUserDataResult);
            if (data.LeaderboardData != null) user.ApplyLeaderboardData(data.LeaderboardData);

            var config = IDosGamesData.Config;
            if (data.TitlePublicConfiguration != null) config.ApplyTitlePublicConfiguration(data.TitlePublicConfiguration);
            if (data.TitlePublicData != null) config.ApplyTitlePublicData(data.TitlePublicData);
            if (data.CatalogItemsResult != null)
            {
                string catalogVersion = data.CatalogItemsResult.Catalog?.Count > 0 ? data.CatalogItemsResult.Catalog[0].CatalogVersion : string.Empty;
                config.ApplyCatalog(catalogVersion, data.CatalogItemsResult);
            }
            if (data.PlatformSettings != null)
            {
                config.ApplyPlatformSettings(data.PlatformSettings);
                ApplyPlatformSDKSettings(data.PlatformSettings);
            }

            if (data.GetCurrencyData != null) config.ApplyCurrencies(data.GetCurrencyData);

            DataService.ProcessingAllData(data);
            IDosGamesData.OnUserLoggedIn();
            SaveAuthType(authType);
        }

        private static void ApplyPlatformSDKSettings(PlatformSettingsModel settings)
        {
            var sdk = IDosGamesSDKSettings.Instance;
            var platform = sdk.BuildForPlatform;

            if (platform == Platforms.GooglePlay)
            {
                sdk.AndroidBundleID = settings.GooglePlay.BundleID;
                sdk.AdEnabled = settings.GooglePlay.AdSettings.AdEnabled;
                sdk.MediationAppKeyAndroid = settings.GooglePlay.AdSettings.AppKey;
                sdk.BannerEnabled = settings.GooglePlay.AdSettings.BannerEnabled;
                sdk.BannerPosition = settings.GooglePlay.AdSettings.BanerPosition;
                sdk.ReferralTrackerLink = settings.GooglePlay.ReferralSystemSettings.ReferralAppLink;
            }
            else if (platform == Platforms.AppleAppStore)
            {
                sdk.IosBundleID = settings.AppleAppStore.BundleID;
                sdk.IosAppStoreID = settings.AppleAppStore.AppStoreID;
                sdk.AdEnabled = settings.AppleAppStore.AdSettings.AdEnabled;
                sdk.MediationAppKeyIOS = settings.AppleAppStore.AdSettings.AppKey;
                sdk.BannerEnabled = settings.AppleAppStore.AdSettings.BannerEnabled;
                sdk.BannerPosition = settings.AppleAppStore.AdSettings.BanerPosition;
                sdk.ReferralTrackerLink = settings.AppleAppStore.ReferralSystemSettings.ReferralAppLink;
            }
            else if (platform == Platforms.Telegram)
            {
                sdk.AdEnabled = settings.Telegram.AdSettings.AdEnabled;
                sdk.AdsGramBlockID = settings.Telegram.AdSettings.BlockID;
                sdk.PlatformCurrencyPriceInCent = settings.Telegram.PlatformCurrencyPriceInCent;
                sdk.TelegramWebAppLink = settings.Telegram.ReferralSystemSettings.ReferralAppLink;
            }
            else if (platform == Platforms.Web)
            {
                sdk.AdEnabled = settings.Web.AdSettings.AdEnabled;
                sdk.BannerEnabled = settings.Web.AdSettings.BannerEnabled;
                sdk.BannerPosition = settings.Web.AdSettings.BanerPosition;
                sdk.PlatformCurrencyPriceInCent = settings.Web.PlatformCurrencyPriceInCent;
                sdk.ReferralTrackerLink = settings.Web.ReferralSystemSettings.ReferralAppLink;
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
    }
}
