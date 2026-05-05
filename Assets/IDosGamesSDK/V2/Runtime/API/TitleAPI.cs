using System.Threading.Tasks;

namespace IDosGames
{
    public static class TitleAPI
    {
        private static string GetEndpoint(TitleAction action, string userID)
        {
            string titleID = !string.IsNullOrEmpty(IDosGamesSDKSettings.Instance.TitleID) ? IDosGamesSDKSettings.Instance.TitleID : "0";
            return $"api/v2/{titleID}/Client/Title/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(TitleAction action, TitleRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static async Task<OperationResult<TitlePublicConfigurationModel>> GetTitlePublicConfiguration(TitleRequest request)
            => await SendRequest<TitlePublicConfigurationModel>(TitleAction.GetTitlePublicConfiguration, request);

        public static async Task<OperationResult<CustomTitleDataResponse>> GetPublicCustomTitleData(TitleRequest request)
            => await SendRequest<CustomTitleDataResponse>(TitleAction.GetPublicCustomTitleData, request);

        public static async Task<OperationResult<CurrencyDefinitions>> GetCurrencyDefinitions(TitleRequest request)
            => await SendRequest<CurrencyDefinitions>(TitleAction.GetCurrencyDefinitions, request);

        public static async Task<OperationResult<ItemDefinitions>> GetItemDefinitions(TitleRequest request)
            => await SendRequest<ItemDefinitions>(TitleAction.GetItemDefinitions, request);

        public static async Task<OperationResult<SuccessResponse>> GetServerTime(TitleRequest request)
            => await SendRequest<SuccessResponse>(TitleAction.GetServerTime, request);
    }
}
