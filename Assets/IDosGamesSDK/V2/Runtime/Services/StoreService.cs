using System.Threading.Tasks;
using IDosGames.ClientModels;
using UnityEngine;

namespace IDosGames
{
    public static class StoreService
    {
        public static event System.Action<StoreDefinitions> OnStoreDefinitionsUpdated;
        public static event System.Action<UserStoreState> OnUserStoreStateUpdated;
        public static event System.Action<StorePurchaseResponse> OnPurchaseSuccess;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();

        /// <summary>
        /// Creates a basic request with required authorization fields.
        /// </summary>
        private static StoreRequest CreateBaseRequest()
        {
            return new StoreRequest
            {
                UserID = Ctx.UserID,
                ClientSessionTicket = Ctx.ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        /// <summary>
        /// Gets Store definitions (config: stores + offers).
        /// </summary>
        public static async Task<OperationResult<StoreDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await StoreAPI.GetDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchStoreDefinitions(result.Data);
                OnStoreDefinitionsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Gets the player's current store state (purchase counters).
        /// </summary>
        public static async Task<OperationResult<UserStoreState>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await StoreAPI.GetUserState(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyStoreState(result.Data);
                OnUserStoreStateUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Purchases a store offer.
        /// </summary>
        /// <param name="offerID">The offer ID to purchase.</param>
        /// <param name="count">Number of times to purchase (default 1).</param>
        public static async Task<OperationResult<StorePurchaseResponse>> Purchase(string offerID, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(offerID))
            {
                Debug.LogWarning("[StoreService] Purchase: offerID is required.");
                return OperationResult<StorePurchaseResponse>.Fail("OfferID is required.");
            }

            if (count < 1 || count > 100)
            {
                Debug.LogWarning($"[StoreService] Purchase: count {count} is out of range [1, 100]. Clamping.");
                count = System.Math.Clamp(count, 1, 100);
            }

            var request = CreateBaseRequest();
            request.OfferID = offerID;
            request.Count = count;

            var result = await StoreAPI.Purchase(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchStorePurchase(offerID, count);
                IDosGamesData.User.ConsumeResources(result.Data.Consumed);
                IDosGamesData.User.GrantResources(result.Data.Granted);
                OnPurchaseSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
