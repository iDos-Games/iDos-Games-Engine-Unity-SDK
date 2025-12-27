using System;
using System.Threading.Tasks;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class GameLoopService
    {
        public static event Action<SpinResponse> OnSpinSuccess;
        public static event Action<AttackResponse> OnAttackSuccess;
        public static event Action<RaidResponse> OnRaidSuccess;
        public static event Action<BuildResponse> OnBuildSuccess;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        public static async Task<OperationResult<SpinResponse>> Spin(string lootboxId, int multiplier)
        {
            var request = new GameLoopRequest()
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                LootboxID = lootboxId,
                Multiplier = multiplier,
            };

            var result = await GameLoopAPI.Spin(request);
            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnSpinSuccess?.Invoke(result.Data);
            }
            
            return result;
        }

        public static async Task<OperationResult<AttackResponse>> Attack(string targetUserId)
        {
            var request = new GameLoopRequest()
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                TargetUserID = targetUserId,
            };

            var result = await GameLoopAPI.Attack(request);
            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnAttackSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RaidResponse>> Raid(int digIndex)
        {
            var request = new GameLoopRequest()
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                DigIndex = digIndex,
            };

            var result = await GameLoopAPI.Raid(request);
            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnRaidSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<BuildResponse>> Build(int buildingIndex)
        {
            var request = new GameLoopRequest()
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                BuildingIndex = buildingIndex,
            };

            var result = await GameLoopAPI.Build(request);
            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnBuildSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
