using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ServerModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class RewardService
    {
        public static event Action<RewardResponse> OnClaimSuccess;
        public static event Action<ClaimDailyRewardResponse> OnClaimDailyRewardSuccess;
        public static event Action<List<DailyRewardsDefinition>> OnDailyRewardsDefinitionsReceived;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
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

        public static async Task<OperationResult<List<DailyRewardsDefinition>>> GetDailyRewardsDefinitions()
        {
            var request = CreateBaseRequest();

            var result = await RewardAPI.GetDailyRewardsDefinitions(request);

            if (result.Success)
            {
                OnDailyRewardsDefinitionsReceived?.Invoke(result.Data);
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

            var inv = IGSUserData.UserInventory;
            if (inv == null) return;

            inv.VirtualCurrency ??= new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(response.RewardCurrencyID)) inv.VirtualCurrency[response.RewardCurrencyID] = response.RewardBalanceNew;
            if (!string.IsNullOrEmpty(response.LimitCurrencyID)) inv.VirtualCurrency[response.LimitCurrencyID] = response.LimitBalanceNew;

            UserDataService.VirtualCurrencyUpdatedInvoke();
        }
    }
}
