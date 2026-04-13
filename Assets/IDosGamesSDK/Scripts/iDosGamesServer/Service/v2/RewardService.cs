using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class RewardService
    {
        public static event Action<RewardResponse> OnClaimSuccess;
        public static event Action<ClaimDailyRewardResponse> OnClaimDailyRewardSuccess;
        public static event Action<List<DailyRewardsDefinition>> OnDailyRewardsDefinitionsReceived;
        public static event Action<Dictionary<string, UserDailyRewardState>> OnUserDailyRewardsStateReceived;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static RewardRequest CreateBaseRequest()
        {
            return new RewardRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
                LeaderboardID = DefaultData.CoinContest,
            };
        }

        public static async Task<OperationResult<DailyRewardsDefinitionsResponse>> GetDailyRewardsDefinitions()
        {
            var request = CreateBaseRequest();

            var result = await RewardAPI.GetDailyRewardsDefinitions(request);

            if (result.Success)
            {
                var definitions = result.Data?.DailyRewardsDefinitions ?? new List<DailyRewardsDefinition>();

                IDosGamesData.Config.ApplyDailyRewardsDefinitions(definitions);
                OnDailyRewardsDefinitionsReceived?.Invoke(definitions);
            }

            return result;
        }

        public static async Task<OperationResult<UserDailyRewardStateResponse>> GetUserDailyRewardsState()
        {
            var request = CreateBaseRequest();

            var result = await RewardAPI.GetUserDailyRewardsState(request);

            if (result.Success)
            {
                var dailyRewards = result.Data?.DailyRewards ?? new Dictionary<string, UserDailyRewardState>();

                IDosGamesData.User.ApplyDailyRewards(dailyRewards);
                OnUserDailyRewardsStateReceived?.Invoke(dailyRewards);
            }

            return result;
        }

        public static async Task<OperationResult<ClaimDailyRewardResponse>> ClaimDailyReward(string calendarId = null)
        {
            var request = CreateBaseRequest();
            request.CalendarID = calendarId;

            var result = await RewardAPI.ClaimDailyReward(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchDailyReward(result.Data.CalendarID, result.Data.DailyState);
                IDosGamesData.User.GrantResources(result.Data.GrantedRewards);
                OnClaimDailyRewardSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RewardResponse>> Claim(
            string currencyId,
            int baseValue,
            float multiplier = 1,
            int points = 0,
            bool includeReferral = false
            )
        {
            var request = CreateBaseRequest();
            request.UsageTime = IDosGamesSDKSettings.Instance.PlayTime;

            request.RewardCurrencyID = currencyId;
            request.BaseValue = baseValue;
            request.Multiplier = multiplier;
            request.Points = points;
            request.IncludeReferral = includeReferral;

            var result = await RewardAPI.Claim(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                UpdateCachedCurrencies(result.Data);
                OnClaimSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RewardResponse>> ClaimVip(
            string currencyId,
            int baseValue,
            float multiplier = 1,
            int points = 0,
            bool includeReferral = false
            )
        {
            var request = CreateBaseRequest();
            request.UsageTime = IDosGamesSDKSettings.Instance.PlayTime;

            request.RewardCurrencyID = currencyId;
            request.BaseValue = baseValue;
            request.Multiplier = multiplier;
            request.Points = points;
            request.IncludeReferral = includeReferral;

            var result = await RewardAPI.ClaimVip(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                UpdateCachedCurrencies(result.Data);
                OnClaimSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RewardResponse>> ClaimItemProfit(
            string currencyId,
            int baseValue,
            float multiplier = 1,
            int points = 0,
            bool includeReferral = false
            )
        {
            var request = CreateBaseRequest();
            request.UsageTime = IDosGamesSDKSettings.Instance.PlayTime;

            request.RewardCurrencyID = currencyId;
            request.BaseValue = baseValue;
            request.Multiplier = multiplier;
            request.Points = points;
            request.IncludeReferral = includeReferral;

            var result = await RewardAPI.ClaimItemProfit(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                UpdateCachedCurrencies(result.Data);
                OnClaimSuccess?.Invoke(result.Data);
            }

            return result;
        }

        private static void UpdateCachedCurrencies(RewardResponse response)
        {
            if (response == null) return;

            if (!string.IsNullOrEmpty(response.RewardCurrencyID)) IDosGamesData.User.PatchVirtualCurrency(response.RewardCurrencyID, response.RewardBalanceNew);
            if (!string.IsNullOrEmpty(response.LimitCurrencyID)) IDosGamesData.User.PatchVirtualCurrency(response.LimitCurrencyID, response.LimitBalanceNew);
        }
    }
}
