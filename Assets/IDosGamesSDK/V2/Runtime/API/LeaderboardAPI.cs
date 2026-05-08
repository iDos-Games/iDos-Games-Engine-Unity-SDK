using System.Threading.Tasks;

namespace IDosGames
{
    public static class LeaderboardAPI
    {
        private static string GetEndpoint(LeaderboardAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Leaderboard/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(LeaderboardAction action, LeaderboardRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<LeaderboardDefinitions>> GetDefinitions(LeaderboardRequest request)
            => SendRequest<LeaderboardDefinitions>(LeaderboardAction.GetDefinitions, request);

        public static Task<OperationResult<GetLeaderboardResponse>> GetLeaderboard(LeaderboardRequest request)
            => SendRequest<GetLeaderboardResponse>(LeaderboardAction.GetLeaderboard, request);

        public static Task<OperationResult<GetMyProgressResponse>> GetMyProgress(LeaderboardRequest request)
            => SendRequest<GetMyProgressResponse>(LeaderboardAction.GetMyProgress, request);

        public static Task<OperationResult<SubmitScoreResponse>> SubmitScore(LeaderboardRequest request)
            => SendRequest<SubmitScoreResponse>(LeaderboardAction.SubmitScore, request);

        public static Task<OperationResult<ClaimCycleRewardResponse>> ClaimCycleReward(LeaderboardRequest request)
            => SendRequest<ClaimCycleRewardResponse>(LeaderboardAction.ClaimCycleReward, request);

        public static Task<OperationResult<ClaimLeaderboardMilestoneResponse>> ClaimMilestone(LeaderboardRequest request)
            => SendRequest<ClaimLeaderboardMilestoneResponse>(LeaderboardAction.ClaimMilestone, request);

        public static Task<OperationResult<GetLeaderboardResponse>> GetFriendsLeaderboard(LeaderboardRequest request)
            => SendRequest<GetLeaderboardResponse>(LeaderboardAction.GetFriendsLeaderboard, request);
    }
}
