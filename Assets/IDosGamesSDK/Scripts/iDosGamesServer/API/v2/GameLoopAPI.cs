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
            return $"v2/{templateID}/{titleID}/Client/GameLoop/{action}/{userID}";
        }

        public static async Task<OperationResult<SpinResponse>> Spin(string userID, string clientSessionTicket, string lootboxId, int multiplier)
        {
            var request = new GameLoopRequest
            {
                LootboxID = lootboxId,
                Multiplier = multiplier
            };

            return await HttpService.Post<SpinResponse>(GetEndpoint(GameLoopAction.Spin, userID), request, clientSessionTicket);
        }

        public static async Task<OperationResult<AttackResponse>> Attack(string userID, string clientSessionTicket, string targetUserId)
        {
            var request = new GameLoopRequest
            {
                TargetUserID = targetUserId
            };

            return await HttpService.Post<AttackResponse>(GetEndpoint(GameLoopAction.Attack, userID), request, clientSessionTicket);
        }

        public static async Task<OperationResult<RaidResponse>> Raid(string userID, string clientSessionTicket, int digIndex)
        {
            var request = new GameLoopRequest
            {
                DigIndex = digIndex
            };

            return await HttpService.Post<RaidResponse>(GetEndpoint(GameLoopAction.Raid, userID), request, clientSessionTicket);
        }

        public static async Task<OperationResult<BuildResponse>> Build(string userID, string clientSessionTicket, int buildingIndex)
        {
            var request = new GameLoopRequest
            {
                BuildingIndex = buildingIndex
            };

            return await HttpService.Post<BuildResponse>(GetEndpoint(GameLoopAction.Build, userID), request, clientSessionTicket);
        }
    }
}
