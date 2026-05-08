using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class GameLoopService
    {
        // =====================================================================
        // Events
        // =====================================================================

        public static event Action<GameLoopDefinitions> OnGameLoopsLoaded;
        public static event Action<BoardLoopDefinition> OnBoardDefinitionLoaded;
        public static event Action<BoardLoopDefinition> OnBoardDefinitionForLevelLoaded;
        public static event Action<BoardLoopState> OnBoardStateLoaded;
        public static event Action<BoardRollResponse> OnBoardRolled;
        public static event Action<AttackResponse> OnBoardAttacked;
        public static event Action<RaidResponse> OnBoardRaided;
        public static event Action<RaidResponse> OnBoardRaidedFast;
        public static event Action<BuildResponse> OnBoardBuilt;

        // =====================================================================
        // Helpers
        // =====================================================================

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static GameLoopRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // =====================================================================
        // Actions
        // =====================================================================

        /// <summary>
        /// Loads all GameLoop definitions from the title config and fires OnGameLoopsLoaded.
        /// Patches TitleConfig locally on success.
        /// </summary>
        public static async Task<OperationResult<GameLoopDefinitions>> GetGameLoops()
        {
            var request = CreateBaseRequest();
            var result = await GameLoopAPI.GetGameLoops(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchGameLoop(result.Data);
                OnGameLoopsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Loads the full board definition and fires OnBoardDefinitionLoaded.
        /// Patches TitleConfig locally on success.
        /// </summary>
        public static async Task<OperationResult<BoardLoopDefinition>> GetBoardDefinition()
        {
            var request = CreateBaseRequest();
            var result = await GameLoopAPI.GetBoardDefinition(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchBoardDefinition(result.Data);
                OnBoardDefinitionLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Loads the board definition for a specific stage level and fires OnBoardDefinitionForLevelLoaded.
        /// Does not patch the full board config — only used for level-specific preview.
        /// </summary>
        public static async Task<OperationResult<BoardLoopDefinition>> GetBoardDefinitionForLevel(int stageLevel)
        {
            if (stageLevel <= 0)
            {
                Debug.LogWarning("[GameLoopService] GetBoardDefinitionForLevel: stageLevel must be > 0.");
                return OperationResult<BoardLoopDefinition>.Fail("stageLevel must be > 0.");
            }

            var request = CreateBaseRequest();
            request.StageLevel = stageLevel;

            var result = await GameLoopAPI.GetBoardDefinitionForLevel(request);

            if (result.Success && result.Data != null)
            {
                OnBoardDefinitionForLevelLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Loads the current board state for the player and fires OnBoardStateLoaded.
        /// Patches UserData locally on success.
        /// </summary>
        public static async Task<OperationResult<BoardLoopState>> GetUserBoardState()
        {
            var request = CreateBaseRequest();
            var result = await GameLoopAPI.GetUserBoardState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyBoardState(result.Data);
                OnBoardStateLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Rolls the dice with the given multiplier.
        /// Patches board position, cycles completed, and resource operation locally on success.
        /// Fires OnBoardRolled.
        /// </summary>
        public static async Task<OperationResult<BoardRollResponse>> BoardLoopRoll(int rollMultiplier = 1)
        {
            if (rollMultiplier < 1)
            {
                Debug.LogWarning("[GameLoopService] BoardLoopRoll: rollMultiplier must be >= 1.");
                return OperationResult<BoardRollResponse>.Fail("rollMultiplier must be >= 1.");
            }

            var request = CreateBaseRequest();
            request.RollMultiplier = rollMultiplier;
            request.RelatedEntityID = $"roll_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await GameLoopAPI.BoardLoopRoll(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchBoardState(board =>
                {
                    board.Position = data.NewPosition;
                    board.CyclesCompleted += data.CyclesCompletedDelta;
                    board.LastRollAtUtc = DateTime.UtcNow;
                    if (data.ActionRequired != null && data.ActionData != null)
                    {
                        board.Pending = new BoardPendingInteraction
                        {
                            Type = data.ActionRequired,
                            TargetUserID = data.ActionData.TargetUserID,
                            TargetPublicData = data.ActionData.PublicData,
                            TargetBuildingStates = data.ActionData.TargetBuildingStates,
                            TargetHasShield = data.ActionData.TargetHasShield,
                            RollMultiplier = data.UsedMultiplier,
                            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                        };
                    }
                });

                if (data.Operation != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Operation, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnBoardRolled?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Resolves the active attack interaction.
        /// Clears pending locally, applies resource operation (bot) or dual result (real player), fires OnBoardAttacked.
        /// </summary>
        public static async Task<OperationResult<AttackResponse>> BoardLoopAttack(int buildingIndex = -1)
        {
            var request = CreateBaseRequest();
            request.BuildingIndex = buildingIndex;
            request.RelatedEntityID = $"attack_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await GameLoopAPI.BoardLoopAttack(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchBoardState(board =>
                {
                    board.Pending = null;
                });

                if (data.IsBotTarget)
                {
                    if (data.Operation != null)
                        IDosGamesData.User.ApplyResourceOperation(data.Operation, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                }
                else
                {
                    if (data.DualResult?.FromResult != null)
                        IDosGamesData.User.ApplyResourceOperation(data.DualResult.FromResult, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                }

                OnBoardAttacked?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Opens one cell in Sequential raid mode. May return CONTINUE or FINISHED_*.
        /// On CONTINUE: updates RaidLayout locally. On FINISH: clears pending and applies resources.
        /// Fires OnBoardRaided.
        /// </summary>
        public static async Task<OperationResult<RaidResponse>> BoardLoopRaid(int digIndex, string existingRelatedEntityID = null)
        {
            if (digIndex < 0 || digIndex > 11)
            {
                Debug.LogWarning("[GameLoopService] BoardLoopRaid: digIndex must be between 0 and 11.");
                return OperationResult<RaidResponse>.Fail("digIndex must be between 0 and 11.");
            }

            var request = CreateBaseRequest();
            request.DigIndex = digIndex;
            // Reuse the same RelatedEntityID for all steps of one raid so the final step deduplicates correctly.
            request.RelatedEntityID = !string.IsNullOrEmpty(existingRelatedEntityID)
                ? existingRelatedEntityID
                : $"raid_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await GameLoopAPI.BoardLoopRaid(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Status == "CONTINUE")
                {
                    IDosGamesData.User.PatchBoardPendingRaidLayout(data.RaidLayout, data.OpenedIndex);
                }
                else
                {
                    // FINISHED_*
                    IDosGamesData.User.PatchBoardState(board => { board.Pending = null; });

                    var op = data.Operation;
                    var dual = data.DualResult;

                    if (op != null)
                        IDosGamesData.User.ApplyResourceOperation(op, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                    else if (dual?.FromResult != null)
                        IDosGamesData.User.ApplyResourceOperation(dual.FromResult, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                }

                OnBoardRaided?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Sends all opened cell indices at once in Fast raid mode.
        /// Clears pending and applies resources on success. Fires OnBoardRaidedFast.
        /// </summary>
        public static async Task<OperationResult<RaidResponse>> BoardLoopRaidFast(List<int> digIndices)
        {
            if (digIndices == null || digIndices.Count == 0)
            {
                Debug.LogWarning("[GameLoopService] BoardLoopRaidFast: digIndices must not be empty.");
                return OperationResult<RaidResponse>.Fail("digIndices must not be empty.");
            }

            var request = CreateBaseRequest();
            request.DigIndices = digIndices;
            request.RelatedEntityID = $"raidfast_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await GameLoopAPI.BoardLoopRaidFast(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Status != "CONTINUE")
                {
                    IDosGamesData.User.PatchBoardState(board => { board.Pending = null; });

                    var op = data.Operation;
                    var dual = data.DualResult;

                    if (op != null)
                        IDosGamesData.User.ApplyResourceOperation(op, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                    else if (dual?.FromResult != null)
                        IDosGamesData.User.ApplyResourceOperation(dual.FromResult, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                }

                OnBoardRaidedFast?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Upgrades or repairs a building at the given slot index.
        /// Patches building state and resource operation locally on success. Fires OnBoardBuilt.
        /// </summary>
        public static async Task<OperationResult<BuildResponse>> BoardLoopBuild(int buildingIndex)
        {
            if (buildingIndex < 0)
            {
                Debug.LogWarning("[GameLoopService] BoardLoopBuild: buildingIndex must be >= 0.");
                return OperationResult<BuildResponse>.Fail("buildingIndex must be >= 0.");
            }

            var request = CreateBaseRequest();
            request.BuildingIndex = buildingIndex;
            request.RelatedEntityID = $"build_{Ctx.UserID}_{buildingIndex}_{Guid.NewGuid():N}";

            var result = await GameLoopAPI.BoardLoopBuild(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.StageComplete)
                {
                    // Server resets the board to the next stage — reload authoritative state.
                    IDosGamesData.User.PatchBoardState(board =>
                    {
                        board.StageLevel += 1;
                        board.Position = 0;
                        board.Pending = null;
                        board.BuildingStates = null; // will be re-initialised server-side; clear locally
                    });
                }
                else
                {
                    IDosGamesData.User.PatchBoardBuilding(data.BuiltIndex, building =>
                    {
                        building.Level = data.NewLevel;
                        building.IsDamaged = false;
                        if (data.MaxLevelRewardClaimed)
                            building.MaxLevelRewardClaimed = true;
                    });
                }

                if (data.Operation != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Operation, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnBoardBuilt?.Invoke(data);
            }

            return result;
        }
    }
}
