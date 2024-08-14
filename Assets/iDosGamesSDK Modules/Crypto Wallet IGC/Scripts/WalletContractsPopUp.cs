using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class WalletContractsPopUp : MonoBehaviour
    {
        [SerializeField] private TMP_Text _igtAddress;
        [SerializeField] private TMP_Text _igtFullAddress;
        [SerializeField] private TMP_Text _igcAddress;
        [SerializeField] private TMP_Text _igcFullAddress;
        [SerializeField] private TMP_Text _nftAddress;
        [SerializeField] private TMP_Text _nftFullAddress;

        private void OnEnable()
        {
            UpdateAddresses();
        }

        private void UpdateAddresses()
        {
            _igtAddress.text = $"{BlockchainSettings.IGT_CONTRACT_ADDRESS[..6]}...{BlockchainSettings.IGT_CONTRACT_ADDRESS[^4..]}";
            _igcAddress.text = $"{BlockchainSettings.IGC_CONTRACT_ADDRESS[..6]}...{BlockchainSettings.IGC_CONTRACT_ADDRESS[^4..]}";
            _nftAddress.text = $"{BlockchainSettings.NFT_CONTRACT_ADDRESS[..6]}...{BlockchainSettings.NFT_CONTRACT_ADDRESS[^4..]}";

            _igtFullAddress.text = BlockchainSettings.IGT_CONTRACT_ADDRESS;
            _igcFullAddress.text = BlockchainSettings.IGC_CONTRACT_ADDRESS;
            _nftFullAddress.text = BlockchainSettings.NFT_CONTRACT_ADDRESS;
        }
    }
}
