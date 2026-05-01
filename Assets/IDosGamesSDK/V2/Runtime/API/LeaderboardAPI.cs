using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class LeaderboardAPI
    {
        private static string GetEndpoint(LeaderboardAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Leaderboard/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(LeaderboardAction action, LeaderboardRequest request)
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

        public static async Task<OperationResult<LeaderboardDefinitions>> GetLeaderboardDefinitions(LeaderboardRequest request)
        {
            return await SendRequest<LeaderboardDefinitions>(LeaderboardAction.GetLeaderboardDefinitions, request);
        }

        public static async Task<OperationResult<GetLeaderboardResponse>> GetLeaderboard(LeaderboardRequest request)
        {
            return await SendRequest<GetLeaderboardResponse>(LeaderboardAction.GetLeaderboard, request);
        }

        public static async Task<OperationResult<GetMyProgressResponse>> GetMyProgress(LeaderboardRequest request)
        {
            return await SendRequest<GetMyProgressResponse>(LeaderboardAction.GetMyProgress, request);
        }

        public static async Task<OperationResult<SubmitScoreResponse>> SubmitScore(LeaderboardRequest request)
        {
            return await SendRequest<SubmitScoreResponse>(LeaderboardAction.SubmitScore, request);
        }

        public static async Task<OperationResult<ClaimCycleRewardResponse>> ClaimCycleReward(LeaderboardRequest request)
        {
            return await SendRequest<ClaimCycleRewardResponse>(LeaderboardAction.ClaimCycleReward, request);
        }
    }
}
