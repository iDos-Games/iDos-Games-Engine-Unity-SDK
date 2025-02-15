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

			string platformPoolAddress = BlockchainSettings.PlatformPoolContractAddress;

			string tokenAddress = BlockchainSettings.GetTokenContractAddress(virtualCurrencyID);

			string approveHash = await WalletBlockchainService.ApproveERC20Token(tokenAddress, platformPoolAddress, amount.ToString(), WalletManager.PrivateKey, BlockchainSettings.ChainID);

            if (string.IsNullOrEmpty(approveHash))
            {
                Debug.LogWarning("Approve transaction failed.");
                return null;
            }

            bool isApproved = await WalletBlockchainService.WaitForTransactionReceipt(approveHash);
            if (!isApproved)
            {
                Debug.LogWarning("Approve transaction was not confirmed.");
                return null;
            }

            var transactionHash = await WalletBlockchainService.DepositERC20Token(tokenAddress, platformPoolAddress, amount.ToString(), AuthService.UserID, WalletManager.PrivateKey, BlockchainSettings.ChainID);

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

        public static async Task<string> TransferTokenToUser(WithdrawalSignatureResult signature)
        {
            if (!IsWalletReady())
            {
                return null;
            }

			var transactionHash = await WalletBlockchainService.WithdrawERC20Token(signature, WalletManager.PrivateKey, BlockchainSettings.ChainID);

            TransactionHashAfterTransactionToGame = transactionHash;

            if (string.IsNullOrEmpty(TransactionHashAfterTransactionToGame))
            {
                return null;
            }

            Message.Show(TransactionHashAfterTransactionToGame);

            return TransactionHashAfterTransactionToGame;
        }

        public static async Task<string> TransferNFTToGame(BigInteger nftID, int amount)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			var companyWalletAddress = BlockchainSettings.HotWalletAddress;

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

		public static async Task<string> GetTokenWithdrawalSignature(VirtualCurrencyID virtualCurrencyID, int amount)
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

			//Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED);

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

        public static async Task<bool> HasSufficientBalanceForGas(decimal gas = 300000)
        {
            if (!IsWalletReady())
            {
                return false;
            }

            decimal balanceInEther = await GetNativeTokenBalance();
            decimal gasPriceInGwei = (decimal)BlockchainSettings.GasPrice;
            decimal gasPriceInEther = gasPriceInGwei * 1e-9m;
            decimal requiredGasInEther = gas * gasPriceInEther;

            if (balanceInEther >= requiredGasInEther)
            {
                return true;
            }

            Debug.LogWarning("Insufficient balance for gas.");
            return false;
        }
    }
}
#endif