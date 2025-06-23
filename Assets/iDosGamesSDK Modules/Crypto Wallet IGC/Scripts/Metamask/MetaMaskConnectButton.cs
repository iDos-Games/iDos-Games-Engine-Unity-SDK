using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class MetaMaskConnectButton : MonoBehaviour
    {
#if UNITY_WEBGL && IDOSGAMES_CRYPTO_WALLET
        [SerializeField] private Button _connectButton;
        [SerializeField] private GameObject _chainChangeInfo;
        [SerializeField] private GameObject _depositTokenInfo;

        [SerializeField] private MetaMaskWalletService _walletService;

        [SerializeField] private TMP_InputField _amountInputField;

        private void Start()
        {
            if (_connectButton == null)
            {
                Debug.LogWarning("Connect Button is not assigned in the Inspector.");
                return;
            }

            if (_walletService == null)
            {
                Debug.LogWarning("MetaMaskWalletService is not assigned in the Inspector.");
                return;
            }

            _connectButton.onClick.AddListener(ConnectWallet);
            UpdateButtonState();
        }

        private void OnEnable()
        {
            _walletService.OnEthereumEnabled += UpdateButtonState;
            _walletService.OnAccountChanged += UpdateButtonState;
            UpdateButtonState();
        }

        private void OnDisable()
        {
            _walletService.OnEthereumEnabled -= UpdateButtonState;
            _walletService.OnAccountChanged -= UpdateButtonState;
            _connectButton?.onClick.RemoveListener(ConnectWallet);
        }

        private async void ConnectWallet()
        {
            try
            {
                await _walletService.Connect();
                UpdateButtonState();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to connect to MetaMask: {ex.Message}");
                UpdateButtonState();
            }
        }

        private void UpdateButtonState()
        {
            if (_walletService == null)
            {
                Debug.LogWarning("Cannot update button state: MetaMaskWalletService is not assigned.");
                return;
            }

            if (_connectButton == null || _chainChangeInfo == null || _depositTokenInfo == null)
            {
                if (_connectButton == null) Debug.LogWarning("Connect Button is not assigned.");
                if (_chainChangeInfo == null) Debug.LogWarning("Chain Change Info is not assigned.");
                if (_depositTokenInfo == null) Debug.LogWarning("Deposit Token Info is not assigned.");
                return;
            }

            _connectButton.gameObject.SetActive(false);
            _chainChangeInfo.SetActive(false);
            _depositTokenInfo.SetActive(false);

            if (!_walletService.IsConnected)
            {
                _connectButton.gameObject.SetActive(true);
                return;
            }

            bool isCorrectChain = _walletService._currentChainID == BlockchainSettings.ChainID;
            if (!isCorrectChain)
            {
                _chainChangeInfo.SetActive(true);
            }
            else
            {
                _depositTokenInfo.SetActive(true);
            }
        }

        public void ChangeAmount(int amount)
        {
            _amountInputField.text = amount.ToString();
        }

        public async void TransferTokenToGame()
        {
            if (!int.TryParse(_amountInputField.text, out int amount))
            {
                Message.Show("Invalid amount entered. Please enter a valid number.");
                return;
            }

            try
            {
                string result = await _walletService.TransferTokenToGame(amount);
                if (result != null)
                {
                    Debug.Log($"Transfer successful. Transaction hash: {result}");
                    UserDataService.RequestUserAllData();
                }
                else
                {
                    Debug.LogWarning("Transfer failed or was cancelled.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Transfer failed: {ex.Message}");
            }
        }

#endif
    }
}
