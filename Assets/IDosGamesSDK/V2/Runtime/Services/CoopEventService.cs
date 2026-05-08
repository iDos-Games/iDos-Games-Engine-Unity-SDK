using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class CoopEventService
    {
        // =====================================================================
        // Events
        // =====================================================================

        public static event Action<CoopEventDefinitions> OnDefinitionsLoaded;
        public static event Action<ActiveCoopEventInfo> OnActiveEventLoaded;
        public static event Action<CoopUserStateResponse> OnUserStateLoaded;
        public static event Action<CoopGroupStateResponse> OnGroupStateLoaded;
        public static event Action<CoopGroupStateResponse> OnGroupJoined;
        public static event Action<CoopSpinResponse> OnSpinCompleted;
        public static event Action<CoopClaimRewardResponse> OnObjectRewardClaimed;
        public static event Action<CoopClaimRewardResponse> OnGrandPrizeClaimed;
        public static event Action<CoopLeaveGroupResponse> OnGroupLeft;

        // =====================================================================
        // Helpers
        // =====================================================================

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static CoopEventRequest CreateBaseRequest() => new()
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
        /// Loads the full coop event config from the server and caches it locally.
        /// Fires <see cref="OnDefinitionsLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<CoopEventDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await CoopEventAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchCoopEvent(result.Data);
                OnDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Returns active event info for the given chain. Does not mutate local state.
        /// Fires <see cref="OnActiveEventLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<ActiveCoopEventInfo>> GetActiveEvent(string coopChainID)
        {
            if (string.IsNullOrWhiteSpace(coopChainID))
            {
                Debug.LogWarning("[CoopEventService] GetActiveEvent: CoopChainID is required.");
                return OperationResult<ActiveCoopEventInfo>.Fail("CoopChainID is required.");
            }

            var request = CreateBaseRequest();
            request.CoopChainID = coopChainID;

            var result = await CoopEventAPI.GetActiveEvent(request);

            if (result.Success && result.Data != null)
                OnActiveEventLoaded?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Loads the player's personal coop state and active group snapshot.
        /// Patches local UserCoopEventState. Fires <see cref="OnUserStateLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<CoopUserStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await CoopEventAPI.GetUserState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyCoopEvent(result.Data.UserState);
                OnUserStateLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Loads the full group state by GroupID. Does not mutate local user state.
        /// Fires <see cref="OnGroupStateLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<CoopGroupStateResponse>> GetGroupState(string groupID)
        {
            if (string.IsNullOrWhiteSpace(groupID))
            {
                Debug.LogWarning("[CoopEventService] GetGroupState: GroupID is required.");
                return OperationResult<CoopGroupStateResponse>.Fail("GroupID is required.");
            }

            var request = CreateBaseRequest();
            request.GroupID = groupID;

            var result = await CoopEventAPI.GetGroupState(request);

            if (result.Success && result.Data != null)
                OnGroupStateLoaded?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Matchmaking: joins an open group or creates a new one.
        /// Patches local ActiveGroupID / ActiveCoopEventID / MyObjectIndex.
        /// Fires <see cref="OnGroupJoined"/>.
        /// </summary>
        public static async Task<OperationResult<CoopGroupStateResponse>> JoinOrCreateGroup(string coopChainID)
        {
            if (string.IsNullOrWhiteSpace(coopChainID))
            {
                Debug.LogWarning("[CoopEventService] JoinOrCreateGroup: CoopChainID is required.");
                return OperationResult<CoopGroupStateResponse>.Fail("CoopChainID is required.");
            }

            var request = CreateBaseRequest();
            request.CoopChainID = coopChainID;

            var result = await CoopEventAPI.JoinOrCreateGroup(request);

            if (result.Success && result.Data?.Group != null)
            {
                var group = result.Data.Group;
                var myMember = group.Members?.Find(m => m.UserID == Ctx.UserID);
                int myObjectIndex = myMember?.BuildObjectsProgress?.ObjectIndex ?? -1;

                IDosGamesData.User.PatchCoopEventActiveGroup(
                    group.GroupID, group.CoopEventID, myObjectIndex);

                OnGroupJoined?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Performs a spinner spin. Consumes event tokens server-side.
        /// Applies the resource operation locally. Fires <see cref="OnSpinCompleted"/>.
        /// </summary>
        public static async Task<OperationResult<CoopSpinResponse>> Spin(string coopChainID, string groupID, string relatedEntityID = null)
        {
            if (string.IsNullOrWhiteSpace(coopChainID))
            {
                Debug.LogWarning("[CoopEventService] Spin: CoopChainID is required.");
                return OperationResult<CoopSpinResponse>.Fail("CoopChainID is required.");
            }

            var request = CreateBaseRequest();
            request.CoopChainID = coopChainID;
            request.GroupID = groupID;
            request.RelatedEntityID = Guid.NewGuid().ToString();

            var result = await CoopEventAPI.Spin(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(
                        result.Data.Resources,
                        IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnSpinCompleted?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Claims the intermediate reward for completing the player's own build object.
        /// Applies granted resources locally. Fires <see cref="OnObjectRewardClaimed"/>.
        /// </summary>
        public static async Task<OperationResult<CoopClaimRewardResponse>> ClaimObjectReward(string groupID, string relatedEntityID = null)
        {
            if (string.IsNullOrWhiteSpace(groupID))
            {
                Debug.LogWarning("[CoopEventService] ClaimObjectReward: GroupID is required.");
                return OperationResult<CoopClaimRewardResponse>.Fail("GroupID is required.");
            }

            var request = CreateBaseRequest();
            request.GroupID = groupID;
            request.RelatedEntityID = Guid.NewGuid().ToString();

            var result = await CoopEventAPI.ClaimObjectReward(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(
                        result.Data.Resources,
                        IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnObjectRewardClaimed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Claims the Grand Prize after all objects in the group are completed.
        /// Applies granted resources locally and clears the active group state.
        /// Fires <see cref="OnGrandPrizeClaimed"/>.
        /// </summary>
        public static async Task<OperationResult<CoopClaimRewardResponse>> ClaimGrandPrize(string groupID, string relatedEntityID = null)
        {
            if (string.IsNullOrWhiteSpace(groupID))
            {
                Debug.LogWarning("[CoopEventService] ClaimGrandPrize: GroupID is required.");
                return OperationResult<CoopClaimRewardResponse>.Fail("GroupID is required.");
            }

            var request = CreateBaseRequest();
            request.GroupID = groupID;
            request.RelatedEntityID = Guid.NewGuid().ToString();

            var result = await CoopEventAPI.ClaimGrandPrize(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(
                        result.Data.Resources,
                        IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                // Server clears ActiveGroupID in the same TX — mirror that locally.
                IDosGamesData.User.PatchCoopEventActiveGroup(null, null, -1);

                OnGrandPrizeClaimed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Leaves the current active group. Clears local active group state.
        /// Fires <see cref="OnGroupLeft"/>.
        /// </summary>
        public static async Task<OperationResult<CoopLeaveGroupResponse>> LeaveGroup(string groupID = null)
        {
            var request = CreateBaseRequest();
            request.GroupID = groupID;

            var result = await CoopEventAPI.LeaveGroup(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchCoopEventActiveGroup(null, null, -1);
                OnGroupLeft?.Invoke(result.Data);
            }

            return result;
        }
    }
}
