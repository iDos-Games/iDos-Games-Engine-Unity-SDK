using UnityEngine;

namespace IDosGames
{
    public class AutoLogin : MonoBehaviour
    {
        public float delayAutoLogin = 2f;
        private AuthType _lastAuthType => AuthService.LastAuthType;
        private string _savedEmail => AuthService.SavedEmail;
        private string _savedPassword => AuthService.SavedPassword;

        // platform auth wait
        private bool _waitingPlatformAuth = false;
        private const float PlatformAuthTimeoutSeconds = 120f;

        private void Start()
        {
            CheckPlatform();

#if UNITY_WEBGL //&& !UNITY_EDITOR
            if (WebFunctionHandler.Instance != null) WebFunctionHandler.Instance.OnPlatformAuthEvent += OnPlatformAuth;
#endif
        }

        private void OnDestroy()
        {
#if UNITY_WEBGL //&& !UNITY_EDITOR
            if (WebFunctionHandler.Instance != null) WebFunctionHandler.Instance.OnPlatformAuthEvent -= OnPlatformAuth;
#endif
        }

        public void Login()
        {
            switch (_lastAuthType)
            {
                case AuthType.iDosGames:
                    PlatformLogin();
                    break;

                case AuthType.Email:
                    AutoLoginWithEmail();
                    break;

                default:
                    PlatformLogin();
                    break;
            }
        }

        public void PlatformLogin()
        {
#if UNITY_WEBGL //&& !UNITY_EDITOR
            TryPlatformLogin();
#endif
        }

        public void AutoLoginWithEmail()
        {
            AuthService.Instance.LoginWithEmailAddress(_savedEmail, _savedPassword, OnSuccessAutoLogin, OnErrorAutoLogin, OnRetryAutoLogin);
        }

        public void AutoLoginWithDeviceID()
        {
            AuthService.Instance.LoginWithDeviceID(OnSuccessAutoLogin, OnErrorAutoLogin, OnRetryAutoLogin);
        }

        private void OnSuccessAutoLogin(ClientStateResponse authContext)
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

#if UNITY_WEBGL //&& !UNITY_EDITOR
        private void TryPlatformLogin()
        {
            if (_waitingPlatformAuth) return;
            _waitingPlatformAuth = true;

            WebSDK.FetchPlatformAuth("WebFunctionHandler", "OnPlatformAuth");

            Invoke(nameof(OnPlatformAuthTimeout), PlatformAuthTimeoutSeconds);
        }

        public void OnPlatformAuth(string json)
        {
            Debug.Log("AutoLogin_OnPlatformAuth:" + json);
            if (!_waitingPlatformAuth) return;

            _waitingPlatformAuth = false;
            CancelInvoke(nameof(OnPlatformAuthTimeout));

            PlatformAuthResponse resp = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatformAuthResponse>(json);

            AuthService.Instance.SetPlatformUser(resp.user);
            AuthService.Instance.LoginWithPlatformToken(resp.user.AuthToken, OnSuccessAutoLogin, OnErrorAutoLogin, OnRetryAutoLogin);
        }

        private void OnPlatformAuthTimeout()
        {
            if (!_waitingPlatformAuth) return;
            _waitingPlatformAuth = false;
            Login();
        }
#endif

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