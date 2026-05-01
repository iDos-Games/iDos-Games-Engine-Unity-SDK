using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class StoreAPI
    {
        private static string GetEndpoint(StoreAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Store/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(StoreAction action, StoreRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        // =================================================================================
        // PUBLIC METHODS
        // =================================================================================

        public static async Task<OperationResult<StoreDefinitions>> GetDefinitions(StoreRequest request)
        {
            return await SendRequest<StoreDefinitions>(StoreAction.GetDefinitions, request);
        }

        public static async Task<OperationResult<UserStoreState>> GetUserState(StoreRequest request)
        {
            return await SendRequest<UserStoreState>(StoreAction.GetUserState, request);
        }

        public static async Task<OperationResult<StorePurchaseResponse>> Purchase(StoreRequest request)
        {
            return await SendRequest<StorePurchaseResponse>(StoreAction.Purchase, request);
        }
    }
}
