using System.Threading.Tasks;
using IDosGames.ClientModels;
using UnityEngine;

namespace IDosGames
{
    public static class ReferralService
    {
        public static event System.Action<GetReferralDefinitionsResponse> OnDefinitionsUpdated;
        public static event System.Action<GetUserReferralStateResponse> OnUserStateUpdated;
        public static event System.Action<ActivateReferralCodeResponse> OnReferralCodeActivated;
        public static event System.Action<ClaimInviteRewardResponse> OnInviteRewardClaimed;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        /// <summary>
        /// Creates a basic request with required authorization fields.
        /// </summary>
        private static ReferralRequest CreateBaseRequest()
        {
            return new ReferralRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        /// <summary>
        /// Gets referral system configuration (definitions).
        /// </summary>
        public static async Task<OperationResult<GetReferralDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await ReferralAPI.GetDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchReferralDefinitions(result.Data.ReferralDefinitions);
                OnDefinitionsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Gets the current user's referral state.
        /// </summary>
        public static async Task<OperationResult<GetUserReferralStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await ReferralAPI.GetUserState(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyReferralState(result.Data.Referral);
                OnUserStateUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Activates a referral code of another user.
        /// </summary>
        /// <param name="referralCode">The UserID / referral code of the referrer.</param>
        public static async Task<OperationResult<ActivateReferralCodeResponse>> ActivateReferralCode(string referralCode)
        {
            if (string.IsNullOrWhiteSpace(referralCode))
            {
                Debug.LogWarning("[ReferralService] ActivateReferralCode: referralCode is null or empty.");
                return OperationResult<ActivateReferralCodeResponse>.Fail("ReferralCode is required");
            }

            var request = CreateBaseRequest();
            request.ReferralCode = referralCode;

            var result = await ReferralAPI.ActivateReferralCode(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchReferralSubscription(result.Data.ReferralCode);

                if (result.Data.GrantedResources != null && result.Data.GrantedResources.Count > 0)
                    IDosGamesData.User.GrantResources(result.Data.GrantedResources);

                OnReferralCodeActivated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Manually claims an invite reward (only for AutoGrant = false rewards).
        /// </summary>
        /// <param name="inviteRewardID">The ID of the invite reward to claim.</param>
        public static async Task<OperationResult<ClaimInviteRewardResponse>> ClaimInviteReward(string inviteRewardID)
        {
            if (string.IsNullOrWhiteSpace(inviteRewardID))
            {
                Debug.LogWarning("[ReferralService] ClaimInviteReward: inviteRewardID is null or empty.");
                return OperationResult<ClaimInviteRewardResponse>.Fail("InviteRewardID is required");
            }

            var request = CreateBaseRequest();
            request.InviteRewardID = inviteRewardID;

            var result = await ReferralAPI.ClaimInviteReward(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchReferralInviteRewardClaimed(result.Data.RewardID);

                if (result.Data.GrantedResources != null && result.Data.GrantedResources.Count > 0)
                    IDosGamesData.User.GrantResources(result.Data.GrantedResources);

                OnInviteRewardClaimed?.Invoke(result.Data);
            }

            return result;
        }
    }
}
