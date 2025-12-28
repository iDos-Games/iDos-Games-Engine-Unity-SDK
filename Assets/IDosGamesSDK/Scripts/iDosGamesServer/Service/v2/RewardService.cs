using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ServerModels;

namespace IDosGames
{
    public static class RewardService
    {
        public static event Action<RewardClaimResponse> OnClaimSuccess;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        public static async Task<OperationResult<RewardClaimResponse>> Claim(
            string currencyId,
            int baseValue,
            float multiplier = 1,
            int points = 0,
            bool includeReferral = false
            )
        {
            var request = new RewardClaimRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                RewardCurrencyId = currencyId,
                BaseValue = baseValue,
                Multiplier = multiplier,
                IncludeReferral = includeReferral,
                Points = points
            };

            var result = await RewardAPI.Claim(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                UpdateCachedCurrencies(result.Data);
                OnClaimSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RewardClaimResponse>> ClaimVip(
            string currencyId,
            int baseValue,
            float multiplier = 1,
            int points = 0,
            bool includeReferral = false
            )
        {
            var request = new RewardClaimRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                RewardCurrencyId = currencyId,
                BaseValue = baseValue,
                Multiplier = multiplier,
                IncludeReferral = includeReferral,
                Points = points
            };

            var result = await RewardAPI.ClaimVip(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                UpdateCachedCurrencies(result.Data);
                OnClaimSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<RewardClaimResponse>> ClaimItemProfit(
            string currencyId,
            int baseValue,
            float multiplier = 1,
            int points = 0,
            bool includeReferral = false
            )
        {
            var request = new RewardClaimRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,

                RewardCurrencyId = currencyId,
                BaseValue = baseValue,
                Multiplier = multiplier,
                IncludeReferral = includeReferral,
                Points = points
            };

            var result = await RewardAPI.ClaimItemProfit(request);

            if (result.Success)
            {
                IDosGamesSDKSettings.Instance.PlayTime = 0;
                UpdateCachedCurrencies(result.Data);
                OnClaimSuccess?.Invoke(result.Data);
            }

            return result;
        }

        private static void UpdateCachedCurrencies(RewardClaimResponse response)
        {
            if (response == null) return;

            var inv = IGSUserData.UserInventory;
            if (inv == null) return;

            inv.VirtualCurrency ??= new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(response.RewardCurrencyId)) inv.VirtualCurrency[response.RewardCurrencyId] = response.RewardBalanceNew;
            if (!string.IsNullOrEmpty(response.LimitCurrencyId)) inv.VirtualCurrency[response.LimitCurrencyId] = response.LimitBalanceNew;

            UserDataService.VirtualCurrencyUpdatedInvoke();
        }
    }
}
