using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            _ = CharacterService.UpgradeStatLevel("Health");
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
