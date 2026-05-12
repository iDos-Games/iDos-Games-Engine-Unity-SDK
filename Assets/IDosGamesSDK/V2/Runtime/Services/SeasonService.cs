using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class SeasonService
    {
        // ---------- Events ----------

        /// <summary>Fired after SeasonDefinitions are fetched and patched into TitleConfig.</summary>
        public static event Action<SeasonDefinitions> OnSeasonDefinitionsLoaded;

        /// <summary>Fired after active season info is fetched; UserSeasonState is patched locally.</summary>
        public static event Action<ActiveSeasonInfo> OnActiveSeasonLoaded;

        /// <summary>Fired after UserSeasonState is fetched and patched locally.</summary>
        public static event Action<UserSeasonState> OnSeasonUserStateLoaded;

        /// <summary>Fired after status tokens are granted; CurrentTier is patched locally.</summary>
        public static event Action<GrantStatusTokensResponse> OnStatusTokensGranted;

        /// <summary>Fired after a tier reward is claimed; resources applied and ClaimedTierRewards patched.</summary>
        public static event Action<ClaimTierRewardResponse> OnTierRewardClaimed;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static SeasonRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Fetches the full season chain config. Patches TitleConfig.Season.
        /// Fires OnSeasonDefinitionsLoaded on success.
        /// </summary>
        public static async Task<OperationResult<SeasonDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await SeasonAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchSeason(result.Data);
                OnSeasonDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches info about the currently active season in the given chain.
        /// Patches UserSeasonState for that chain locally. Fires OnActiveSeasonLoaded.
        /// </summary>
        public static async Task<OperationResult<ActiveSeasonInfo>> GetActiveSeason(string seasonChainID)
        {
            if (string.IsNullOrWhiteSpace(seasonChainID))
            {
                Debug.LogWarning("[SeasonService] GetActiveSeason: SeasonChainID is required.");
                return OperationResult<ActiveSeasonInfo>.Fail("SeasonChainID is required.");
            }

            var request = CreateBaseRequest();
            request.SeasonChainID = seasonChainID;

            var result = await SeasonAPI.GetActiveSeason(request);

            if (result.Success && result.Data != null)
            {
                if (result.Data.UserState != null)
                    IDosGamesData.User.PatchSeasonState(seasonChainID, result.Data.UserState);

                OnActiveSeasonLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the player's UserSeasonState for the given chain.
        /// Patches local state. Fires OnSeasonUserStateLoaded.
        /// </summary>
        public static async Task<OperationResult<UserSeasonState>> GetUserState(string seasonChainID)
        {
            if (string.IsNullOrWhiteSpace(seasonChainID))
            {
                Debug.LogWarning("[SeasonService] GetUserState: SeasonChainID is required.");
                return OperationResult<UserSeasonState>.Fail("SeasonChainID is required.");
            }

            var request = CreateBaseRequest();
            request.SeasonChainID = seasonChainID;

            var result = await SeasonAPI.GetUserState(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchSeasonState(seasonChainID, result.Data);
                OnSeasonUserStateLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Grants status tokens to the player in the given chain.
        /// Accessible from client only when the chain's GrantTokensAccessMode allows it.
        /// Patches CurrentTier locally if it changed. Fires OnStatusTokensGranted.
        /// </summary>
        public static async Task<OperationResult<GrantStatusTokensResponse>> GrantStatusTokens(string seasonChainID, long amount)
        {
            if (string.IsNullOrWhiteSpace(seasonChainID))
            {
                Debug.LogWarning("[SeasonService] GrantStatusTokens: SeasonChainID is required.");
                return OperationResult<GrantStatusTokensResponse>.Fail("SeasonChainID is required.");
            }

            if (amount <= 0)
            {
                Debug.LogWarning("[SeasonService] GrantStatusTokens: Amount must be > 0.");
                return OperationResult<GrantStatusTokensResponse>.Fail("Amount must be > 0.");
            }

            var request = CreateBaseRequest();
            request.SeasonChainID = seasonChainID;
            request.Amount = amount;
            request.RelatedEntityID = $"season_grant_{seasonChainID}_{Guid.NewGuid():N}";

            var result = await SeasonAPI.GrantStatusTokens(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;
                IDosGamesData.User.PatchSeasonCurrentTier(seasonChainID, data.NewTier);
                OnStatusTokensGranted?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Claims the one-time reward for reaching a tier in the given season chain.
        /// Applies the ResourceOperation locally and patches ClaimedTierRewards.
        /// Fires OnTierRewardClaimed.
        /// </summary>
        public static async Task<OperationResult<ClaimTierRewardResponse>> ClaimTierReward(string seasonChainID, int tierNumber)
        {
            if (string.IsNullOrWhiteSpace(seasonChainID))
            {
                Debug.LogWarning("[SeasonService] ClaimTierReward: SeasonChainID is required.");
                return OperationResult<ClaimTierRewardResponse>.Fail("SeasonChainID is required.");
            }

            if (tierNumber <= 0)
            {
                Debug.LogWarning("[SeasonService] ClaimTierReward: TierNumber must be > 0.");
                return OperationResult<ClaimTierRewardResponse>.Fail("TierNumber must be > 0.");
            }

            var request = CreateBaseRequest();
            request.SeasonChainID = seasonChainID;
            request.TierNumber = tierNumber;
            request.RelatedEntityID = $"season_tier_{seasonChainID}_t{tierNumber}_{Guid.NewGuid():N}";

            var result = await SeasonAPI.ClaimTierReward(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchSeasonClaimedTier(seasonChainID, tierNumber);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnTierRewardClaimed?.Invoke(data);
            }

            return result;
        }
    }
}
