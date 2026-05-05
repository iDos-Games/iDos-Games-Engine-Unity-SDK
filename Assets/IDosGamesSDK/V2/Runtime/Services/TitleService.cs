using System;
using System.Threading.Tasks;

namespace IDosGames
{
    public static class TitleService
    {
        public static event Action<TitlePublicConfigurationModel> OnTitlePublicConfigurationReceived;
        public static event Action<TitleCustomDataResponse> OnPublicCustomTitleDataReceived;
        public static event Action<CurrencyDefinitions> OnCurrencyDefinitionsReceived;
        public static event Action<ItemDefinitions> OnItemDefinitionsReceived;
        public static event Action<SuccessResponse> OnServerTimeReceived;

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static TitleRequest CreateBaseRequest()
        {
            return new TitleRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        public static async Task<OperationResult<TitlePublicConfigurationModel>> GetTitlePublicConfiguration()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetTitlePublicConfiguration(request);

            if (result.Success)
            {
                IDosGamesData.Config.ApplyTitlePublicConfiguration(result.Data);
                OnTitlePublicConfigurationReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<TitleCustomDataResponse>> GetPublicTitleCustomData()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetPublicTitleCustomData(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchTitleCustomData(result.Data);
                OnPublicCustomTitleDataReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<CurrencyDefinitions>> GetCurrencyDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetCurrencyDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchCurrencyDefinitions(result.Data);
                OnCurrencyDefinitionsReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<ItemDefinitions>> GetItemDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetItemDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchItemDefinitions(result.Data);
                OnItemDefinitionsReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> GetServerTime()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetServerTime(request);

            if (result.Success)
            {
                OnServerTimeReceived?.Invoke(result.Data);
            }

            return result;
        }
    }
}
