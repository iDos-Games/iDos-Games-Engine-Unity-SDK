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
        public static async Task<OperationResult<LootboxDefinitionsResponse>> GetDefinitions(string userID, string clientSessionTicket)
        {
            var request = new LootboxRequest();

            return await HttpService.Post<LootboxDefinitionsResponse>(
                GetEndpoint(LootboxAction.GetDefinitions, userID),
                request,
                clientSessionTicket
            );
        }

        /// <summary>
        /// Request to open a loot box
        /// </summary>
        public static async Task<OperationResult<LootboxOpenResponse>> Open(string userID, string clientSessionTicket, string lootboxId, int selectedOptionId, int count = 1)
        {
            var request = new LootboxRequest
            {
                LootboxID = lootboxId,
                SelectedOptionID = selectedOptionId,
                Count = count
            };

            return await HttpService.Post<LootboxOpenResponse>(
                GetEndpoint(LootboxAction.Open, userID),
                request,
                clientSessionTicket
            );
        }
    }
}
