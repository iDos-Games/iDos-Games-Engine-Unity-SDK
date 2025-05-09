using UnityEngine;

namespace IDosGames
{
    public class AutoLogin : MonoBehaviour
    {
        public float delayAutoLogin = 2f;
        private AuthType _lastAuthType => AuthService.LastAuthType;
        private string _savedEmail => AuthService.SavedEmail;
        private string _savedPassword => AuthService.SavedPassword;

        private void Start()
        {
            CheckPlatform();
            //Login();
        }

        public void Login()
        {
            switch (_lastAuthType)
            {
                case AuthType.Email:
                    AutoLoginWithEmail();
                    break;

                default:
                    AutoLoginWithDeviceID();
                    break;
            }
        }

        public void AutoLoginWithEmail()
        {
            AuthService.Instance.LoginWithEmailAddress(_savedEmail, _savedPassword, OnSuccessAutoLogin, OnErrorAutoLogin, OnRetryAutoLogin);
        }

        public void AutoLoginWithDeviceID()
        {
            AuthService.Instance.LoginWithDeviceID(OnSuccessAutoLogin, OnErrorAutoLogin, OnRetryAutoLogin);
        }

        private void OnSuccessAutoLogin(GetAllUserDataResult authContext)
        {
            Loading.SwitchToNextScene();
        }

        private void OnRetryAutoLogin()
        {
            Invoke(nameof(Login), delayAutoLogin);
        }

        private void OnErrorAutoLogin(string errorResponse)
        {
            Message.ShowConnectionError(Login);
        }

        private void CheckPlatform()
        {
            AuthService.WebGLPlatform = WebGLPlatform.None;
#if UNITY_WEBGL && !UNITY_EDITOR

            WebSDK.FetchPlatform();
            WebSDK.FetchFullURL();

            if (WebSDK.platform == "web")
            {
                AuthService.WebGLPlatform = WebGLPlatform.Web;
                IDosGamesSDKSettings.Instance.BuildForPlatform = Platforms.Web;
            }
            else if (WebSDK.platform == "telegram")
            {
                AuthService.WebGLPlatform = WebGLPlatform.Telegram;
                IDosGamesSDKSettings.Instance.BuildForPlatform = Platforms.Telegram;

                WebSDK.FetchInitDataUnsafe();
                AuthService.TelegramInitData = WebSDK.initDataUnsafe;
            }

#endif
        }
    }
}