using System.Threading.Tasks;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class LootboxAPI
    {
        private static string GetEndpoint(LootboxAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Lootbox/{action}/{userID}";
        }

        /// <summary>
        /// Request to get the configuration of all loot boxes (prices, contents)
        /// </summary>
        public static async Task<OperationResult<LootboxDefinitionsResponse>> GetDefinitions(LootboxRequest request)
        {
            return await HttpService.Post<LootboxDefinitionsResponse>(
                GetEndpoint(LootboxAction.GetDefinitions, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        /// <summary>
        /// Request to open a loot box
        /// </summary>
        public static async Task<OperationResult<LootboxOpenResponse>> Open(LootboxRequest request)
        {
            return await HttpService.Post<LootboxOpenResponse>(
                GetEndpoint(LootboxAction.Open, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }
    }
}
