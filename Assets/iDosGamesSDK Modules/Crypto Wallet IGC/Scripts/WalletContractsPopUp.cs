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
            _igtAddress.text = $"{BlockchainSettings.IgtContractAddress[..6]}...{BlockchainSettings.IgtContractAddress[^4..]}";
            _igcAddress.text = $"{BlockchainSettings.IgcContractAddress[..6]}...{BlockchainSettings.IgcContractAddress[^4..]}";
            _nftAddress.text = $"{BlockchainSettings.NftContractAddress[..6]}...{BlockchainSettings.NftContractAddress[^4..]}";

            _igtFullAddress.text = BlockchainSettings.IgtContractAddress;
            _igcFullAddress.text = BlockchainSettings.IgcContractAddress;
            _nftFullAddress.text = BlockchainSettings.NftContractAddress;
        }
    }
}
