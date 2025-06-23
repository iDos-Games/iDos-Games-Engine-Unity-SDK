#if IDOSGAMES_CRYPTO_WALLET
using Nethereum.Web3;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
	public static class WalletService
	{
		public static string TransactionHashAfterTransactionToGame { get; private set; }
        public static string TransactionHashAfterTransferToUser { get; private set; }
        public static string TransactionHashAfterTransferToExternalAddress { get; private set; }

        public static async Task<string> TransferTokenToGame(VirtualCurrencyID virtualCurrencyID, int amount)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			string platformPoolAddress = BlockchainSettings.PlatformPoolContractAddress;
			string tokenAddress = BlockchainSettings.GetTokenContractAddress(virtualCurrencyID);
            string userAddress = WalletManager.WalletAddress;

            BigInteger currentAllowanceWei = await WalletBlockchainService.GetERC20Allowance(
                tokenAddress,
                userAddress,
                platformPoolAddress
            );

			Debug.Log("CurrentAllowanceWei: " + currentAllowanceWei);

            BigInteger requiredAmountWei = Web3.Convert.ToWei(amount);

            string maxAllowance = "115792089237316195423570985008687907853269984665640564039457584007913129639935";

            if (currentAllowanceWei < requiredAmountWei)
            {
                string approveHash = await WalletBlockchainService.ApproveERC20Token(
                    tokenAddress,
                    platformPoolAddress,
                    maxAllowance,
                    WalletManager.PrivateKey,
                    BlockchainSettings.ChainID
                );

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
            }

            var transactionHash = await WalletBlockchainService.DepositERC20Token(tokenAddress, platformPoolAddress, requiredAmountWei.ToString(), AuthService.UserID, WalletManager.PrivateKey, BlockchainSettings.ChainID);

            TransactionHashAfterTransactionToGame = transactionHash;

			if (string.IsNullOrEmpty(TransactionHashAfterTransactionToGame))
			{
				Debug.LogWarning("TransferTokenToGame TransactionHash is null");
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

            TransactionHashAfterTransferToUser = transactionHash;

            if (string.IsNullOrEmpty(TransactionHashAfterTransferToUser))
            {
                return null;
            }

            Message.Show(TransactionHashAfterTransferToUser);

            return TransactionHashAfterTransferToUser;
        }

        public static async Task<string> TransferNFTToGame(BigInteger nftID, int amount)
		{
			if (!IsWalletReady())
			{
				return null;
			}

			var transactionHash = await WalletBlockchainService.TransferNFT1155AndGetHash(WalletManager.WalletAddress, BlockchainSettings.PlatformPoolContractAddress, nftID, amount, WalletManager.PrivateKey, BlockchainSettings.ChainID);

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

        public static async Task<string> TransferNFTToUser(WithdrawalSignatureResult signature)
        {
            if (!IsWalletReady())
            {
                return null;
            }

            var transactionHash = await WalletBlockchainService.WithdrawERC1155Token(signature, WalletManager.PrivateKey, BlockchainSettings.ChainID);

            TransactionHashAfterTransferToUser = transactionHash;

            if (string.IsNullOrEmpty(TransactionHashAfterTransferToUser))
            {
                return null;
            }

            Message.Show(TransactionHashAfterTransferToUser);

            return TransactionHashAfterTransferToUser;
        }

        public static async Task<string> TransferTokenToExternalAddress(VirtualCurrencyID virtualCurrencyID, int amount, string toAddress)
        {
            if (!IsWalletReady())
            {
                return null;
            }

            var transactionHash = await WalletBlockchainService.TransferERC20TokenAndGetHash(WalletManager.WalletAddress, toAddress, virtualCurrencyID, amount, WalletManager.PrivateKey, BlockchainSettings.ChainID);

            TransactionHashAfterTransferToExternalAddress = transactionHash;

            if (string.IsNullOrEmpty(TransactionHashAfterTransferToExternalAddress))
            {
                return null;
            }

            Message.Show(TransactionHashAfterTransferToExternalAddress);

            return TransactionHashAfterTransferToExternalAddress;
        }

        public static async Task<string> TransferNFTToExternalAddress(BigInteger nftID, int amount, string toAddress)
        {
            if (!IsWalletReady())
            {
                return null;
            }

            var transactionHash = await WalletBlockchainService.TransferNFT1155AndGetHash(WalletManager.WalletAddress, toAddress, nftID, amount, WalletManager.PrivateKey, BlockchainSettings.ChainID);

            TransactionHashAfterTransactionToGame = transactionHash;

            if (string.IsNullOrEmpty(TransactionHashAfterTransactionToGame))
            {
                return null;
            }

            Message.Show(TransactionHashAfterTransferToExternalAddress);

            return TransactionHashAfterTransferToExternalAddress;
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

		public static async Task<string> GetNFTWithdrawalSignature(string skinID, int amount)
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

		public static async Task<BigInteger> GetTokenBalance(VirtualCurrencyID virtualCurrencyID)
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

		public static async Task<BigInteger> GetNativeTokenBalanceInWei()
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
				Debug.LogWarning("Crypto Wallet is not ready.");
			}

			return isReady;
		}

        public static async Task<bool> HasSufficientBalanceForGas(decimal gas = 300000)
        {
            if (!IsWalletReady())
            {
                return false;
            }

            BigInteger balanceInWei = await GetNativeTokenBalanceInWei();

            BigInteger gasBigInt = new BigInteger(gas);
            decimal gasPriceInGwei = BlockchainSettings.GasPrice;
            BigInteger gasPriceInWei = new BigInteger(gasPriceInGwei * 1_000_000_000m);
            BigInteger requiredGasInWei = gasBigInt * gasPriceInWei;

            if (balanceInWei >= requiredGasInWei)
            {
                return true;
            }

            Debug.LogWarning("Insufficient balance for gas.");
            return false;
        }
    }
}
#endif