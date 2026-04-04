using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class LimitedTimeEventAPI
    {
        private static string GetEndpoint(LimitedTimeEventAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/LimitedTimeEvent/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(
            LimitedTimeEventAction action, LimitedTimeEventRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<GetActiveEventsResponse>> GetActiveEvents(LimitedTimeEventRequest request)
        {
            return await SendRequest<GetActiveEventsResponse>(LimitedTimeEventAction.GetActiveEvents, request);
        }

        public static async Task<OperationResult<LimitedTimeEventsDefinition>> GetDefinitions(LimitedTimeEventRequest request)
        {
            return await SendRequest<LimitedTimeEventsDefinition>(LimitedTimeEventAction.GetDefinitions, request);
        }

        public static async Task<OperationResult<UserLimitedTimeEventsState>> GetUserState(LimitedTimeEventRequest request)
        {
            return await SendRequest<UserLimitedTimeEventsState>(LimitedTimeEventAction.GetUserState, request);
        }

        public static async Task<OperationResult<EventTokenGrantInfo>> GrantTokens(LimitedTimeEventRequest request)
        {
            return await SendRequest<EventTokenGrantInfo>(LimitedTimeEventAction.GrantTokens, request);
        }

        public static async Task<OperationResult<EventTokenSpendResponse>> SpendTokens(LimitedTimeEventRequest request)
        {
            return await SendRequest<EventTokenSpendResponse>(LimitedTimeEventAction.SpendTokens, request);
        }

        public static async Task<OperationResult<EventMilestoneClaimResponse>> ClaimMilestone(LimitedTimeEventRequest request)
        {
            return await SendRequest<EventMilestoneClaimResponse>(LimitedTimeEventAction.ClaimMilestone, request);
        }

        public static async Task<OperationResult<EventStreakClaimResponse>> ClaimStreakReward(LimitedTimeEventRequest request)
        {
            return await SendRequest<EventStreakClaimResponse>(LimitedTimeEventAction.ClaimStreakReward, request);
        }
    }
}
