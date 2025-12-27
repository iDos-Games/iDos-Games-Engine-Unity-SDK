using System;
using System.Threading.Tasks;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class LootboxService
    {
        public static event Action<LootboxDefinitionsResponse> OnDefinitionsLoaded;
        public static event Action<LootboxOpenResponse> OnLootboxOpened;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        /// <summary>
        /// Get a list of available loot boxes
        /// </summary>
        public static async Task<OperationResult<LootboxDefinitionsResponse>> GetDefinitions()
        {
            var request = new LootboxRequest()
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };

            var result = await LootboxAPI.GetDefinitions(request);
            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Open the loot box
        /// </summary>
        /// <param name="lootboxId">Lootbox ID</param>
        /// <param name="selectedOptionId">Price ID (what currency/item we pay with)</param>
        /// <param name="count">Number of openings</param>
        public static async Task<OperationResult<LootboxOpenResponse>> Open(string lootboxId, int selectedOptionId, int count = 1)
        {
            var request = new LootboxRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                LootboxID = lootboxId,
                SelectedOptionID = selectedOptionId,
                Count = count
            };

            var result = await LootboxAPI.Open(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                OnLootboxOpened?.Invoke(result.Data);
            }

            return result;
        }
    }
}
