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
            UserDataService.UpdateCustomReadOnlyData("test", "test value");
        }

        public void GetValue()
        {
            string value = UserDataService.GetCachedUserReadOnlyData("test");
            Debug.Log(value);
        }
    }
}