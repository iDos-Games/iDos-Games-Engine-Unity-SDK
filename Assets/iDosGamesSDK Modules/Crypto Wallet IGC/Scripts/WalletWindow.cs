using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
	public class WalletWindow : MonoBehaviour
	{
		[SerializeField] private WalletManager _walletManager;

#if IDOSGAMES_CRYPTO_WALLET
		
		public async Task<string> TransferToken(TransactionDirection direction, VirtualCurrencyID virtualCurrencyID, int amount)
		{
			string transferResult = null;

			string transactionHash = string.Empty;

			if (direction == TransactionDirection.Game)
			{
                Loading.ShowTransparentPanel();

				bool balance = await WalletService.HasSufficientBalanceForGas(150000);
				if (balance)
				{
                    transferResult = await WalletService.TransferTokenToGame(virtualCurrencyID, amount);
                    transactionHash = WalletService.TransactionHashAfterTransactionToGame;
                    Loading.HideAllPanels();
                }
				else
				{
                    Message.Show(MessageCode.INSUFFICIENT_BALANCE_FOR_GAS);
                    Loading.HideAllPanels();
                    return null;
				}
                
                if (string.IsNullOrEmpty(transactionHash))
				{
					return transferResult; // User cancelled
				}
			}
			else if (direction == TransactionDirection.UsersCryptoWallet)
			{
				Loading.ShowTransparentPanel();

                bool balance = await WalletService.HasSufficientBalanceForGas(150000);
                if (balance)
				{
                    string signatureString = await WalletService.GetTokenWithdrawalSignature(virtualCurrencyID, amount);
                    var signature = JsonConvert.DeserializeObject<WithdrawalSignatureResult>(signatureString);
                    transactionHash = await WalletService.TransferTokenToUser(signature);
                    Loading.HideAllPanels();
                }
				else
				{
                    Message.Show(MessageCode.INSUFFICIENT_BALANCE_FOR_GAS);
                    Loading.HideAllPanels();
                    return null;
                }
            }
            else if (direction == TransactionDirection.ExternalWalletAddress)
            {
                Loading.ShowTransparentPanel();

                bool balance = await WalletService.HasSufficientBalanceForGas(150000);
                if (balance)
                {
                    transferResult = await WalletService.TransferTokenToExternalAddress(virtualCurrencyID, amount, WalletManager.ToAddress);
                    transactionHash = WalletService.TransactionHashAfterTransferToExternalAddress;
                    Loading.HideAllPanels();
                }
                else
                {
                    Message.Show(MessageCode.INSUFFICIENT_BALANCE_FOR_GAS);
                    Loading.HideAllPanels();
                    return null;
                }
            }

			if(IDosGamesSDKSettings.Instance.DebugLogging)
			{
                Debug.Log("Transaction Hash: " + transactionHash);
            }

            //ProcessResultMessage(transferResult);

            if (transactionHash != null && transactionHash != string.Empty)
            {
                int chainID = BlockchainSettings.ChainID;
                WalletTransactionHistory.SaveNewItem(chainID, transactionHash, direction,
                    GetTokenName(virtualCurrencyID), amount,
                    GetTokenImagePath(virtualCurrencyID));

                _walletManager.RefreshWalletBalance();
                _ = UserService.GetUserInventory();
            }

            Message.Show(transferResult);

            return transferResult;
		}

		public async Task<string> TransferNFT(TransactionDirection direction, string skinID, int amount)
		{
			string transferResult = null;

			string transactionHash = string.Empty;

			if (direction == TransactionDirection.Game)
			{
                Loading.ShowTransparentPanel();

                bool balance = await WalletService.HasSufficientBalanceForGas(150000);
				if (balance)
				{
                    var nftID = DataService.GetCachedSkinItem(skinID).NFTID;
                    transferResult = await WalletService.TransferNFTToGame(nftID, amount);
                    transactionHash = WalletService.TransactionHashAfterTransactionToGame;
                    Loading.HideAllPanels();
                }
                else
                {
                    Message.Show(MessageCode.INSUFFICIENT_BALANCE_FOR_GAS);
                    Loading.HideAllPanels();
                    return null;
                }

                if (string.IsNullOrEmpty(transactionHash))
				{
					return transferResult; // User cancelled
				}
			}
			else if (direction == TransactionDirection.UsersCryptoWallet)
			{
                Loading.ShowTransparentPanel();

                bool balance = await WalletService.HasSufficientBalanceForGas(150000);
				if (balance)
				{
                    string signatureString = await WalletService.GetNFTWithdrawalSignature(skinID, amount);
                    var signature = JsonConvert.DeserializeObject<WithdrawalSignatureResult>(signatureString);
                    transactionHash = await WalletService.TransferNFTToUser(signature);
                    Loading.HideAllPanels();
                }
				else
				{
                    Message.Show(MessageCode.INSUFFICIENT_BALANCE_FOR_GAS);
                    Loading.HideAllPanels();
                    return null;
                }
            }
            else if (direction == TransactionDirection.ExternalWalletAddress)
            {
                Loading.ShowTransparentPanel();

                bool balance = await WalletService.HasSufficientBalanceForGas(150000);
                if (balance)
                {
                    var nftID = DataService.GetCachedSkinItem(skinID).NFTID;
                    transferResult = await WalletService.TransferNFTToExternalAddress(nftID, amount, WalletManager.ToAddress);
                    transactionHash = WalletService.TransactionHashAfterTransferToExternalAddress;
                    Loading.HideAllPanels();
                }
                else
                {
                    Message.Show(MessageCode.INSUFFICIENT_BALANCE_FOR_GAS);
                    Loading.HideAllPanels();
                    return null;
                }
            }

            if (IDosGamesSDKSettings.Instance.DebugLogging)
			{
                Debug.Log("TransferResult: " + transferResult);
            }

            //ProcessResultMessage(transferResult);

            if (transactionHash != null && transactionHash != string.Empty)
            {
                int chainID = BlockchainSettings.ChainID;
                WalletTransactionHistory.SaveNewItem(chainID, transactionHash, direction,
                    DataService.GetCachedSkinItem(skinID).DisplayName, amount,
                    DataService.GetCachedSkinItem(skinID).ImagePath);

                _walletManager.RefreshWalletBalance();
                _ = UserService.GetUserInventory();
            }

            Message.Show(transferResult);

            return transferResult;
		}

		private string GetTokenName(VirtualCurrencyID currencyID)
		{
			switch (currencyID)
			{
				case VirtualCurrencyID.IG: return JsonProperty.IGT.ToUpper();
				case VirtualCurrencyID.CO: return JsonProperty.IGC.ToUpper();
			}

			return string.Empty;
		}

		private string GetTokenImagePath(VirtualCurrencyID currencyID)
		{
			switch (currencyID)
			{
				case VirtualCurrencyID.IG:
					return $"Sprites/Currency/{JsonProperty.IGT.ToUpper()}";
				case VirtualCurrencyID.CO:
					return $"Sprites/Currency/{JsonProperty.IGC.ToUpper()}";
			}

			return string.Empty;
		}

#endif
    }
}
