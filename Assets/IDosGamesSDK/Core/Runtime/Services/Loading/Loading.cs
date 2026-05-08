using UnityEngine;

namespace IDosGames
{
	public class Loading : MonoBehaviour
	{
		[SerializeField] private LoadingPanel _panel;
		[SerializeField] private TouchBlocker _touchBlocker;
		[SerializeField] private SceneSwitcher _sceneSwitcher;

		private static Loading _instance;

		private void Awake()
		{
			if (_instance == null || ReferenceEquals(this, _instance))
			{
				_instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}

		private void OnEnable()
		{
			if (IDosGamesSDKSettings.Instance.ShowLoadingOnExecuteServerFunction)
			{
                HttpService.OnBusyStateChanged += OnHttpBusyStateChanged;
			}

			SceneSwitcher.SwitchSceneStarted += ShowOpaquePanel;
			SceneSwitcher.SwitchSceneFinished += HideOpaquePanel;
			Message.Showed += HideAllPanels;

#if IDOSGAMES_MOBILE_IAP
			IAPService.PurchaseInitiated += ShowTransparentPanel;
			IAPService.PurchaseProcessStarted += HideTransparentPanel;
			IAPService.PurchaseFailed += (failReason) => HideTransparentPanel();
#endif
        }

        private void OnDisable()
		{
			if (IDosGamesSDKSettings.Instance.ShowLoadingOnExecuteServerFunction)
			{
                HttpService.OnBusyStateChanged -= OnHttpBusyStateChanged;
			}

			SceneSwitcher.SwitchSceneStarted -= ShowOpaquePanel;
			SceneSwitcher.SwitchSceneFinished -= HideOpaquePanel;
			Message.Showed -= HideAllPanels;

#if IDOSGAMES_MOBILE_IAP
			IAPService.PurchaseInitiated -= ShowTransparentPanel;
			IAPService.PurchaseProcessStarted -= HideTransparentPanel;
#endif
        }

        private static void OnHttpBusyStateChanged(bool isBusy)
        {
            if (isBusy)
            {
                ShowTransparentPanel();
            }
            else
            {
                HideTransparentPanel();
            }
        }

        public static void ShowTransparentPanel()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._panel.Show(LoadingPanelType.Transparent);
		}

		public static void ShowOpaquePanel()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._panel.Show(LoadingPanelType.Opaque);
		}

		public static void HideAllPanels()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._panel.HideAllPanels();
		}

		private static void HideOpaquePanel()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._panel.HideOpaquePanel();
		}

		private static void HideTransparentPanel()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._panel.HideTransparentPanel();
		}

		public static void SwitchToNextScene()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._sceneSwitcher.SwitchToNextScene();
		}

		public static void SwitchToPreviousScene()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._sceneSwitcher.SwitchToPreviousScene();
		}

        public static void SwitchToLoginScene()
        {
            if (_instance == null)
            {
                return;
            }

            _instance._sceneSwitcher.SwitchToLoginScene();
        }

        public static void BlockTouch()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._touchBlocker.Block();
		}

		public static void UnblockTouch()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._touchBlocker.Unblock();
		}
	}
}