using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class DealOfferService
    {
        // ---------- Events ----------

        public static event Action<DealOfferDefinitions> OnDefinitionLoaded;
        public static event Action<UserDealOffersState> OnUserStateLoaded;
        public static event Action<GetActiveDealsResponse> OnActiveDealsLoaded;
        public static event Action<DismissDealResponse> OnDealDismissed;
        public static event Action<ExecuteNodeResponse> OnNodeExecuted;
        public static event Action<RecordShowResponse> OnShowRecorded;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static DealOfferRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Fetches the DealOffer config definition and caches it in TitleConfig.
        /// Fires OnDefinitionLoaded on success.
        /// </summary>
        public static async Task<OperationResult<DealOffersDefinitionResponse>> GetDefinition()
        {
            var request = CreateBaseRequest();
            var result = await DealOfferAPI.GetDefinition(request);

            if (result.Success && result.Data?.DealOfferDefinitions != null)
            {
                IDosGamesData.Config.PatchDealOffer(result.Data.DealOfferDefinitions);
                OnDefinitionLoaded?.Invoke(result.Data.DealOfferDefinitions);
            }

            return result;
        }

        /// <summary>
        /// Fetches the current user's DealOffer state and caches it in UserData.
        /// Fires OnUserStateLoaded on success.
        /// </summary>
        public static async Task<OperationResult<UserDealOffersStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await DealOfferAPI.GetUserState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyDealOffer(result.Data.DealOffers);
                OnUserStateLoaded?.Invoke(result.Data.DealOffers);
            }

            return result;
        }

        /// <summary>
        /// Fetches the currently active deals for all slots (resolved for UI rendering).
        /// Does not mutate local state — UI reads directly from the response.
        /// Fires OnActiveDealsLoaded on success.
        /// </summary>
        public static async Task<OperationResult<GetActiveDealsResponse>> GetActiveDeals()
        {
            var request = CreateBaseRequest();
            var result = await DealOfferAPI.GetActiveDeals(request);

            if (result.Success && result.Data != null)
            {
                OnActiveDealsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Dismisses the active offer in the given slot. Updates local DealOffer state and fires OnDealDismissed.
        /// </summary>
        public static async Task<OperationResult<DismissDealResponse>> DismissDeal(string slotID)
        {
            if (string.IsNullOrWhiteSpace(slotID))
            {
                Debug.LogWarning("[DealOfferService] DismissDeal: SlotID is required.");
                return OperationResult<DismissDealResponse>.Fail("SlotID is required.");
            }

            var request = CreateBaseRequest();
            request.SlotID = slotID.Trim();
            request.RelatedEntityID = $"deal_dismiss_{slotID.Trim()}_{Guid.NewGuid():N}";

            var result = await DealOfferAPI.DismissDeal(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchDealOfferSlotDismissed(result.Data.SlotID, result.Data.ServerTimeUtc);

                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnDealDismissed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Executes a node in the given slot. Applies resource changes locally and fires OnNodeExecuted.
        /// </summary>
        public static async Task<OperationResult<ExecuteNodeResponse>> ExecuteNode(string slotID, string nodeID, string externalRefID = null)
        {
            if (string.IsNullOrWhiteSpace(slotID))
            {
                Debug.LogWarning("[DealOfferService] ExecuteNode: SlotID is required.");
                return OperationResult<ExecuteNodeResponse>.Fail("SlotID is required.");
            }

            if (string.IsNullOrWhiteSpace(nodeID))
            {
                Debug.LogWarning("[DealOfferService] ExecuteNode: NodeID is required.");
                return OperationResult<ExecuteNodeResponse>.Fail("NodeID is required.");
            }

            var request = CreateBaseRequest();
            request.SlotID = slotID.Trim();
            request.NodeID = nodeID.Trim();

            // Use stable external ref (receipt/ad-verification ID) when available for server-side idempotency.
            request.RelatedEntityID = !string.IsNullOrWhiteSpace(externalRefID)
                ? externalRefID.Trim()
                : $"deal_exec_{slotID.Trim()}_{nodeID.Trim()}_{Guid.NewGuid():N}";

            var result = await DealOfferAPI.ExecuteNode(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (!data.Idempotent)
                {
                    IDosGamesData.User.PatchDealOfferNodeExecuted(
                        data.SlotID, data.NodeID, data.NodeCompleted, data.OfferExhausted, data.ServerTimeUtc);
                }

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnNodeExecuted?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Records a show impression for the active offer in the given slot. Updates local show counters and fires OnShowRecorded.
        /// </summary>
        public static async Task<OperationResult<RecordShowResponse>> RecordShow(string slotID)
        {
            if (string.IsNullOrWhiteSpace(slotID))
            {
                Debug.LogWarning("[DealOfferService] RecordShow: SlotID is required.");
                return OperationResult<RecordShowResponse>.Fail("SlotID is required.");
            }

            var request = CreateBaseRequest();
            request.SlotID = slotID.Trim();
            request.RelatedEntityID = $"deal_show_{slotID.Trim()}_{Guid.NewGuid():N}";

            var result = await DealOfferAPI.RecordShow(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchDealOfferShowRecorded(result.Data.SlotID, result.Data.ServerTimeUtc);

                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnShowRecorded?.Invoke(result.Data);
            }

            return result;
        }
    }
}
