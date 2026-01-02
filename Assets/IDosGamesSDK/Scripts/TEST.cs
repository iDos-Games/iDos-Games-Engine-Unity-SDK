using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            _ = UserService.TransferVirtualCurrency("SS", "BO", 1);
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
