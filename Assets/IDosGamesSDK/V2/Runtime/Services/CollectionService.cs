using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class CollectionService
    {
        // =====================================================================
        // Events
        // =====================================================================

        public static event Action<CollectionDefinitions> OnCollectionDefinitionsLoaded;
        public static event Action<UserCollectionState> OnCollectionUserStateLoaded;
        public static event Action<OpenPackResponse> OnPackOpened;
        public static event Action<OpenCollectionChestResponse> OnCollectionChestOpened;
        public static event Action<UseCollectibleJokerResponse> OnCollectibleJokerUsed;
        public static event Action<ClaimSetRewardResponse> OnSetRewardClaimed;
        public static event Action<ClaimGrandPrizeResponse> OnGrandPrizeClaimed;
        public static event Action<SendTradeOfferResponse> OnTradeOfferSent;
        public static event Action<CancelTradeOfferResponse> OnTradeOfferCancelled;
        public static event Action<AcceptTradeOfferResponse> OnTradeOfferAccepted;
        public static event Action<DeclineTradeOfferResponse> OnTradeOfferDeclined;
        public static event Action<GetTradeOffersResponse> OnMyTradeOffersLoaded;
        public static event Action<GetTradeOffersResponse> OnIncomingTradeOffersLoaded;

        // =====================================================================
        // Helpers
        // =====================================================================

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static CollectionRequest CreateBaseRequest() => new()
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
        /// Loads and caches the full collection config. Fires OnCollectionDefinitionsLoaded.
        /// </summary>
        public static async Task<OperationResult<CollectionDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await CollectionAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchCollection(result.Data);
                OnCollectionDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Loads the player's current collection state. Fires OnCollectionUserStateLoaded.
        /// </summary>
        public static async Task<OperationResult<UserCollectionState>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await CollectionAPI.GetUserState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyCollection(result.Data);
                OnCollectionUserStateLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Opens a pack of the given type. Applies resource operation locally. Fires OnPackOpened.
        /// </summary>
        public static async Task<OperationResult<OpenPackResponse>> OpenPack(string collectionID, string packTypeID)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] OpenPack: CollectionID is required.");
                return OperationResult<OpenPackResponse>.Fail("CollectionID is required.");
            }
            if (string.IsNullOrWhiteSpace(packTypeID))
            {
                Debug.LogWarning("[CollectionService] OpenPack: PackTypeID is required.");
                return OperationResult<OpenPackResponse>.Fail("PackTypeID is required.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;
            request.PackTypeID = packTypeID;
            request.RelatedEntityID = $"open_pack_{collectionID}_{packTypeID}_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.OpenPack(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnPackOpened?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Opens a collection chest. Applies resource operation locally. Fires OnCollectionChestOpened.
        /// </summary>
        public static async Task<OperationResult<OpenCollectionChestResponse>> OpenCollectionChest(string collectionID, string collectionChestID)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] OpenCollectionChest: CollectionID is required.");
                return OperationResult<OpenCollectionChestResponse>.Fail("CollectionID is required.");
            }
            if (string.IsNullOrWhiteSpace(collectionChestID))
            {
                Debug.LogWarning("[CollectionService] OpenCollectionChest: CollectionChestID is required.");
                return OperationResult<OpenCollectionChestResponse>.Fail("CollectionChestID is required.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;
            request.CollectionChestID = collectionChestID;
            request.RelatedEntityID = $"open_chest_{collectionID}_{collectionChestID}_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.OpenCollectionChest(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnCollectionChestOpened?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Uses a CollectibleJoker to claim any missing regular Collectible. Consumes 1 Joker item. Fires OnCollectibleJokerUsed.
        /// </summary>
        public static async Task<OperationResult<UseCollectibleJokerResponse>> UseCollectibleJoker(string collectionID, string collectibleID)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] UseCollectibleJoker: CollectionID is required.");
                return OperationResult<UseCollectibleJokerResponse>.Fail("CollectionID is required.");
            }
            if (string.IsNullOrWhiteSpace(collectibleID))
            {
                Debug.LogWarning("[CollectionService] UseCollectibleJoker: CollectibleID is required.");
                return OperationResult<UseCollectibleJokerResponse>.Fail("CollectibleID is required.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;
            request.CollectibleID = collectibleID;
            request.CollectibleIsSpecial = false;
            request.RelatedEntityID = $"use_joker_{collectionID}_{collectibleID}_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.UseCollectibleJoker(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnCollectibleJokerUsed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Claims the reward for a completed set. Applies granted resources locally. Fires OnSetRewardClaimed.
        /// </summary>
        public static async Task<OperationResult<ClaimSetRewardResponse>> ClaimSetReward(string collectionID, string setID)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] ClaimSetReward: CollectionID is required.");
                return OperationResult<ClaimSetRewardResponse>.Fail("CollectionID is required.");
            }
            if (string.IsNullOrWhiteSpace(setID))
            {
                Debug.LogWarning("[CollectionService] ClaimSetReward: SetID is required.");
                return OperationResult<ClaimSetRewardResponse>.Fail("SetID is required.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;
            request.SetID = setID;
            request.RelatedEntityID = $"claim_set_{collectionID}_{setID}_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.ClaimSetReward(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnSetRewardClaimed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Claims the Grand Prize for fully completing the collection. Applies granted resources locally. Fires OnGrandPrizeClaimed.
        /// </summary>
        public static async Task<OperationResult<ClaimGrandPrizeResponse>> ClaimGrandPrize(string collectionID)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] ClaimGrandPrize: CollectionID is required.");
                return OperationResult<ClaimGrandPrizeResponse>.Fail("CollectionID is required.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;
            request.RelatedEntityID = $"grand_prize_{collectionID}_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.ClaimGrandPrize(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnGrandPrizeClaimed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Sends a P2P trade offer to a friend. Fires OnTradeOfferSent.
        /// </summary>
        public static async Task<OperationResult<SendTradeOfferResponse>> SendTradeOffer(
            string collectionID,
            string collectibleID,
            bool collectibleIsSpecial,
            string receiverUserID,
            string requestedCollectibleID = null,
            bool requestedCollectibleIsSpecial = false)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] SendTradeOffer: CollectionID is required.");
                return OperationResult<SendTradeOfferResponse>.Fail("CollectionID is required.");
            }
            if (string.IsNullOrWhiteSpace(collectibleID))
            {
                Debug.LogWarning("[CollectionService] SendTradeOffer: CollectibleID is required.");
                return OperationResult<SendTradeOfferResponse>.Fail("CollectibleID is required.");
            }
            if (string.IsNullOrWhiteSpace(receiverUserID))
            {
                Debug.LogWarning("[CollectionService] SendTradeOffer: ReceiverUserID is required.");
                return OperationResult<SendTradeOfferResponse>.Fail("ReceiverUserID is required.");
            }
            if (receiverUserID == Ctx.UserID)
            {
                Debug.LogWarning("[CollectionService] SendTradeOffer: Cannot trade with yourself.");
                return OperationResult<SendTradeOfferResponse>.Fail("Cannot trade with yourself.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;
            request.CollectibleID = collectibleID;
            request.CollectibleIsSpecial = collectibleIsSpecial;
            request.ReceiverUserID = receiverUserID;
            request.RequestedCollectibleID = requestedCollectibleID;
            request.RequestedCollectibleIsSpecial = requestedCollectibleIsSpecial;
            request.RelatedEntityID = $"trade_send_{Ctx.UserID}_{receiverUserID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.SendTradeOffer(request);

            if (result.Success && result.Data != null)
                OnTradeOfferSent?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Cancels an outgoing trade offer. Returns the Collectible to the sender server-side. Fires OnTradeOfferCancelled.
        /// </summary>
        public static async Task<OperationResult<CancelTradeOfferResponse>> CancelTradeOffer(string offerID)
        {
            if (string.IsNullOrWhiteSpace(offerID))
            {
                Debug.LogWarning("[CollectionService] CancelTradeOffer: OfferID is required.");
                return OperationResult<CancelTradeOfferResponse>.Fail("OfferID is required.");
            }

            var request = CreateBaseRequest();
            request.OfferID = offerID;
            request.RelatedEntityID = $"trade_cancel_{offerID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.CancelTradeOffer(request);

            if (result.Success && result.Data != null)
                OnTradeOfferCancelled?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Accepts an incoming trade offer. Fires OnTradeOfferAccepted.
        /// </summary>
        public static async Task<OperationResult<AcceptTradeOfferResponse>> AcceptTradeOffer(string offerID)
        {
            if (string.IsNullOrWhiteSpace(offerID))
            {
                Debug.LogWarning("[CollectionService] AcceptTradeOffer: OfferID is required.");
                return OperationResult<AcceptTradeOfferResponse>.Fail("OfferID is required.");
            }

            var request = CreateBaseRequest();
            request.OfferID = offerID;
            request.RelatedEntityID = $"trade_accept_{offerID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.AcceptTradeOffer(request);

            if (result.Success && result.Data != null)
                OnTradeOfferAccepted?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Declines an incoming trade offer. Returns the Collectible to the sender server-side. Fires OnTradeOfferDeclined.
        /// </summary>
        public static async Task<OperationResult<DeclineTradeOfferResponse>> DeclineTradeOffer(string offerID)
        {
            if (string.IsNullOrWhiteSpace(offerID))
            {
                Debug.LogWarning("[CollectionService] DeclineTradeOffer: OfferID is required.");
                return OperationResult<DeclineTradeOfferResponse>.Fail("OfferID is required.");
            }

            var request = CreateBaseRequest();
            request.OfferID = offerID;
            request.RelatedEntityID = $"trade_decline_{offerID}_{Guid.NewGuid():N}";

            var result = await CollectionAPI.DeclineTradeOffer(request);

            if (result.Success && result.Data != null)
                OnTradeOfferDeclined?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Loads all outgoing trade offers for the given collection. Fires OnMyTradeOffersLoaded.
        /// </summary>
        public static async Task<OperationResult<GetTradeOffersResponse>> GetMyTradeOffers(string collectionID)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] GetMyTradeOffers: CollectionID is required.");
                return OperationResult<GetTradeOffersResponse>.Fail("CollectionID is required.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;

            var result = await CollectionAPI.GetMyTradeOffers(request);

            if (result.Success && result.Data != null)
                OnMyTradeOffersLoaded?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Loads all incoming pending trade offers for the given collection. Fires OnIncomingTradeOffersLoaded.
        /// </summary>
        public static async Task<OperationResult<GetTradeOffersResponse>> GetIncomingTradeOffers(string collectionID)
        {
            if (string.IsNullOrWhiteSpace(collectionID))
            {
                Debug.LogWarning("[CollectionService] GetIncomingTradeOffers: CollectionID is required.");
                return OperationResult<GetTradeOffersResponse>.Fail("CollectionID is required.");
            }

            var request = CreateBaseRequest();
            request.CollectionID = collectionID;

            var result = await CollectionAPI.GetIncomingTradeOffers(request);

            if (result.Success && result.Data != null)
                OnIncomingTradeOffersLoaded?.Invoke(result.Data);

            return result;
        }
    }
}
