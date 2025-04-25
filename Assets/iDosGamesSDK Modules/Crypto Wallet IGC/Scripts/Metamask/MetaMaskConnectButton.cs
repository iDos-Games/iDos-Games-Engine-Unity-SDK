using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace IDosGames
{
    public class MetaMaskConnectButton : MonoBehaviour
    {
        [SerializeField] private Button _connectButton;
        [SerializeField] private MetaMaskWalletService _walletService;

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
            // Подписка на события MetaMaskWalletService, если они есть
            // Например, если есть делегаты или UnityEvents для EthereumEnabled и NewAccountSelected
            // _walletService.OnEthereumEnabled += UpdateButtonState;
            // _walletService.OnNewAccountSelected += UpdateButtonState;
        }

        private void OnDisable()
        {
            // Отписка от событий
            // _walletService.OnEthereumEnabled -= UpdateButtonState;
            // _walletService.OnNewAccountSelected -= UpdateButtonState;
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
            if (_connectButton != null)
            {
                _connectButton.interactable = !_walletService.IsConnected;
            }
        }
    }
}