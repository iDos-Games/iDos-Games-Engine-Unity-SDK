using System.Threading.Tasks;

namespace IDosGames
{
    public static class TimedEventAPI
    {
        private static string GetEndpoint(TimedEventAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/TimedEvent/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(TimedEventAction action, TimedEventRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<GetActiveEventsResponse>> GetActiveEvents(TimedEventRequest request)
            => SendRequest<GetActiveEventsResponse>(TimedEventAction.GetActiveEvents, request);

        public static Task<OperationResult<TimedEventDefinitions>> GetDefinitions(TimedEventRequest request)
            => SendRequest<TimedEventDefinitions>(TimedEventAction.GetDefinitions, request);

        public static Task<OperationResult<UserTimedEventStateResponse>> GetUserLteState(TimedEventRequest request)
            => SendRequest<UserTimedEventStateResponse>(TimedEventAction.GetUserLteState, request);

        public static Task<OperationResult<ResourceOperation>> GrantTokens(TimedEventRequest request)
            => SendRequest<ResourceOperation>(TimedEventAction.GrantTokens, request);

        public static Task<OperationResult<EventTokenSpendResponse>> SpendTokens(TimedEventRequest request)
            => SendRequest<EventTokenSpendResponse>(TimedEventAction.SpendTokens, request);

        public static Task<OperationResult<EventMilestoneClaimResponse>> ClaimMilestone(TimedEventRequest request)
            => SendRequest<EventMilestoneClaimResponse>(TimedEventAction.ClaimMilestone, request);
    }
}
