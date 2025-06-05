using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IDosGames
{
    public static class IGSClientAPI
    {
        static IGSClientAPI() {}

        private const int delay = 2000;

        public static event Action RequestError;
        public static event Action<Action> ConnectionError;

        public static event Action ServerFunctionCalled;
        public static event Action ServerFunctionResponsed;

        public static async void GetUserAllData(Action<GetAllUserDataResult> resultCallback, Action<string> notConnectionErrorCallback, Action connectionErrorCallback = null)
        {
            if (!AuthService.AuthContext.IsClientLoggedIn()) throw new IGSException(IGSExceptionCode.NotLoggedIn, "Must be logged in to call this method");

            ServerFunctionCalled?.Invoke();

            try
            {
                GetAllUserDataResult response = await IGSService.GetUserAllData(AuthService.UserID, AuthService.ClientSessionTicket);
                if (response != null)
                {
                    resultCallback?.Invoke(response);
                    ServerFunctionResponsed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnIGSError(ex.Message, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        public static async void GetUserInventory(Action<GetUserInventoryResult> resultCallback, Action<string> notConnectionErrorCallback, Action connectionErrorCallback = null)
        {
            
            //var context = request.AuthenticationContext;
            if (!AuthService.AuthContext.IsClientLoggedIn()) throw new IGSException(IGSExceptionCode.NotLoggedIn, "Must be logged in to call this method");

            ServerFunctionCalled?.Invoke();

            try
            {
                string response = await IGSService.RequestUserInventory(AuthService.UserID, AuthService.ClientSessionTicket);
                if (!string.IsNullOrEmpty(response))
                {
                    var inventoryResult = JsonConvert.DeserializeObject<GetUserInventoryResult>(response);
                    resultCallback?.Invoke(inventoryResult);
                    ServerFunctionResponsed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnIGSError(ex.Message, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        public static async void GetTitlePublicConfiguration(Action<TitlePublicConfigurationModel> resultCallback, Action<string> notConnectionErrorCallback = null, Action connectionErrorCallback = null)
        {
            if (!AuthService.AuthContext.IsClientLoggedIn()) throw new IGSException(IGSExceptionCode.NotLoggedIn, "Must be logged in to call this method");

            ServerFunctionCalled?.Invoke();

            try
            {
                var response = await IGSService.RequestTitlePublicConfiguration(AuthService.UserID, AuthService.ClientSessionTicket);
                if (response != null)
                {
                    resultCallback?.Invoke(response);
                    ServerFunctionResponsed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnIGSError(ex.Message, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        public static async void GetCustomUserData(Action<GetCustomUserDataResult> resultCallback, Action<string> notConnectionErrorCallback = null, Action connectionErrorCallback = null)
        {
            if (!AuthService.AuthContext.IsClientLoggedIn()) throw new IGSException(IGSExceptionCode.NotLoggedIn, "Must be logged in to call this method");

            ServerFunctionCalled?.Invoke();

            try
            {
                string response = await IGSService.RequestCustomUserData(AuthService.UserID, AuthService.ClientSessionTicket);
                if (!string.IsNullOrEmpty(response))
                {
                    var result = JsonConvert.DeserializeObject<GetCustomUserDataResult>(response);
                    resultCallback?.Invoke(result);

                    ServerFunctionResponsed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnIGSError(ex.Message, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        public static async void GetCatalogItems(string catalogVersion, Action<GetCatalogItemsResult> resultCallback, Action<string> notConnectionErrorCallback = null, Action connectionErrorCallback = null)
        {
            if (!AuthService.AuthContext.IsClientLoggedIn()) throw new IGSException(IGSExceptionCode.NotLoggedIn, "Must be logged in to call this method");

            ServerFunctionCalled?.Invoke();

            try
            {
                var response = await IGSService.RequestCatalogItems(catalogVersion, AuthService.UserID, AuthService.ClientSessionTicket);
                if (response != null)
                {
                    resultCallback?.Invoke(response);
                    ServerFunctionResponsed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnIGSError(ex.Message, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        public static async void GetLeaderboard(string leaderboardID, Action<GetLeaderboardResult> resultCallback, Action<string> notConnectionErrorCallback = null, Action connectionErrorCallback = null)
        {
            if (!AuthService.AuthContext.IsClientLoggedIn()) throw new IGSException(IGSExceptionCode.NotLoggedIn, "Must be logged in to call this method");

            ServerFunctionCalled?.Invoke();

            try
            {
                string response = await IGSService.RequestLeaderboard(leaderboardID, AuthService.UserID, AuthService.ClientSessionTicket);
                if (!string.IsNullOrEmpty(response))
                {
                    var resultData = JsonConvert.DeserializeObject<GetLeaderboardResult>(response);
                    resultCallback?.Invoke(resultData);
                    ServerFunctionResponsed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnIGSError(ex.Message, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        public static async void ClaimTournamentReward(string statisticName, Action<UserLeaderboardRewards> resultCallback, Action<string> notConnectionErrorCallback = null, Action connectionErrorCallback = null)
        {
            if (!AuthService.AuthContext.IsClientLoggedIn()) throw new IGSException(IGSExceptionCode.NotLoggedIn, "Must be logged in to call this method");

            ServerFunctionCalled?.Invoke();

            try
            {
                string response = await IGSService.ClaimTournamentReward(statisticName, AuthService.UserID, AuthService.ClientSessionTicket);
                if (!string.IsNullOrEmpty(response))
                {
                    var resultData = JsonConvert.DeserializeObject<UserLeaderboardRewards>(response);
                    resultCallback?.Invoke(resultData);
                    ServerFunctionResponsed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnIGSError(ex.Message, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        public static async Task ExecuteFunction(ServerFunctionHandlers functionName, Action<string> resultCallback = null, Action<string> notConnectionErrorCallback = null, Action connectionErrorCallback = null, FunctionParameters functionParameter = null)
        {
            var request = new IGSRequest
            {
                FunctionName = functionName.ToString(),
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
                UserID = AuthService.UserID,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                FunctionParameter = functionParameter // Установите дополнительные параметры
            };

            string functionURL = GetFunctionURL(functionName) + functionName.ToString();

            ServerFunctionCalled?.Invoke();

            try
            {
                // Отправка объекта IGSRequest напрямую
                string result = await IGSService.SendPostRequest(functionURL, request);

                ServerFunctionResponsed?.Invoke();

                resultCallback?.Invoke(result);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                OnIGSError(error, notConnectionErrorCallback, connectionErrorCallback);
            }
        }

        private static string GetFunctionURL(ServerFunctionHandlers functionName)
        {
            switch (functionName)
            {
                case ServerFunctionHandlers.GetCommonChestReward:
                case ServerFunctionHandlers.GetRareChestReward:
                case ServerFunctionHandlers.GetLegendaryChestReward:
                    return IDosGamesSDKSettings.Instance.ChestSystemLink;
                case ServerFunctionHandlers.ActivateReferralCode:
                    return IDosGamesSDKSettings.Instance.ReferralSystemLink;
                case ServerFunctionHandlers.GetFreeSpinReward:
                case ServerFunctionHandlers.GetPremiumSpinReward:
                case ServerFunctionHandlers.GetStandardSpinReward:
                case ServerFunctionHandlers.GetSecondarySpinReward:
                case ServerFunctionHandlers.GetSecondarySpinRewardForVC:
                    return IDosGamesSDKSettings.Instance.SpinSystemLink;
                case ServerFunctionHandlers.GrantSkinProfitFromEquippedSkins:
                case ServerFunctionHandlers.ClaimTokenReward:
                case ServerFunctionHandlers.ClaimCoinReward:
                case ServerFunctionHandlers.ClaimX3CoinReward:
                case ServerFunctionHandlers.ClaimX5CoinReward:
                case ServerFunctionHandlers.ClaimCoinWithSkinReward:
                case ServerFunctionHandlers.ClaimX3CoinWithSkinReward:
                case ServerFunctionHandlers.ClaimX5CoinWithSkinReward:
                case ServerFunctionHandlers.UpdateEquippedSkins:
                    return IDosGamesSDKSettings.Instance.RewardAndProfitSystemLink;
                case ServerFunctionHandlers.StartNewWeeklyEventForPlayer:
                case ServerFunctionHandlers.AddWeeklyEventPoints:
                    return IDosGamesSDKSettings.Instance.EventSystemLink;
                case ServerFunctionHandlers.UpdateCustomUserData:
                case ServerFunctionHandlers.DeleteTitlePlayerAccount:
                case ServerFunctionHandlers.SubtractVirtualCurrencyHandler:
                case ServerFunctionHandlers.GetTitlePublicConfiguration:
                    return IDosGamesSDKSettings.Instance.UserDataSystemLink;
                case ServerFunctionHandlers.BuyItemDailyOffer:
                case ServerFunctionHandlers.BuyItemForVirtualCurrency:
                case ServerFunctionHandlers.BuyItemSpecialOffer:
                case ServerFunctionHandlers.GrantItemsAfterIAPPurchase:
                case ServerFunctionHandlers.GetFreeDailyReward:
                case ServerFunctionHandlers.UpdateDailyFreeProducts:
                    return IDosGamesSDKSettings.Instance.ShopSystemLink;
                default:
                    throw new ArgumentException("Unknown function name");
            }
        }

        public static void OnIGSError(string error, Action<string> notConnectionErrorCallback, Action retryCallback = null)
        {
            RequestError?.Invoke();

            if (string.IsNullOrEmpty(error))
            {
                return;
            }

            bool isConnectionError = error.StartsWith("ConnectionError");

            if (isConnectionError)
            {
                //ConnectionError?.Invoke(retryCallback);
                ConnectionError?.Invoke(() =>
                {
                    ScheduleRetry(retryCallback);
                });
            }
            else
            {
                notConnectionErrorCallback?.Invoke(error);
            }
        }

        private static void ScheduleRetry(Action retryCallback)
        {
            var timer = new System.Threading.Timer(_ =>
            {
                retryCallback?.Invoke();
            }, null, delay, Timeout.Infinite);
        }
    }
}
