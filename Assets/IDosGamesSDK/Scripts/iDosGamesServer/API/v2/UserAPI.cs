using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class UserAPI
    {
        private static string GetEndpoint(UserAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/User/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(UserAction action, UserRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<ClientStateResponse>> GetClientState(UserRequest request)
        {
            return await SendRequest<ClientStateResponse>(UserAction.GetClientState, request);
        }

        public static async Task<OperationResult<GetUserInventoryResult>> GetUserInventory(UserRequest request)
        {
            return await SendRequest<GetUserInventoryResult>(UserAction.GetUserInventory, request);
        }

        public static async Task<OperationResult<VirtualCurrencyResponse>> GetVirtualCurrency(UserRequest request)
        {
            return await SendRequest<VirtualCurrencyResponse>(UserAction.GetVirtualCurrency, request);
        }

        public static async Task<OperationResult<GetCustomUserDataResult>> GetCustomUserData(UserRequest request)
        {
            return await SendRequest<GetCustomUserDataResult>(UserAction.GetCustomUserData, request);
        }

        public static async Task<OperationResult<SuccessResponse>> UpdateCustomUserData(UserRequest request)
        {
            return await SendRequest<SuccessResponse>(UserAction.UpdateCustomUserData, request);
        }

        public static async Task<OperationResult<CurrencyUpdateResponse>> SubtractVirtualCurrency(UserRequest request)
        {
            return await SendRequest<CurrencyUpdateResponse>(UserAction.SubtractVirtualCurrency, request);
        }

        public static async Task<OperationResult<CurrencyTransferResponse>> TransferVirtualCurrency(UserRequest request)
        {
            return await SendRequest<CurrencyTransferResponse>(UserAction.TransferVirtualCurrency, request);
        }

        public static async Task<OperationResult<ConsumeItemResponse>> ConsumeItem(UserRequest request)
        {
            return await SendRequest<ConsumeItemResponse>(UserAction.ConsumeItem, request);
        }

        public static async Task<OperationResult<SuccessResponse>> DeleteUserAccount(UserRequest request)
        {
            return await SendRequest<SuccessResponse>(UserAction.DeleteUserAccount, request);
        }

        public static async Task<OperationResult<UsageTimeStats>> GetUsageTime(UserRequest request)
        {
            return await SendRequest<UsageTimeStats>(UserAction.GetUsageTime, request);
        }

        public static async Task<OperationResult<UsageTimeStats>> AddUsageTime(UserRequest request)
        {
            return await SendRequest<UsageTimeStats>(UserAction.AddUsageTime, request);
        }
    }
}
