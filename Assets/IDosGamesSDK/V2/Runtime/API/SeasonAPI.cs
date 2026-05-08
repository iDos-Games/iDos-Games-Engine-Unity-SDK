using System.Threading.Tasks;

namespace IDosGames
{
    public static class SeasonAPI
    {
        private static string GetEndpoint(SeasonAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Season/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(SeasonAction action, SeasonRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<SeasonDefinitions>> GetDefinitions(SeasonRequest request)
            => SendRequest<SeasonDefinitions>(SeasonAction.GetDefinitions, request);

        public static Task<OperationResult<ActiveSeasonInfo>> GetActiveSeason(SeasonRequest request)
            => SendRequest<ActiveSeasonInfo>(SeasonAction.GetActiveSeason, request);

        public static Task<OperationResult<UserSeasonState>> GetUserState(SeasonRequest request)
            => SendRequest<UserSeasonState>(SeasonAction.GetUserState, request);

        public static Task<OperationResult<GrantStatusTokensResponse>> GrantStatusTokens(SeasonRequest request)
            => SendRequest<GrantStatusTokensResponse>(SeasonAction.GrantStatusTokens, request);

        public static Task<OperationResult<ClaimTierRewardResponse>> ClaimTierReward(SeasonRequest request)
            => SendRequest<ClaimTierRewardResponse>(SeasonAction.ClaimTierReward, request);
    }
}
