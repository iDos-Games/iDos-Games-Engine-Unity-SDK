using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class PremiumService
    {
        // =====================================================================
        // Events
        // =====================================================================

        /// <summary>Fired after premium definitions are loaded from the server.</summary>
        public static event Action<PremiumDefinitionsResponse> OnPremiumDefinitionsLoaded;

        /// <summary>Fired after the player's premium state is refreshed from the server.</summary>
        public static event Action<PremiumStateResponse> OnPremiumStateLoaded;

        /// <summary>Fired after a trial is successfully activated.</summary>
        public static event Action<PremiumPurchaseResponse> OnTrialActivated;

        /// <summary>Fired after a resource-based subscription purchase succeeds.</summary>
        public static event Action<PremiumPurchaseResponse> OnPurchaseCompleted;

        /// <summary>Fired after a real-money subscription purchase succeeds.</summary>
        public static event Action<PremiumPurchaseResponse> OnRealMoneyPurchaseCompleted;

        // =====================================================================
        // Helpers
        // =====================================================================

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static PremiumRequest CreateBaseRequest() => new()
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
        /// Loads premium definitions (subscription products) from TitlePublicConfiguration.
        /// Patches local TitleConfig and fires OnPremiumDefinitionsLoaded on success.
        /// </summary>
        public static async Task<OperationResult<PremiumDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await PremiumAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Premium != null)
                    IDosGamesData.Config.PatchPremium(data.Premium);

                OnPremiumDefinitionsLoaded?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Refreshes the player's premium state from the server (recalculates MaxActiveTier, cleans expired subscriptions).
        /// Patches local UserData and fires OnPremiumStateLoaded on success.
        /// </summary>
        public static async Task<OperationResult<PremiumStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await PremiumAPI.GetUserState(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Premium != null)
                    IDosGamesData.User.ApplyPremium(data.Premium);

                OnPremiumStateLoaded?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Activates a free trial for the given premium subscription.
        /// Patches local premium state and fires OnTrialActivated on success.
        /// No resources are consumed; the server returns an empty ResourceOperation.
        /// </summary>
        /// <param name="premiumID">ID of the premium product (e.g. "silver_vip").</param>
        /// <param name="transactionID">Unique client-generated transaction ID for idempotency.</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> ActivateTrial(string premiumID, string transactionID)
        {
            if (string.IsNullOrWhiteSpace(transactionID))
            {
                Debug.LogWarning("[PremiumService] ActivateTrial: TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required.");
            }

            var request = CreateBaseRequest();
            request.PremiumID = premiumID;
            request.TransactionID = transactionID;
            request.RelatedEntityID = $"premium_trial_{premiumID}_{transactionID}";

            var result = await PremiumAPI.ActivateTrial(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Premium != null)
                    IDosGamesData.User.ApplyPremium(data.Premium);

                OnTrialActivated?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Purchases a premium subscription using in-game resources (currencies, items, event tokens).
        /// Patches local premium state and resource balances. Fires OnPurchaseCompleted on success.
        /// </summary>
        /// <param name="premiumID">ID of the premium product to purchase.</param>
        /// <param name="transactionID">Unique client-generated transaction ID for idempotency.</param>
        /// <param name="selectedOptionID">Price option key. Defaults to "Default" when null.</param>
        /// <param name="count">Number of periods to purchase at once. Defaults to 1.</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> PurchaseItemOrCurrency(
            string premiumID,
            string transactionID,
            string selectedOptionID = null,
            int count = 1)
        {
            if (string.IsNullOrWhiteSpace(transactionID))
            {
                Debug.LogWarning("[PremiumService] PurchaseItemOrCurrency: TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required.");
            }
            if (count <= 0) count = 1;

            var request = CreateBaseRequest();
            request.PremiumID = premiumID;
            request.TransactionID = transactionID;
            request.SelectedOptionID = string.IsNullOrWhiteSpace(selectedOptionID) ? DefaultData.Default : selectedOptionID;
            request.Count = count;
            request.RelatedEntityID = $"premium_purchase_{premiumID}_{transactionID}";

            var result = await PremiumAPI.PurchaseItemOrCurrency(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Premium != null)
                    IDosGamesData.User.ApplyPremium(data.Premium);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnPurchaseCompleted?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Purchases a premium subscription via a real-money store receipt (Apple/Google).
        /// Patches local premium state and fires OnRealMoneyPurchaseCompleted on success.
        /// No resource deduction on the client side — server validates the receipt.
        /// </summary>
        /// <param name="premiumID">ID of the premium product.</param>
        /// <param name="transactionID">Store transaction ID for idempotency.</param>
        /// <param name="store">Apple or Google.</param>
        /// <param name="productID">Store product identifier.</param>
        /// <param name="receiptData">Raw receipt data from the store SDK.</param>
        /// <param name="purchaseToken">Google Play purchase token (pass null for Apple).</param>
        /// <param name="packageName">Google Play app package name (pass null for Apple).</param>
        /// <param name="appStoreEnvironment">"Sandbox" or "Production" (Apple only).</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> PurchaseRealMoney(
            string premiumID,
            string transactionID,
            StoreType store,
            string productID,
            string receiptData,
            string purchaseToken = null,
            string packageName = null,
            string appStoreEnvironment = null)
        {
            if (string.IsNullOrWhiteSpace(transactionID))
            {
                Debug.LogWarning("[PremiumService] PurchaseRealMoney: TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required.");
            }
            if (string.IsNullOrWhiteSpace(productID))
            {
                Debug.LogWarning("[PremiumService] PurchaseRealMoney: ProductID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("ProductID is required.");
            }
            if (string.IsNullOrWhiteSpace(receiptData))
            {
                Debug.LogWarning("[PremiumService] PurchaseRealMoney: ReceiptData is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("ReceiptData is required.");
            }

            var request = CreateBaseRequest();
            request.PremiumID = premiumID;
            request.TransactionID = transactionID;
            request.Store = store;
            request.ProductID = productID;
            request.ReceiptData = receiptData;
            request.PurchaseToken = purchaseToken;
            request.PackageName = packageName;
            request.AppStoreEnvironment = appStoreEnvironment;
            request.RelatedEntityID = $"premium_iap_{premiumID}_{transactionID}";

            var result = await PremiumAPI.PurchaseRealMoney(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Premium != null)
                    IDosGamesData.User.ApplyPremium(data.Premium);

                // Real-money flow: Resources.Consume is typically empty (no in-game cost).
                // Still call ApplyResourceOperation to handle any bonus grants the server might attach.
                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnRealMoneyPurchaseCompleted?.Invoke(data);
            }

            return result;
        }
    }
}
