using System.Threading.Tasks;

namespace IDosGames
{
    public static class UserCustomDataAPI
    {
        private static string GetEndpoint(UserCustomDataAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/UserCustomData/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(UserCustomDataAction action, UserCustomDataRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<UserCustomDataDefinitions>> GetUserCustomDataDefinitions(UserCustomDataRequest request)
            => SendRequest<UserCustomDataDefinitions>(UserCustomDataAction.GetUserCustomDataDefinitions, request);

        public static Task<OperationResult<GetMyUserCustomDataResponse>> GetMyUserCustomData(UserCustomDataRequest request)
            => SendRequest<GetMyUserCustomDataResponse>(UserCustomDataAction.GetMyUserCustomData, request);

        public static Task<OperationResult<GetPublicUserCustomDataResponse>> GetPublicUserCustomDataOf(UserCustomDataRequest request)
            => SendRequest<GetPublicUserCustomDataResponse>(UserCustomDataAction.GetPublicUserCustomDataOf, request);

        public static Task<OperationResult<SetUserCustomDataResponse>> SetPrivateData(UserCustomDataRequest request)
            => SendRequest<SetUserCustomDataResponse>(UserCustomDataAction.SetPrivateData, request);

        public static Task<OperationResult<SetUserCustomDataResponse>> SetPublicData(UserCustomDataRequest request)
            => SendRequest<SetUserCustomDataResponse>(UserCustomDataAction.SetPublicData, request);

        public static Task<OperationResult<SuccessResponse>> DeleteKey(UserCustomDataRequest request)
            => SendRequest<SuccessResponse>(UserCustomDataAction.DeleteKey, request);

        public static Task<OperationResult<BatchSetUserCustomDataResponse>> BatchSet(UserCustomDataRequest request)
            => SendRequest<BatchSetUserCustomDataResponse>(UserCustomDataAction.BatchSet, request);

        public static Task<OperationResult<BatchDeleteUserCustomDataResponse>> BatchDelete(UserCustomDataRequest request)
            => SendRequest<BatchDeleteUserCustomDataResponse>(UserCustomDataAction.BatchDelete, request);

        public static Task<OperationResult<BatchGetPublicUserCustomDataResponse>> BatchGetPublicUserCustomDataOf(UserCustomDataRequest request)
            => SendRequest<BatchGetPublicUserCustomDataResponse>(UserCustomDataAction.BatchGetPublicUserCustomDataOf, request);
    }
}
