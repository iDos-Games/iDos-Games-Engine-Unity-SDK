using System.Threading.Tasks;

namespace IDosGames
{
    public static class StoreAPI
    {
        private static string GetEndpoint(StoreAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Store/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(StoreAction action, StoreRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<StorePurchaseResponse>> Purchase(StoreRequest request)
            => SendRequest<StorePurchaseResponse>(StoreAction.Purchase, request);

        public static Task<OperationResult<StoreDefinitions>> GetDefinitions(StoreRequest request)
            => SendRequest<StoreDefinitions>(StoreAction.GetDefinitions, request);

        public static Task<OperationResult<UserStoreState>> GetUserState(StoreRequest request)
            => SendRequest<UserStoreState>(StoreAction.GetUserState, request);
    }
}
