using System.Threading.Tasks;

namespace IDosGames
{
    public static class RewardAPI
    {
        private static string GetEndpoint(RewardAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Reward/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(RewardAction action, RewardRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<RewardDefinitionsResponse>> GetRewardDefinitions(RewardRequest request)
            => SendRequest<RewardDefinitionsResponse>(RewardAction.GetRewardDefinitions, request);

        public static Task<OperationResult<UserRewardsStateResponse>> GetUserRewardsState(RewardRequest request)
            => SendRequest<UserRewardsStateResponse>(RewardAction.GetUserRewardsState, request);

        public static Task<OperationResult<ClaimDailyRewardResponse>> ClaimDailyReward(RewardRequest request)
            => SendRequest<ClaimDailyRewardResponse>(RewardAction.ClaimDailyReward, request);

        public static Task<OperationResult<CollectIdleAccrualResponse>> CollectIdleAccrual(RewardRequest request)
            => SendRequest<CollectIdleAccrualResponse>(RewardAction.CollectIdleAccrual, request);

        public static Task<OperationResult<ClaimComebackRewardResponse>> ClaimComebackReward(RewardRequest request)
            => SendRequest<ClaimComebackRewardResponse>(RewardAction.ClaimComebackReward, request);

        public static Task<OperationResult<ClaimRewardResponse>> ClaimReward(RewardRequest request)
            => SendRequest<ClaimRewardResponse>(RewardAction.ClaimReward, request);
    }
}
