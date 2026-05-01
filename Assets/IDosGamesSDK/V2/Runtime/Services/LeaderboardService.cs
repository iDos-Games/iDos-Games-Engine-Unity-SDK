using System;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using UnityEngine;

namespace IDosGames
{
    public static class LeaderboardService
    {
        public static event Action<LeaderboardDefinitions> OnLeaderboardDefinitionsUpdated;
        public static event Action<GetLeaderboardResponse> OnLeaderboardUpdated;
        public static event Action<GetMyProgressResponse> OnMyProgressUpdated;
        public static event Action<SubmitScoreResponse> OnSubmitScoreSuccess;
        public static event Action<ClaimCycleRewardResponse> OnClaimCycleRewardSuccess;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();

        /// <summary>
        /// Creates a basic request with required authorization fields.
        /// </summary>
        private static LeaderboardRequest CreateBaseRequest()
        {
            return new LeaderboardRequest
            {
                UserID = Ctx.UserID,
                ClientSessionTicket = Ctx.ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        /// <summary>
        /// Gets Leaderboard Definitions (config).
        /// </summary>
        public static async Task<OperationResult<LeaderboardDefinitions>> GetLeaderboardDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await LeaderboardAPI.GetLeaderboardDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchLeaderboardDefinitions(result.Data);
                OnLeaderboardDefinitionsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Gets the top users and current cycle info for a leaderboard.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard ID from config (e.g. "weekly_pvp")</param>
        public static async Task<OperationResult<GetLeaderboardResponse>> GetLeaderboard(string leaderboardId)
        {
            if (string.IsNullOrWhiteSpace(leaderboardId))
            {
                Debug.LogWarning("[LeaderboardService] GetLeaderboard: leaderboardId is required");
                return OperationResult<GetLeaderboardResponse>.Fail("leaderboardId is required");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardId;

            var result = await LeaderboardAPI.GetLeaderboard(request);

            if (result.Success)
            {
                IDosGamesData.Title.ApplyLeaderboardResponse(result.Data);
                OnLeaderboardUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Gets the current user's score, rank and unclaimed reward state for a leaderboard.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard ID from config</param>
        public static async Task<OperationResult<GetMyProgressResponse>> GetMyProgress(string leaderboardId)
        {
            if (string.IsNullOrWhiteSpace(leaderboardId))
            {
                Debug.LogWarning("[LeaderboardService] GetMyProgress: leaderboardId is required");
                return OperationResult<GetMyProgressResponse>.Fail("leaderboardId is required");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardId;

            var result = await LeaderboardAPI.GetMyProgress(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchLeaderboardProgress(result.Data);
                OnMyProgressUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Submits a score for the current user.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard ID from config</param>
        /// <param name="score">Score value to submit (must be > 0)</param>
        public static async Task<OperationResult<SubmitScoreResponse>> SubmitScore(string leaderboardId, long score)
        {
            if (string.IsNullOrWhiteSpace(leaderboardId))
            {
                Debug.LogWarning("[LeaderboardService] SubmitScore: leaderboardId is required");
                return OperationResult<SubmitScoreResponse>.Fail("leaderboardId is required");
            }

            if (score <= 0)
            {
                Debug.LogWarning("[LeaderboardService] SubmitScore: score must be greater than 0");
                return OperationResult<SubmitScoreResponse>.Fail("Score must be greater than 0");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardId;
            request.Score = score;

            var result = await LeaderboardAPI.SubmitScore(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchLeaderboardScore(result.Data);
                OnSubmitScoreSuccess?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Claims the cycle reward for a leaderboard where the user has an unclaimed reward.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard ID from config</param>
        public static async Task<OperationResult<ClaimCycleRewardResponse>> ClaimCycleReward(string leaderboardId)
        {
            if (string.IsNullOrWhiteSpace(leaderboardId))
            {
                Debug.LogWarning("[LeaderboardService] ClaimCycleReward: leaderboardId is required");
                return OperationResult<ClaimCycleRewardResponse>.Fail("leaderboardId is required");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardId;

            var result = await LeaderboardAPI.ClaimCycleReward(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchLeaderboardRewardClaimed(result.Data);
                IDosGamesData.User.GrantResources(result.Data.BaseRewards);
                IDosGamesData.User.GrantResources(result.Data.PremiumRewards);
                OnClaimCycleRewardSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
