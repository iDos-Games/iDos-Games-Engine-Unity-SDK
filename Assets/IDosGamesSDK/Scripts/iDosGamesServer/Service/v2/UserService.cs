using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class UserService
    {
        public static event Action<AuthenticationResponse> OnUserAllDataReceived;
        public static event Action<GetUserInventoryResult> OnUserInventoryReceived;
        public static event Action<GetCustomUserDataResult> OnCustomUserDataReceived;
        public static event Action<SuccessResponse> OnCustomUserDataUpdated;
        public static event Action<CurrencyUpdateResponse> OnVirtualCurrencySubtracted;
        public static event Action<SuccessResponse> OnUserAccountDeleted;
        public static event Action<UsageTimeStats> OnUsageTimeReceived;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static UserRequest CreateBaseRequest()
        {
            return new UserRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink
            };
        }

        public static async Task<OperationResult<AuthenticationResponse>> GetUserAllData()
        {
            var request = CreateBaseRequest();
            request.UsageTime = IDosGamesSDKSettings.Instance.PlayTime;

            var result = await UserAPI.GetUserAllData(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnUserAllDataReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<GetUserInventoryResult>> GetUserInventory()
        {
            var request = CreateBaseRequest();
            var result = await UserAPI.GetUserInventory(request);

            if (result.Success)
            {
                OnUserInventoryReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<GetCustomUserDataResult>> GetCustomUserData()
        {
            var request = CreateBaseRequest();
            var result = await UserAPI.GetCustomUserData(request);

            if (result.Success)
            {
                OnCustomUserDataReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> UpdateCustomUserData(string key, object value)
        {
            var request = CreateBaseRequest();

            request.Key = key;
            request.Value = value;

            var result = await UserAPI.UpdateCustomUserData(request);

            if (result.Success)
            {
                OnCustomUserDataUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<CurrencyUpdateResponse>> SubtractVirtualCurrency(string currencyId, int amount)
        {
            var request = CreateBaseRequest();

            request.CurrencyID = currencyId;
            request.SubtractAmount = amount;

            var result = await UserAPI.SubtractVirtualCurrency(request);

            if (result.Success)
            {
                UpdateCachedCurrencies(result.Data);
                OnVirtualCurrencySubtracted?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> DeleteUserAccount()
        {
            var request = CreateBaseRequest();
            var result = await UserAPI.DeleteUserAccount(request);

            if (result.Success)
            {
                OnUserAccountDeleted?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UsageTimeStats>> GetUsageTime()
        {
            var request = CreateBaseRequest();

            var result = await UserAPI.GetUsageTime(request);

            if (result.Success)
            {
                OnUsageTimeReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UsageTimeStats>> AddUsageTime()
        {
            var request = CreateBaseRequest();
            request.UsageTime = IDosGamesSDKSettings.Instance.PlayTime;

            var result = await UserAPI.AddUsageTime(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnUsageTimeReceived?.Invoke(result.Data);
            }

            return result;
        }

        private static void UpdateCachedCurrencies(CurrencyUpdateResponse response)
        {
            if (response == null) return;

            var inv = IGSUserData.UserInventory;
            if (inv == null) return;

            inv.VirtualCurrency ??= new Dictionary<string, int>();

            inv.VirtualCurrency[response.CurrencyID] = response.NewBalance;

            UserDataService.VirtualCurrencyUpdatedInvoke();
        }
    }
}
