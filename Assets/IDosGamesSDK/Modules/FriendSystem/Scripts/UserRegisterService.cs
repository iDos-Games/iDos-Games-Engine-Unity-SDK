using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IDosGames.Friends
{
    public class UserRegisterService
    {
        private static UserRegisterService _instance;
        public static UserRegisterService Instance => _instance;

        private static string readOnlyKey = "user_registed_in_db";

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new();
        }

        public UserRegisterService()
        {
            _instance = this;

            //AdditionalDataService.UserReadOnlyDataUpdated += OnReadOnlyDataUpdated;
        }

        private void OnReadOnlyDataUpdated()
        {
            string data = UserDataService.GetUserReadOnlyData(readOnlyKey);
            Debug.Log(data);
            if (data == "" || data == null || data == "false")
            {
                RegisterUserIGS();
            }
        }

        private async void RegisterUser()
        {
            IGSRequest requestBody = new IGSRequest
            {
                DeviceID = SystemInfo.deviceUniqueIdentifier
            };

            string resultString = await IGSService.SendPostRequest(IDosGamesSDKSettings.Instance.RegisterUserLink, requestBody);

            if (!string.IsNullOrEmpty(resultString))
            {
                JObject result = JObject.Parse(resultString);

                if (result["FunctionResultTooLarge"]?.ToObject<bool>() ?? false)
                {
                    Debug.Log("This can happen if you exceed the limit that can be returned from an Azure Function, See PlayFab Limits Page for details.");
                    return;
                }

                if (result["Message"].ToString() == "SUCCESS")
                {
                    Debug.Log(result);
                    UserDataService.UpdateCustomReadOnlyData(readOnlyKey, true);
                    return;
                }
                
                Debug.Log($"The function completed successfully.");
            }
            else
            {
                Debug.Log($"Opps Something went wrong.");
            }
        }

        private void RegisterUserIGS()
        {
            
        }

        public static void ResetValueRegistered()
        {
            UserDataService.UpdateCustomReadOnlyData(readOnlyKey, false);
        }
    }
}