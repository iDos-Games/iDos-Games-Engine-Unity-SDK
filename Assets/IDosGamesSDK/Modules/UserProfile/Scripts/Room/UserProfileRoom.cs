using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using UnityEngine;

namespace IDosGames.UserProfile
{
    public class UserProfileRoom : Room
    {
        [SerializeField] private UserProfileWindow _profileWindow;
        public static DefaultAvatarSkin _equipedAvatarSkins;
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
                var data = UserDataService.GetCachedCustomUserData(CustomUserDataKey.equipped_avatar_skins.ToString());
                if (!string.IsNullOrEmpty(data))
                {
                    if (_equipedAvatarSkins == null)
                    {
                        _equipedAvatarSkins = JsonConvert.DeserializeObject<DefaultAvatarSkin>(data);
                    }
                    
                    _profileWindow.Init(_user, _equipedAvatarSkins);
                }
                else
                {
                    var defaultSkin = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.DefaultAvatarSkin);
                    if (!string.IsNullOrEmpty(defaultSkin))
                    {
                        _equipedAvatarSkins = JsonConvert.DeserializeObject<DefaultAvatarSkin>(defaultSkin);

                        if(IDosGamesSDKSettings.Instance.DebugLogging)
                        {
                            Debug.Log(_equipedAvatarSkins.ToString());
                        }
                        
                        _profileWindow.Init(_user, _equipedAvatarSkins);
                    }
                }
                Loading.HideAllPanels();
                SetActiveRoom(true);

            }
            else
            {
                IGSClientAPI.GetUserAllData
                    (
                    resultCallback: (result) => { UserDataService.ProcessingAllData(result); OnDataReceived(result.CustomUserDataResult); }, 
                    notConnectionErrorCallback: null, 
                    connectionErrorCallback: null
                    );
            }
        }

        private void OnDataReceived(GetCustomUserDataResult result)
        {
            string dataString = null;
            foreach (var data in result.Data)
            {
                if (data.Key == CustomUserDataKey.equipped_avatar_skins.ToString())
                {
                    dataString = data.Value.Value;
                }

            }
            if (!string.IsNullOrEmpty(dataString))
            {
                DefaultAvatarSkin jsonData = JsonConvert.DeserializeObject<DefaultAvatarSkin>(dataString);
                Debug.Log(jsonData.ToString());
                _profileWindow.Init(_user, jsonData);
            }
            else
            {
                var defaultSkin = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.DefaultAvatarSkin);
                if (!string.IsNullOrEmpty(defaultSkin))
                {
                    DefaultAvatarSkin jsonData = JsonConvert.DeserializeObject<DefaultAvatarSkin>(defaultSkin);
                    Debug.Log(jsonData.ToString());
                    _profileWindow.Init(_user, jsonData);
                }
            }

            Loading.HideAllPanels();
            SetActiveRoom(true);

        }
    }
}
