using System;
using System.Threading.Tasks;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class GameLoopService
    {
        public static event Action<RollResponse> OnSpinSuccess;
        public static event Action<AttackResponse> OnAttackSuccess;
        public static event Action<RaidResponse> OnRaidSuccess;
        public static event Action<BuildResponse> OnBuildSuccess;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static GameLoopRequest CreateBaseRequest()
        {
            return new GameLoopRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink
            };
        }

        public static async Task<OperationResult<RollResponse>> Spin(string lootboxId, int multiplier)
        {
            var request = CreateBaseRequest();

            request.LootboxID = lootboxId;
            request.Multiplier = multiplier;

            var result = await GameLoopAPI.Spin(request);
            if (result.Success)
            {
                OnSpinSuccess?.Invoke(result.Data);
            }
            
            return result;
        }

        public static async Task<OperationResult<AttackResponse>> Attack(string targetUserId)
        {
            var request = CreateBaseRequest();

            request.TargetUserID = targetUserId;

            var result = await GameLoopAPI.Attack(request);
            if (result.Success)
            {
                OnAttackSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RaidResponse>> Raid(int digIndex)
        {
            var request = CreateBaseRequest();

            request.DigIndex = digIndex;

            var result = await GameLoopAPI.Raid(request);
            if (result.Success)
            {
                OnRaidSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<BuildResponse>> Build(int buildingIndex)
        {
            var request = CreateBaseRequest();

            request.BuildingIndex = buildingIndex;

            var result = await GameLoopAPI.Build(request);
            if (result.Success)
            {
                OnBuildSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
