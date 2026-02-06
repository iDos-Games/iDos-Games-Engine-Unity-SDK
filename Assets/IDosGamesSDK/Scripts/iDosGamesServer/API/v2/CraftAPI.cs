using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class CraftAPI
    {
        private static string GetEndpoint(CraftAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Craft/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(CraftAction action, CraftRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<CraftDefinitionsResponse>> GetDefinitions(CraftRequest request)
        {
            return await SendRequest<CraftDefinitionsResponse>(CraftAction.GetDefinitions, request);
        }

        public static async Task<OperationResult<CraftResponse>> Craft(CraftRequest request)
        {
            return await SendRequest<CraftResponse>(CraftAction.Craft, request);
        }
    }
}
