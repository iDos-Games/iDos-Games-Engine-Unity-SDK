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

        public static async Task<OperationResult<SpinResponse>> Spin(GameLoopRequest request)
        {
            return await HttpService.Post<SpinResponse>(
                GetEndpoint(GameLoopAction.Spin, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<AttackResponse>> Attack(GameLoopRequest request)
        {
            return await HttpService.Post<AttackResponse>(GetEndpoint(
                GameLoopAction.Attack,
                request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<RaidResponse>> Raid(GameLoopRequest request)
        {
            return await HttpService.Post<RaidResponse>(
                GetEndpoint(GameLoopAction.Raid, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<BuildResponse>> Build(GameLoopRequest request)
        {
            return await HttpService.Post<BuildResponse>(
                GetEndpoint(GameLoopAction.Build,
                request.UserID),
                request,
                request.ClientSessionTicket
            );
        }
    }
}
