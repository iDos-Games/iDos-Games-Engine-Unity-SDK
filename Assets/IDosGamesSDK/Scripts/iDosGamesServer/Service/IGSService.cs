using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    public static class IGSService
    {
        public static event Action<Action> ConnectionError;

        public static string URL_LOGIN_SYSTEM = IDosGamesSDKSettings.Instance.AuthenticationLink;
        public static string URL_USER_DATA_SYSTEM = IDosGamesSDKSettings.Instance.UserDataSystemLink;
        public static string URL_WALLET_MAKE_TRANSACTION = IDosGamesSDKSettings.Instance.CryptoWalletLink;
        public static string URL_MARKETPLACE_DO_ACTION = IDosGamesSDKSettings.Instance.MarketplaceLink;
        public static string URL_MARKETPLACE_GET_DATA = IDosGamesSDKSettings.Instance.MarketplaceDataLink;
        public static string URL_VALIDATE_IAP_SUBSCRIPTION = IDosGamesSDKSettings.Instance.ValidateIAPSubscriptionLink;
        public static string URL_TOURNAMENT = IDosGamesSDKSettings.Instance.TournamentLink;

        public static async Task<GetAllUserDataResult> GetUserAllData(string userID, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.GetUserAllData.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                ClientSessionTicket = clientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
            };

            string responseString = await SendPostRequest(URL_USER_DATA_SYSTEM + nameof(GetUserAllData), requestBody);
            var response = JsonConvert.DeserializeObject<GetAllUserDataResult>(responseString);

            if (response != null ) { IDosGamesSDKSettings.Instance.PlayTime = 0; }

            return response;
        }

        // ------------------ Login / Registration ------------------ //
        public static async Task<GetAllUserDataResult> LoginWithDeviceID()
        {
            string deviceID = GetOrCreateDeviceID();
            string userName = GetUserName();
            string device = GetDevice();
            string platform = GetPlatform();

            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.LoginWithDeviceID.ToString(),
                WebAppLink = WebSDK.webAppLink,
                Platform = platform,
                Device = device,
                DeviceID = deviceID,
                UserName = userName,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
            };

            string response = await SendPostRequest(URL_LOGIN_SYSTEM + nameof(LoginWithDeviceID), requestBody);
            
            // Десериализация строки в объект GetAllUserDataResult  
            GetAllUserDataResult result = JsonConvert.DeserializeObject<GetAllUserDataResult>(response);

            if (result != null ) { IDosGamesSDKSettings.Instance.PlayTime = 0; }

            return result;
        }

        public static async Task<GetAllUserDataResult> LoginWithTelegram(string telegramInitData)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                TelegramInitData = telegramInitData,
            };

            string response = await SendPostRequest(URL_LOGIN_SYSTEM + ServerFunctionHandlers.LoginWithTelegram.ToString(), requestBody);

            // Десериализация строки в объект GetAllUserDataResult  
            GetAllUserDataResult result = JsonConvert.DeserializeObject<GetAllUserDataResult>(response);

            if (result != null) { IDosGamesSDKSettings.Instance.PlayTime = 0; }

            return result;
        }

        private static string GetOrCreateDeviceID()
        {
            string deviceID;

#if UNITY_WEBGL
            deviceID = PlayerPrefs.GetString("DeviceID", null);

            if (string.IsNullOrEmpty(deviceID))
            {
                deviceID = Guid.NewGuid().ToString();

                PlayerPrefs.SetString("DeviceID", deviceID);
                PlayerPrefs.Save();
            }

#else
            deviceID = SystemInfo.deviceUniqueIdentifier;
#endif

            return deviceID;
        }

        private static string GetUserName()
        {
            string userName = null;
#if UNITY_WEBGL
            if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
            {
                //userName = AuthService.TelegramInitData.user.username;
            }
#endif
            return userName;
        }

        private static string GetDevice()
        {
            string device = null;
#if UNITY_WEBGL
            device = "0";
#else
            device = SystemInfo.deviceModel;
#endif
            return device;
        }

        private static string GetPlatform()
        {
            string platform = null;
#if UNITY_WEBGL
            if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
            {
                platform = "Telegram";
            }
            else
            {
                platform = "WebGLPlayer";
            }
#else
            platform = Application.platform.ToString();
#endif
            return platform;
        }

        public static async Task<GetAllUserDataResult> LoginWithEmail(string email, string password)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.LoginWithEmail.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                Email = email,
                Password = password
            };

            string response = await SendPostRequest(URL_LOGIN_SYSTEM + nameof(LoginWithEmail), requestBody);

            GetAllUserDataResult result = JsonConvert.DeserializeObject<GetAllUserDataResult>(response);

            if (result != null) { IDosGamesSDKSettings.Instance.PlayTime = 0; }

            return result;
        }

        public static async Task<string> AddEmailAndPassword(string userID, string email, string password, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.AddEmailAndPassword.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                Email = email,
                Password = password,
                ClientSessionTicket = clientSessionTicket
            };

            return await SendPostRequest(URL_LOGIN_SYSTEM + nameof(AddEmailAndPassword), requestBody);
        }

        public static async Task<GetAllUserDataResult> RegisterUserByEmail(string email, string password)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.RegisterUserByEmail.ToString(),
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                WebAppLink = WebSDK.webAppLink,
                Email = email,
                Password = password,
                Platform = Application.platform.ToString(),
                Device = SystemInfo.deviceModel,
                DeviceID = SystemInfo.deviceUniqueIdentifier
            };

            string response = await SendPostRequest(URL_LOGIN_SYSTEM + nameof(RegisterUserByEmail), requestBody);

            GetAllUserDataResult result = JsonConvert.DeserializeObject<GetAllUserDataResult>(response);

            if (result != null) { IDosGamesSDKSettings.Instance.PlayTime = 0; }

            return result;
        }

        public static async Task<string> ForgotPassword(string email)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.ForgotPassword.ToString(),
                WebAppLink = WebSDK.webAppLink,
                Email = email,
            };

            return await SendPostRequest(URL_LOGIN_SYSTEM + ServerFunctionHandlers.ForgotPassword.ToString(), requestBody);
        }

        public static async Task<string> ResetPassword(string resetToken, string password)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.ResetPassword.ToString(),
                WebAppLink = WebSDK.webAppLink,
                ResetToken = resetToken,
                Password = password,
            };

            return await SendPostRequest(URL_LOGIN_SYSTEM + ServerFunctionHandlers.ResetPassword.ToString(), requestBody);
        }
        // ------------------ Login / Registration END ------------------ //

        // ------------------------ Inventory ------------------------ //
        public static async Task<string> RequestUserInventory(string userID, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.GetUserInventory.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                ClientSessionTicket = clientSessionTicket
            };

            return await SendPostRequest(URL_USER_DATA_SYSTEM + ServerFunctionHandlers.GetUserInventory.ToString(), requestBody);
        }
        
        public static async Task<string> RequestCustomUserData(string userID, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.GetCustomUserData.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                ClientSessionTicket = clientSessionTicket
            };

            return await SendPostRequest(URL_USER_DATA_SYSTEM + ServerFunctionHandlers.GetCustomUserData.ToString(), requestBody);
        }

        public static async Task<TitlePublicConfigurationModel> RequestTitlePublicConfiguration(string userID, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.GetTitlePublicConfiguration.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                ClientSessionTicket = clientSessionTicket,
            };

            string response = await SendPostRequest(URL_USER_DATA_SYSTEM + ServerFunctionHandlers.GetTitlePublicConfiguration.ToString(), requestBody);

            TitlePublicConfigurationModel result = JsonConvert.DeserializeObject<TitlePublicConfigurationModel>(response);

            return result;
        }

        public static async Task<GetCatalogItemsResult> RequestCatalogItems(string catalogVersion, string userID, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.GetCatalogItems.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                ClientSessionTicket = clientSessionTicket,
                CatalogVersion = catalogVersion
            };

            string response = await SendPostRequest(URL_USER_DATA_SYSTEM + ServerFunctionHandlers.GetCatalogItems.ToString(), requestBody);

            GetCatalogItemsResult result = JsonConvert.DeserializeObject<GetCatalogItemsResult>(response);

            return result;
        }
        // ------------------------ Inventory END ------------------------ //

        public static async Task<string> ValidateIAPSubscription(string receipt = null)
        {
            var requestBody = new JObject
            {
				{ "TitleID", IDosGamesSDKSettings.Instance.TitleID },
                { "BuildKey", IDosGamesSDKSettings.Instance.BuildKey },
                { "WebAppLink", WebSDK.webAppLink },
                { "UserID", AuthService.UserID },
                { "ClientSessionTicket", AuthService.ClientSessionTicket },
                { "Receipt", receipt }
            };

            return await SendJObjectRequest(URL_VALIDATE_IAP_SUBSCRIPTION, requestBody);
        }

        public static async Task<string> RequestLeaderboard(string leaderboardID, string userID, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.GetLeaderboard.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                ClientSessionTicket = clientSessionTicket,
                LeaderboardID = leaderboardID
            };

            return await SendPostRequest(URL_USER_DATA_SYSTEM + ServerFunctionHandlers.GetLeaderboard.ToString(), requestBody);
        }

        public static async Task<string> ClaimTournamentReward(string statisticName, string userID, string clientSessionTicket)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.ClaimTournamentReward.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = userID,
                ClientSessionTicket = clientSessionTicket,
                StatisticName = statisticName
            };

            return await SendPostRequest(URL_TOURNAMENT + ServerFunctionHandlers.ClaimTournamentReward.ToString(), requestBody);
        }

