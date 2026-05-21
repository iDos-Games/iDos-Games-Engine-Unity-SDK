using System.Threading.Tasks;

namespace IDosGames
{
    public static class ItemAPI
    {
        private static string GetEndpoint(ItemAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Item/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(ItemAction action, ItemRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<UpgradeItemLevelResponse>> UpgradeLevel(ItemRequest request)
            => SendRequest<UpgradeItemLevelResponse>(ItemAction.UpgradeLevel, request);
    }
}
