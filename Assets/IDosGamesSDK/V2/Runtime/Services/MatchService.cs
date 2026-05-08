using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class MatchService
    {
        // -------------------------------------------------------------------------
        // Events
        // -------------------------------------------------------------------------

        /// <summary>Fired after CreateMatch succeeds. Arg: server response with MatchID and Resources.</summary>
        public static event Action<CreateMatchResponse> OnMatchCreated;

        /// <summary>Fired after InstantBattle succeeds. Arg: full battle response including outcome and resources.</summary>
        public static event Action<InstantBattleResponse> OnInstantBattleCompleted;

        /// <summary>Fired after SaveStrategy succeeds. Arg: the saved strategy list.</summary>
        public static event Action<List<BattleStepConfig>> OnStrategySaved;

        /// <summary>Fired after GetMyMatches succeeds. Arg: paged result.</summary>
        public static event Action<MatchesPageResponse> OnMyMatchesLoaded;

        /// <summary>Fired after GetAvailableMatches succeeds. Arg: paged result.</summary>
        public static event Action<MatchesPageResponse> OnAvailableMatchesLoaded;

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static MatchRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        private static string ResolveCharacterID(string characterID)
            => string.IsNullOrEmpty(characterID) ? DefaultData.Main : characterID;

        // -------------------------------------------------------------------------
        // Actions
        // -------------------------------------------------------------------------

        /// <summary>
        /// Creates a new open PvP match, deducting the entry fee from the creator.
        /// On success: applies resource operation locally and fires OnMatchCreated.
        /// Only RuleID == DefaultData.InstantBattle1v1 is accepted server-side.
        /// </summary>
        public static async Task<OperationResult<CreateMatchResponse>> CreateMatch(
            string currencyID,
            long entryFeeAmount,
            string ruleID,
            string characterID = null,
            List<BattleStepConfig> battleStrategy = null,
            string targetUserID = null)
        {
            if (string.IsNullOrWhiteSpace(currencyID))
            {
                Debug.LogWarning("[MatchService] CreateMatch: CurrencyID is required.");
                return OperationResult<CreateMatchResponse>.Fail("CurrencyID is required.");
            }

            if (entryFeeAmount <= 0)
            {
                Debug.LogWarning("[MatchService] CreateMatch: EntryFeeAmount must be greater than zero.");
                return OperationResult<CreateMatchResponse>.Fail("EntryFeeAmount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(ruleID))
            {
                Debug.LogWarning("[MatchService] CreateMatch: RuleID is required.");
                return OperationResult<CreateMatchResponse>.Fail("RuleID is required.");
            }

            var request = CreateBaseRequest();
            request.CurrencyID = currencyID;
            request.EntryFeeAmount = entryFeeAmount;
            request.RuleID = ruleID;
            request.CharacterID = ResolveCharacterID(characterID);
            request.BattleStrategy = battleStrategy;
            request.TargetUserID = targetUserID;
            // Stable idempotency key — server uses RelatedEntityID to deduplicate CreateMatch within the same second.
            request.RelatedEntityID = $"pvp_create_{Ctx.UserID}_{Guid.NewGuid():N}";

            var result = await MatchAPI.CreateMatch(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnMatchCreated?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Joins an open match and instantly simulates the battle.
        /// On draw: applies creator refund via Resources. On decisive outcome: applies dual-party via ResourcesDual.
        /// Fires OnInstantBattleCompleted with the full result.
        /// </summary>
        public static async Task<OperationResult<InstantBattleResponse>> InstantBattle(
            string matchID,
            string ruleID,
            string characterID = null,
            List<BattleStepConfig> battleStrategy = null)
        {
            if (string.IsNullOrWhiteSpace(matchID))
            {
                Debug.LogWarning("[MatchService] InstantBattle: MatchID is required.");
                return OperationResult<InstantBattleResponse>.Fail("MatchID is required.");
            }

            if (string.IsNullOrWhiteSpace(ruleID))
            {
                Debug.LogWarning("[MatchService] InstantBattle: RuleID is required.");
                return OperationResult<InstantBattleResponse>.Fail("RuleID is required.");
            }

            var request = CreateBaseRequest();
            request.MatchID = matchID;
            request.RuleID = ruleID;
            request.CharacterID = ResolveCharacterID(characterID);
            request.BattleStrategy = battleStrategy;
            request.RelatedEntityID = $"pvp_battle_{matchID}_{Guid.NewGuid():N}";

            var result = await MatchAPI.InstantBattle(request);

            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                // Draw: single-op refund to creator. Apply locally only for the current user.
                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                // Win/loss: dual-party. Apply the side that belongs to the current user.
                if (data.ResourcesDual != null)
                {
                    var userID = Ctx.UserID;
                    var dual = data.ResourcesDual;

                    if (string.Equals(dual.FromUserID, userID, StringComparison.Ordinal) && dual.FromResult != null)
                        IDosGamesData.User.ApplyResourceOperation(dual.FromResult, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                    else if (string.Equals(dual.ToUserID, userID, StringComparison.Ordinal) && dual.ToResult != null)
                        IDosGamesData.User.ApplyResourceOperation(dual.ToResult, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                }

                OnInstantBattleCompleted?.Invoke(data);
            }

            return result;
        }

        /// <summary>
        /// Saves the player's default PvP battle strategy (up to 10 steps, server clamps).
        /// Patches local UserMatchState and fires OnStrategySaved.
        /// </summary>
        public static async Task<OperationResult<SuccessResponse>> SaveStrategy(List<BattleStepConfig> battleStrategy)
        {
            if (battleStrategy == null || battleStrategy.Count == 0)
            {
                Debug.LogWarning("[MatchService] SaveStrategy: BattleStrategy must not be empty.");
                return OperationResult<SuccessResponse>.Fail("BattleStrategy is required.");
            }

            var request = CreateBaseRequest();
            request.BattleStrategy = battleStrategy;

            var result = await MatchAPI.SaveStrategy(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchMatchStrategy(battleStrategy);
                OnStrategySaved?.Invoke(battleStrategy);
            }

            return result;
        }

        /// <summary>
        /// Fetches a paged list of matches the current player is involved in.
        /// Fires OnMyMatchesLoaded with the paged result.
        /// </summary>
        public static async Task<OperationResult<MatchesPageResponse>> GetMyMatches(
            int page = 0,
            int pageSize = 20,
            List<string> statuses = null)
        {
            var request = CreateBaseRequest();
            request.Page = page;
            request.PageSize = pageSize;
            request.Statuses = statuses;

            var result = await MatchAPI.GetMyMatches(request);

            if (result.Success && result.Data != null)
                OnMyMatchesLoaded?.Invoke(result.Data);

            return result;
        }

        /// <summary>
        /// Fetches a paged list of open matches available to join (excludes player's own).
        /// Fires OnAvailableMatchesLoaded with the paged result.
        /// </summary>
        public static async Task<OperationResult<MatchesPageResponse>> GetAvailableMatches(
            int page = 0,
            int pageSize = 20,
            string currencyID = null,
            long? minEntryFeeAmount = null,
            long? maxEntryFeeAmount = null,
            bool onlyPublic = false)
        {
            var request = CreateBaseRequest();
            request.Page = page;
            request.PageSize = pageSize;
            request.CurrencyID = currencyID;
            request.MinEntryFeeAmount = minEntryFeeAmount;
            request.MaxEntryFeeAmount = maxEntryFeeAmount;
            request.OnlyPublic = onlyPublic;

            var result = await MatchAPI.GetAvailableMatches(request);

            if (result.Success && result.Data != null)
                OnAvailableMatchesLoaded?.Invoke(result.Data);

            return result;
        }
    }
}
