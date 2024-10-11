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
            _igtAddress.text = $"{BlockchainSettings.HardTokenContractAddress[..6]}...{BlockchainSettings.HardTokenContractAddress[^4..]}";
            _igcAddress.text = $"{BlockchainSettings.SoftTokenContractAddress[..6]}...{BlockchainSettings.SoftTokenContractAddress[^4..]}";
            _nftAddress.text = $"{BlockchainSettings.NftContractAddress[..6]}...{BlockchainSettings.NftContractAddress[^4..]}";

            _igtFullAddress.text = BlockchainSettings.HardTokenContractAddress;
            _igcFullAddress.text = BlockchainSettings.SoftTokenContractAddress;
            _nftFullAddress.text = BlockchainSettings.NftContractAddress;
        }
    }
}
