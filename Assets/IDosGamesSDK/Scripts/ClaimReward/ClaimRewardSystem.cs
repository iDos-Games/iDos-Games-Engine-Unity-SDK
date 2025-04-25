using Newtonsoft.Json;

namespace IDosGames
{
    public class ClaimRewardSystem
    {
        public static string tokenLimitCurrencyID = "TL";

        // For VIP
        public static void ClaimTokenReward(int value, int point)
        {
            if (UserInventory.HasVIPStatus)
            {
                //Debug.Log($"Daily Token Limit: {UserInventory.GetVirtualCurrencyAmount(tokenLimitCurrencyID)}");
                ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimTokenReward, point);
            }
            else
            {
                //Debug.Log($"Daily Token Limit: {UserInventory.GetVirtualCurrencyAmount(tokenLimitCurrencyID)}");
                ShopSystem.PopUpSystem.ShowVIPPopUp();
            }
        }

        public static void ClaimSkinProfit()
        {
            if (GetSkinProfitAmount() > 0 && UserInventory.HasVIPStatus)
            {
                ExecuteClaimFunction(0, ServerFunctionHandlers.GrantSkinProfitFromEquippedSkins);
            }
        }
        // For VIP End

        public static void ClaimCoinReward(int value, int point)
        {
            ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimCoinReward, point);
            Loading.HideAllPanels();
        }

        public static void ClaimX3CoinReward(int value, int point)
        {
            ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX3CoinReward, point);
            Loading.HideAllPanels();
        }

        public static void ClaimX5CoinReward(int value, int point)
        {
            ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX5CoinReward, point);
            Loading.HideAllPanels();
        }

        public static void ClaimRewardWithoutSkinProfit(int value)
        {
            ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimCoinReward);
            Loading.HideAllPanels();
        }

        public static void ClaimX3RewardWithoutSkinProfit(int value)
        {
            ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX3CoinReward);
            Loading.HideAllPanels();
        }

        public static void ClaimX5RewardWithoutSkinProfit(int value)
        {
            ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX5CoinReward);
            Loading.HideAllPanels();
        }

        private static void ExecuteClaimFunction(int value, ServerFunctionHandlers functionName, int points = 0)
        {
            FunctionParameters parameter = new()
            {
                IntValue = value,
                Points = points
            };

            _ = IGSClientAPI.ExecuteFunction
                (
                functionName: functionName,
                resultCallback: OnSuccessClaimReward,
                notConnectionErrorCallback: OnErrorClaimReward,
                connectionErrorCallback: () => Message.ShowConnectionError(() => ExecuteClaimFunction(value, functionName)),
                functionParameter: parameter
                );
        }

        private static void OnSuccessClaimReward(string result)
        {
            var userData = JsonConvert.DeserializeObject<GetAllUserDataResult>(result);
            UserDataService.ProcessingAllData(userData);
            Loading.HideAllPanels();
        }

        private static void OnErrorClaimReward(string error)
        {
            Message.Show(MessageCode.FAILED_TO_CLAIM_REWARD);
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