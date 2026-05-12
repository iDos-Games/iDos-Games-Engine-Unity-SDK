using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class ReferralService
    {
        // ---------- Typed events ----------

        /// <summary>Fired after referral definitions are loaded and cached locally.</summary>
        public static event Action<ReferralDefinitionsResponse> OnDefinitionsLoaded;

        /// <summary>Fired after user referral state is loaded and cached locally.</summary>
        public static event Action<UserReferralStateResponse> OnUserStateLoaded;

        /// <summary>Fired after a referral code is successfully activated.</summary>
        public static event Action<ActivateReferralCodeResponse> OnReferralCodeActivated;

        /// <summary>Fired after an invite reward is successfully claimed.</summary>
        public static event Action<ClaimInviteRewardResponse> OnInviteRewardClaimed;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static ReferralRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Loads referral system configuration from the server and caches it locally.
        /// Fires OnDefinitionsLoaded on success.
        /// </summary>
        public static async Task<OperationResult<ReferralDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await ReferralAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchReferral(result.Data.ReferralDefinitions);
                OnDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Loads the current user's referral state from the server and caches it locally.
        /// Fires OnUserStateLoaded on success.
        /// </summary>
        public static async Task<OperationResult<UserReferralStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await ReferralAPI.GetUserState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyReferral(result.Data.Referral);
                OnUserStateLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Activates a referrer's code for the current user.
        /// On first activation, applies the activation reward resources locally.
        /// Fires OnReferralCodeActivated on success.
        /// </summary>
        public static async Task<OperationResult<ActivateReferralCodeResponse>> ActivateReferralCode(string referralCode)
        {
            if (string.IsNullOrWhiteSpace(referralCode))
            {
                Debug.LogWarning("[ReferralService] ActivateReferralCode: referralCode is required.");
                return OperationResult<ActivateReferralCodeResponse>.Fail("ReferralCode is required.");
            }

            var request = CreateBaseRequest();
            request.ReferralCode = referralCode.Trim().ToUpper();
            request.RelatedEntityID = $"referral_activation_{Ctx.UserID}";

            var result = await ReferralAPI.ActivateReferralCode(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchReferralSubscription(data.ReferralCode);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnReferralCodeActivated?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Manually claims an invite reward by ID (only for AutoGrant=false rewards).
        /// Applies granted resources locally on success.
        /// Fires OnInviteRewardClaimed on success.
        /// </summary>
        public static async Task<OperationResult<ClaimInviteRewardResponse>> ClaimInviteReward(string inviteRewardID)
        {
            if (string.IsNullOrWhiteSpace(inviteRewardID))
            {
                Debug.LogWarning("[ReferralService] ClaimInviteReward: inviteRewardID is required.");
                return OperationResult<ClaimInviteRewardResponse>.Fail("InviteRewardID is required.");
            }

            var request = CreateBaseRequest();
            request.InviteRewardID = inviteRewardID.Trim();
            request.RelatedEntityID = $"referral_invite_{inviteRewardID}_{Ctx.UserID}";

            var result = await ReferralAPI.ClaimInviteReward(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchReferralInviteRewardClaimed(data.RewardID);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnInviteRewardClaimed?.Invoke(data);
            }

            return result;
        }
    }
}
