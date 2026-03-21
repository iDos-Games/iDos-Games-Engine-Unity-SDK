using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class GameLoopAPI
    {
        private static string GetEndpoint(GameLoopAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/GameLoop/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(GameLoopAction action, GameLoopRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<GameLoopsDefinition>> GetGameLoops(GameLoopRequest request)
        {
            return await SendRequest<GameLoopsDefinition>(GameLoopAction.GetGameLoops, request);
        }

        public static async Task<OperationResult<BoardLoopDefinition>> GetBoardDefinition(GameLoopRequest request)
        {
            return await SendRequest<BoardLoopDefinition>(GameLoopAction.GetBoardDefinition, request);
        }

        public static async Task<OperationResult<BoardLoopDefinition>> GetBoardDefinitionForLevel(GameLoopRequest request)
        {
            return await SendRequest<BoardLoopDefinition>(GameLoopAction.GetBoardDefinitionForLevel, request);
        }

        public static async Task<OperationResult<BoardLoopState>> GetUserBoardState(GameLoopRequest request)
        {
            return await SendRequest<BoardLoopState>(GameLoopAction.GetUserBoardState, request);
        }

        public static async Task<OperationResult<BoardRollResponse>> BoardLoopRoll(GameLoopRequest request)
        {
            return await SendRequest<BoardRollResponse>(GameLoopAction.BoardLoopRoll, request);
        }

        public static async Task<OperationResult<AttackResponse>> BoardLoopAttack(GameLoopRequest request)
        {
            return await SendRequest<AttackResponse>(GameLoopAction.BoardLoopAttack, request);
        }

        public static async Task<OperationResult<RaidResponse>> BoardLoopRaid(GameLoopRequest request)
        {
            return await SendRequest<RaidResponse>(GameLoopAction.BoardLoopRaid, request);
        }

        public static async Task<OperationResult<RaidResponse>> BoardLoopRaidFast(GameLoopRequest request)
        {
            return await SendRequest<RaidResponse>(GameLoopAction.BoardLoopRaidFast, request);
        }

        public static async Task<OperationResult<BuildResponse>> BoardLoopBuild(GameLoopRequest request)
        {
            return await SendRequest<BuildResponse>(GameLoopAction.BoardLoopBuild, request);
        }
    }
}
