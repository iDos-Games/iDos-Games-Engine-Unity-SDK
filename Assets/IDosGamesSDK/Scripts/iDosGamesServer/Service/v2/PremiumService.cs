using System.Threading.Tasks;
using IDosGames.ClientModels;
using UnityEngine;

namespace IDosGames
{
    public static class PremiumService
    {
        public static event System.Action<PremiumDefinitionsResponse> OnDefinitionsUpdated;
        public static event System.Action<PremiumStateResponse> OnUserStateUpdated;
        public static event System.Action<PremiumPurchaseResponse> OnTrialActivated;
        public static event System.Action<PremiumPurchaseResponse> OnPurchaseSuccess;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();

        private static PremiumRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
        };

        // ─────────────────────────────────────────────────────────────────
        // GetDefinitions
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads Premium definitions (config) and stores them in IDosGamesData.Config.
        /// </summary>
        public static async Task<OperationResult<PremiumDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await PremiumAPI.GetDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchPremiumDefinitions(result.Data.PremiumDefinitions);
                OnDefinitionsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        // GetUserState
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fetches the current user's Premium state and stores it in IDosGamesData.User.
        /// </summary>
        public static async Task<OperationResult<PremiumStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await PremiumAPI.GetUserState(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyPremium(result.Data.Premium);
                OnUserStateUpdated?.Invoke(result.Data);
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        // ActivateTrial
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Activates a free trial for the given Premium.
        /// </summary>
        /// <param name="premiumId">PremiumID from config (e.g. "silver_vip")</param>
        /// <param name="transactionId">Unique client-side transaction ID for idempotency</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> ActivateTrial(string premiumId, string transactionId)
        {
            if (string.IsNullOrWhiteSpace(premiumId))
            {
                Debug.LogWarning("[PremiumService] ActivateTrial: PremiumID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("PremiumID is required");
            }

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                Debug.LogWarning("[PremiumService] ActivateTrial: TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required");
            }

            var request = CreateBaseRequest();
            request.PremiumID = premiumId;
            request.TransactionID = transactionId;

            var result = await PremiumAPI.ActivateTrial(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyPremium(result.Data.Premium);
                OnTrialActivated?.Invoke(result.Data);
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        // PurchaseItemOrCurrency
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Purchases a Premium subscription using in-game resources.
        /// </summary>
        /// <param name="premiumId">PremiumID from config</param>
        /// <param name="transactionId">Unique client-side transaction ID for idempotency</param>
        /// <param name="selectedOptionId">Index of the PriceOption to use</param>
        /// <param name="count">How many periods to purchase (default 1)</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> PurchaseItemOrCurrency(string premiumId, string transactionId, int selectedOptionId = 0, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(premiumId))
            {
                Debug.LogWarning("[PremiumService] PurchaseItemOrCurrency: PremiumID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("PremiumID is required");
            }

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                Debug.LogWarning("[PremiumService] PurchaseItemOrCurrency: TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required");
            }

            var request = CreateBaseRequest();
            request.PremiumID = premiumId;
            request.TransactionID = transactionId;
            request.SelectedOptionID = selectedOptionId;
            request.Count = count;

            var result = await PremiumAPI.PurchaseItemOrCurrency(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyPremium(result.Data.Premium);
                IDosGamesData.User.ConsumeResources(result.Data.ConsumedResources);
                OnPurchaseSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
