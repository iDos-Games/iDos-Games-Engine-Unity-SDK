using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class WalletButton : MonoBehaviour
	{
#if IDOSGAMES_CRYPTO_WALLET
		[SerializeField] private WalletWindow _window;
#endif
        private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();
		}

		private void OnEnable()
		{
			UserDataService.TitlePublicConfigurationUpdated += SetEnable;
		}

		private void OnDisable()
		{
			UserDataService.TitlePublicConfigurationUpdated -= SetEnable;
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(OpenWalletWindow);
		}

		private void OpenWalletWindow()
		{
#if IDOSGAMES_CRYPTO_WALLET
			_window.gameObject.SetActive(true);
#endif
		}

		private void SetEnable()
		{
			gameObject.SetActive(GetEnableState());
		}

		private bool GetEnableState()
		{
			bool enabled = true;

			var titleData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.SystemState);

			if (titleData == string.Empty)
			{
				return enabled;
			}

			var systemStateData = JsonConvert.DeserializeObject<JObject>(titleData);

			var platformData = systemStateData[JsonProperty.WALLET];

			string state = string.Empty;

#if UNITY_ANDROID
			state = $"{platformData[JsonProperty.ANDROID]}";
#elif UNITY_IOS
			state = $"{platformData[JsonProperty.IOS]}";
#endif

			if (state == string.Empty)
			{
				return enabled;
			}

			enabled = state == JsonProperty.ENABLED_VALUE;

			return enabled;
		}
	}
}
