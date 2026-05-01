using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class MatchAPI
    {
        private static string GetEndpoint(MatchAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Match/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(MatchAction action, MatchRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<CreateMatchResponse>> CreateMatch(MatchRequest request)
        {
            return await SendRequest<CreateMatchResponse>(MatchAction.CreateMatch, request);
        }

        public static async Task<OperationResult<BattleResult>> InstantBattle(MatchRequest request)
        {
            return await SendRequest<BattleResult>(MatchAction.InstantBattle, request);
        }

        public static async Task<OperationResult<SuccessResponse>> SaveStrategy(MatchRequest request)
        {
            return await SendRequest<SuccessResponse>(MatchAction.SaveStrategy, request);
        }

        public static async Task<OperationResult<MatchesPageResponse>> GetMyMatches(MatchRequest request)
        {
            return await SendRequest<MatchesPageResponse>(MatchAction.GetMyMatches, request);
        }

        public static async Task<OperationResult<MatchesPageResponse>> GetAvailableMatches(MatchRequest request)
        {
            return await SendRequest<MatchesPageResponse>(MatchAction.GetAvailableMatches, request);
        }
    }
}
