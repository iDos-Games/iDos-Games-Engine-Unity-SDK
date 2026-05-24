using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class ItemService
    {
        // ---------- Typed events ----------
        public static event Action<UpgradeItemLevelResponse> OnItemLevelUpgraded;

        // ---------- Helpers ----------
        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static ItemRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Upgrade an unstackable item instance level by +1. On success applies the consumed
        /// resources via <c>ApplyResourceOperation</c>, patches the instance Level in the local
        /// inventory, and fires <see cref="OnItemLevelUpgraded"/>.
        /// </summary>
        public static async Task<OperationResult<UpgradeItemLevelResponse>> UpgradeLevel(string itemInstanceID)
        {
            var trimmed = (itemInstanceID ?? "").Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                Debug.LogWarning("[ItemService] UpgradeLevel: ItemInstanceID is required.");
                return OperationResult<UpgradeItemLevelResponse>.Fail("ItemInstanceID is required.");
            }
            if (trimmed.Contains('.') || trimmed.Contains('$'))
            {
                Debug.LogWarning($"[ItemService] UpgradeLevel: ItemInstanceID '{trimmed}' contains invalid characters ('.' or '$').");
                return OperationResult<UpgradeItemLevelResponse>.Fail($"ItemInstanceID '{trimmed}' contains invalid characters ('.' or '$').");
            }

            var request = CreateBaseRequest();
            request.ItemInstanceID = trimmed;
            request.RelatedEntityID = $"upgrade_item_{trimmed}_{Guid.NewGuid():N}";

            var result = await ItemAPI.UpgradeLevel(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                // 1. Domain patch: new Level on the instance.
                IDosGamesData.User.PatchUnstackableItemLevel(data.ItemInstanceID, data.Level);

                // 2. Apply consumed resources.
                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                // 3. Event.
                OnItemLevelUpgraded?.Invoke(data);
            }
            return result;
        }
    }
}
