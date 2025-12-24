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
        private static string UserId => Ctx.UserID;
        private static string Token => Ctx.ClientSessionTicket;

        /// <summary>
        /// Get a list of available loot boxes
        /// </summary>
        public static async Task<OperationResult<LootboxDefinitionsResponse>> GetDefinitions()
        {
            var result = await LootboxAPI.GetDefinitions(UserId, Token);

            if (result.Success)
            {
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
            var result = await LootboxAPI.Open(UserId, Token, lootboxId, selectedOptionId, count);

            if (result.Success)
            {
                OnLootboxOpened?.Invoke(result.Data);
            }

            return result;
        }
    }
}
