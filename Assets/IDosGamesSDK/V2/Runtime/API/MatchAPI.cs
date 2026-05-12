using System.Threading.Tasks;

namespace IDosGames
{
    public static class MatchAPI
    {
        private static string GetEndpoint(MatchAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Match/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(MatchAction action, MatchRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<CreateMatchResponse>> CreateMatch(MatchRequest request)
            => SendRequest<CreateMatchResponse>(MatchAction.CreateMatch, request);

        public static Task<OperationResult<InstantBattleResponse>> InstantBattle(MatchRequest request)
            => SendRequest<InstantBattleResponse>(MatchAction.InstantBattle, request);

        public static Task<OperationResult<SuccessResponse>> SaveStrategy(MatchRequest request)
            => SendRequest<SuccessResponse>(MatchAction.SaveStrategy, request);

        public static Task<OperationResult<MatchesPageResponse>> GetMyMatches(MatchRequest request)
            => SendRequest<MatchesPageResponse>(MatchAction.GetMyMatches, request);

        public static Task<OperationResult<MatchesPageResponse>> GetAvailableMatches(MatchRequest request)
            => SendRequest<MatchesPageResponse>(MatchAction.GetAvailableMatches, request);
    }
}
