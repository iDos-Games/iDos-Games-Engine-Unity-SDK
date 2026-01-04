using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class CraftService
    {
        public static event Action<TradeUpCollectionResponse> OnTradeUpCollectionSuccess;
        public static event Action<TradeUpRarityResponse> OnTradeUpRaritySuccess;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static CraftRequest CreateBaseRequest()
        {
            return new CraftRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket
            };
        }

        /// <summary>
        /// Gets a list of available crafting recipes.
        /// </summary>
        public static async Task<OperationResult<CraftDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            return await CraftAPI.GetDefinitions(request);
        }

        /// <summary>
        /// Performs crafting of the "Collection" type (Trade Up).
        /// </summary>
        /// <param name="craftID">Crafting recipe ID</param>
        /// <param name="inputItemIDs">List of ItemIDs of items in inventory (10 items * count)</param>
        /// <param name="count">Number of crafts at a time</param>
        /// <param name="optionID">Price option ID (usually 0)</param>
        public static async Task<OperationResult<TradeUpCollectionResponse>> TradeUpCollection(
            string craftID,
            List<string> inputItemIDs,
            int count = 1,
            int optionID = 0)
        {
            var request = CreateBaseRequest();
            request.CraftID = craftID;
            request.InputItemIDs = inputItemIDs;
            request.Count = count;
            request.SelectedOptionID = optionID;

            var result = await CraftAPI.TradeUpCollection(request);

            if (result.Success)
            {
                OnTradeUpCollectionSuccess?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Performs crafting of the "Rarity" type.
        /// </summary>
        public static async Task<OperationResult<TradeUpRarityResponse>> TradeUpRarity(
            string craftID,
            List<string> inputItemIDs,
            int count = 1,
            int optionID = 0)
        {
            var request = CreateBaseRequest();
            request.CraftID = craftID;
            request.InputItemIDs = inputItemIDs;
            request.Count = count;
            request.SelectedOptionID = optionID;

            var result = await CraftAPI.TradeUpRarity(request);

            if (result.Success)
            {
                OnTradeUpRaritySuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
