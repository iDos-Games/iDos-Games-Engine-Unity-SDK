using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using UnityEngine;

namespace IDosGames
{
    public static class LimitedTimeEventService
    {
        public static event System.Action<GetActiveEventsResponse> OnActiveEventsUpdated;
        public static event System.Action<LimitedTimeEventsDefinition> OnDefinitionsUpdated;
        public static event System.Action<UserLimitedTimeEventsState> OnUserStateUpdated;
        public static event System.Action<EventTokenGrantInfo> OnTokensGranted;
        public static event System.Action<EventTokenSpendResponse> OnTokensSpent;
        public static event System.Action<EventMilestoneClaimResponse> OnMilestoneClaimed;
        public static event System.Action<EventStreakClaimResponse> OnStreakRewardClaimed;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();

        private static LimitedTimeEventRequest CreateBaseRequest()
        {
            return new LimitedTimeEventRequest
            {
                UserID = Ctx.UserID,
                ClientSessionTicket = Ctx.ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        // =================================================================================
        // PUBLIC METHODS
        // =================================================================================

        /// <summary>
        /// Fetches all currently active events and the player's progress for each.
        /// </summary>
        public static async Task<OperationResult<GetActiveEventsResponse>> GetActiveEvents()
        {
            var request = CreateBaseRequest();
            var result = await LimitedTimeEventAPI.GetActiveEvents(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyLimitedTimeEventsFromActive(result.Data);
                OnActiveEventsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the full LimitedTimeEvents config (definitions).
        /// </summary>
        public static async Task<OperationResult<LimitedTimeEventsDefinition>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await LimitedTimeEventAPI.GetDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchLimitedTimeEventsDefinition(result.Data);
                OnDefinitionsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the raw UserLimitedTimeEventsState for the local player.
        /// </summary>
        public static async Task<OperationResult<UserLimitedTimeEventsState>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await LimitedTimeEventAPI.GetUserState(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyLimitedTimeEvents(result.Data);
                OnUserStateUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Grants event tokens from a client-allowed source.
        /// </summary>
        /// <param name="sourceType">Token source (must not be a server-only source).</param>
        /// <param name="eventId">Scheduled event ID (pass null when using a chain).</param>
        /// <param name="chainId">Chain event ID (pass null when using a scheduled event).</param>
        /// <param name="sourceParams">Optional params, e.g. {"ActionName":"watch_ad"} for CustomAction.</param>
        /// <param name="amountOverride">Override base amount; null = use config value.</param>
        public static async Task<OperationResult<EventTokenGrantInfo>> GrantTokens(
            EventTokenSourceType sourceType,
            string eventId = null,
            string chainId = null,
            Dictionary<string, string> sourceParams = null,
            long? amountOverride = null)
        {
            if (string.IsNullOrWhiteSpace(eventId) && string.IsNullOrWhiteSpace(chainId))
            {
                Debug.LogWarning("[LimitedTimeEventService] GrantTokens: EventID or EventChainID is required.");
                return OperationResult<EventTokenGrantInfo>.Fail("EventID or EventChainID is required.");
            }

            var request = CreateBaseRequest();
            request.EventID = eventId;
            request.EventChainID = chainId;
            request.SourceType = sourceType.ToString();
            request.SourceParams = sourceParams;
            request.AmountOverride = amountOverride;

            var result = await LimitedTimeEventAPI.GrantTokens(request);

            if (result.Success)
            {
                var targetId = result.Data.EventID;
                var chainID = result.Data.EventChainID;

                if (!string.IsNullOrEmpty(chainID))
                {
                    IDosGamesData.User.PatchEventChainTokenBalance(chainID, result.Data.Amount);
                }
                else if (!string.IsNullOrEmpty(targetId))
                {
                    IDosGamesData.User.PatchScheduledEventTokenBalance(targetId, result.Data.Amount);
                }

                OnTokensGranted?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Spends event tokens (e.g. in an event shop).
        /// </summary>
        /// <param name="spendAmount">Amount to spend (must be > 0).</param>
        /// <param name="eventId">Scheduled event ID.</param>
        /// <param name="chainId">Chain event ID.</param>
        public static async Task<OperationResult<EventTokenSpendResponse>> SpendTokens(
            long spendAmount,
            string eventId = null,
            string chainId = null)
        {
            if (spendAmount <= 0)
            {
                Debug.LogWarning("[LimitedTimeEventService] SpendTokens: SpendAmount must be > 0.");
                return OperationResult<EventTokenSpendResponse>.Fail("SpendAmount must be > 0.");
            }

            if (string.IsNullOrWhiteSpace(eventId) && string.IsNullOrWhiteSpace(chainId))
            {
                Debug.LogWarning("[LimitedTimeEventService] SpendTokens: EventID or EventChainID is required.");
                return OperationResult<EventTokenSpendResponse>.Fail("EventID or EventChainID is required.");
            }

            var request = CreateBaseRequest();
            request.EventID = eventId;
            request.EventChainID = chainId;
            request.SpendAmount = spendAmount;

            var result = await LimitedTimeEventAPI.SpendTokens(request);

            if (result.Success)
            {
                if (!string.IsNullOrEmpty(result.Data.EventChainID))
                {
                    IDosGamesData.User.PatchEventChainTokenBalanceDirect(
                        result.Data.EventChainID, result.Data.NewBalance);
                }
                else if (!string.IsNullOrEmpty(result.Data.EventID))
                {
                    IDosGamesData.User.PatchScheduledEventTokenBalanceDirect(
                        result.Data.EventID, result.Data.NewBalance);
                }

                OnTokensSpent?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Claims a milestone reward for the given event.
        /// </summary>
        /// <param name="milestoneId">Milestone ID to claim.</param>
        /// <param name="eventId">Scheduled event ID.</param>
        /// <param name="chainId">Chain event ID.</param>
        public static async Task<OperationResult<EventMilestoneClaimResponse>> ClaimMilestone(
            string milestoneId,
            string eventId = null,
            string chainId = null)
        {
            if (string.IsNullOrWhiteSpace(milestoneId))
            {
                Debug.LogWarning("[LimitedTimeEventService] ClaimMilestone: MilestoneID is required.");
                return OperationResult<EventMilestoneClaimResponse>.Fail("MilestoneID is required.");
            }

            if (string.IsNullOrWhiteSpace(eventId) && string.IsNullOrWhiteSpace(chainId))
            {
                Debug.LogWarning("[LimitedTimeEventService] ClaimMilestone: EventID or EventChainID is required.");
                return OperationResult<EventMilestoneClaimResponse>.Fail("EventID or EventChainID is required.");
            }

            var request = CreateBaseRequest();
            request.EventID = eventId;
            request.EventChainID = chainId;
            request.MilestoneID = milestoneId;

            var result = await LimitedTimeEventAPI.ClaimMilestone(request);

            if (result.Success)
            {
                // Mark milestone as claimed in local state
                if (!string.IsNullOrEmpty(result.Data.EventChainID))
                {
                    IDosGamesData.User.PatchEventChainClaimedMilestone(
                        result.Data.EventChainID, result.Data.MilestoneID);
                }
                else if (!string.IsNullOrEmpty(result.Data.EventID))
                {
                    IDosGamesData.User.PatchScheduledEventClaimedMilestone(
                        result.Data.EventID, result.Data.MilestoneID);
                }

                // Grant resources to local inventory/currency
                var all = new List<ItemOrCurrency>();
                if (result.Data.StandardRewards != null) all.AddRange(result.Data.StandardRewards);
                if (result.Data.VipRewards != null) all.AddRange(result.Data.VipRewards);
                IDosGamesData.User.GrantResources(all);

                OnMilestoneClaimed?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Claims a streak reward for the given event and streak day.
        /// </summary>
        /// <param name="streakDay">Streak day to claim (must be > 0).</param>
        /// <param name="eventId">Scheduled event ID.</param>
        /// <param name="chainId">Chain event ID.</param>
        public static async Task<OperationResult<EventStreakClaimResponse>> ClaimStreakReward(
            int streakDay,
            string eventId = null,
            string chainId = null)
        {
            if (streakDay <= 0)
            {
                Debug.LogWarning("[LimitedTimeEventService] ClaimStreakReward: StreakDay must be > 0.");
                return OperationResult<EventStreakClaimResponse>.Fail("StreakDay must be > 0.");
            }

            if (string.IsNullOrWhiteSpace(eventId) && string.IsNullOrWhiteSpace(chainId))
            {
                Debug.LogWarning("[LimitedTimeEventService] ClaimStreakReward: EventID or EventChainID is required.");
                return OperationResult<EventStreakClaimResponse>.Fail("EventID or EventChainID is required.");
            }

            var request = CreateBaseRequest();
            request.EventID = eventId;
            request.EventChainID = chainId;
            request.StreakDay = streakDay;

            var result = await LimitedTimeEventAPI.ClaimStreakReward(request);

            if (result.Success)
            {
                // Mark streak day as claimed in local state
                if (!string.IsNullOrEmpty(result.Data.EventChainID))
                {
                    IDosGamesData.User.PatchEventChainClaimedStreakDay(
                        result.Data.EventChainID, result.Data.StreakDay);
                }
                else if (!string.IsNullOrEmpty(result.Data.EventID))
                {
                    IDosGamesData.User.PatchScheduledEventClaimedStreakDay(
                        result.Data.EventID, result.Data.StreakDay);
                }

                // Grant resources to local inventory/currency
                var all = new List<ItemOrCurrency>();
                if (result.Data.StandardRewards != null) all.AddRange(result.Data.StandardRewards);
                if (result.Data.VipRewards != null) all.AddRange(result.Data.VipRewards);
                IDosGamesData.User.GrantResources(all);

                // BonusTokens are already applied server-side; local state will reflect
                // after the next GetActiveEvents / GetUserState call.
                // ASSUMPTION: UI should call GetActiveEvents after ClaimStreakReward to
                // refresh TokenBalance if BonusTokens > 0.

                OnStreakRewardClaimed?.Invoke(result.Data);
            }

            return result;
        }
    }
}
