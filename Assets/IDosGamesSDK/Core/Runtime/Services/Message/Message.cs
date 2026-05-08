using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class Message : MonoBehaviour
	{
		private const int MAX_MESSAGE_LENGTH = 200;
        private const int SHOW_CONNECTION_ERROR_POPUP_DELAY = 2;

        [SerializeField] private MessagePopUp _messagePopUp;
		[SerializeField] private RewardPopUp _rewardPopUp;
        [SerializeField] private RewardsListPopUp _rewardsListPopUp;
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

			_instance._messagePopUp.Set(message); //LocalizationSystem
            _instance.ShowPopUp(_instance._messagePopUp);
		}

		public static void Show(MessageCode messageCode)
		{
			Show(messageCode.ToString());
		}

		public static void ShowReward(string message, string imagePath)
		{
			if (_instance == null) return;

            _instance._rewardPopUp.Set(message, imagePath);
			_instance.ShowPopUp(_instance._rewardPopUp);
		}

        public static void ShowRewards(IReadOnlyList<ResourceEntry> rewards)
        {
            if (_instance == null) return;
            if (rewards == null || rewards.Count == 0) return;

            _instance.HideAllPopUps();
            _instance.ShowPopUp(_instance._rewardsListPopUp);
            _instance._rewardsListPopUp.Set(rewards);
        }

        private static void StartDelayShowConnectionError(Action callbackAction)
		{
			if (_instance == null) return;

            _instance.StartCoroutine(_instance.ShowDelayedConnectionError(callbackAction));
		}

		private IEnumerator ShowDelayedConnectionError(Action callbackAction)
		{
			yield return new WaitForSeconds(SHOW_CONNECTION_ERROR_POPUP_DELAY);
			ShowConnectionError(callbackAction);
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

            if (_instance._rewardPopUp != null)
                _instance._rewardPopUp.gameObject.SetActive(false);

            if (_instance._rewardsListPopUp != null)
                _instance._rewardsListPopUp.gameObject.SetActive(false);

            if (_instance._connectionErrorPopUp != null)
                _instance._connectionErrorPopUp.gameObject.SetActive(false);
        }

        public static bool CheckMessage(string serverResponse, MessageCode messageCode)
        {
            try
            {
                var msg = JObject.Parse(serverResponse)?["Message"]?.ToString();
                return string.Equals(msg, messageCode.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false; // кривой JSON
            }
        }

		public static string MessageResult(string serverResponse)
		{
			return JObject.Parse(serverResponse)?["Message"]?.ToString();
        }

        private void OnHttpConnectionError(string error)
        {
            ShowConnectionError(null);
        }

#if IDOSGAMES_MOBILE_IAP
        private void OnIAPServiceNotInitialized()
        {
            Show("IAP Service Not Initialized");
        }
#endif
    }
}
