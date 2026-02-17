using System.Threading.Tasks;
using IDosGames.ServerModels;

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

        public static async Task<OperationResult<RollResponse>> Spin(GameLoopRequest request)
        {
            return await SendRequest<RollResponse>(GameLoopAction.RaidBuildLoopRoll, request);
        }

        public static async Task<OperationResult<AttackResponse>> Attack(GameLoopRequest request)
        {
            return await SendRequest<AttackResponse>(GameLoopAction.RaidBuildLoopAttack, request);
        }

        public static async Task<OperationResult<RaidResponse>> Raid(GameLoopRequest request)
        {
            return await SendRequest<RaidResponse>(GameLoopAction.RaidBuildLoopRaid, request);
        }

        public static async Task<OperationResult<BuildResponse>> Build(GameLoopRequest request)
        {
            return await SendRequest<BuildResponse>(GameLoopAction.RaidBuildLoopBuild, request);
        }
    }
}
