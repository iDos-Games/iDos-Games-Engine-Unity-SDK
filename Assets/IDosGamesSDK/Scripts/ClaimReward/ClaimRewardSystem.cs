using Newtonsoft.Json;
using UnityEngine;

namespace IDosGames
{
    public class ClaimRewardSystem
    {
        public static string coinCurrencyId = "CO";
        public static string tokenCurrencyId = "IG";

        public static async void ClaimCoinReward(int baseValue, float multiplier = 1, int points = 0, bool includeReferral = false)
        {
            RewardAnimations.ShowIgcAnimation();
            if (points > 0) RewardAnimations.ShowEventPointAnimation();
            await RewardService.Claim(coinCurrencyId, baseValue, multiplier, points, includeReferral);
        }

        public static async void ClaimTokenReward(int baseValue, float multiplier = 1, int points = 0, bool includeReferral = false)
        {
            if (UserInventory.HasVIPStatus)
            {
                RewardAnimations.ShowIgtAnimation();
                if (points > 0) RewardAnimations.ShowEventPointAnimation();
                await RewardService.ClaimVip(tokenCurrencyId, baseValue, multiplier, points, includeReferral);
            }
            else
            {
                ShopSystem.PopUpSystem.ShowVIPPopUp();
            }
        }

        public static async void ClaimSkinProfit(string currencyId) // Coin or Token
        {
            var amount = GetSkinProfitAmount();
            if (amount <= 0) return;

            await RewardService.ClaimItemProfit(currencyId, amount);
        }

        private static void OnSuccessClaimReward(string result)
        {
            var userData = JsonConvert.DeserializeObject<AuthenticationResponse>(result);
            UserDataService.ProcessingAllData(userData);
            Loading.HideAllPanels();
        }

        private static void OnErrorClaimReward(string error)
        {
            Debug.LogWarning(error);

            if (error == MessageCode.FAILED_TO_CLAIM_REWARD.ToString())
            {

            }
            else
            {
                Message.Show(error);
            }
        }

        public static int GetSkinProfitAmount()
        {
            var amount = 0;

            foreach (var itemID in UserDataService.EquippedSkins)
            {
                amount += (int)UserDataService.GetCachedSkinItem(itemID).Profit;
            }

            return amount;
        }
    }
}
