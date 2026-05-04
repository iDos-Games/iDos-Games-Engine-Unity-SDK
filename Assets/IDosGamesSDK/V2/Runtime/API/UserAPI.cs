using System.Threading.Tasks;

namespace IDosGames
{
    public static class UserAPI
    {
        private static string GetEndpoint(UserAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/User/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(UserAction action, UserRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static async Task<OperationResult<ClientState>> GetClientState(UserRequest request)
            => await SendRequest<ClientState>(UserAction.GetClientState, request);

        public static async Task<OperationResult<UserInventoryState>> GetInventory(UserRequest request)
            => await SendRequest<UserInventoryState>(UserAction.GetInventory, request);

        public static async Task<OperationResult<UserEventTokensState>> GetEventTokens(UserRequest request)
            => await SendRequest<UserEventTokensState>(UserAction.GetEventTokens, request);

        public static async Task<OperationResult<UsageTimeStats>> GetUsageTime(UserRequest request)
            => await SendRequest<UsageTimeStats>(UserAction.GetUsageTime, request);

        public static async Task<OperationResult<UsageTimeStats>> AddUsageTime(UserRequest request)
            => await SendRequest<UsageTimeStats>(UserAction.AddUsageTime, request);

        public static async Task<OperationResult<SuccessResponse>> DeleteUserAccount(UserRequest request)
            => await SendRequest<SuccessResponse>(UserAction.DeleteUserAccount, request);
    }
}
