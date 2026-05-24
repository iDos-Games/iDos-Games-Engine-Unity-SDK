using System;
using UnityEngine;

namespace IDosGames
{
	public class Message : MonoBehaviour
	{
		private const int MAX_MESSAGE_LENGTH = 200;

        [SerializeField] private MessagePopUp _messagePopUp;
        [SerializeField] private ResourceOperationPopUp _resourceOperationPopUp;
        [SerializeField] private ConnectionErrorPopUp _connectionErrorPopUp;

		private static Message _instance;
		public static event Action Showed;

		private void Awake()
		{
			if (_instance == null || ReferenceEquals(this, _instance))
			{
				_instance = this;
				DontDestroyOnLoad(gameObject);
			}

			HideAllPopUps();
		}

		private void OnEnable()
		{
            HttpService.OnGlobalError += Show;
            HttpService.ConnectionError += OnHttpConnectionError;
#if IDOSGAMES_MOBILE_IAP
            IAPService.NotInitialized += OnIAPServiceNotInitialized;
#endif
        }

        private void OnDisable()
		{
            HttpService.OnGlobalError -= Show;
            HttpService.ConnectionError -= OnHttpConnectionError;
#if IDOSGAMES_MOBILE_IAP
            IAPService.NotInitialized -= OnIAPServiceNotInitialized;
#endif
        }

        public static void Show(string message)
		{
			if (_instance == null) return;
            if (message == null) return;

            if (message.Length > MAX_MESSAGE_LENGTH)
			{
				message = message[..MAX_MESSAGE_LENGTH];
			}

			_instance._messagePopUp.Set(message);
            _instance.ShowPopUp(_instance._messagePopUp);
		}

		public static void Show(MessageCode messageCode)
		{
			Show(messageCode.ToString());
		}

        public static void ShowResourceOperation(ResourceOperation operation)
        {
            if (_instance == null) return;
            if (operation == null) return;
            if (!_instance._resourceOperationPopUp.WouldDisplay(operation)) return;

            _instance.HideAllPopUps();
            _instance.ShowPopUp(_instance._resourceOperationPopUp);
            _instance._resourceOperationPopUp.Set(operation);
        }

		public static void ShowConnectionError(Action callbackAction)
		{
			if (_instance == null) return;

            _instance.HideAllPopUps();
			_instance._connectionErrorPopUp.Set(callbackAction);
			_instance.ShowPopUp(_instance._connectionErrorPopUp);
		}

		private void ShowPopUp(PopUp popUp)
		{
            if (popUp == null) return;

            popUp.gameObject.SetActive(true);
			Showed?.Invoke();
		}

        private void HideAllPopUps()
        {
            if (_instance == null) return;

            if (_instance._messagePopUp != null)
                _instance._messagePopUp.gameObject.SetActive(false);

            if (_instance._resourceOperationPopUp != null)
                _instance._resourceOperationPopUp.gameObject.SetActive(false);

            if (_instance._connectionErrorPopUp != null)
                _instance._connectionErrorPopUp.gameObject.SetActive(false);
        }

        private void OnHttpConnectionError(string error)
        {
            ShowConnectionError(null);
        }
    }
}
