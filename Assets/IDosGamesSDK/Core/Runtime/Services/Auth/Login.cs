using UnityEngine;

namespace IDosGames
{
    public class Login : MonoBehaviour
    {
        public float delayAutoLogin = 2f;
        private AuthType _lastAuthType => AuthenticationService.LastAuthType;
        private string _savedEmail => AuthenticationService.SavedEmail;
        private string _savedPassword => AuthenticationService.SavedPassword;

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

        public void Authorization()
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

        public async void AutoLoginWithEmail()
        {
            var result = await AuthenticationService.LoginWithEmail(_savedEmail, _savedPassword);
            if (result.Success)
                OnSuccessAutoLogin(result.Data);
            else
                OnErrorAutoLogin(result.Error);
        }

        public async void AutoLoginWithDeviceID()
        {
            var result = await AuthenticationService.LoginWithDeviceID();
            if (result.Success)
                OnSuccessAutoLogin(result.Data);
            else
                OnErrorAutoLogin(result.Error);
        }

        private void OnSuccessAutoLogin(ClientState authContext)
        {
            Loading.SwitchToNextScene();
        }

        private void OnRetryAutoLogin()
        {
            Invoke(nameof(Authorization), delayAutoLogin);
        }

        private void OnErrorAutoLogin(string errorResponse)
        {
            Message.ShowConnectionError(Authorization);
        }

#if UNITY_WEBGL //&& !UNITY_EDITOR
        private void TryPlatformLogin()
        {
            if (_waitingPlatformAuth) return;
            _waitingPlatformAuth = true;

            WebSDK.FetchPlatformAuth("WebFunctionHandler", "OnPlatformAuth");

            Invoke(nameof(OnPlatformAuthTimeout), PlatformAuthTimeoutSeconds);
        }

        public async void OnPlatformAuth(string json)
        {
            Debug.Log("AutoLogin_OnPlatformAuth:" + json);
            if (!_waitingPlatformAuth) return;

            _waitingPlatformAuth = false;
            CancelInvoke(nameof(OnPlatformAuthTimeout));

            PlatformAuthResponse resp = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatformAuthResponse>(json);

            AuthenticationService.PlatformUser = resp.user;

            var result = await AuthenticationService.LoginWithPlatformToken(resp.user.AuthToken);
            if (result.Success)
                OnSuccessAutoLogin(result.Data);
            else
                OnErrorAutoLogin(result.Error);
        }

        private void OnPlatformAuthTimeout()
        {
            if (!_waitingPlatformAuth) return;
            _waitingPlatformAuth = false;
            Authorization();
        }
#endif

        private void CheckPlatform()
        {
            AuthenticationService.WebGLPlatform = WebGLPlatform.None;
#if UNITY_WEBGL && !UNITY_EDITOR

            WebSDK.FetchPlatform();
            WebSDK.FetchFullURL();

            if (WebSDK.platform == "web")
            {
                AuthenticationService.WebGLPlatform = WebGLPlatform.Web;
                IDosGamesSDKSettings.Instance.BuildForPlatform = Platforms.Web;
            }
            else if (WebSDK.platform == "telegram")
            {
                AuthenticationService.WebGLPlatform = WebGLPlatform.Telegram;
                IDosGamesSDKSettings.Instance.BuildForPlatform = Platforms.Telegram;

                WebSDK.FetchInitDataUnsafe();
                AuthenticationService.TelegramInitData = WebSDK.initDataUnsafe;
            }

#endif
        }
    }
}
