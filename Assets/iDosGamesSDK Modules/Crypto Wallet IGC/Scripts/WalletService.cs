#if IDOSGAMES_CRYPTO_WALLET
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
	public static class WalletService
	{
		public static string TransactionHashAfterTransactionToGame { get; private set; }

        public static async Task<string> TransferTokenToGame(VirtualCurrencyID virtualCurrencyID, int amount)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			var companyWalletAddress = BlockchainSettings.CompanyCryptoWallet;

			var transactionHash = await WalletBlockchainService.TransferERC20TokenAndGetHash(WalletManager.WalletAddress, companyWalletAddress, virtualCurrencyID, amount, WalletManager.PrivateKey);


            TransactionHashAfterTransactionToGame = transactionHash;

			if (string.IsNullOrEmpty(TransactionHashAfterTransactionToGame))
			{
				return null;
			}

			var request = new WalletTransactionRequest
			{
				ChainID = BlockchainSettings.ChainID,
				TransactionType = CryptoTransactionType.Token,
				Direction = TransactionDirection.Game,
				TransactionHash = transactionHash
			};

			Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED);

			return await IGSService.TryMakeTransaction(request);
		}

		public static async Task<string> TransferNFTToGame(BigInteger nftID, int amount)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			var companyWalletAddress = BlockchainSettings.CompanyCryptoWallet;

			var transactionHash = await WalletBlockchainService.TransferNFT1155AndGetHash(WalletManager.WalletAddress, companyWalletAddress, nftID, amount, WalletManager.PrivateKey);

			TransactionHashAfterTransactionToGame = transactionHash;

			if (string.IsNullOrEmpty(TransactionHashAfterTransactionToGame))
			{
				return null;
			}

			var request = new WalletTransactionRequest
			{
				ChainID = BlockchainSettings.ChainID,
				TransactionType = CryptoTransactionType.NFT,
				Direction = TransactionDirection.Game,
				TransactionHash = transactionHash
			};

			Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED);

			return await IGSService.TryMakeTransaction(request);
		}

		public static async Task<string> TransferTokenToUsersCryptoWallet(VirtualCurrencyID virtualCurrencyID, int amount)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			var request = new WalletTransactionRequest
			{
				ChainID = BlockchainSettings.ChainID,
				TransactionType = CryptoTransactionType.Token,
				Direction = TransactionDirection.UsersCryptoWallet,
				CurrencyID = virtualCurrencyID,
				Amount = amount,
				ConnectedWalletAddress = WalletManager.WalletAddress
            };

			Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED);

			return await IGSService.TryMakeTransaction(request);
		}

		public static async Task<string> TransferNFTToUsersCryptoWallet(string skinID, int amount)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			var request = new WalletTransactionRequest
			{
				ChainID = BlockchainSettings.ChainID,
				TransactionType = CryptoTransactionType.NFT,
				Direction = TransactionDirection.UsersCryptoWallet,
				SkinID = skinID,
				Amount = amount,
                ConnectedWalletAddress = WalletManager.WalletAddress
            };

			Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED);

			return await IGSService.TryMakeTransaction(request);
		}

		public static async Task<decimal> GetTokenBalance(VirtualCurrencyID virtualCurrencyID)
		{
			if (!IsWalletReady())
			{
				return 0;
			}

			return await WalletBlockchainService.GetERC20TokenBalance(WalletManager.WalletAddress, virtualCurrencyID);
		}

		public static async Task<List<BigInteger>> GetNFTBalance(List<BigInteger> nftIDs)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			return await WalletBlockchainService.GetNFTBalance(WalletManager.WalletAddress, nftIDs);

        }

		public static async Task<decimal> GetNativeTokenBalance()
		{
			if (!IsWalletReady())
			{
				return 0;
			}

			return await WalletBlockchainService.GetNativeTokenBalance(WalletManager.WalletAddress);

        }

		private static bool IsWalletReady()
		{
			bool isReady = true;

			if (string.IsNullOrEmpty(WalletManager.WalletAddress))
			{
				isReady = false;
			}

			if (!isReady)
			{
				Debug.LogWarning("WalletConnect is not ready.");
			}

			return isReady;
		}
	}
}
#endif