using System;
using System.Threading.Tasks;

namespace IDosGames
{
    public static class LootboxService
    {
        // ---------- Typed events ----------

        /// <summary>Fired after lootbox definitions are fetched and cached locally.</summary>
        public static event Action<LootboxDefinitions> OnLootboxDefinitionsLoaded;

        /// <summary>Fired after a lootbox is successfully opened and local state is patched.</summary>
        public static event Action<LootboxOpenResponse> OnLootboxOpened;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static LootboxRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // =====================================================================
        // GetDefinitions
        // =====================================================================

        /// <summary>
        /// Fetches all lootbox definitions from the server and caches them in TitleConfig.
        /// Fires OnLootboxDefinitionsLoaded on success.
        /// </summary>
        public static async Task<OperationResult<LootboxDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();

            var result = await LootboxAPI.GetDefinitions(request);

            if (result.Success && result.Data?.LootboxDefinitions != null)
            {
                IDosGamesData.Config.PatchLootbox(result.Data.LootboxDefinitions);
                OnLootboxDefinitionsLoaded?.Invoke(result.Data.LootboxDefinitions);
            }

            return result;
        }

        // =====================================================================
        // Open
        // =====================================================================

        /// <summary>
        /// Opens one or more lootboxes atomically.
        /// Applies earned/spent resources via ApplyResourceOperation and patches pity counters locally.
        /// Fires OnLootboxOpened on success.
        /// </summary>
        /// <param name="lootboxID">ID of the lootbox to open.</param>
        /// <param name="count">Number of boxes to open (1–100).</param>
        /// <param name="selectedOptionID">Price option to use for payment.</param>
        public static async Task<OperationResult<LootboxOpenResponse>> Open(string lootboxID, int count, int selectedOptionID)
        {
            if (string.IsNullOrWhiteSpace(lootboxID))
            {
                UnityEngine.Debug.LogWarning("[LootboxService] Open: LootboxID is required.");
                return OperationResult<LootboxOpenResponse>.Fail("LootboxID is required.");
            }

            if (count <= 0)
            {
                UnityEngine.Debug.LogWarning("[LootboxService] Open: Count must be greater than 0.");
                return OperationResult<LootboxOpenResponse>.Fail("Count must be greater than 0.");
            }

            var request = CreateBaseRequest();
            request.LootboxID = lootboxID;
            request.Count = count;
            request.SelectedOptionID = selectedOptionID;
            request.RelatedEntityID = $"lootbox_{lootboxID}_{selectedOptionID}_{Guid.NewGuid():N}";

            var result = await LootboxAPI.Open(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                // 1. Apply pity state returned from server.
                if (data.TriggeredPity != null && data.TriggeredPity.Count > 0)
                {
                    IDosGamesData.User.ApplyLootboxPityTriggers(lootboxID, data.TriggeredPity, count);
                }

                // 2. Standard resource operation — always via ApplyResourceOperation.
                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnLootboxOpened?.Invoke(data);
            }

            return result;
        }
    }
}
