using System;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class GameLoopService
    {
        public static event Action<GameLoopsDefinition> OnGameLoopsUpdated;
        public static event Action<BoardLoopDefinition> OnBoardDefinitionUpdated;
        public static event Action<BoardLoopState> OnBoardStateUpdated;

        public static event Action<BoardRollResponse> OnBoardRollSuccess;
        public static event Action<AttackResponse> OnBoardAttackSuccess;
        public static event Action<RaidResponse> OnBoardRaidSuccess;
        public static event Action<BuildResponse> OnBoardBuildSuccess;

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
                WebAppLink = WebSDK.webAppLink,
            };
        }

        // =================================================================================
        // PUBLIC METHODS
        // =================================================================================

        public static async Task<OperationResult<GameLoopsDefinition>> GetGameLoops()
        {
            var request = CreateBaseRequest();
            var result = await GameLoopAPI.GetGameLoops(request);

            if (result.Success)
            {
                OnGameLoopsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<BoardLoopDefinition>> GetBoardDefinition()
        {
            var request = CreateBaseRequest();
            var result = await GameLoopAPI.GetBoardDefinition(request);

            if (result.Success)
            {
                OnBoardDefinitionUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<BoardLoopDefinition>> GetBoardDefinitionForLevel(int stageLevel)
        {
            var request = CreateBaseRequest();
            request.StageLevel = stageLevel;

            var result = await GameLoopAPI.GetBoardDefinitionForLevel(request);

            if (result.Success)
            {
                OnBoardDefinitionUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<BoardLoopState>> GetUserBoardState()
        {
            var request = CreateBaseRequest();
            var result = await GameLoopAPI.GetUserBoardState(request);

            if (result.Success)
            {
                OnBoardStateUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<BoardRollResponse>> BoardLoopRoll(int rollMultiplier)
        {
            var request = CreateBaseRequest();
            request.RollMultiplier = rollMultiplier;

            var result = await GameLoopAPI.BoardLoopRoll(request);

            if (result.Success)
            {
                OnBoardRollSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<AttackResponse>> BoardLoopAttack(int buildingIndex)
        {
            var request = CreateBaseRequest();
            request.BuildingIndex = buildingIndex;

            var result = await GameLoopAPI.BoardLoopAttack(request);

            if (result.Success)
            {
                OnBoardAttackSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RaidResponse>> BoardLoopRaid(int digIndex)
        {
            var request = CreateBaseRequest();
            request.DigIndex = digIndex;

            var result = await GameLoopAPI.BoardLoopRaid(request);

            if (result.Success)
            {
                OnBoardRaidSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<BuildResponse>> BoardLoopBuild(int buildingIndex)
        {
            var request = CreateBaseRequest();
            request.BuildingIndex = buildingIndex;

            var result = await GameLoopAPI.BoardLoopBuild(request);

            if (result.Success)
            {
                OnBoardBuildSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
