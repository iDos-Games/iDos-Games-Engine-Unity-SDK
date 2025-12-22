using System;
using System.Threading.Tasks;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class GameLoopService
    {
        // =================================================================================
        // ACTIONS (SUCCESS EVENTS)
        // =================================================================================
        public static event Action<SpinResponse> OnSpinSuccess;
        public static event Action<AttackResponse> OnAttackSuccess;
        public static event Action<RaidResponse> OnRaidSuccess;
        public static event Action<BuildResponse> OnBuildSuccess;

        // =================================================================================
        // PRIVATE HELPERS
        // =================================================================================
        private static string GetEndpoint(GameLoopAction action)
        {
            string templateId = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleId = IDosGamesSDKSettings.Instance.TitleID;
            string userId = AuthenticationService.UserID;

            return $"v2/{templateId}/{titleId}/Client/GameLoop/{action}/{userId}";
        }

        // =================================================================================
        // PUBLIC API METHODS
        // =================================================================================

        /// <summary>
        /// Loot box (slot) rotation
        /// </summary>
        public static async Task<OperationResult<SpinResponse>> Spin(string lootboxId, int multiplier)
        {
            var request = new GameLoopRequest
            {
                LootboxID = lootboxId,
                Multiplier = multiplier
            };

            var result = await HttpService.Post<SpinResponse>(GetEndpoint(GameLoopAction.Spin), request);

            if (result.Success) OnSpinSuccess?.Invoke(result.Data);
            return result;
        }

        /// <summary>
        /// Attacking another player's building
        /// </summary>
        public static async Task<OperationResult<AttackResponse>> Attack(string targetUserId)
        {
            var request = new GameLoopRequest
            {
                TargetUserID = targetUserId
            };

            var result = await HttpService.Post<AttackResponse>(GetEndpoint(GameLoopAction.Attack), request);

            if (result.Success) OnAttackSuccess?.Invoke(result.Data);
            return result;
        }

        /// <summary>
        /// Performing a step in a raid (digging a hole)
        /// </summary>
        public static async Task<OperationResult<RaidResponse>> Raid(int digIndex)
        {
            var request = new GameLoopRequest
            {
                DigIndex = digIndex
            };

            var result = await HttpService.Post<RaidResponse>(GetEndpoint(GameLoopAction.Raid), request);

            if (result.Success) OnRaidSuccess?.Invoke(result.Data);
            return result;
        }

        /// <summary>
        /// Construction or upgrade of a building
        /// </summary>
        public static async Task<OperationResult<BuildResponse>> Build(int buildingIndex)
        {
            var request = new GameLoopRequest
            {
                BuildingIndex = buildingIndex
            };

            var result = await HttpService.Post<BuildResponse>(GetEndpoint(GameLoopAction.Build), request);

            if (result.Success) OnBuildSuccess?.Invoke(result.Data);
            return result;
        }
    }
}
