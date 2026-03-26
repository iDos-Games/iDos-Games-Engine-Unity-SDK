using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class MatchService
    {
        public static event Action<CreateMatchResponse> OnMatchCreated;
        public static event Action<BattleResult> OnBattleFinished;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static MatchRequest CreateBaseRequest()
        {
            return new MatchRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        /// <summary>
        /// Create a new match
        /// </summary>
        public static async Task<OperationResult<CreateMatchResponse>> CreateMatch(
            int entryFee,
            string currencyId = "CO",
            string characterId = null,
            List<BattleStepConfig> strategy = null)
        {
            var request = CreateBaseRequest();
            request.EntryFeeAmount = entryFee;
            request.CurrencyID = currencyId;
            request.CharacterID = characterId;
            request.BattleStrategy = strategy;
            request.RuleID = DefaultData.InstantBattle1v1;

            var result = await MatchAPI.CreateMatch(request);

            if (result.Success)
            {
                OnMatchCreated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Accept the fight (Instant Battle)
        /// </summary>
        public static async Task<OperationResult<BattleResult>> InstantBattle(
            string matchId,
            string characterId = null,
            List<BattleStepConfig> strategy = null)
        {
            var request = CreateBaseRequest();
            request.MatchID = matchId;
            request.CharacterID = characterId;
            request.BattleStrategy = strategy;
            request.RuleID = DefaultData.InstantBattle1v1;

            var result = await MatchAPI.InstantBattle(request);

            if (result.Success)
            {
                OnBattleFinished?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<MatchesPageResponse>> GetMyMatches(int page = 0, int pageSize = 20, List<string> statuses = null)
        {
            var request = CreateBaseRequest();
            request.Page = page;
            request.PageSize = pageSize;
            request.Statuses = statuses;

            return await MatchAPI.GetMyMatches(request);
        }

        /// <summary>
        /// Get a list of matches available for battle
        /// </summary>
        public static async Task<OperationResult<MatchesPageResponse>> GetAvailableMatches(
            string currencyId = "CO",
            int page = 0,
            int pageSize = 20)
        {
            var request = CreateBaseRequest();
            request.CurrencyID = currencyId;
            request.Page = page;
            request.PageSize = pageSize;
            request.OnlyPublic = true;

            return await MatchAPI.GetAvailableMatches(request);
        }

        /// <summary>
        /// Save the user's default strategy
        /// </summary>
        public static async Task<OperationResult<SuccessResponse>> SaveStrategy(List<BattleStepConfig> strategy)
        {
            var request = CreateBaseRequest();
            request.BattleStrategy = strategy;

            return await MatchAPI.SaveStrategy(request);
        }
    }
}
