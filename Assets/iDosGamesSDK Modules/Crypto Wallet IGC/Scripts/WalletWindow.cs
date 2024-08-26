using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
				transferResult = await WalletService.TransferTokenToGame(virtualCurrencyID, amount);
				transactionHash = WalletService.TransactionHashAfterTransactionToGame;

				if (string.IsNullOrEmpty(transactionHash))
				{
					return transferResult; // User cancelled
				}
			}
			else if (direction == TransactionDirection.UsersCryptoWallet)
			{
				transferResult = await WalletService.TransferTokenToUsersCryptoWallet(virtualCurrencyID, amount);
				transactionHash = GetTransactionHashFromResultMessage(transferResult);
			}

			if(IDosGamesSDKSettings.Instance.DebugLogging)
			{
                Debug.Log("TransferResult: " + transferResult);
            }
			
			ProcessResultMessage(transferResult);

			if (transferResult != null)
			{
				if (transactionHash != null && transactionHash != string.Empty)
				{
					int chainID = (int)BlockchainNetwork.IgcTestnet;
					WalletTransactionHistory.SaveNewItem(chainID, transactionHash, direction,
						GetTokenName(virtualCurrencyID), amount,
						GetTokenImagePath(virtualCurrencyID));

					_walletManager.RefreshWalletBalance();
					UserDataService.RequestUserAllData();
				}
			}

			return transferResult;
		}

		public async Task<string> TransferNFT(TransactionDirection direction, string skinID, int amount)
		{
			string transferResult = null;

			string transactionHash = string.Empty;

			if (direction == TransactionDirection.Game)
			{
				var nftID = UserDataService.GetCachedSkinItem(skinID).NFTID;

				transferResult = await WalletService.TransferNFTToGame(nftID, amount);
				transactionHash = WalletService.TransactionHashAfterTransactionToGame;

				if (string.IsNullOrEmpty(transactionHash))
				{
					return transferResult; // User cancelled
				}
			}
			else if (direction == TransactionDirection.UsersCryptoWallet)
			{
				transferResult = await WalletService.TransferNFTToUsersCryptoWallet(skinID, amount);
				transactionHash = GetTransactionHashFromResultMessage(transferResult);
			}

            if (IDosGamesSDKSettings.Instance.DebugLogging)
			{
                Debug.Log("TransferResult: " + transferResult);
            }

			ProcessResultMessage(transferResult);

			if (transferResult != null)
			{
				if (transactionHash != null && transactionHash != string.Empty)
				{
					int chainID = (int)BlockchainNetwork.IgcTestnet;
					WalletTransactionHistory.SaveNewItem(chainID, transactionHash, direction,
						UserDataService.GetCachedSkinItem(skinID).DisplayName, amount,
						UserDataService.GetCachedSkinItem(skinID).ImagePath);

					_walletManager.RefreshWalletBalance();
					UserDataService.RequestUserAllData();
				}
			}

			return transferResult;
		}

		private static void ProcessResultMessage(string result)
		{
			if (result == null)
			{
				Message.Show(MessageCode.SOMETHING_WENT_WRONG);
				return;
			}

			var resultJson = JsonConvert.DeserializeObject<JObject>(result);

			if (resultJson.ContainsKey(JsonProperty.MESSAGE_KEY))
			{
				Message.Show(resultJson[JsonProperty.MESSAGE_KEY].ToString());
			}
			else
			{
				Message.Show(MessageCode.SOMETHING_WENT_WRONG);
			}
		}

		private static string GetTransactionHashFromResultMessage(string result)
		{
			if (result == null)
			{
				Message.Show(MessageCode.SOMETHING_WENT_WRONG);
				return result;
			}

			var resultJson = JsonConvert.DeserializeObject<JObject>(result);

			if (resultJson.ContainsKey("TransactionHash"))
			{
				return resultJson["TransactionHash"].ToString();
			}
			else
			{
				Message.Show(MessageCode.SOMETHING_WENT_WRONG);
			}

			return null;
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

		private void OnWalletConnected()
		{
			Loading.HideAllPanels();
			_walletManager.UpdateView();
		}

		private void OnWalletDisconnected()
		{
			_walletManager.UpdateView();
		}

		private void OnInitializationFailed()
		{
			//Message.ShowConnectionError(async () => await WalletConnectV2.Instance.Initialize());
		}

		private void OnFailedToConnectToWallet()
		{
			Message.Show(MessageCode.FAILED_TO_CONNECT);
		}
#endif
    }
}
