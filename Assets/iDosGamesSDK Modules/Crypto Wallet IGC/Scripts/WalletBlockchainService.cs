using Nethereum.Contracts.Standards.ERC1155;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using BalanceOfERC20Function = Nethereum.Contracts.Standards.ERC20.ContractDefinition.BalanceOfFunction;
using TransferFunction = Nethereum.Contracts.Standards.ERC20.ContractDefinition.TransferFunction;

namespace IDosGames
{
    public static class WalletBlockchainService
    {
        public static async Task<decimal> GetERC20TokenBalance(string walletAddress, VirtualCurrencyID virtualCurrencyID)
        {
            try
            {
                string contractABI = BlockchainSettings.GetTokenContractABI(virtualCurrencyID);
                string contractAddress = BlockchainSettings.GetTokenContractAddress(virtualCurrencyID);
                var web3 = new Web3(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));
                var contract = web3.Eth.GetContract(contractABI, contractAddress);
                var balanceOfFunction = contract.GetFunction<BalanceOfERC20Function>();
                var functionInput = new BalanceOfERC20Function
                {
                    Owner = walletAddress
                };
                var balanceResult = await balanceOfFunction.CallAsync<BigInteger>(functionInput);
                return Web3.Convert.FromWei(balanceResult);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Get BalanceOf Error: " + ex.Message);
                return 0;
            }
        }

        public static async Task<List<BigInteger>> GetNFTBalance(string walletAddress, List<BigInteger> nftIDs)
        {
            try
            {
                var web3 = new Web3(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));
                var erc1155Service = new ERC1155Service(web3.Eth);
                var contractAddress = BlockchainSettings.NFT_CONTRACT_ADDRESS;
                var contractService = erc1155Service.GetContractService(contractAddress);
                var balanceOfBatchFunction = new BalanceOfBatchFunction
                {
                    Accounts = Enumerable.Repeat(walletAddress, nftIDs.Count).ToList(),
                    Ids = nftIDs
                };
                var balance = await contractService.BalanceOfBatchQueryAsync(balanceOfBatchFunction);
                return balance;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Get BalanceOf Error: " + ex.Message);
                return new List<BigInteger>();
            }
        }

        public static async Task<decimal> GetNativeTokenBalance(string walletAddress)
        {
            try
            {
                var web3 = new Web3(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));
                var balanceResult = await web3.Eth.GetBalance.SendRequestAsync(walletAddress);
                return Web3.Convert.FromWei(balanceResult);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Get BalanceOf Error: " + ex.Message);
                return 0;
            }
        }

        public static async Task<string> TransferERC20TokenAndGetHash(string fromAddress, string toAddress, VirtualCurrencyID tokenID, int amount, string privateKey)
        {
            try
            {
                var account = new Account(privateKey);
                var web3 = new Web3(account, BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));
                var contractAddress = BlockchainSettings.GetTokenContractAddress(tokenID);

                var transferFunction = new TransferFunction
                {
                    FromAddress = fromAddress,
                    To = toAddress,
                    Value = new HexBigInteger(Web3.Convert.ToWei(amount, UnitConversion.EthUnit.Ether)),
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GAS_PRICE, UnitConversion.EthUnit.Gwei)),
                    AmountToSend = new HexBigInteger(BlockchainSettings.DEFAULT_VALUE_IN_NATIVE_TOKEN)
                };

                var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();

                var estimateGas = await transferHandler.EstimateGasAsync(contractAddress, transferFunction);

                transferFunction.Gas = estimateGas;

                var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transferFunction);

                return transactionReceipt.TransactionHash;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Transfer Error: " + ex.Message);
                return null;
            }
        }

        public static async Task<string> TransferNFT1155AndGetHash(string fromAddress, string toAddress, BigInteger nftID, int amount, string privateKey)
        {
            try
            {
                var account = new Account(privateKey);
                var web3 = new Web3(account, BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));

                var contractAddress = BlockchainSettings.NFT_CONTRACT_ADDRESS;
                var contractABI = BlockchainSettings.NFT_CONTRACT_ABI;

                var transferFunction = new SafeTransferFromFunction
                {
                    From = fromAddress,
                    To = toAddress,
                    Id = nftID,
                    Amount = amount,
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GAS_PRICE, UnitConversion.EthUnit.Gwei)),
                    Data = Array.Empty<byte>()
                };

                var transferHandler = web3.Eth.GetContractTransactionHandler<SafeTransferFromFunction>();

                var estimateGas = await transferHandler.EstimateGasAsync(contractAddress, transferFunction);

                transferFunction.Gas = estimateGas;

                var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transferFunction);

                return transactionReceipt.TransactionHash;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Transfer Error: " + ex.Message);
                return null;
            }
        }
    }
}