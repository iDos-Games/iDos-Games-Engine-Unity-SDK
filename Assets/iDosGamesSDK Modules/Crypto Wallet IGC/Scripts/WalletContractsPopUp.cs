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
            UpdateAddress(BlockchainSettings.HardTokenContractAddress, _igtAddress, _igtFullAddress);
            UpdateAddress( BlockchainSettings.SoftTokenContractAddress, _igcAddress, _igcFullAddress);
            UpdateAddress(BlockchainSettings.NftContractAddress, _nftAddress, _nftFullAddress);
        }

        private void UpdateAddress(string contractAddress, TMP_Text shortText, TMP_Text fullText)
        {
            if (!string.IsNullOrEmpty(contractAddress))
            {
                shortText.text = $"{contractAddress[..6]}...{contractAddress[^4..]}";
                fullText.text = contractAddress;
            }
            else
            {
                shortText.text = "N/A";
                fullText.text = "N/A";
            }
        }
    }
}
