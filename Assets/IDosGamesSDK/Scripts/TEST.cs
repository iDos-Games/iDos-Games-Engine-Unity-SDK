using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            //_ = RewardService.GetDailyRewardsDefinitions();
            List<string> itemIDs = new()
            {
                "avatar_hat_v1_01", "avatar_hat_v1_01",
            };
            _ = CraftService.Craft("test", itemIDs);
        }

        public void SaveValueToServer()
        {
            UserDataService.UpdateCustomUserData("test", "test value");
        }

        public void GetValue()
        {
            string value = UserDataService.GetCachedCustomUserData("test");
            Debug.Log(value);
        }

        public void GetToken()
        {
            ClaimRewardSystem.ClaimTokenReward(10, 1);
        }
    }
}
