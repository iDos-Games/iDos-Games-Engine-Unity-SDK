using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class CraftService
    {
        public static event Action<CraftResponse> OnCraftSuccess;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static CraftRequest CreateBaseRequest()
        {
            return new CraftRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
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

        public static async Task<OperationResult<CraftResponse>> Craft(string craftID, List<string> inputItemIDs, int count = 1, int optionID = 0)
        {
            var request = CreateBaseRequest();
            request.CraftID = craftID;
            request.InputItemIDs = inputItemIDs;
            request.Count = count;
            request.SelectedOptionID = optionID;

            var result = await CraftAPI.Craft(request);

            if (result.Success)
            {
                OnCraftSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
