using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            //_ = CraftService.TradeUpRarity("test", new List<string> { "avatar_glasses_v1_01", "avatar_glasses_v1_01", "avatar_glasses_v1_01", "avatar_glasses_v1_01", "avatar_glasses_v1_01", });
            _ = CharacterService.UpgradeStatLevel("Damage");
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
