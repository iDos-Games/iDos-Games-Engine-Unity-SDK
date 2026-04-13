using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class CraftService
    {
        public static event Action<CraftResponse> OnCraftSuccess;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
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
            var result = await CraftAPI.GetDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.ApplyCraftDefinitions(result.Data.CraftDefinitions);
            }

            return result;
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
                // Group the template and multiply by Count
                var toConsume = inputItemIDs
                    .GroupBy(id => id)
                    .Select(g => new ItemOrCurrency
                    {
                        Type = ItemType.Item,
                        ItemID = g.Key,
                        Amount = g.Count() * count  // number of occurrences in pattern × count
                    })
                    .ToList();
                IDosGamesData.User.ConsumeResources(toConsume);

                var outputs = result.Data.Results
                    .Where(r => r.Output != null)
                    .Select(r => r.Output)
                    .ToList();
                IDosGamesData.User.GrantResources(outputs);

                OnCraftSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
