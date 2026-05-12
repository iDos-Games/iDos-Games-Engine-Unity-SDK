using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class LeaderboardService
    {
        // ---------- Events ----------

        /// <summary>Fired after definitions are fetched and patched into TitleConfig.</summary>
        public static event Action<LeaderboardDefinitions> OnLeaderboardDefinitionsLoaded;

        /// <summary>Fired after the top list is fetched (global or bracket). No local state patched.</summary>
        public static event Action<GetLeaderboardResponse> OnLeaderboardLoaded;

        /// <summary>Fired after GetMyProgress; local UserLeaderboardProgress is patched.</summary>
        public static event Action<GetMyProgressResponse> OnMyProgressLoaded;

        /// <summary>Fired after SubmitScore; CurrentScore and ScoreEarnedThisCycle are optimistically patched.</summary>
        public static event Action<SubmitScoreResponse> OnScoreSubmitted;

        /// <summary>Fired after ClaimCycleReward; resources applied and unclaimed flag cleared.</summary>
        public static event Action<ClaimCycleRewardResponse> OnCycleRewardClaimed;

        /// <summary>Fired after ClaimMilestone; resources applied and ClaimedMilestoneIDs patched.</summary>
        public static event Action<ClaimLeaderboardMilestoneResponse> OnMilestoneClaimed;

        /// <summary>Fired after GetFriendsLeaderboard. No local state patched.</summary>
        public static event Action<GetLeaderboardResponse> OnFriendsLeaderboardLoaded;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static LeaderboardRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        /// <summary>Mirrors the server-side BuildCycleDocumentID helper.</summary>
        private static string BuildCycleDocID(string leaderboardID)
            => $"{IDosGamesSDKSettings.Instance.TitleID}_{leaderboardID}";

        // ===================== Actions =====================

        /// <summary>
        /// Fetches leaderboard definitions and patches TitleConfig.Leaderboard.
        /// Fires OnLeaderboardDefinitionsLoaded.
        /// </summary>
        public static async Task<OperationResult<LeaderboardDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await LeaderboardAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchLeaderboard(result.Data);
                OnLeaderboardDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the top users for a leaderboard. Returns the player's bracket top when brackets
        /// are enabled; falls back to the global top otherwise.
        /// No local state is patched. Fires OnLeaderboardLoaded.
        /// </summary>
        public static async Task<OperationResult<GetLeaderboardResponse>> GetLeaderboard(string leaderboardID)
        {
            if (string.IsNullOrWhiteSpace(leaderboardID))
            {
                Debug.LogWarning("[LeaderboardService] GetLeaderboard: LeaderboardID is required.");
                return OperationResult<GetLeaderboardResponse>.Fail("LeaderboardID is required.");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardID;

            var result = await LeaderboardAPI.GetLeaderboard(request);

            if (result.Success && result.Data != null)
                OnLeaderboardLoaded?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Fetches the player's progress in a leaderboard for the current cycle.
        /// Patches local UserLeaderboardProgress with score, rank, and unclaimed flags.
        /// Fires OnMyProgressLoaded.
        /// </summary>
        public static async Task<OperationResult<GetMyProgressResponse>> GetMyProgress(string leaderboardID)
        {
            if (string.IsNullOrWhiteSpace(leaderboardID))
            {
                Debug.LogWarning("[LeaderboardService] GetMyProgress: LeaderboardID is required.");
                return OperationResult<GetMyProgressResponse>.Fail("LeaderboardID is required.");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardID;

            var result = await LeaderboardAPI.GetMyProgress(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchLeaderboardMyProgress(BuildCycleDocID(leaderboardID), result.Data);
                OnMyProgressLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Submits a score to a leaderboard. Optimistically patches CurrentScore and
        /// ScoreEarnedThisCycle locally (call GetMyProgress for authoritative values).
        /// Fires OnScoreSubmitted.
        /// </summary>
        public static async Task<OperationResult<SubmitScoreResponse>> SubmitScore(string leaderboardID, long score)
        {
            if (string.IsNullOrWhiteSpace(leaderboardID))
            {
                Debug.LogWarning("[LeaderboardService] SubmitScore: LeaderboardID is required.");
                return OperationResult<SubmitScoreResponse>.Fail("LeaderboardID is required.");
            }

            if (score <= 0)
            {
                Debug.LogWarning("[LeaderboardService] SubmitScore: Score must be > 0.");
                return OperationResult<SubmitScoreResponse>.Fail("Score must be greater than 0.");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardID;
            request.Score = score;

            var result = await LeaderboardAPI.SubmitScore(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;
                IDosGamesData.User.PatchLeaderboardScoreSubmitted(BuildCycleDocID(leaderboardID), data.NewScore, score, data.CycleVersion);
                OnScoreSubmitted?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Claims the end-of-cycle rank reward for a leaderboard.
        /// Applies the ResourceOperation locally and clears the unclaimed reward flag.
        /// Fires OnCycleRewardClaimed.
        /// </summary>
        public static async Task<OperationResult<ClaimCycleRewardResponse>> ClaimCycleReward(string leaderboardID)
        {
            if (string.IsNullOrWhiteSpace(leaderboardID))
            {
                Debug.LogWarning("[LeaderboardService] ClaimCycleReward: LeaderboardID is required.");
                return OperationResult<ClaimCycleRewardResponse>.Fail("LeaderboardID is required.");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardID;
            request.RelatedEntityID = $"lb_claim_{leaderboardID}_{Guid.NewGuid():N}";

            var result = await LeaderboardAPI.ClaimCycleReward(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchLeaderboardCycleRewardClaimed(BuildCycleDocID(leaderboardID));

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnCycleRewardClaimed?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Claims a milestone reward within the current leaderboard cycle.
        /// Applies the ResourceOperation locally and patches ClaimedMilestoneIDs.
        /// Fires OnMilestoneClaimed.
        /// </summary>
        public static async Task<OperationResult<ClaimLeaderboardMilestoneResponse>> ClaimMilestone(string leaderboardID, string milestoneID)
        {
            if (string.IsNullOrWhiteSpace(leaderboardID))
            {
                Debug.LogWarning("[LeaderboardService] ClaimMilestone: LeaderboardID is required.");
                return OperationResult<ClaimLeaderboardMilestoneResponse>.Fail("LeaderboardID is required.");
            }

            if (string.IsNullOrWhiteSpace(milestoneID))
            {
                Debug.LogWarning("[LeaderboardService] ClaimMilestone: MilestoneID is required.");
                return OperationResult<ClaimLeaderboardMilestoneResponse>.Fail("MilestoneID is required.");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardID;
            request.MilestoneID = milestoneID;
            request.RelatedEntityID = $"lb_milestone_{leaderboardID}_{milestoneID}_{Guid.NewGuid():N}";

            var result = await LeaderboardAPI.ClaimMilestone(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchLeaderboardMilestoneClaimed(BuildCycleDocID(leaderboardID), milestoneID);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnMilestoneClaimed?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Fetches a leaderboard ranked by the player's friends (plus the player themselves).
        /// No local state is patched. Fires OnFriendsLeaderboardLoaded.
        /// </summary>
        public static async Task<OperationResult<GetLeaderboardResponse>> GetFriendsLeaderboard(string leaderboardID)
        {
            if (string.IsNullOrWhiteSpace(leaderboardID))
            {
                Debug.LogWarning("[LeaderboardService] GetFriendsLeaderboard: LeaderboardID is required.");
                return OperationResult<GetLeaderboardResponse>.Fail("LeaderboardID is required.");
            }

            var request = CreateBaseRequest();
            request.LeaderboardID = leaderboardID;

            var result = await LeaderboardAPI.GetFriendsLeaderboard(request);

            if (result.Success && result.Data != null)
                OnFriendsLeaderboardLoaded?.Invoke(result.Data);

            return result;
        }
    }
}
