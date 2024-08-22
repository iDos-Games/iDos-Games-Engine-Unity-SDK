using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IDosGames
{
    public class AuthService
    {
        private const int PASSWORD_MIN_LENGTH = 6;
        private const int PASSWORD_MAX_LENGTH = 100;
        private const string EMAIL_REGEX = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        private const string SAVED_AUTH_TYPE_KEY = "Saved_AuthType";
        private const string SAVED_AUTH_EMAIL_KEY = "Saved_Auth_Email";
        private const string SAVED_AUTH_PASSWORD_KEY = "Saved_Auth_Password";

        public static WebGLPlatform WebGLPlatform { get; set; }
        public static AuthType LastAuthType => (AuthType)PlayerPrefs.GetInt(SAVED_AUTH_TYPE_KEY, (int)AuthType.None);
        public static bool IsLoggedIn => LastAuthType != AuthType.Device && LastAuthType != AuthType.None;
        public static string SavedEmail => PlayerPrefs.GetString(SAVED_AUTH_EMAIL_KEY, string.Empty);
        public static string SavedPassword => PlayerPrefs.GetString(SAVED_AUTH_PASSWORD_KEY, string.Empty);

        public static string UserID { get; private set; }
        public static string ClientSessionTicket { get; private set; }
        public static string EntityToken { get; private set; }
        public static IGSAuthenticationContext AuthContext { get; private set; }

        public static InitData TelegramInitData { get; set; }

        private static AuthService _instance;

        public static event Action RequestSent;

        public static event Action LoggedIn;

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

        public async void LoginWithDeviceID(Action<GetAllUserDataResult> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            RequestSent?.Invoke();
            try
            {
                GetAllUserDataResult result = await IGSService.LoginWithDeviceID();

                if (result != null) //&& result.AuthContext != null && !string.IsNullOrEmpty(result.AuthContext.ClientSessionTicket)
                {
                    SetCredentials(result);
                    SaveAuthType(AuthType.Device);
                    ClearEmailAndPassword();

                    resultCallback?.Invoke(result);
                    LoggedIn?.Invoke();
                }
                else
                {
                    IGSClientAPI.OnIGSError("Invalid result", errorCallback, retryCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback, retryCallback);
            }
        }

        public void LogOut(Action<GetAllUserDataResult> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            LoginWithDeviceID(resultCallback, errorCallback, retryCallback);
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
                    ClearEmailAndPassword();

                    resultCallback?.Invoke(result);
                    LoggedIn?.Invoke();
                }
                else
                {
                    IGSClientAPI.OnIGSError("Invalid result", errorCallback, retryCallback);
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
                    ClearEmailAndPassword();

                    resultCallback?.Invoke(result);
                    LoggedIn?.Invoke();
                }
                else
                {
                    IGSClientAPI.OnIGSError("Invalid result", errorCallback, retryCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback, retryCallback);
            }
        }

        public void SendAccountRecoveryEmail(string email, Action<string> resultCallback = null, Action<string> errorCallback = null) //SendAccountRecoveryEmailResult, PlayFabError
        {
            /*
            RequestSent?.Invoke();

            IGSClientAPI.SendAccountRecoveryEmail
            (
                request: new SendAccountRecoveryEmailRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    Email = email
                },
                resultCallback: (result) => resultCallback?.Invoke(result),
                errorCallback: (error) => IGServerAPI.OnPlayFabError(error, errorCallback)
            );
            */
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

            UpdateProperty(result.UserInventoryResult, value => IGSUserData.UserInventory = value);
            UpdateProperty(result.BlobTitleDataResult, value => IGSUserData.TitleData = value);
            UpdateProperty(result.UserDataResult, value => IGSUserData.ReadOnlyData = value);
            UpdateProperty(result.CatalogItemsResult, value => IGSUserData.SkinCatalogItems = value);
            UpdateProperty(result.LeaderboardResult, value => IGSUserData.Leaderboard = value);
            UpdateProperty(result.GetFriends, value => IGSUserData.Friends = value?.ToString());
            UpdateProperty(result.GetFriendRequests, value => IGSUserData.FriendRequests = value?.ToString());
            UpdateProperty(result.GetRecommendedFriends, value => IGSUserData.RecommendedFriends = value?.ToString());
            UpdateProperty(result.GetMarketplaceGroupedOffers, value => IGSUserData.MarketplaceGroupedOffers = value?.ToString());
            UpdateProperty(result.GetMarketplaceActiveOffers, value => IGSUserData.MarketplaceActiveOffers = value?.ToString());
            UpdateProperty(result.GetMarketplaceHistory, value => IGSUserData.MarketplaceHistory = value?.ToString());
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