using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ServerModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class RewardAPI
    {
        private static string GetEndpoint(RewardAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Reward/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(RewardAction action, RewardRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<RewardResponse>> Claim(RewardRequest request)
        {
            return await SendRequest<RewardResponse>(RewardAction.Claim, request);
        }

        public static async Task<OperationResult<RewardResponse>> ClaimVip(RewardRequest request)
        {
            return await SendRequest<RewardResponse>(RewardAction.ClaimVip, request);
        }

        public static async Task<OperationResult<RewardResponse>> ClaimItemProfit(RewardRequest request)
        {
            return await SendRequest<RewardResponse>(RewardAction.ClaimItemProfit, request);
        }

        public static async Task<OperationResult<ClaimDailyRewardResponse>> ClaimDailyReward(RewardRequest request)
        {
            return await SendRequest<ClaimDailyRewardResponse>(RewardAction.ClaimDailyReward, request);
        }

        public static async Task<OperationResult<List<DailyRewardsDefinition>>> GetDailyRewardsDefinitions(RewardRequest request)
        {
            return await SendRequest<List<DailyRewardsDefinition>>(RewardAction.GetDailyRewardsDefinitions, request);
        }
    }
}
