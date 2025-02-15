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

        private void OnEnable()
        {
            UpdateHistory();
        }

        public void UpdateHistory()
        {
            JArray historyArray = WalletTransactionHistory.GetHistoryArray();

            // Check if historyArray is null  
            if (historyArray == null)
            {
                Debug.Log("History array is null.");
                SetActivateVoidText(true);
                return;
            }

            foreach (Transform child in _historyItemParent)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }

            SetActivateVoidText(historyArray.Count < 1);

            if (historyArray.Count < 1)
            {
                return;
            }

            foreach (var historyItem in historyArray)
            {
                if (historyItem == null)
                {
                    Debug.Log("Encountered a null history item.");
                    continue;
                }

                var item = Instantiate(_historyItemPrefab, _historyItemParent);

                if (item == null)
                {
                    Debug.LogError("Failed to instantiate history item prefab.");
                    continue;
                }

                // Check and parse transaction direction  
                TransactionDirection direction;
                string directionString = historyItem[JsonProperty.DIRECTION]?.ToString();
                if (!Enum.TryParse(directionString, out direction))
                {
                    Debug.Log($"Invalid transaction direction: {directionString}");
                    continue;
                }

                // Parse amount and chain ID safely  
                int amount = 0;
                int.TryParse(historyItem[JsonProperty.AMOUNT]?.ToString(), out amount);

                int chainID = 0;
                int.TryParse(historyItem[JsonProperty.CHAIN_ID]?.ToString(), out chainID);

                // Check and set item properties  
                string hash = historyItem[JsonProperty.HASH]?.ToString();
                string itemName = historyItem[JsonProperty.NAME]?.ToString();
                string imagePath = historyItem[JsonProperty.IMAGE_PATH]?.ToString();

                if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(imagePath))
                {
                    Debug.Log("Incomplete transaction data.");
                    continue;
                }

                item.Set(
                    hash: GetTransactionHashShortcut(hash),
                    direction: direction,
                    itemName: itemName,
                    amount: amount,
                    imagePath: imagePath,
                    urlToOpen: GetURLToTransactionExplorer(hash)
                );
            }
        }

        public string GetURLToTransactionExplorer(string transactionHash)
        {
            string urlExplorer = string.Empty;

#if IDOSGAMES_CRYPTO_WALLET
            urlExplorer = BlockchainSettings.BlockchainExplorerUrl + "/tx/" + transactionHash;
#endif

            return urlExplorer;
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