using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class TimedEventService
    {
        // ---------- Events ----------

        public static event Action<TimedEventDefinitions> OnTimedEventDefinitionsLoaded;
        public static event Action<GetActiveEventsResponse> OnActiveEventsLoaded;
        public static event Action<UserTimedEventStateResponse> OnUserLteStateLoaded;
        public static event Action<ResourceOperation> OnTokensGranted;
        public static event Action<EventTokenSpendResponse> OnTokensSpent;
        public static event Action<EventMilestoneClaimResponse> OnMilestoneClaimed;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static TimedEventRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Fetches all currently active events with computed time windows and player progress.
        /// Caches result in IDosGamesData and fires OnActiveEventsLoaded.
        /// </summary>
        public static async Task<OperationResult<GetActiveEventsResponse>> GetActiveEvents()
        {
            var request = CreateBaseRequest();
            var result = await TimedEventAPI.GetActiveEvents(request);
            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyActiveEvents(result.Data);
                OnActiveEventsLoaded?.Invoke(result.Data);
            }
            return result;
        }

        /// <summary>
        /// Fetches TimedEvent config (ScheduledEvents, EventChains, global settings).
        /// Patches IDosGamesData.Config and fires OnTimedEventDefinitionsLoaded.
        /// </summary>
        public static async Task<OperationResult<TimedEventDefinitions>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await TimedEventAPI.GetDefinitions(request);
            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchTimedEvent(result.Data);
                OnTimedEventDefinitionsLoaded?.Invoke(result.Data);
            }
            return result;
        }

        /// <summary>
        /// Fetches player's LTE token progress (Scheduled + Chain dictionaries).
        /// Patches IDosGamesData.User and fires OnUserLteStateLoaded.
        /// </summary>
        public static async Task<OperationResult<UserTimedEventStateResponse>> GetUserLteState()
        {
            var request = CreateBaseRequest();
            var result = await TimedEventAPI.GetUserLteState(request);
            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyUserLteState(result.Data);
                OnUserLteStateLoaded?.Invoke(result.Data);
            }
            return result;
        }

        /// <summary>
        /// Grants tokens to the player for a client-allowed source (CustomAction, DailyLogin, ReferralInvite).
        /// Applies returned ResourceOperation via ApplyResourceOperation and fires OnTokensGranted.
        /// </summary>
        public static async Task<OperationResult<ResourceOperation>> GrantTokens(
            TimedEventType type,
            string lteID,
            string sourceType,
            string outcome = null,
            long? amountOverride = null,
            Dictionary<string, string> sourceParams = null,
            int rollMultiplier = 1)
        {
            if (string.IsNullOrWhiteSpace(lteID))
            {
                Debug.LogWarning("[TimedEventService] GrantTokens: LteID is required.");
                return OperationResult<ResourceOperation>.Fail("LteID is required.");
            }
            if (string.IsNullOrWhiteSpace(sourceType))
            {
                Debug.LogWarning("[TimedEventService] GrantTokens: SourceType is required.");
                return OperationResult<ResourceOperation>.Fail("SourceType is required.");
            }

            var request = CreateBaseRequest();
            request.Type = type;
            request.LteID = lteID.Trim();
            request.SourceType = sourceType.Trim();
            request.Outcome = outcome;
            request.AmountOverride = amountOverride;
            request.SourceParams = sourceParams;
            request.RollMultiplier = rollMultiplier;
            request.RelatedEntityID = $"grant_{type}_{lteID}_{sourceType}_{Guid.NewGuid():N}";

            var result = await TimedEventAPI.GrantTokens(request);
            if (result.Success && result.Data != null)
            {
                if (result.Data != null)
                    IDosGamesData.User.ApplyResourceOperation(result.Data, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnTokensGranted?.Invoke(result.Data);
            }
            return result;
        }

        /// <summary>
        /// Spends tokens for the specified event (e.g. event store purchase).
        /// Applies returned ResourceOperation via ApplyResourceOperation and fires OnTokensSpent.
        /// </summary>
        public static async Task<OperationResult<EventTokenSpendResponse>> SpendTokens(
            TimedEventType type,
            string lteID,
            long spendAmount)
        {
            if (string.IsNullOrWhiteSpace(lteID))
            {
                Debug.LogWarning("[TimedEventService] SpendTokens: LteID is required.");
                return OperationResult<EventTokenSpendResponse>.Fail("LteID is required.");
            }
            if (spendAmount <= 0)
            {
                Debug.LogWarning("[TimedEventService] SpendTokens: SpendAmount must be > 0.");
                return OperationResult<EventTokenSpendResponse>.Fail("SpendAmount must be > 0.");
            }

            var request = CreateBaseRequest();
            request.Type = type;
            request.LteID = lteID.Trim();
            request.SpendAmount = spendAmount;
            request.RelatedEntityID = $"spend_{type}_{lteID}_{Guid.NewGuid():N}";

            var result = await TimedEventAPI.SpendTokens(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;
                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnTokensSpent?.Invoke(data);
            }
            return result;
        }

        /// <summary>
        /// Claims a milestone reward for the specified event.
        /// Applies returned ResourceOperation (Rewards) via ApplyResourceOperation and fires OnMilestoneClaimed.
        /// </summary>
        public static async Task<OperationResult<EventMilestoneClaimResponse>> ClaimMilestone(
            TimedEventType type,
            string lteID,
            string milestoneID)
        {
            if (string.IsNullOrWhiteSpace(lteID))
            {
                Debug.LogWarning("[TimedEventService] ClaimMilestone: LteID is required.");
                return OperationResult<EventMilestoneClaimResponse>.Fail("LteID is required.");
            }
            if (string.IsNullOrWhiteSpace(milestoneID))
            {
                Debug.LogWarning("[TimedEventService] ClaimMilestone: MilestoneID is required.");
                return OperationResult<EventMilestoneClaimResponse>.Fail("MilestoneID is required.");
            }

            var request = CreateBaseRequest();
            request.Type = type;
            request.LteID = lteID.Trim();
            request.MilestoneID = milestoneID.Trim();
            // MilestoneID is scoped to a single event; prefix with Type+LteID to avoid cross-event
            // idempotency collisions when two events share the same milestone ID (e.g. "m_100").
            request.RelatedEntityID = $"{type}_{lteID}_{milestoneID}";

            var result = await TimedEventAPI.ClaimMilestone(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;
                if (data.Rewards != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Rewards, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                // Mark milestone as claimed locally so UI updates immediately
                IDosGamesData.User.PatchTimedEventMilestoneClaimed(type, lteID, milestoneID);

                OnMilestoneClaimed?.Invoke(data);
            }
            return result;
        }
    }
}
