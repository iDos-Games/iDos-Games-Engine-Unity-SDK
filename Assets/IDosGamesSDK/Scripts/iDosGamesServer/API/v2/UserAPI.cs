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

        public static async Task<OperationResult<AuthenticationResponse>> GetUserAllData(UserRequest request)
        {
            return await HttpService.Post<AuthenticationResponse>(
                GetEndpoint(UserAction.GetUserAllData, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<GetUserInventoryResult>> GetUserInventory(UserRequest request)
        {
            return await HttpService.Post<GetUserInventoryResult>(
                GetEndpoint(UserAction.GetUserInventory, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<GetCustomUserDataResult>> GetCustomUserData(UserRequest request)
        {
            return await HttpService.Post<GetCustomUserDataResult>(
                GetEndpoint(UserAction.GetCustomUserData, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<SuccessResponse>> UpdateCustomUserData(UserRequest request)
        {
            return await HttpService.Post<SuccessResponse>(
                GetEndpoint(UserAction.UpdateCustomUserData, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<CurrencyUpdateResponse>> SubtractVirtualCurrency(UserRequest request)
        {
            return await HttpService.Post<CurrencyUpdateResponse>(
                GetEndpoint(UserAction.SubtractVirtualCurrency, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<SuccessResponse>> DeleteUserAccount(UserRequest request)
        {
            return await HttpService.Post<SuccessResponse>(
                GetEndpoint(UserAction.DeleteUserAccount, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }
    }
}
