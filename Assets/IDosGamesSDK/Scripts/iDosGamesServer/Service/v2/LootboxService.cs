using System;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class LootboxService
    {
        public static event Action<LootboxDefinitionsResponse> OnDefinitionsReceived;
        public static event Action<LootboxOpenResponse> OnLootboxOpened;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static LootboxRequest CreateBaseRequest()
        {
            return new LootboxRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink
            };
        }

        /// <summary>
        /// Get a list of available loot boxes
        /// </summary>
        public static async Task<OperationResult<LootboxDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();

            var result = await LootboxAPI.GetDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.ApplyLootboxDefinitions(result.Data.LootboxDefinitions);
                OnDefinitionsReceived?.Invoke(result.Data);
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
            var request = CreateBaseRequest();

            request.LootboxID = lootboxId;
            request.SelectedOptionID = selectedOptionId;
            request.Count = count;

            var result = await LootboxAPI.Open(request);

            if (result.Success)
            {
                var definition = IDosGamesData.Config.TitlePublicConfiguration.LootboxDefinitions ?.Find(l => l.LootboxID == lootboxId);
                var option = definition?.PriceOptions?.Find(o => o.OptionID == selectedOptionId);
                if (option?.RequiredResources != null)
                {
                    var toConsume = option.RequiredResources.Select(r => new ItemOrCurrency
                    {
                        Type = r.Type,
                        CurrencyID = r.CurrencyID,
                        ItemID = r.ItemID,
                        Catalog = r.Catalog,
                        Amount = r.Amount * count
                    }).ToList();
                    IDosGamesData.User.ConsumeResources(toConsume);
                }

                var allGranted = result.Data.Results?.SelectMany(r => r).ToList();
                IDosGamesData.User.GrantResources(allGranted);

                OnLootboxOpened?.Invoke(result.Data);
            }

            return result;
        }
    }
}
