using System;
using System.Collections.Generic;
using System.Linq;
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

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
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
                IDosGamesData.Config.ApplyGameLoops(result.Data);
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
                IDosGamesData.Config.PatchBoardDefinition(result.Data);
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
                IDosGamesData.Config.PatchBoardDefinition(result.Data);
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
                IDosGamesData.User.ApplyBoard(result.Data);
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
                IDosGamesData.User.PatchBoard(b =>
                {
                    b.Position = result.Data.NewPosition;
                    b.CyclesCompleted += result.Data.CyclesCompletedDelta;
                    b.LastRollAtUtc = DateTime.UtcNow;

                    if (result.Data.ActionRequired != null && result.Data.ActionData != null)
                    {
                        b.Pending = new BoardPendingInteraction
                        {
                            Type = result.Data.ActionRequired,
                            TargetUserID = result.Data.ActionData.TargetUserID,
                            TargetPublicData = result.Data.ActionData.PublicData,
                            TargetBuildingStates = result.Data.ActionData.TargetBuildingStates,
                            TargetHasShield = result.Data.ActionData.TargetHasShield,
                            RollMultiplier = rollMultiplier,
                            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
                        };
                    }
                    else
                    {
                        b.Pending = null;
                    }
                });
                IDosGamesData.User.ConsumeResources(result.Data.ConsumedResources);
                IDosGamesData.User.GrantResources(result.Data.GrantedRewards);
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
                IDosGamesData.User.PatchBoard(b => b.Pending = null);
                IDosGamesData.User.GrantResources(new List<ItemOrCurrency> { result.Data.RewardResource });
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
                IDosGamesData.User.PatchBoard(b =>
                {
                    if (b.Pending != null)
                    {
                        b.Pending.OpenedIndices ??= new();
                        if (!b.Pending.OpenedIndices.Contains(result.Data.OpenedIndex)) b.Pending.OpenedIndices.Add(result.Data.OpenedIndex);
                        if (result.Data.Status != "CONTINUE") b.Pending = null;
                    }
                });

                if (result.Data.StolenResource != null) IDosGamesData.User.GrantResources(new List<ItemOrCurrency> { result.Data.StolenResource });

                OnBoardRaidSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RaidResponse>> BoardLoopRaidFast(List<int> digIndices)
        {
            var request = CreateBaseRequest();
            request.DigIndices = digIndices;

            var result = await GameLoopAPI.BoardLoopRaidFast(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchBoard(b => b.Pending = null);

                if (result.Data.StolenResource != null) IDosGamesData.User.GrantResources(new List<ItemOrCurrency> { result.Data.StolenResource });

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
                IDosGamesData.User.PatchBoard(b =>
                {
                    b.BuildingStates ??= new();
                    var building = b.BuildingStates.FirstOrDefault(s => s.SlotIndex == result.Data.BuiltIndex);
                    if (building != null)
                    {
                        building.Level = result.Data.NewLevel;
                        building.IsDamaged = false;
                        if (result.Data.MaxLevelRewardClaimed) building.MaxLevelRewardClaimed = true;
                    }

                    if (result.Data.StageComplete)
                    {
                        b.StageLevel += 1;
                        b.Position = 0;
                        b.Pending = null;
                        b.BuildingStates = null; // сервер вернёт новые состояния при следующем GetUserBoardState
                    }
                });

                IDosGamesData.User.ConsumeResources(new List<ItemOrCurrency> { result.Data.ConsumedResource });

                if (result.Data.CompletionReward != null) IDosGamesData.User.GrantResources(result.Data.CompletionReward);

                if (result.Data.MaxLevelRewardClaimed && result.Data.MaxLevelReward != null) IDosGamesData.User.GrantResources(result.Data.MaxLevelReward);

                OnBoardBuildSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
