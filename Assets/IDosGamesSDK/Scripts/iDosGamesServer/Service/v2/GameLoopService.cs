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
        private static string UserId => Ctx.UserID;
        private static string Token => Ctx.ClientSessionTicket;

        public static async Task<OperationResult<SpinResponse>> Spin(string lootboxId, int multiplier)
        {
            var result = await GameLoopAPI.Spin(UserId, Token, lootboxId, multiplier);
            if (result.Success) OnSpinSuccess?.Invoke(result.Data);
            return result;
        }

        public static async Task<OperationResult<AttackResponse>> Attack(string targetUserId)
        {
            var result = await GameLoopAPI.Attack(UserId, Token, targetUserId);
            if (result.Success) OnAttackSuccess?.Invoke(result.Data);
            return result;
        }

        public static async Task<OperationResult<RaidResponse>> Raid(int digIndex)
        {
            var result = await GameLoopAPI.Raid(UserId, Token, digIndex);
            if (result.Success) OnRaidSuccess?.Invoke(result.Data);
            return result;
        }

        public static async Task<OperationResult<BuildResponse>> Build(int buildingIndex)
        {
            var result = await GameLoopAPI.Build(UserId, Token, buildingIndex);
            if (result.Success) OnBuildSuccess?.Invoke(result.Data);
            return result;
        }
    }
}
