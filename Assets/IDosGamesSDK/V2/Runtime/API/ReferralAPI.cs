using System.Threading.Tasks;

namespace IDosGames
{
    public static class ReferralAPI
    {
        private static string GetEndpoint(ReferralAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Referral/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(ReferralAction action, ReferralRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<ReferralDefinitionsResponse>> GetDefinitions(ReferralRequest request)
            => SendRequest<ReferralDefinitionsResponse>(ReferralAction.GetDefinitions, request);

        public static Task<OperationResult<UserReferralStateResponse>> GetUserState(ReferralRequest request)
            => SendRequest<UserReferralStateResponse>(ReferralAction.GetUserState, request);

        public static Task<OperationResult<ActivateReferralCodeResponse>> ActivateReferralCode(ReferralRequest request)
            => SendRequest<ActivateReferralCodeResponse>(ReferralAction.ActivateReferralCode, request);

        public static Task<OperationResult<ClaimInviteRewardResponse>> ClaimInviteReward(ReferralRequest request)
            => SendRequest<ClaimInviteRewardResponse>(ReferralAction.ClaimInviteReward, request);
    }
}
