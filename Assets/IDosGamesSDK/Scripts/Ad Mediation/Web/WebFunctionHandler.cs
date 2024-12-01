using UnityEngine;

#pragma warning disable 0067
namespace IDosGames
{
    public class WebFunctionHandler : MonoBehaviour
    {
        public static WebFunctionHandler Instance { get; private set; }

        public delegate void AdEventHandler(string resultJson);
        public event AdEventHandler OnAdCompleteEvent;
        public event AdEventHandler OnAdErrorEvent;

        public delegate void FocusEventHandler();
        public event FocusEventHandler OnWebAppFocusTrueEvent;
        public event FocusEventHandler OnWebAppFocusFalseEvent;
        public event FocusEventHandler OnWebAppQuitEvent;

        private string _args;
        public string _clipboardText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

#if UNITY_WEBGL
        public void ShowAd(string blockID, string args = null)
        {
            if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
            {
                _args = args;
                WebSDK.ShowAdInternal(blockID);
                Loading.ShowTransparentPanel();
            }
        }

        public void OnAdComplete(string resultJson)
        {
            OnAdCompleteEvent?.Invoke(_args);
            Loading.HideAllPanels();
        }

        public void OnAdError(string resultJson)
        {
            OnAdErrorEvent?.Invoke(_args);
            Loading.HideAllPanels();
        }

        public void OnPasteFromClipboard(string text)
        {
            _clipboardText = text;
        }

        public void OnWebAppFocusTrue()
        {
            OnWebAppFocusTrueEvent?.Invoke();
        }

        public void OnWebAppFocusFalse()
        {
            OnWebAppFocusFalseEvent?.Invoke();
        }

        public void OnWebAppQuit()
        {
            OnWebAppQuitEvent?.Invoke();
        }
#endif
    }
}
