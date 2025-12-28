namespace IDosGames
{
    public class ClaimRewardSystem
    {
        public static async void ClaimCoinReward(int baseValue, float multiplier = 1, int points = 0, bool includeReferral = false)
        {
            RewardAnimations.ShowIgcAnimation();
            if (points > 0) RewardAnimations.ShowEventPointAnimation();
            await RewardService.Claim(DefaultData.CoinCurrencyId, baseValue, multiplier, points, includeReferral);
        }

        public static async void ClaimTokenReward(int baseValue, float multiplier = 1, int points = 0, bool includeReferral = false)
        {
            if (UserInventory.HasVIPStatus)
            {
                RewardAnimations.ShowIgtAnimation();
                if (points > 0) RewardAnimations.ShowEventPointAnimation();
                await RewardService.ClaimVip(DefaultData.TokenCurrencyId, baseValue, multiplier, points, includeReferral);
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
