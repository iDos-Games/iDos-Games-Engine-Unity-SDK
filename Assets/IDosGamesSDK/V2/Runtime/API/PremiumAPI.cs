using System.Threading.Tasks;

namespace IDosGames
{
    public static class PremiumAPI
    {
        private static string GetEndpoint(PremiumAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Premium/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(PremiumAction action, PremiumRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<PremiumDefinitionsResponse>> GetDefinitions(PremiumRequest request)
            => SendRequest<PremiumDefinitionsResponse>(PremiumAction.GetDefinitions, request);

        public static Task<OperationResult<PremiumStateResponse>> GetUserState(PremiumRequest request)
            => SendRequest<PremiumStateResponse>(PremiumAction.GetUserState, request);

        public static Task<OperationResult<PremiumPurchaseResponse>> ActivateTrial(PremiumRequest request)
            => SendRequest<PremiumPurchaseResponse>(PremiumAction.ActivateTrial, request);

        public static Task<OperationResult<PremiumPurchaseResponse>> PurchaseItemOrCurrency(PremiumRequest request)
            => SendRequest<PremiumPurchaseResponse>(PremiumAction.PurchaseItemOrCurrency, request);

        public static Task<OperationResult<PremiumPurchaseResponse>> PurchaseRealMoney(PremiumRequest request)
            => SendRequest<PremiumPurchaseResponse>(PremiumAction.PurchaseRealMoney, request);
    }
}
