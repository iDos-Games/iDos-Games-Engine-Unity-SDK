using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class RewardService
    {
        // ==================== Events ====================

        public static event Action<RewardDefinitions> OnRewardDefinitionsLoaded;
        public static event Action<UserRewardState> OnUserRewardsStateLoaded;
        public static event Action<ClaimDailyRewardResponse> OnDailyRewardClaimed;
        public static event Action<CollectIdleAccrualResponse> OnIdleAccrualCollected;
        public static event Action<ClaimComebackRewardResponse> OnComebackRewardClaimed;
        public static event Action<ClaimRewardResponse> OnRewardClaimed;

        // ==================== Helpers ====================

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static RewardRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ==================== Actions ====================

        /// <summary>
        /// Loads reward definitions from the server and updates TitleConfig.
        /// Fires <see cref="OnRewardDefinitionsLoaded"/> on success.
        /// </summary>
        public static async Task<OperationResult<RewardDefinitionsResponse>> GetRewardDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await RewardAPI.GetRewardDefinitions(request);

            if (result.Success && result.Data?.RewardDefinitions != null)
            {
                IDosGamesData.Config.PatchReward(result.Data.RewardDefinitions);
                OnRewardDefinitionsLoaded?.Invoke(result.Data.RewardDefinitions);
            }

            return result;
        }

        /// <summary>
        /// Loads the player's reward state and patches local UserData.
        /// Fires <see cref="OnUserRewardsStateLoaded"/> on success.
        /// </summary>
        public static async Task<OperationResult<UserRewardsStateResponse>> GetUserRewardsState()
        {
            var request = CreateBaseRequest();
            var result = await RewardAPI.GetUserRewardsState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyReward(result.Data.Rewards);
                OnUserRewardsStateLoaded?.Invoke(result.Data.Rewards);
            }

            return result;
        }

        /// <summary>
        /// Claims the next day of a daily login calendar.
        /// Patches local calendar state and applies resource changes.
        /// Fires <see cref="OnDailyRewardClaimed"/> on success.
        /// </summary>
        /// <param name="calendarID">Calendar to claim. null/empty = server picks the default.</param>
        public static async Task<OperationResult<ClaimDailyRewardResponse>> ClaimDailyReward(string calendarID = null)
        {
            var request = CreateBaseRequest();
            request.CalendarID = calendarID;
            request.RelatedEntityID = $"dailyreward_{Ctx.UserID}_{calendarID ?? "default"}_{Guid.NewGuid():N}";

            var result = await RewardAPI.ClaimDailyReward(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchDailyCalendarState(data.CalendarID, data.DailyState);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnDailyRewardClaimed?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Collects accumulated idle accrual payout.
        /// Patches local accrual state and applies resource changes.
        /// Fires <see cref="OnIdleAccrualCollected"/> on success.
        /// </summary>
        /// <param name="accrualID">Required. ID of the accrual to collect.</param>
        public static async Task<OperationResult<CollectIdleAccrualResponse>> CollectIdleAccrual(string accrualID)
        {
            if (string.IsNullOrWhiteSpace(accrualID))
            {
                Debug.LogWarning("[RewardService] CollectIdleAccrual: accrualID is required.");
                return OperationResult<CollectIdleAccrualResponse>.Fail("accrualID is required.");
            }

            if (accrualID.Contains('.') || accrualID.Contains('$'))
            {
                Debug.LogWarning($"[RewardService] CollectIdleAccrual: accrualID '{accrualID}' contains invalid characters ('.' or '$').");
                return OperationResult<CollectIdleAccrualResponse>.Fail($"accrualID '{accrualID}' contains invalid characters.");
            }

            var request = CreateBaseRequest();
            request.AccrualID = accrualID;
            request.RelatedEntityID = $"idle_collect_{accrualID}_{Guid.NewGuid():N}";

            var result = await RewardAPI.CollectIdleAccrual(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchIdleAccrualState(data.AccrualID, data.IdleState);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnIdleAccrualCollected?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Claims a pending comeback reward.
        /// Patches local comeback state and applies resource changes.
        /// Fires <see cref="OnComebackRewardClaimed"/> on success.
        /// </summary>
        /// <param name="comebackID">Required. ID of the comeback config to claim.</param>
        public static async Task<OperationResult<ClaimComebackRewardResponse>> ClaimComebackReward(string comebackID)
        {
            if (string.IsNullOrWhiteSpace(comebackID))
            {
                Debug.LogWarning("[RewardService] ClaimComebackReward: comebackID is required.");
                return OperationResult<ClaimComebackRewardResponse>.Fail("comebackID is required.");
            }

            if (comebackID.Contains('.') || comebackID.Contains('$'))
            {
                Debug.LogWarning($"[RewardService] ClaimComebackReward: comebackID '{comebackID}' contains invalid characters ('.' or '$').");
                return OperationResult<ClaimComebackRewardResponse>.Fail($"comebackID '{comebackID}' contains invalid characters.");
            }

            var request = CreateBaseRequest();
            request.ComebackID = comebackID;
            request.RelatedEntityID = $"comeback_claim_{comebackID}_{Guid.NewGuid():N}";

            var result = await RewardAPI.ClaimComebackReward(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchComebackState(data.ComebackID, data.ComebackState);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnComebackRewardClaimed?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Claims a simple manual reward by ID.
        /// Patches local claim state and applies resource changes.
        /// Fires <see cref="OnRewardClaimed"/> on success.
        /// </summary>
        /// <param name="claimID">Required. ID of the claim reward to claim.</param>
        public static async Task<OperationResult<ClaimRewardResponse>> ClaimReward(string claimID)
        {
            if (string.IsNullOrWhiteSpace(claimID))
            {
                Debug.LogWarning("[RewardService] ClaimReward: claimID is required.");
                return OperationResult<ClaimRewardResponse>.Fail("claimID is required.");
            }

            if (claimID.Contains('.') || claimID.Contains('$'))
            {
                Debug.LogWarning($"[RewardService] ClaimReward: claimID '{claimID}' contains invalid characters ('.' or '$').");
                return OperationResult<ClaimRewardResponse>.Fail($"claimID '{claimID}' contains invalid characters.");
            }

            var request = CreateBaseRequest();
            request.ClaimID = claimID;

            // Idempotency key is TotalClaims+1 based on server side; we use a GUID here because
            // the client does not know TotalClaims before the server responds. The server uses
            // its own stable key derived from the claim count — the RelatedEntityID here is just
            // a fallback prefix if the server's own key generation falls back to it.
            request.RelatedEntityID = $"reward_claim_{claimID}_{Guid.NewGuid():N}";

            var result = await RewardAPI.ClaimReward(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchClaimRewardState(data.ClaimID, data.ClaimState);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnRewardClaimed?.Invoke(data);
            }

            return result;
        }
    }
}
