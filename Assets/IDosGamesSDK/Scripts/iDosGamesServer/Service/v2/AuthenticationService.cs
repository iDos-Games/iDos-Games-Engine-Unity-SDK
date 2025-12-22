using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using IDosGames.ServerModels;

namespace IDosGames
{
    /// <summary>
    /// Authorization service for API V2.
    /// Works in parallel with the old AuthService to facilitate migration.
    /// </summary>
    public static class AuthenticationService
    {
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 100;
        private const string EMAIL_REGEX = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        private static string SAVED_AUTH_TYPE_KEY => "Saved_AuthType" + GetTitleID();
        private static string SAVED_AUTH_EMAIL_KEY => "Saved_Auth_Email" + GetTitleID();
        private static string SAVED_AUTH_PASSWORD_KEY => "Saved_Auth_Password" + GetTitleID();

        private static string KEY_TOKEN => "IDos_V2_Token_" + GetTitleID();
        private static string KEY_USERID => "IDos_V2_UserID_" + GetTitleID();

        public static string UserID { get; private set; }
        public static string ClientSessionTicket { get; private set; }
        public static string EntityToken { get; private set; }
        
        // Events to subscribe to from UI or other systems
        public static event Action OnLoginSuccess;
        public static event Action<string> OnLoginFailed;

        public static bool IsLoggedIn => !string.IsNullOrEmpty(ClientSessionTicket);
        public static bool IsValidEmail(string email) => Regex.IsMatch(email, EMAIL_REGEX, RegexOptions.IgnoreCase);

        private static string GetEndpoint(string action)
        {
            string templateId = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleId = IDosGamesSDKSettings.Instance.TitleID;
            string userId = AuthService.UserID;

            return $"v2/{templateId}/{titleId}/Client/Authentication/{action}";
        }

        // =================================================================================
        // LOGIN METHODS
        // =================================================================================

        public static async Task<IDosGamesApiResult<AuthenticationResponse>> LoginWithDevice()
        {
            var request = new { DeviceID = SystemInfo.deviceUniqueIdentifier };
            var result = await HttpService.Post<AuthenticationResponse>(GetEndpoint("LoginWithDevice"), request);

            if (result.Success) HandleSuccess(result.Data);
            else OnLoginFailed?.Invoke(result.Error);

            return result;
        }

        public static async Task<IDosGamesApiResult<AuthenticationResponse>> LoginWithEmail(string email, string password)
        {
            var request = new { Email = email, Password = password };
            var result = await HttpService.Post<AuthenticationResponse>(GetEndpoint("LoginWithEmail"), request);

            if (result.Success) HandleSuccess(result.Data);
            else OnLoginFailed?.Invoke(result.Error);

            return result;
        }

        public static async Task<IDosGamesApiResult<AuthenticationResponse>> LoginWithTelegram(string initData)
        {
            var request = new { InitData = initData };
            var result = await HttpService.Post<AuthenticationResponse>(GetEndpoint("LoginWithTelegram"), request);

            if (result.Success) HandleSuccess(result.Data);
            else OnLoginFailed?.Invoke(result.Error);

            return result;
        }

        // =================================================================================
        // Internal logic
        // =================================================================================

        private static void HandleSuccess(AuthenticationResponse response)
        {
            UserID = response.AuthContext.UserID;
            ClientSessionTicket = response.AuthContext.ClientSessionTicket;

            // Synchronize with the old AuthService to prevent V1 modules from breaking.
            //AuthService.UpdateV2Credentials(UserID, ClientSessionTicket);

            SaveToPrefs();
            OnLoginSuccess?.Invoke();

            Debug.Log($"<color=green>[AuthorizationService]</color> User logged in: {UserID}");
        }

        private static void SaveToPrefs()
        {
            PlayerPrefs.SetString(KEY_TOKEN, ClientSessionTicket);
            PlayerPrefs.SetString(KEY_USERID, UserID);
            PlayerPrefs.Save();
        }

        public static void LoadSavedSession()
        {
            ClientSessionTicket = PlayerPrefs.GetString(KEY_TOKEN, "");
            UserID = PlayerPrefs.GetString(KEY_USERID, "");
        }

        public static void Logout()
        {
            ClientSessionTicket = null;
            UserID = null;
            PlayerPrefs.DeleteKey(KEY_TOKEN);
            PlayerPrefs.DeleteKey(KEY_USERID);
            PlayerPrefs.Save();
            Loading.SwitchToLoginScene();
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

    }
}
