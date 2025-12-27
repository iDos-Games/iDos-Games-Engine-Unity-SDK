using System.Threading.Tasks;
using IDosGames.ServerModels;

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

        public static async Task<OperationResult<RewardClaimResponse>> Claim(RewardClaimRequest request)
        {
            return await HttpService.Post<RewardClaimResponse>(
                GetEndpoint(RewardAction.Claim, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<RewardClaimResponse>> ClaimVip(RewardClaimRequest request)
        {
            return await HttpService.Post<RewardClaimResponse>(
                GetEndpoint(RewardAction.ClaimVip, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<RewardClaimResponse>> ClaimItemProfit(RewardClaimRequest request)
        {
            return await HttpService.Post<RewardClaimResponse>(
                GetEndpoint(RewardAction.ClaimItemProfit, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }
    }
}
