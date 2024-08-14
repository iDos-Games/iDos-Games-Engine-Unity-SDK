#if IDOSGAMES_CRYPTO_WALLET
using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class WalletView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _walletAddress;
        [SerializeField] private GameObject _connectedView;
        [SerializeField] private GameObject _disconnectedView;
        [SerializeField] private PanelCryptoWalletTokenBalance _walletBalance;
        [SerializeField] private PopUpInstallCryptoWallet _popUpInstallCryptoWallet;

        private void Start()
        {
            SetActivateConnectedView(false);
        }

        private void OnEnable()
        {
            UpdateView();
        }

        public void UpdateWalletAddress()
        {
            var address = "null"; //WalletConnectV2.ConnectedWalletAddress;

            _walletAddress.text = $"{address[..6]}...{address[^4..]}";
        }

        public void RefreshWalletBalance()
        {
            _walletBalance.Refresh();
        }

        public void ShowInstallCryptoWalletPopUp()
        {
            _popUpInstallCryptoWallet.gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            var isConnected = false; //WalletConnectV2.IsConnected;

            SetActivateConnectedView(isConnected);

            if (isConnected)
            {
                UpdateWalletAddress();
            }
        }

        private void SetActivateConnectedView(bool active)
        {
            _connectedView.SetActive(active);
            _disconnectedView.SetActive(!active);
        }
    }
}
#endif