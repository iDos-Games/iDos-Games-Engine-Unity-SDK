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

        private static string SAVED_AUTH_TYPE_KEY => "Saved_AuthType" + AuthenticationAPI.GetTitleID();
        private static string SAVED_AUTH_EMAIL_KEY => "Saved_Auth_Email" + AuthenticationAPI.GetTitleID();
        private static string SAVED_AUTH_PASSWORD_KEY => "Saved_Auth_Password" + AuthenticationAPI.GetTitleID();

        public static WebGLPlatform WebGLPlatform { get; set; }
        public static AuthType LastAuthType => (AuthType)PlayerPrefs.GetInt(SAVED_AUTH_TYPE_KEY, (int)AuthType.None);
        public static bool IsLoggedIn => LastAuthType != AuthType.Device && LastAuthType != AuthType.None;
        public static string SavedEmail => PlayerPrefs.GetString(SAVED_AUTH_EMAIL_KEY, string.Empty);
        public static string SavedPassword => PlayerPrefs.GetString(SAVED_AUTH_PASSWORD_KEY, string.Empty);

        public static string UserID { get; set; }
        public static string ClientSessionTicket { get; set; }
        public static IGSAuthenticationContext AuthContext { get; set; }

        public static string TelegramInitData { get; set; }

        private static AuthService _instance;

        public static event Action RequestSent;
        public static event Action LoggedIn;
        //public static event Action PlatformSettingsUpdated;

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

        public static IGSAuthenticationContext GetAuthContext()
        {
            var context = AuthContext;
            if (context == null) throw new Exception("Not logged in. AuthContext is null.");
            if (string.IsNullOrEmpty(context.UserID)) throw new Exception("Not logged in. UserID is empty.");
            if (string.IsNullOrEmpty(context.ClientSessionTicket)) throw new Exception("Not logged in. ClientSessionTicket is empty.");
            return context;
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

        public async void LoginWithPlatformToken(string authToken, Action<ClientStateResponse> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.Log("AuthToken is Null rr Empty");
                Message.Show("AuthToken is Null rr Empty");
                return;
            }
            RequestSent?.Invoke();

            try
            {
                var request = new AuthenticationRequest
                {
                    BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                    WebAppLink = WebSDK.webAppLink,

                    PlatformAuthToken = authToken,
                    DeviceID = SystemInfo.deviceUniqueIdentifier,
                    Device = SystemInfo.deviceModel,
                    Platform = Application.platform.ToString(),
                };

                var login = await AuthenticationAPI.LoginTokensWithPlatformToken(request);
                if (login.Success)
                {
                    AuthContext = new IGSAuthenticationContext
                    {
                        UserID = login.Data.TitleUserID,
                        ClientSessionTicket = login.Data.TitleClientSessionTicket,
                        ClientSessionTicketExpiration = login.Data.TitleClientSessionTicketExpiration,
                    };

                    var result = await UserService.GetClientState();
                    if (result.Success && result.Data != null && result.Data.AuthContext != null && !string.IsNullOrEmpty(result.Data.AuthContext.ClientSessionTicket))
                    {
                        //SetCredentials(result.Data);
                        SaveAuthType(AuthType.iDosGames);

                        resultCallback?.Invoke(result.Data);
                        LoggedIn?.Invoke();
                    }
                    else
                    {
                        var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Invalid result";
                        IGSClientAPI.OnIGSError(err, errorCallback, retryCallback);
                    }
                }
                else
                {
                    IGSClientAPI.OnIGSError(login.Error, errorCallback, retryCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback, retryCallback);
            }
        }

        public async void LoginWithDeviceID(Action<ClientStateResponse> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            RequestSent?.Invoke();

            try
            {
                OperationResult<ClientStateResponse> result;

                if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.Telegram)
                {
                    var request = new AuthenticationRequest
                    {
                        BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                        WebAppLink = WebSDK.webAppLink,

                        TelegramInitData = TelegramInitData,
                    };

                    result = await AuthenticationAPI.LoginWithTelegram(request);
                }
                else
                {
                    var request = new AuthenticationRequest
                    {
                        BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                        WebAppLink = WebSDK.webAppLink,

                        DeviceID = SystemInfo.deviceUniqueIdentifier,
                        Device = SystemInfo.deviceModel,
                        Platform = Application.platform.ToString(),
                    };

                    result = await AuthenticationAPI.LoginWithDeviceID(request);
                }

                if (result.Success && result.Data != null && result.Data.AuthContext != null && !string.IsNullOrEmpty(result.Data.AuthContext.ClientSessionTicket))
                {
                    //SetCredentials(result.Data);
                    SaveAuthType(AuthType.Device);

                    resultCallback?.Invoke(result.Data);
                    LoggedIn?.Invoke();
                }
                else
                {
                    var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Invalid result";
                    IGSClientAPI.OnIGSError(err, errorCallback, retryCallback);
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

        public async void LoginWithEmailAddress(string email, string password, Action<ClientStateResponse> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            RequestSent?.Invoke();

            try
            {
                var request = new AuthenticationRequest
                {
                    BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                    WebAppLink = WebSDK.webAppLink,

                    Email = email,
                    Password = password,
                };

                var result = await AuthenticationAPI.LoginWithEmail(request);

                if (result.Success && result.Data != null && result.Data.AuthContext != null && !string.IsNullOrEmpty(result.Data.AuthContext.ClientSessionTicket))
                {
                    //SetCredentials(result.Data);
                    SaveAuthType(AuthType.Email);
                    SaveEmailAndPassword(email, password);

                    resultCallback?.Invoke(result.Data);
                    LoggedIn?.Invoke();
                }
                else
                {
                    var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Invalid result";
                    IGSClientAPI.OnIGSError(err, errorCallback, retryCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback, retryCallback);
            }
        }

        public async void RegisterUserByEmail(string email, string password, Action<ClientStateResponse> resultCallback = null, Action<string> errorCallback = null, Action retryCallback = null)
        {
            RequestSent?.Invoke();

            try
            {
                var request = new AuthenticationRequest
                {
                    BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                    WebAppLink = WebSDK.webAppLink,

                    Email = email,
                    Password = password,
                    DeviceID = SystemInfo.deviceUniqueIdentifier,
                    Device = SystemInfo.deviceModel,
                    Platform = Application.platform.ToString(),
                };

                var result = await AuthenticationAPI.RegisterWithEmail(request);

                if (result.Success && result.Data != null && result.Data.AuthContext != null && !string.IsNullOrEmpty(result.Data.AuthContext.ClientSessionTicket))
                {
                    //SetCredentials(result.Data);
                    SaveAuthType(AuthType.Device);
                    SaveEmailAndPassword(email, password);

                    resultCallback?.Invoke(result.Data);
                    LoggedIn?.Invoke();
                }
                else
                {
                    var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Invalid result";
                    IGSClientAPI.OnIGSError(err, errorCallback, retryCallback);
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

            try
            {
                var request = new AuthenticationRequest
                {
                    BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                    WebAppLink = WebSDK.webAppLink,

                    Email = email,
                };

                var result = await AuthenticationAPI.ForgotPassword(request);

                if (result.Success)
                {
                    resultCallback?.Invoke("Success");
                }
                else
                {
                    var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Request failed";
                    IGSClientAPI.OnIGSError(err, errorCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback);
            }
        }

        public async void SendResetPassword(string email, string resetToken, string password, Action<string> resultCallback = null, Action<string> errorCallback = null)
        {
            RequestSent?.Invoke();

            try
            {
                var request = new AuthenticationRequest
                {
                    BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                    WebAppLink = WebSDK.webAppLink,

                    Email = email,
                    ResetToken = resetToken,
                    Password = password
                };

                var result = await AuthenticationAPI.ResetPassword(request);

                if (result.Success)
                {
                    resultCallback?.Invoke("Success");
                }
                else
                {
                    var err = !string.IsNullOrEmpty(result.Error) ? result.Error : "Request failed";
                    IGSClientAPI.OnIGSError(err, errorCallback);
                }
            }
            catch (Exception ex)
            {
                IGSClientAPI.OnIGSError(ex.Message, errorCallback);
            }
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

        public static void ShowErrorMessage(string error)
        {
            //var message = GenerateErrorMessage(error);
            Message.Show(error);
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
    }
}
