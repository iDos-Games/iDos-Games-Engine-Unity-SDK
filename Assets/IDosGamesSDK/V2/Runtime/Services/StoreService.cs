using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class StoreService
    {
        // ---------- Typed events ----------

        /// <summary>Fired after a successful purchase. Carries the full server response.</summary>
        public static event Action<StorePurchaseResponse> OnOfferPurchased;

        /// <summary>Fired after store definitions are fetched and cached locally.</summary>
        public static event Action<StoreDefinitions> OnStoreDefinitionsLoaded;

        /// <summary>Fired after the player's store state is fetched and cached locally.</summary>
        public static event Action<UserStoreState> OnStoreUserStateLoaded;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static StoreRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Purchases the specified offer. Applies the resulting resource operation locally
        /// and updates purchase counters. Fires <see cref="OnOfferPurchased"/> on success.
        /// </summary>
        /// <param name="offerID">ID of the offer to purchase.</param>
        /// <param name="count">Number of units to buy (clamped server-side to [1, 100]).</param>
        public static async Task<OperationResult<StorePurchaseResponse>> Purchase(string offerID, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(offerID))
            {
                Debug.LogWarning("[StoreService] Purchase: OfferID is required.");
                return OperationResult<StorePurchaseResponse>.Fail("OfferID is required.");
            }

            if (count < 1) count = 1;

            var request = CreateBaseRequest();
            request.OfferID = offerID;
            request.Count = count;
            request.RelatedEntityID = $"store_buy_{offerID}_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await StoreAPI.Purchase(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                // 1. Apply purchase counters locally.
                IDosGamesData.User.PatchStorePurchase(offerID, count, data.ServerTimeUtc);

                // 2. Apply resource operation (currencies, items, event tokens).
                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                // 3. Fire event.
                OnOfferPurchased?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Fetches store definitions from the server and caches them in TitleConfig.
        /// Fires <see cref="OnStoreDefinitionsLoaded"/> on success.
        /// </summary>
        public static async Task<OperationResult<StoreDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await StoreAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchStore(result.Data);
                OnStoreDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the player's purchase counters from the server and caches them locally.
        /// Fires <see cref="OnStoreUserStateLoaded"/> on success.
        /// </summary>
        public static async Task<OperationResult<UserStoreState>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await StoreAPI.GetUserState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyStore(result.Data);
                OnStoreUserStateLoaded?.Invoke(result.Data);
            }

            return result;
        }
    }
}
