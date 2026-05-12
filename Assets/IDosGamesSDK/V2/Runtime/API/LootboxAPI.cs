using System.Threading.Tasks;

namespace IDosGames
{
    public static class LootboxAPI
    {
        private static string GetEndpoint(LootboxAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Lootbox/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(LootboxAction action, LootboxRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<LootboxDefinitionsResponse>> GetDefinitions(LootboxRequest request)
            => SendRequest<LootboxDefinitionsResponse>(LootboxAction.GetDefinitions, request);

        public static Task<OperationResult<LootboxOpenResponse>> Open(LootboxRequest request)
            => SendRequest<LootboxOpenResponse>(LootboxAction.Open, request);
    }
}
