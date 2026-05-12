using System.Threading.Tasks;

namespace IDosGames
{
    public static class TimedBoostAPI
    {
        private static string GetEndpoint(TimedBoostAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/TimedBoost/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(TimedBoostAction action, TimedBoostRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<TimedBoostDefinitions>> GetDefinitions(TimedBoostRequest request)
            => SendRequest<TimedBoostDefinitions>(TimedBoostAction.GetDefinitions, request);

        public static Task<OperationResult<GetActiveTimedBoostsResponse>> GetActive(TimedBoostRequest request)
            => SendRequest<GetActiveTimedBoostsResponse>(TimedBoostAction.GetActive, request);

        public static Task<OperationResult<ActivateTimedBoostResponse>> Activate(TimedBoostRequest request)
            => SendRequest<ActivateTimedBoostResponse>(TimedBoostAction.Activate, request);

        public static Task<OperationResult<SuccessResponse>> CleanupExpired(TimedBoostRequest request)
            => SendRequest<SuccessResponse>(TimedBoostAction.CleanupExpired, request);
    }
}
