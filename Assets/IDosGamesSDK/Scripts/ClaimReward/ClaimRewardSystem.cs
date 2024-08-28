using Newtonsoft.Json;
using System.Collections.Generic;

namespace IDosGames
{
    public class ClaimRewardSystem
    {
        private static int _currentRewardValue = 0;

        public static void ClaimCoinReward(int value, int point)
        {
            _currentRewardValue = value + GetSkinProfitAmount();

            if (GetSkinProfitAmount() == 0)
            {
                ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimCoinReward, point);
            }
            else
            {
                ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimCoinWithSkinReward, point);
            }
            Loading.HideAllPanels();
        }

        public static void ClaimX3CoinReward(int value, int point)
        {
            _currentRewardValue = (value + GetSkinProfitAmount()) * 3;

            if (GetSkinProfitAmount() == 0)
            {
                ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX3CoinReward, point);
            }
            else
            {
                ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX3CoinWithSkinReward, point);
            }
            Loading.HideAllPanels();
        }

        public static void ClaimX5CoinReward(int value, int point)
        {
            _currentRewardValue = (value + GetSkinProfitAmount()) * 5;

            if (GetSkinProfitAmount() == 0)
            {
                ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX5CoinReward, point);
            }
            else
            {
                ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimX5CoinWithSkinReward, point);
            }
            Loading.HideAllPanels();
        }

        public static void ClaimRewardWithoutSkinProfit(int value)
        {
            ExecuteClaimFunction(value, ServerFunctionHandlers.ClaimCoinReward);
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