using System.Threading.Tasks;

namespace IDosGames
{
    public static class CoopEventAPI
    {
        private static string GetEndpoint(CoopEventAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/CoopEvent/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(
            CoopEventAction action, CoopEventRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<CoopEventDefinitions>> GetDefinitions(CoopEventRequest request)
            => SendRequest<CoopEventDefinitions>(CoopEventAction.GetDefinitions, request);

        public static Task<OperationResult<ActiveCoopEventInfo>> GetActiveEvent(CoopEventRequest request)
            => SendRequest<ActiveCoopEventInfo>(CoopEventAction.GetActiveEvent, request);

        public static Task<OperationResult<CoopUserStateResponse>> GetUserState(CoopEventRequest request)
            => SendRequest<CoopUserStateResponse>(CoopEventAction.GetUserState, request);

        public static Task<OperationResult<CoopGroupStateResponse>> GetGroupState(CoopEventRequest request)
            => SendRequest<CoopGroupStateResponse>(CoopEventAction.GetGroupState, request);

        public static Task<OperationResult<CoopGroupStateResponse>> JoinOrCreateGroup(CoopEventRequest request)
            => SendRequest<CoopGroupStateResponse>(CoopEventAction.JoinOrCreateGroup, request);

        public static Task<OperationResult<CoopSpinResponse>> Spin(CoopEventRequest request)
            => SendRequest<CoopSpinResponse>(CoopEventAction.Spin, request);

        public static Task<OperationResult<CoopClaimRewardResponse>> ClaimObjectReward(CoopEventRequest request)
            => SendRequest<CoopClaimRewardResponse>(CoopEventAction.ClaimObjectReward, request);

        public static Task<OperationResult<CoopClaimRewardResponse>> ClaimGrandPrize(CoopEventRequest request)
            => SendRequest<CoopClaimRewardResponse>(CoopEventAction.ClaimGrandPrize, request);

        public static Task<OperationResult<CoopLeaveGroupResponse>> LeaveGroup(CoopEventRequest request)
            => SendRequest<CoopLeaveGroupResponse>(CoopEventAction.LeaveGroup, request);
    }
}
