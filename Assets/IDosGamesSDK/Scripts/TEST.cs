using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            List<string> itemIDs = new()
            {
                "avatar_hat_v1_01", "avatar_glasses_v3_01", "avatar_glasses_v4_01", "avatar_glasses_v5_01", "avatar_glasses_v5_01",
            };
            _ = CraftService.Craft("test", itemIDs, 2, 1);
            //_ = LootboxService.Open("test", 1);
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
