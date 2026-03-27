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
                _user = AuthenticationService.AuthContext.UserID;
            }
            GetProfileData();
        }

        public void CloseRoom()
        {
            SetActiveRoom(false);
        }

        private DefaultAvatarSkin GetDefaultAvatarSkin()
        {
            return IDosGamesData.Config.TitlePublicConfiguration?.DefaultAvatarSkin;
        }

        private async void GetProfileData()
        {
            if (_user == AuthenticationService.AuthContext.UserID)
            {
                var data = DataService.GetCachedCustomUserData(CustomUserDataKey.equipped_avatar_skins.ToString());
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
                    var defaultSkin = GetDefaultAvatarSkin();
                    if (defaultSkin != null)
                    {
                        _equipedAvatarSkins = defaultSkin;
                        if (IDosGamesSDKSettings.Instance.DebugLogging)
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
                var result = await UserService.GetClientState();
                if (result.Success)
                {
                    OnDataReceived(result.Data.CustomUserDataResult);
                }
                else
                {
                    Loading.HideAllPanels();
                }
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
                var defaultSkin = GetDefaultAvatarSkin();
                if (defaultSkin != null)
                {
                    Debug.Log(defaultSkin.ToString());
                    _profileWindow.Init(_user, defaultSkin);
                }
            }

            Loading.HideAllPanels();
            SetActiveRoom(true);
        }
    }
}
