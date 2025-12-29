using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.ServerModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class TitleAPI
    {
        private static string GetEndpoint(TitleAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Title/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(TitleAction action, TitleRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<Dictionary<string, object>>> GetTitlePublicData(TitleRequest request)
        {
            return await SendRequest<Dictionary<string, object>>(TitleAction.GetTitlePublicData, request);
        }

        public static async Task<OperationResult<TitlePublicConfigurationModel>> GetTitlePublicConfiguration(TitleRequest request)
        {
            return await SendRequest<TitlePublicConfigurationModel>(TitleAction.GetTitlePublicConfiguration, request);
        }

        public static async Task<OperationResult<GetCatalogItemsResult>> GetCatalogItems(TitleRequest request)
        {
            return await SendRequest<GetCatalogItemsResult>(TitleAction.GetCatalogItems, request);
        }

        public static async Task<OperationResult<GetLeaderboardResult>> GetLeaderboard(TitleRequest request)
        {
            return await SendRequest<GetLeaderboardResult>(TitleAction.GetLeaderboard, request);
        }

        public static async Task<OperationResult<PlatformSettingsModel>> GetPlatformSettings(TitleRequest request)
        {
            return await SendRequest<PlatformSettingsModel>(TitleAction.GetPlatformSettings, request);
        }

        public static async Task<OperationResult<Currencies>> GetCurrencyData(TitleRequest request)
        {
            return await SendRequest<Currencies>(TitleAction.GetCurrencyData, request);
        }

        public static async Task<OperationResult<SuccessResponse>> GetServerTime(TitleRequest request)
        {
            return await SendRequest<SuccessResponse>(TitleAction.GetServerTime, request);
        }
    }
}
