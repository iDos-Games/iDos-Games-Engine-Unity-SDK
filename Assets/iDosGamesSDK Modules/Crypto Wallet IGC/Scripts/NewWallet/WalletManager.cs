using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class WalletManager : MonoBehaviour
    {
        [SerializeField] private PanelCryptoWalletTokenBalance _walletBalance;

        [SerializeField] private TMP_Text _walletAddress;
        [SerializeField] private TMP_Text _walletAddressCopy;
        [SerializeField] private GameObject disconnectedPanel;
        [SerializeField] private GameObject connectedPanel;
        [SerializeField] private GameObject createWalletPanel;
        [SerializeField] private GameObject importWalletPanel;
        [SerializeField] private GameObject getPrivateKeyPanel;

        public static string WalletAddress { get; set; }
        public static int ChainID;

        public static string PrivateKey { get; set; }
        public static string SeedPhrase { get; set; }

        private void OnEnable()
        {
            WalletAddress = PlayerPrefs.GetString("WalletAddress", null);

            if(string.IsNullOrEmpty(WalletAddress))
            {
                OpenDisconnectedPanel();
            }
            else
            {
                OpenConnectedWallet();
            }
        }

        private void OnDisable()
        {
            NulledPrivateKey();
        }

        public void NulledPrivateKey()
        {
            PrivateKey = null;
            SeedPhrase = null;
        }

        public void OpenConnectedWallet()
        {
            BlockchainSettings.SetWallet();

            connectedPanel.SetActive(true);
            disconnectedPanel.SetActive(false);
            createWalletPanel.SetActive(false);
            importWalletPanel.SetActive(false);
            getPrivateKeyPanel.SetActive(false);

            UpdateWalletAddress();

#if IDOSGAMES_CRYPTO_WALLET
            _walletBalance.Refresh();
#endif
        }

        public void OpenDisconnectedPanel()
        {
            connectedPanel.SetActive(false);
            disconnectedPanel.SetActive(true);
            createWalletPanel.SetActive(false);
            importWalletPanel.SetActive(false);
            getPrivateKeyPanel.SetActive(false);
        }

        public void OpenCreateWalletPanel()
        {
            connectedPanel.SetActive(false);
            disconnectedPanel.SetActive(false);
            createWalletPanel.SetActive(true);
            importWalletPanel.SetActive(false);
            getPrivateKeyPanel.SetActive(false);
        }

        public void OpenImportWalletPanel()
        {
            connectedPanel.SetActive(false);
            disconnectedPanel.SetActive(false);
            createWalletPanel.SetActive(false);
            importWalletPanel.SetActive(true);
            getPrivateKeyPanel.SetActive(false);
        }

        public void UpdateWalletAddress()
        {
            var address = WalletAddress;

            _walletAddress.text = $"{address[..6]}...{address[^4..]}";
            _walletAddressCopy.text = WalletAddress;
        }

        public void Disconnect()
        {
            WalletAddress = null;
            PlayerPrefs.SetString("WalletAddress", null);
            OpenDisconnectedPanel();
        }

        public void RefreshWalletBalance()
        {
#if IDOSGAMES_CRYPTO_WALLET
            _walletBalance.Refresh();
#endif
        }

        public void UpdateView()
        {
            UpdateWalletAddress();
        }
    }
}
