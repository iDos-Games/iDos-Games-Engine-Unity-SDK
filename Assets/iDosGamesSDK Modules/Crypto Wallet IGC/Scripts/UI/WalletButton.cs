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
            IDosGamesData.Config.OnTitlePublicConfigurationUpdated += SetEnable;
		}

		private void OnDisable()
		{
            IDosGamesData.Config.OnTitlePublicConfigurationUpdated -= SetEnable;
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
            var systemState = IDosGamesData.Config.TitlePublicConfiguration?.SystemState;

            if (systemState?.Wallet == null)
            {
                return true;
            }

#if UNITY_ANDROID
            return systemState.Wallet.Android;
#elif UNITY_IOS
            return systemState.Wallet.Ios;
#else
            return true;
#endif
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
