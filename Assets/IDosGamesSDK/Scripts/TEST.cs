using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        private void Start()
        {
            
        }

        public void Test()
        {
            GetValue();
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
    }
}