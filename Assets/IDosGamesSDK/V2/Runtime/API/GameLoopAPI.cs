using System.Threading.Tasks;

namespace IDosGames
{
    public static class GameLoopAPI
    {
        private static string GetEndpoint(GameLoopAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/GameLoop/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(GameLoopAction action, GameLoopRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<GameLoopDefinitions>> GetGameLoops(GameLoopRequest request)
            => SendRequest<GameLoopDefinitions>(GameLoopAction.GetGameLoops, request);

        public static Task<OperationResult<BoardLoopDefinition>> GetBoardDefinition(GameLoopRequest request)
            => SendRequest<BoardLoopDefinition>(GameLoopAction.GetBoardDefinition, request);

        public static Task<OperationResult<BoardLoopDefinition>> GetBoardDefinitionForLevel(GameLoopRequest request)
            => SendRequest<BoardLoopDefinition>(GameLoopAction.GetBoardDefinitionForLevel, request);

        public static Task<OperationResult<BoardLoopState>> GetUserBoardState(GameLoopRequest request)
            => SendRequest<BoardLoopState>(GameLoopAction.GetUserBoardState, request);

        public static Task<OperationResult<BoardRollResponse>> BoardLoopRoll(GameLoopRequest request)
            => SendRequest<BoardRollResponse>(GameLoopAction.BoardLoopRoll, request);

        public static Task<OperationResult<AttackResponse>> BoardLoopAttack(GameLoopRequest request)
            => SendRequest<AttackResponse>(GameLoopAction.BoardLoopAttack, request);

        public static Task<OperationResult<RaidResponse>> BoardLoopRaid(GameLoopRequest request)
            => SendRequest<RaidResponse>(GameLoopAction.BoardLoopRaid, request);

        public static Task<OperationResult<RaidResponse>> BoardLoopRaidFast(GameLoopRequest request)
            => SendRequest<RaidResponse>(GameLoopAction.BoardLoopRaidFast, request);

        public static Task<OperationResult<BuildResponse>> BoardLoopBuild(GameLoopRequest request)
            => SendRequest<BuildResponse>(GameLoopAction.BoardLoopBuild, request);
    }
}