#if IDOSGAMES_MARKETPLACE

        public static async Task<string> GetDataFromMarketplace(MarketplaceGetDataRequest request)
        {
            request.TitleID = IDosGamesSDKSettings.Instance.TitleID;
            request.BuildKey = IDosGamesSDKSettings.Instance.BuildKey;
            request.UserID = AuthService.UserID;
            request.ClientSessionTicket = AuthService.ClientSessionTicket;
            request.WebAppLink = WebSDK.webAppLink;

            var requestBody = (JObject)JToken.FromObject(request);

            return await SendJObjectRequest(URL_MARKETPLACE_GET_DATA + request.Panel.ToString(), requestBody);
        }

        public static async Task<string> TryDoMarketplaceAction(MarketplaceActionRequest request)
        {
            request.TitleID = IDosGamesSDKSettings.Instance.TitleID;
            request.BuildKey = IDosGamesSDKSettings.Instance.BuildKey;
            request.UserID = AuthService.UserID;
            request.ClientSessionTicket = AuthService.ClientSessionTicket;
            request.WebAppLink = WebSDK.webAppLink;

            var requestBody = (JObject)JToken.FromObject(request);

            return await SendJObjectRequest(URL_MARKETPLACE_DO_ACTION + request.Action.ToString(), requestBody);
        }

