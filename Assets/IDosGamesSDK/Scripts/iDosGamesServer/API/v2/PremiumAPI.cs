using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class PremiumAPI
    {
        private static string GetEndpoint(PremiumAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Premium/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(PremiumAction action, PremiumRequest request)
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

        public static async Task<OperationResult<PremiumDefinitionsResponse>> GetDefinitions(PremiumRequest request)
        {
            return await SendRequest<PremiumDefinitionsResponse>(PremiumAction.GetDefinitions, request);
        }

        public static async Task<OperationResult<PremiumStateResponse>> GetUserState(PremiumRequest request)
        {
            return await SendRequest<PremiumStateResponse>(PremiumAction.GetUserState, request);
        }

        public static async Task<OperationResult<PremiumPurchaseResponse>> ActivateTrial(PremiumRequest request)
        {
            return await SendRequest<PremiumPurchaseResponse>(PremiumAction.ActivateTrial, request);
        }

        public static async Task<OperationResult<PremiumPurchaseResponse>> PurchaseItemOrCurrency(PremiumRequest request)
        {
            return await SendRequest<PremiumPurchaseResponse>(PremiumAction.PurchaseItemOrCurrency, request);
        }
    }
}
