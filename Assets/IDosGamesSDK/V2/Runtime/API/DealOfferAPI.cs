using System.Threading.Tasks;

namespace IDosGames
{
    public static class DealOfferAPI
    {
        private static string GetEndpoint(DealOfferAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/DealOffer/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(DealOfferAction action, DealOfferRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<DealOffersDefinitionResponse>> GetDefinition(DealOfferRequest request)
            => SendRequest<DealOffersDefinitionResponse>(DealOfferAction.GetDefinition, request);

        public static Task<OperationResult<UserDealOffersStateResponse>> GetUserState(DealOfferRequest request)
            => SendRequest<UserDealOffersStateResponse>(DealOfferAction.GetUserState, request);

        public static Task<OperationResult<GetActiveDealsResponse>> GetActiveDeals(DealOfferRequest request)
            => SendRequest<GetActiveDealsResponse>(DealOfferAction.GetActiveDeals, request);

        public static Task<OperationResult<DismissDealResponse>> DismissDeal(DealOfferRequest request)
            => SendRequest<DismissDealResponse>(DealOfferAction.DismissDeal, request);

        public static Task<OperationResult<ExecuteNodeResponse>> ExecuteNode(DealOfferRequest request)
            => SendRequest<ExecuteNodeResponse>(DealOfferAction.ExecuteNode, request);

        public static Task<OperationResult<RecordShowResponse>> RecordShow(DealOfferRequest request)
            => SendRequest<RecordShowResponse>(DealOfferAction.RecordShow, request);
    }
}