#endif

#if IDOSGAMES_CRYPTO_WALLET

        public static async Task<string> TryMakeTransaction(WalletTransactionRequest request)
        {
            request.TitleID = IDosGamesSDKSettings.Instance.TitleID;
            request.BuildKey = IDosGamesSDKSettings.Instance.BuildKey;
            request.UserID = AuthService.UserID;
            request.ClientSessionTicket = AuthService.ClientSessionTicket;
            request.WebAppLink = WebSDK.webAppLink;

            var requestBody = (JObject)JToken.FromObject(request);

            return await SendJObjectRequest(URL_WALLET_MAKE_TRANSACTION, requestBody);
        }

#endif

        public static async Task<string> SendPostRequest(string functionURL, IGSRequest request)
        {
            var requestBody = JObject.FromObject(request);
            byte[] bodyRaw = new UTF8Encoding(true).GetBytes(requestBody.ToString());

            using (UnityWebRequest webRequest = new UnityWebRequest(functionURL, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string result = webRequest.downloadHandler.text;

                    if (result.Contains(MessageCode.SESSION_EXPIRED.ToString()) || result.Contains(MessageCode.INVALID_SESSION_TICKET.ToString()))
                    {
                        AuthService.Instance.AutoLogin();
                    }

                    if (IDosGamesSDKSettings.Instance.DebugLogging)
                    {
                        Debug.Log(result);
                        //LogLongString(result);  
                    }

                    return result;
                }
                else
                {
                    string errorDetail = $"Error request: {webRequest.error}";
                    Debug.LogError(errorDetail);

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                    {
                        ConnectionError?.Invoke(null);
                        return "ConnectionError: " + errorDetail;
                    }

                    return "Error: " + errorDetail;
                }
            }
        }

        private static async Task<string> SendJObjectRequest(string functionURL, JObject requestBody)
        {
            byte[] bodyRaw = new UTF8Encoding(true).GetBytes(requestBody.ToString());

            using (UnityWebRequest webRequest = new UnityWebRequest(functionURL, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string result = webRequest.downloadHandler.text;

                    if (result.Contains(MessageCode.SESSION_EXPIRED.ToString()) || result.Contains(MessageCode.INVALID_SESSION_TICKET.ToString()))
                    {
                        AuthService.Instance.AutoLogin();
                    }

                    if (IDosGamesSDKSettings.Instance.DebugLogging)
                    {
                        Debug.Log(result);
                    }

                    return result;
                }
                else
                {
                    string errorDetail = $"Error request: {webRequest.error}";
                    Debug.LogError(errorDetail);

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                    {
                        ConnectionError?.Invoke(null);
                        return "ConnectionError: " + errorDetail;
                    }

                    return "Error: " + errorDetail;
                }
            }
        }

        public static void LogLongString(string longString)
        {
            const int chunkSize = 15000; // Размер фрагмента  
            for (int i = 0; i < longString.Length; i += chunkSize)
            {
                if (i + chunkSize > longString.Length)
                {
                    Debug.Log(longString.Substring(i));
                }
                else
                {
                    Debug.Log(longString.Substring(i, chunkSize));
                }
            }
        }
    }
}
