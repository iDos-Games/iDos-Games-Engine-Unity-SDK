using UnityEngine;

namespace IDosGames
{
	public class AppMetricaAnalytics : MonoBehaviour
	{
		private static bool _isInitialized;

#if IDOSGAMES_APP_METRICA
		private static IYandexAppMetrica _instance;
		private static readonly object _syncRoot = new Object();

		private string _apiKey => IDosGamesSDKSettings.Instance.AppMetricaApiKey;

		[SerializeField] private bool _exceptionsReporting = true;

		[SerializeField] private uint _sessionTimeoutSec = 10;

		[SerializeField] private bool _locationTracking = false;

		[SerializeField] private bool _logs = false;

		[SerializeField] private bool _handleFirstActivationAsUpdate = false;

		[SerializeField] private bool _statisticsSending = true;

		private bool _actualPauseStatus;

		public static IYandexAppMetrica Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_syncRoot)
					{
#if UNITY_IPHONE || UNITY_IOS
                    if (_instance == null && Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        _instance = new YandexAppMetricaIOS();
                    }
#elif UNITY_ANDROID
						if (_instance == null && Application.platform == RuntimePlatform.Android)
						{
							_instance = new YandexAppMetricaAndroid();
						}
#endif
						if (_instance == null)
						{
							_instance = new YandexAppMetricaDummy();
						}
					}
				}

				return _instance;
			}
		}

		private void Awake()
		{
			if (!_isInitialized)
			{
				_isInitialized = true;
				SetupMetrica();
			}
			else
			{
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			Instance.ResumeSession();
		}

		private void OnEnable()
		{
			if (_exceptionsReporting)
			{
#if UNITY_5 || UNITY_5_3_OR_NEWER
				Application.logMessageReceived += HandleLog;
#else
			Application.RegisterLogCallback(HandleLog);
#endif
			}
		}

		private void OnDisable()
		{
			if (_exceptionsReporting)
			{
#if UNITY_5 || UNITY_5_3_OR_NEWER
				Application.logMessageReceived -= HandleLog;
#else
			Application.RegisterLogCallback(null);
#endif
			}
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (_actualPauseStatus != pauseStatus)
			{
				_actualPauseStatus = pauseStatus;
				if (pauseStatus)
				{
					Instance.PauseSession();
				}
				else
				{
					Instance.ResumeSession();
				}
			}
		}

		private void SetupMetrica()
		{
			YandexAppMetricaConfig configuration = new YandexAppMetricaConfig(_apiKey)
			{
				SessionTimeout = (int)_sessionTimeoutSec,
				Logs = _logs,
				HandleFirstActivationAsUpdate = _handleFirstActivationAsUpdate,
				StatisticsSending = _statisticsSending,
				LocationTracking = _locationTracking
			};

			Instance.ActivateWithConfiguration(configuration);
		}

		private static void HandleLog(string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Exception)
			{
				Instance.ReportErrorFromLogCallback(condition, stackTrace);
			}
		}
#endif

    }
}