using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class DealOfferAPI
    {
        /// <summary>
        /// Generates a URL for the request.
        /// </summary>
        private static string GetEndpoint(DealOfferAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/DealOffer/{action}/{userID}";
        }

        /// <summary>
        /// A generic method for sending a request that hides the details of HttpService.
        /// </summary>
        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(DealOfferAction action, DealOfferRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        // =================================================================================
        // PUBLIC METHODS
        // =================================================================================

        public static async Task<OperationResult<DealOffersDefinitionResponse>> GetDefinition(DealOfferRequest request)
        {
            return await SendRequest<DealOffersDefinitionResponse>(DealOfferAction.GetDefinition, request);
        }

        public static async Task<OperationResult<UserDealOffersStateResponse>> GetUserState(DealOfferRequest request)
        {
            return await SendRequest<UserDealOffersStateResponse>(DealOfferAction.GetUserState, request);
        }

        public static async Task<OperationResult<GetActiveDealsResponse>> GetActiveDeals(DealOfferRequest request)
        {
            return await SendRequest<GetActiveDealsResponse>(DealOfferAction.GetActiveDeals, request);
        }

        public static async Task<OperationResult<SuccessResponse>> DismissDeal(DealOfferRequest request)
        {
            return await SendRequest<SuccessResponse>(DealOfferAction.DismissDeal, request);
        }

        public static async Task<OperationResult<ExecuteNodeResponse>> ExecuteNode(DealOfferRequest request)
        {
            return await SendRequest<ExecuteNodeResponse>(DealOfferAction.ExecuteNode, request);
        }

        public static async Task<OperationResult<SuccessResponse>> RecordShow(DealOfferRequest request)
        {
            return await SendRequest<SuccessResponse>(DealOfferAction.RecordShow, request);
        }
    }
}
