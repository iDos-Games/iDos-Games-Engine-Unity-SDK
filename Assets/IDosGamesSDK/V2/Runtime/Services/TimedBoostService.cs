using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class TimedBoostService
    {
        // ---------- Events ----------

        /// <summary>Fired after boost definitions are loaded and patched into TitleConfig.</summary>
        public static event Action<TimedBoostDefinitions> OnTimedBoostDefinitionsLoaded;

        /// <summary>Fired after the active boosts snapshot is loaded and patched into UserData.</summary>
        public static event Action<GetActiveTimedBoostsResponse> OnActiveTimedBoostsLoaded;

        /// <summary>Fired after a boost is successfully activated and UserData is patched.</summary>
        public static event Action<ActivateTimedBoostResponse> OnTimedBoostActivated;

        /// <summary>Fired after expired boosts are cleaned up server-side.</summary>
        public static event Action OnExpiredTimedBoostsCleaned;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static TimedBoostRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Loads all TimedBoost definitions from the server and patches them into TitleConfig.
        /// Fires <see cref="OnTimedBoostDefinitionsLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<TimedBoostDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await TimedBoostAPI.GetDefinitions(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchTimedBoost(result.Data);
                OnTimedBoostDefinitionsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Loads the player's live active boosts and patches them into UserData.
        /// Fires <see cref="OnActiveTimedBoostsLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<GetActiveTimedBoostsResponse>> GetActive()
        {
            var request = CreateBaseRequest();
            var result = await TimedBoostAPI.GetActive(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyTimedBoost(result.Data.Active != null
                    ? new UserTimedBoostsState { Active = result.Data.Active }
                    : new UserTimedBoostsState());

                OnActiveTimedBoostsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Activates a boost by ID: deducts the activation cost and applies the stacking policy.
        /// Patches UserData (active boosts + resources) on success.
        /// Fires <see cref="OnTimedBoostActivated"/>.
        /// Note: <see cref="ActivateTimedBoostResponse.ActivatedBoost"/> may be null for
        /// <see cref="TimedBoostStackingPolicy.KeepBest"/> when the existing boost is not worse.
        /// </summary>
        public static async Task<OperationResult<ActivateTimedBoostResponse>> Activate(string boostID)
        {
            if (string.IsNullOrWhiteSpace(boostID))
            {
                Debug.LogWarning("[TimedBoostService] Activate: BoostID is required.");
                return OperationResult<ActivateTimedBoostResponse>.Fail("BoostID is required.");
            }

            string trimmed = boostID.Trim();
            if (trimmed.Contains('.') || trimmed.Contains('$'))
            {
                Debug.LogWarning($"[TimedBoostService] Activate: BoostID '{trimmed}' contains invalid characters ('.' or '$').");
                return OperationResult<ActivateTimedBoostResponse>.Fail(
                    $"BoostID '{trimmed}' contains invalid characters ('.' or '$').");
            }

            var request = CreateBaseRequest();
            request.BoostID = trimmed;
            request.RelatedEntityID = $"timed_boost_activate_{trimmed}_{Guid.NewGuid():N}";

            var result = await TimedBoostAPI.Activate(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                // 1. Patch active boosts: upsert or remove the instance depending on policy outcome.
                if (data.ActivatedBoost != null)
                {
                    IDosGamesData.User.PatchActiveTimedBoost(data.ActivatedBoost, data.StackingPolicy);
                }

                // 2. Apply resource changes (activation cost consumed).
                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(
                        data.Resources,
                        IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnTimedBoostActivated?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Requests the server to remove expired or charge-exhausted boost instances.
        /// Reloads active boosts into UserData on success.
        /// Fires <see cref="OnExpiredTimedBoostsCleaned"/>.
        /// </summary>
        public static async Task<OperationResult<SuccessResponse>> CleanupExpired()
        {
            var request = CreateBaseRequest();
            var result = await TimedBoostAPI.CleanupExpired(request);

            if (result.Success)
            {
                // Reload the authoritative active-boosts snapshot after cleanup.
                await GetActive();
                OnExpiredTimedBoostsCleaned?.Invoke();
            }

            return result;
        }
    }
}
