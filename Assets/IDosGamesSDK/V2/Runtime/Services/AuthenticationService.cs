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
        public static bool IsLoggedIn => _authContext != null && !string.IsNullOrEmpty(_authContext.UserID) && !string.IsNullOrEmpty(_authContext.ClientSessionTicket);
        public static string SavedEmail => PlayerPrefs.GetString(SAVED_AUTH_EMAIL_KEY, string.Empty);
        public static string SavedPassword => PlayerPrefs.GetString(SAVED_AUTH_PASSWORD_KEY, string.Empty);

        public static string TelegramInitData { get; set; }
        public static WebGLPlatform WebGLPlatform { get; set; }
        public static PlatformUser PlatformUser { get; set; }

        public static event Action OnLoggedIn;
        public static event Action OnLoggedOut;
        public static event Action OnRequestSent;

        public static AuthContext AuthContext => _authContext;
        private static AuthContext _authContext;

        // ─── Auth context ─────────────────────────────────────────────────────────

        public static AuthContext GetAuthContext()
        {
            if (!IsLoggedIn) throw new Exception("Not logged in.");
            return _authContext;
        }

        // ─── Base request ─────────────────────────────────────────────────────────

        private static AuthenticationRequest CreateBaseRequest() => new()
        {
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
        };

        // ─── Login methods ────────────────────────────────────────────────────────

        public static async Task<OperationResult<ClientState>> LoginWithPlatformToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                const string err = "AuthToken is null or empty.";
                Debug.LogWarning(err);
                Message.Show(err);
                return OperationResult<ClientState>.Fail(err);
            }

            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.PlatformAuthToken = authToken;
            request.DeviceID = SystemInfo.deviceUniqueIdentifier;
            request.Device = SystemInfo.deviceModel;
            request.Platform = Application.platform.ToString();

            var tokenResult = await AuthenticationAPI.LoginWithPlatformToken(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientState>.Fail(tokenResult.Error);
            }

            ApplyAuthContext(tokenResult.Data);
            return await FetchAndApplyClientState(AuthType.iDosGames);
        }

        public static async Task<OperationResult<ClientState>> LoginWithDeviceID()
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            OperationResult<PlatformLoginResponse> tokenResult;

            if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Telegram)
            {
                var request = CreateBaseRequest();
                request.TelegramInitData = TelegramInitData;
                tokenResult = await AuthenticationAPI.LoginWithTelegram(request);
            }
            else
            {
                var request = CreateBaseRequest();
                request.DeviceID = SystemInfo.deviceUniqueIdentifier;
                request.Device = SystemInfo.deviceModel;
                request.Platform = Application.platform.ToString();
                tokenResult = await AuthenticationAPI.LoginWithDeviceID(request);
            }

            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientState>.Fail(tokenResult.Error);
            }

            ApplyAuthContext(tokenResult.Data);
            return await FetchAndApplyClientState(AuthType.Device);
        }

        public static async Task<OperationResult<ClientState>> LoginWithEmail(string email, string password)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.Email = email;
            request.Password = password;

            var tokenResult = await AuthenticationAPI.LoginWithEmail(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientState>.Fail(tokenResult.Error);
            }

            ApplyAuthContext(tokenResult.Data);
            var result = await FetchAndApplyClientState(AuthType.Email);
            if (result.Success) SaveEmailAndPassword(email, password);

            return result;
        }

        public static async Task<OperationResult<ClientState>> RegisterWithEmail(string email, string password)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.Email = email;
            request.Password = password;
            request.DeviceID = SystemInfo.deviceUniqueIdentifier;
            request.Device = SystemInfo.deviceModel;
            request.Platform = Application.platform.ToString();

            var tokenResult = await AuthenticationAPI.RegisterWithEmail(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientState>.Fail(tokenResult.Error);
            }

            ApplyAuthContext(tokenResult.Data);
            var result = await FetchAndApplyClientState(AuthType.Email);
            if (result.Success) SaveEmailAndPassword(email, password);

            return result;
        }

        public static async Task<OperationResult<ClientState>> LoginWithGoogle(string googleIDToken)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.GoogleIDToken = googleIDToken;

            var tokenResult = await AuthenticationAPI.LoginWithGoogle(request);
            if (!tokenResult.Success)
            {
                Message.Show(tokenResult.Error);
                return OperationResult<ClientState>.Fail(tokenResult.Error);
            }

            ApplyAuthContext(tokenResult.Data);
            return await FetchAndApplyClientState(AuthType.Google);
        }

        // ─── Password recovery ────────────────────────────────────────────────────

        public static async Task<OperationResult<SuccessResponse>> ForgotPassword(string email)
        {
            OnRequestSent?.Invoke();
            Loading.ShowTransparentPanel();

            var request = CreateBaseRequest();
            request.Email = email;

            var result = await AuthenticationAPI.ForgotPassword(request);

            if (!result.Success) Message.Show(result.Error);

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

            if (!result.Success) Message.Show(result.Error);

            return result;
        }

        // ─── Auto-login ───────────────────────────────────────────────────────────

        public static Task<OperationResult<ClientState>> AutoLogin() => LastAuthType switch
        {
            AuthType.Email => LoginWithEmail(SavedEmail, SavedPassword),
            _ => LoginWithDeviceID(),
        };

        // ─── Logout ───────────────────────────────────────────────────────────────

        public static void LogOut()
        {
            _authContext = null;
            IDosGamesData.User.Clear();
            OnLoggedOut?.Invoke();
            Loading.SwitchToLoginScene();
        }

        // ─── Validation ───────────────────────────────────────────────────────────

        public static bool IsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) && System.Text.RegularExpressions.Regex.IsMatch(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public static bool IsValidPasswordLength(string password) =>
            !string.IsNullOrEmpty(password) && password.Length >= PASSWORD_MIN_LENGTH && password.Length <= PASSWORD_MAX_LENGTH;

        // ─── Private helpers ──────────────────────────────────────────────────────

        private static async Task<OperationResult<ClientState>> FetchAndApplyClientState(AuthType authType)
        {
            var result = await UserService.GetClientState();

            if (!result.Success)
            {
                var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Invalid ClientState response.";
                Message.Show(err);
                return OperationResult<ClientState>.Fail(err);
            }

            SaveAuthType(authType);
            OnLoggedIn?.Invoke();
            return result;
        }

        private static void ApplyAuthContext(PlatformLoginResponse data)
        {
            _authContext = new AuthContext(
                userID: data.TitleUserID,
                clientSessionTicket: data.TitleClientSessionTicket,
                clientSessionTicketExpiration: data.TitleClientSessionTicketExpiration,
                platformUserID: data.PlatformUserID,
                platformAuthToken: data.PlatformAuthToken,
                platformAuthTokenExpiration: data.PlatformAuthTokenExpiration
            );
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
