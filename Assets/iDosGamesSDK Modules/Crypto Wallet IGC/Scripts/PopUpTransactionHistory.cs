using Newtonsoft.Json.Linq;
using System;
using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class PopUpTransactionHistory : PopUp
    {
        [SerializeField] private TransactionHistoryItem _historyItemPrefab;
        [SerializeField] private Transform _historyItemParent;
        [SerializeField] private TMP_Text _voidText;

        public static string EXPLORER_URL_PREFIX;
        public const string ETHERSCAN_URL_PREFIX = "https://etherscan.io/tx/";

        private void OnEnable()
        {
            UpdateHistory();
        }

        public void UpdateHistory()
        {
            JArray historyArray = WalletTransactionHistory.GetHistoryArray();

            foreach (Transform child in _historyItemParent)
            {
                Destroy(child.gameObject);
            }

            SetActivateVoidText(historyArray.Count < 1);

            if (historyArray.Count < 1)
            {
                return;
            }

            foreach (var historyItem in historyArray)
            {
                var item = Instantiate(_historyItemPrefab, _historyItemParent);

                var direction = (TransactionDirection)Enum.Parse(typeof(TransactionDirection), historyItem[JsonProperty.DIRECTION].ToString());
                int amount = int.Parse(historyItem[JsonProperty.AMOUNT].ToString());
                int chainID = int.Parse(historyItem[JsonProperty.CHAIN_ID].ToString());
                BlockchainNetwork blockchainNetwork = (BlockchainNetwork)chainID;

                item.Set(
                    hash: GetTransactionHashShortcut(historyItem[JsonProperty.HASH].ToString()),
                    direction: direction,
                    itemName: historyItem[JsonProperty.NAME].ToString(),
                    amount: amount,
                    imagePath: historyItem[JsonProperty.IMAGE_PATH].ToString(),
                    urlToOpen: GetURLToTransactionExplorer(blockchainNetwork, historyItem[JsonProperty.HASH].ToString())
                );
            }
        }

        public string GetURLToTransactionExplorer(BlockchainNetwork blockchainNetwork, string transactionHash)
        {
#if IDOSGAMES_CRYPTO_WALLET
            EXPLORER_URL_PREFIX = IDosGamesSDKSettings.Instance.BlockchainExplorerUrl;
#endif
            string prefix;
            switch (blockchainNetwork)
            {
                default:
                case BlockchainNetwork.IgcTestnet:
                    prefix = EXPLORER_URL_PREFIX;
                    break;
                case BlockchainNetwork.None:
                    prefix = ETHERSCAN_URL_PREFIX;
                    break;
            }

            return prefix + transactionHash;
        }

        private string GetTransactionHashShortcut(string hash)
        {

            return $"{hash[..5]}...{hash[^3..]}";
        }

        private void SetActivateVoidText(bool active)
        {
            _voidText.gameObject.SetActive(active);
        }
    }
}