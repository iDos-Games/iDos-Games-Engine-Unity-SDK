using System.Threading.Tasks;

namespace IDosGames
{
    public static class CraftAPI
    {
        private static string GetEndpoint(CraftAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Craft/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(CraftAction action, CraftRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<CraftDefinitionsResponse>> GetDefinitions(CraftRequest request)
            => SendRequest<CraftDefinitionsResponse>(CraftAction.GetDefinitions, request);

        public static Task<OperationResult<CraftResponse>> Craft(CraftRequest request)
            => SendRequest<CraftResponse>(CraftAction.Craft, request);
    }
}
