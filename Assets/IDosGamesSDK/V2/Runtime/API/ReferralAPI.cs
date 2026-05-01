using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class ReferralAPI
    {
        private static string GetEndpoint(ReferralAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Referral/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(ReferralAction action, ReferralRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        // =================================================================================
        // PUBLIC METHODS
        // =================================================================================

        public static async Task<OperationResult<GetReferralDefinitionsResponse>> GetDefinitions(ReferralRequest request)
        {
            return await SendRequest<GetReferralDefinitionsResponse>(ReferralAction.GetDefinitions, request);
        }

        public static async Task<OperationResult<GetUserReferralStateResponse>> GetUserState(ReferralRequest request)
        {
            return await SendRequest<GetUserReferralStateResponse>(ReferralAction.GetUserState, request);
        }

        public static async Task<OperationResult<ActivateReferralCodeResponse>> ActivateReferralCode(ReferralRequest request)
        {
            return await SendRequest<ActivateReferralCodeResponse>(ReferralAction.ActivateReferralCode, request);
        }

        public static async Task<OperationResult<ClaimInviteRewardResponse>> ClaimInviteReward(ReferralRequest request)
        {
            return await SendRequest<ClaimInviteRewardResponse>(ReferralAction.ClaimInviteReward, request);
        }
    }
}
