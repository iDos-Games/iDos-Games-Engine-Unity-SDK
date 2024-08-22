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
            Login();
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

        private void AutoLoginWithEmail()
        {
            AuthService.Instance.LoginWithEmailAddress(_savedEmail, _savedPassword, OnSuccessAutoLogin, OnErrorAutoLogin, OnRetryAutoLogin);
        }

        private void AutoLoginWithDeviceID()
        {
            AuthService.Instance.LoginWithDeviceID(OnSuccessAutoLogin, OnErrorAutoLogin, OnRetryAutoLogin);
        }

        private void OnSuccessAutoLogin(GetAllUserDataResult authContext)
        {
            Loading.SwitchToNextScene();
        }

        private void OnRetryAutoLogin()
        {
            Login();
        }

        private void OnErrorAutoLogin(string errorResponse)
        {
            Debug.Log("Auto login Error: " + errorResponse);
            Invoke(nameof(AutoLoginWithDeviceID), delayAutoLogin);
        }

        private void CheckPlatform()
        {
            AuthService.WebGLPlatform = WebGLPlatform.None;
#if UNITY_WEBGL && !UNITY_EDITOR

            WebSDK.FetchPlatform();

            if (WebSDK.platform == "web")
            {
                AuthService.WebGLPlatform = WebGLPlatform.Web;
            }
            else if (WebSDK.platform == "telegram")
            {
                AuthService.WebGLPlatform = WebGLPlatform.Telegram;

                WebSDK.FetchInitDataUnsafe();
                AuthService.TelegramInitData = WebSDK.ParseInitDataUnsafe();
            }

#endif
        }
    }
}