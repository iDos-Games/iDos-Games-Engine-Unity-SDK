using System;
using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class Message : MonoBehaviour
	{
		private const int MAX_MESSAGE_LENGTH = 200;
		[SerializeField] private MessagePopUp _messagePopUp;
		[SerializeField] private RewardPopUp _rewardPopUp;
		[SerializeField] private ConnectionErrorPopUp _connectionErrorPopUp;

		private const int SHOW_CONNECTION_ERROR_POPUP_DELAY = 2;

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
            IGSClientAPI.ConnectionError += StartDelayShowConnectionError;
			UserDataService.AllDataRequestError += OnAllDataRequestError;
            IGSService.ConnectionError += ShowConnectionError;
#if IDOSGAMES_MOBILE_IAP
            IAPService.NotInitialized += OnIAPServiceNotInitialized;
#endif
        }

        private void OnDisable()
		{
            IGSClientAPI.ConnectionError -= StartDelayShowConnectionError;
			UserDataService.AllDataRequestError -= OnAllDataRequestError;
            IGSService.ConnectionError -= ShowConnectionError;
#if IDOSGAMES_MOBILE_IAP
            IAPService.NotInitialized -= OnIAPServiceNotInitialized;
#endif
        }

        public static void Show(string message)
		{
			if (_instance == null)
			{
				return;
			}

			if (message == null)
			{
				return;
			}

			if (message.Length > MAX_MESSAGE_LENGTH)
			{
				message = message[..MAX_MESSAGE_LENGTH];
			}

			_instance._messagePopUp.Set(message); //LocalizationSystem
            _instance.ShowPopUp(_instance._messagePopUp);
		}

		public static void Show(MessageCode messageCode)
		{
			Show(messageCode.ToString());
		}

		public static void ShowReward(string message, string imagePath)
		{
			if (_instance == null)
			{
				return;
			}

			_instance._rewardPopUp.Set(message, imagePath);
			_instance.ShowPopUp(_instance._rewardPopUp);
		}

		private static void StartDelayShowConnectionError(Action callbackAction)
		{
			if (_instance == null)
			{
				return;
			}

			_instance.StartCoroutine(_instance.ShowDelayedConnectionError(callbackAction));
		}

		private IEnumerator ShowDelayedConnectionError(Action callbackAction)
		{
			yield return new WaitForSeconds(SHOW_CONNECTION_ERROR_POPUP_DELAY);
			ShowConnectionError(callbackAction);
		}

		public static void ShowConnectionError(Action callbackAction)
		{
			if (_instance == null)
			{
				return;
			}

			_instance.HideAllPopUps();
			_instance._connectionErrorPopUp.Set(callbackAction);
			_instance.ShowPopUp(_instance._connectionErrorPopUp);
		}

		private void ShowPopUp(PopUp popUp)
		{
			popUp.gameObject.SetActive(true);

			Showed?.Invoke();
		}

		private void HideAllPopUps()
		{
			if (_instance == null)
			{
				return;
			}

			_instance._messagePopUp.gameObject.SetActive(false);
			_instance._rewardPopUp.gameObject.SetActive(false);
			_instance._connectionErrorPopUp.gameObject.SetActive(false);
		}

		private void OnAllDataRequestError(string error)
		{
			Show($"<color=red><b>{MessageCode.SOMETHING_WENT_WRONG.ToString()}</b></color> \n\n" +
				error); //LocalizationSystem
        }

		private void OnIAPServiceNotInitialized()
		{
			Show(MessageCode.IAP_SERVICE_NOT_INITIALIZED);
		}
	}
}