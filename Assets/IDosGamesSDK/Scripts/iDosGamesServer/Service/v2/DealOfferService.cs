using System.Threading.Tasks;
using IDosGames.ClientModels;
using UnityEngine;

namespace IDosGames
{
    public static class DealOfferService
    {
        public static event System.Action<DealOffersDefinitionResponse> OnDefinitionLoaded;
        public static event System.Action<UserDealOffersStateResponse> OnUserStateLoaded;
        public static event System.Action<GetActiveDealsResponse> OnActiveDealsLoaded;
        public static event System.Action<SuccessResponse> OnDealDismissed;
        public static event System.Action<ExecuteNodeResponse> OnNodeExecuted;
        public static event System.Action<SuccessResponse> OnShowRecorded;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        /// <summary>
        /// Creates a basic request with required authorization fields.
        /// </summary>
        private static DealOfferRequest CreateBaseRequest()
        {
            return new DealOfferRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        /// <summary>
        /// Gets Deal Offer definitions (config).
        /// </summary>
        public static async Task<OperationResult<DealOffersDefinitionResponse>> GetDefinition()
        {
            var request = CreateBaseRequest();
            var result = await DealOfferAPI.GetDefinition(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchDealOfferDefinitions(result.Data.DealOfferDefinitions);
                OnDefinitionLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Gets the player's current Deal Offers runtime state.
        /// </summary>
        public static async Task<OperationResult<UserDealOffersStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await DealOfferAPI.GetUserState(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyDealOffers(result.Data.DealOffers);
                OnUserStateLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Gets the list of active deal slots with resolved offer states.
        /// Primary endpoint for rendering the Deal Offers UI.
        /// </summary>
        // В DealOfferService.cs — исправить GetActiveDeals

        public static async Task<OperationResult<GetActiveDealsResponse>> GetActiveDeals()
        {
            var request = CreateBaseRequest();
            var result = await DealOfferAPI.GetActiveDeals(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyActiveDealSlots(result.Data.Slots);
                OnActiveDealsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Dismisses the active deal in the given slot.
        /// </summary>
        /// <param name="slotID">Slot ID containing the active deal.</param>
        public static async Task<OperationResult<SuccessResponse>> DismissDeal(string slotID)
        {
            if (string.IsNullOrWhiteSpace(slotID))
            {
                Debug.LogWarning("[DealOfferService] DismissDeal: slotID is required.");
                return OperationResult<SuccessResponse>.Fail("slotID is required");
            }

            var request = CreateBaseRequest();
            request.SlotID = slotID;

            var result = await DealOfferAPI.DismissDeal(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchDealOfferSlotDismissed(slotID);
                OnDealDismissed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Executes a node in the active deal of the given slot.
        /// </summary>
        /// <param name="slotID">Slot ID containing the active deal.</param>
        /// <param name="nodeID">Node ID to execute.</param>
        /// <param name="externalRefID">Optional idempotency ref (ad receipt, purchase token, etc.).</param>
        public static async Task<OperationResult<ExecuteNodeResponse>> ExecuteNode(string slotID, string nodeID, string externalRefID = null)
        {
            if (string.IsNullOrWhiteSpace(slotID))
            {
                Debug.LogWarning("[DealOfferService] ExecuteNode: slotID is required.");
                return OperationResult<ExecuteNodeResponse>.Fail("slotID is required");
            }

            if (string.IsNullOrWhiteSpace(nodeID))
            {
                Debug.LogWarning("[DealOfferService] ExecuteNode: nodeID is required.");
                return OperationResult<ExecuteNodeResponse>.Fail("nodeID is required");
            }

            var request = CreateBaseRequest();
            request.SlotID = slotID;
            request.NodeID = nodeID;
            request.ExternalRefID = externalRefID;

            var result = await DealOfferAPI.ExecuteNode(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchDealOfferNodeExecuted(slotID, nodeID, result.Data);
                IDosGamesData.User.ConsumeResources(result.Data.ConsumedResources);
                IDosGamesData.User.GrantResources(result.Data.GrantedResources);
                OnNodeExecuted?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Records a show impression for the active deal in the given slot.
        /// </summary>
        /// <param name="slotID">Slot ID whose deal was shown to the player.</param>
        public static async Task<OperationResult<SuccessResponse>> RecordShow(string slotID)
        {
            if (string.IsNullOrWhiteSpace(slotID))
            {
                Debug.LogWarning("[DealOfferService] RecordShow: slotID is required.");
                return OperationResult<SuccessResponse>.Fail("slotID is required");
            }

            var request = CreateBaseRequest();
            request.SlotID = slotID;

            var result = await DealOfferAPI.RecordShow(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchDealOfferShowRecorded(slotID);
                OnShowRecorded?.Invoke(result.Data);
            }

            return result;
        }
    }
}
