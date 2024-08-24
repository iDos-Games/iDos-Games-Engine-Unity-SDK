using IDosGames.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IDosGames.UserProfile
{
    public class UserProfileRoom : Room
    {
        [SerializeField] private UserProfileWindow _profileWindow;
        private string _user;

        public void OpenRoom(string playfabID = null)
        {
            Loading.ShowTransparentPanel();
            _user = playfabID;
            if (string.IsNullOrEmpty(playfabID))
            {
                _user = AuthService.UserID;
            }

            GetProfileData();
        }

        public void CloseRoom()
        {
            SetActiveRoom(false);
        }

        private void GetProfileData()
        {
            if (_user == AuthService.UserID)
            {
                var data = UserDataService.GetCachedUserReadOnlyData(UserReadOnlyDataKey.equipped_avatar_skins.ToString());
                if (!string.IsNullOrEmpty(data))
                {
                    JToken jsonData = JsonConvert.DeserializeObject<JToken>(data);
                    _profileWindow.Init(_user, jsonData);
                }
                else
                {
                    var defaultSkin = UserDataService.GetCachedTitleData(TitleDataKey.default_avatar_skin);
                    if (!string.IsNullOrEmpty(defaultSkin))
                    {
                        JToken jsonData = JsonConvert.DeserializeObject<JToken>(defaultSkin);

                        if(IDosGamesSDKSettings.Instance.DebugLogging)
                        {
                            Debug.Log(jsonData.ToString());
                        }
                        
                        _profileWindow.Init(_user, jsonData);
                    }
                }
                Loading.HideAllPanels();
                SetActiveRoom(true);

            }
            else
            {
                IGSClientAPI.GetUserAllData
                    (
                    resultCallback: (result) => { UserDataService.ProcessingAllData(result); OnDataReceived(result.UserDataResult); }, 
                    notConnectionErrorCallback: null, 
                    connectionErrorCallback: null
                    );
            }
        }

        private void OnDataReceived(GetUserDataResult result)
        {
            string dataString = null;
            foreach (var data in result.Data)
            {
                if (data.Key == UserReadOnlyDataKey.equipped_avatar_skins.ToString())
                {
                    dataString = data.Value.Value;
                }

            }
            if (!string.IsNullOrEmpty(dataString))
            {
                JToken jsonData = JsonConvert.DeserializeObject<JToken>(dataString);
                Debug.Log(jsonData.ToString());
                _profileWindow.Init(_user, jsonData);
            }
            else
            {
                var defaultSkin = UserDataService.GetCachedTitleData(TitleDataKey.default_avatar_skin);
                if (!string.IsNullOrEmpty(defaultSkin))
                {
                    JToken jsonData = JsonConvert.DeserializeObject<JToken>(defaultSkin);
                    Debug.Log(jsonData.ToString());
                    _profileWindow.Init(_user, jsonData);
                }
            }

            Loading.HideAllPanels();
            SetActiveRoom(true);

        }
    }
}
