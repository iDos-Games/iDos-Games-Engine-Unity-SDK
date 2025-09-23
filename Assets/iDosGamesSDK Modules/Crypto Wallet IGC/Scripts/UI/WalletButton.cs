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
		[SerializeField] private WalletWindow _evmWallet;
        [SerializeField] private GameObject _solanaWallet;
#endif
        private Button _button;
        private ChainType _chainType = ChainType.EVM;

        private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();
			SetEnable();
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
            switch (_chainType)
            {
                case ChainType.EVM:
                    if (_evmWallet != null)
                    {
                        if (_solanaWallet != null) _solanaWallet.SetActive(false);
                        _evmWallet.gameObject.SetActive(true);
                    }
                    break;

                case ChainType.Solana:
                    if (_solanaWallet != null)
                    {
                        if (_evmWallet != null) _evmWallet.gameObject.SetActive(false);
                        _solanaWallet.SetActive(true);
                    }
                    break;
            }
#endif
        }

		private void SetEnable()
		{
            _chainType = ParseChainType(BlockchainSettings.ChainType);
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

        private static ChainType ParseChainType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ChainType.EVM;

            var s = value.Trim();

            if (s.Equals("EVM", System.StringComparison.OrdinalIgnoreCase))
                return ChainType.EVM;
            if (s.Equals("Solana", System.StringComparison.OrdinalIgnoreCase))
                return ChainType.Solana;
            return ChainType.EVM;
        }
    }
}
