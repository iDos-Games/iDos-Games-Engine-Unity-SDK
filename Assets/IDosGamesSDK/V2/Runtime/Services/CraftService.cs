using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class CraftService
    {
        // ── Events ──────────────────────────────────────────────────────────

        /// <summary>Fired after craft definitions are successfully loaded and cached.</summary>
        public static event Action<CraftDefinitions> OnCraftDefinitionsLoaded;

        /// <summary>Fired after a craft is confirmed by the server and inventory is updated locally.</summary>
        public static event Action<CraftResponse> OnCraftCompleted;

        // ── Helpers ─────────────────────────────────────────────────────────

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static CraftRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ── Actions ─────────────────────────────────────────────────────────

        /// <summary>
        /// Fetches craft definitions from the server and caches them in TitleConfig.
        /// Fires <see cref="OnCraftDefinitionsLoaded"/> on success.
        /// </summary>
        public static async Task<OperationResult<CraftDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await CraftAPI.GetDefinitions(request);

            if (result.Success && result.Data?.CraftDefinitions != null)
            {
                IDosGamesData.Config.PatchCraft(result.Data.CraftDefinitions);
                OnCraftDefinitionsLoaded?.Invoke(result.Data.CraftDefinitions);
            }

            return result;
        }

        /// <summary>
        /// Executes a craft recipe. Validates inputs client-side before sending.
        /// On success, applies resource changes to local inventory and fires <see cref="OnCraftCompleted"/>.
        /// </summary>
        /// <param name="craftID">Recipe ID from <see cref="CraftDefinitions"/>.</param>
        /// <param name="inputItemIDs">
        /// Template of item instance IDs to burn per craft (exactly RequiredItemCount entries).
        /// </param>
        /// <param name="count">Number of crafts to perform in one call (1–20).</param>
        /// <param name="selectedOptionID">
        /// Price option ID to use. Null/empty = server picks the first available option.
        /// </param>
        public static async Task<OperationResult<CraftResponse>> Craft(
            string craftID,
            List<string> inputItemIDs,
            int count = 1,
            string selectedOptionID = null)
        {
            if (string.IsNullOrWhiteSpace(craftID))
            {
                Debug.LogWarning("[CraftService] Craft: CraftID is required.");
                return OperationResult<CraftResponse>.Fail("CraftID is required.");
            }

            if (inputItemIDs == null || inputItemIDs.Count == 0)
            {
                Debug.LogWarning("[CraftService] Craft: InputItemIDs must not be empty.");
                return OperationResult<CraftResponse>.Fail("InputItemIDs must not be empty.");
            }

            if (count < 1)
            {
                Debug.LogWarning("[CraftService] Craft: Count must be at least 1.");
                return OperationResult<CraftResponse>.Fail("Count must be at least 1.");
            }

            var request = CreateBaseRequest();
            request.RelatedEntityID = $"craft_{craftID}_{Ctx.UserID}_{Guid.NewGuid():N}";
            request.CraftID = craftID;
            request.InputItemIDs = inputItemIDs;
            request.Count = count;
            request.SelectedOptionID = selectedOptionID;

            var result = await CraftAPI.Craft(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(
                        data.Resources,
                        IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnCraftCompleted?.Invoke(data);
            }

            return result;
        }
    }
}
