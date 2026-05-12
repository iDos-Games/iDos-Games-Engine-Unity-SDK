using System.Threading.Tasks;

namespace IDosGames
{
    public static class CollectionAPI
    {
        private static string GetEndpoint(CollectionAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Collection/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(CollectionAction action, CollectionRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<CollectionDefinitions>> GetDefinitions(CollectionRequest request)
            => SendRequest<CollectionDefinitions>(CollectionAction.GetDefinitions, request);

        public static Task<OperationResult<UserCollectionState>> GetUserState(CollectionRequest request)
            => SendRequest<UserCollectionState>(CollectionAction.GetUserState, request);

        public static Task<OperationResult<OpenPackResponse>> OpenPack(CollectionRequest request)
            => SendRequest<OpenPackResponse>(CollectionAction.OpenPack, request);

        public static Task<OperationResult<OpenCollectionChestResponse>> OpenCollectionChest(CollectionRequest request)
            => SendRequest<OpenCollectionChestResponse>(CollectionAction.OpenCollectionChest, request);

        public static Task<OperationResult<UseCollectibleJokerResponse>> UseCollectibleJoker(CollectionRequest request)
            => SendRequest<UseCollectibleJokerResponse>(CollectionAction.UseCollectibleJoker, request);

        public static Task<OperationResult<ClaimSetRewardResponse>> ClaimSetReward(CollectionRequest request)
            => SendRequest<ClaimSetRewardResponse>(CollectionAction.ClaimSetReward, request);

        public static Task<OperationResult<ClaimGrandPrizeResponse>> ClaimGrandPrize(CollectionRequest request)
            => SendRequest<ClaimGrandPrizeResponse>(CollectionAction.ClaimGrandPrize, request);

        public static Task<OperationResult<SendTradeOfferResponse>> SendTradeOffer(CollectionRequest request)
            => SendRequest<SendTradeOfferResponse>(CollectionAction.SendTradeOffer, request);

        public static Task<OperationResult<CancelTradeOfferResponse>> CancelTradeOffer(CollectionRequest request)
            => SendRequest<CancelTradeOfferResponse>(CollectionAction.CancelTradeOffer, request);

        public static Task<OperationResult<AcceptTradeOfferResponse>> AcceptTradeOffer(CollectionRequest request)
            => SendRequest<AcceptTradeOfferResponse>(CollectionAction.AcceptTradeOffer, request);

        public static Task<OperationResult<DeclineTradeOfferResponse>> DeclineTradeOffer(CollectionRequest request)
            => SendRequest<DeclineTradeOfferResponse>(CollectionAction.DeclineTradeOffer, request);

        public static Task<OperationResult<GetTradeOffersResponse>> GetMyTradeOffers(CollectionRequest request)
            => SendRequest<GetTradeOffersResponse>(CollectionAction.GetMyTradeOffers, request);

        public static Task<OperationResult<GetTradeOffersResponse>> GetIncomingTradeOffers(CollectionRequest request)
            => SendRequest<GetTradeOffersResponse>(CollectionAction.GetIncomingTradeOffers, request);
    }
}
