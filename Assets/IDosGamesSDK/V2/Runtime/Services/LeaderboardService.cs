using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Fired after AutoClaimAllCycleRewards completes and at least one reward was claimed.
        /// Contains all responses for leaderboards that had an unclaimed cycle reward.
        /// Use this to show a "congratulations" popup on game startup.
        /// </summary>
        public static event Action<List<ClaimCycleRewardResponse>> OnStartupRewardsClaimed;

        /// <summary>
        /// Cached result of the last AutoClaimAllCycleRewards call.
        /// Non-null if rewards were claimed but not yet consumed by the UI.
        /// Set to null by ConsumeStartupRewards() once the popup has shown.
        /// </summary>
        public static List<ClaimCycleRewardResponse> PendingStartupRewards { get; private set; }

        /// <summary>Call this after the popup has shown the rewards to clear the cache.</summary>
        public static void ConsumeStartupRewards() => PendingStartupRewards = null;

        // ---------- Startup ----------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterHooks()
        {
            AuthenticationService.OnLoggedIn += () => _ = AutoClaimAllCycleRewards();

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, _) =>
            {
                if (PendingStartupRewards == null) return;
                OnStartupRewardsClaimed?.Invoke(PendingStartupRewards);
            };
        }

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
        /// Intended to be called once after login.
        /// Fetches definitions and progress for every leaderboard; for each one that has an
        /// unclaimed cycle reward, claims it automatically (no player interaction required).
        /// Fires OnCycleRewardClaimed per claim and OnStartupRewardsClaimed with the full
        /// batch when done (only if at least one reward was claimed).
        /// </summary>
        private static async Task AutoClaimAllCycleRewards()
        {
            var defsResult = await GetDefinitions();
            if (!defsResult.Success || defsResult.Data?.Definitions == null) return;

            var claimed = new List<ClaimCycleRewardResponse>();

            foreach (var leaderboardID in defsResult.Data.Definitions.Keys)
            {
                var progressResult = await GetMyProgress(leaderboardID);
                if (!progressResult.Success || progressResult.Data == null) continue;
                if (!progressResult.Data.HasUnclaimedReward) continue;

                var claimResult = await ClaimCycleReward(leaderboardID);
                if (claimResult.Success && claimResult.Data != null)
                    claimed.Add(claimResult.Data);
            }

            if (claimed.Count > 0)
            {
                PendingStartupRewards = claimed;
                OnStartupRewardsClaimed?.Invoke(claimed);
            }
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
