using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Util;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts.Standards.ERC1155;
using Nethereum.Contracts;
using Nethereum.Signer;
using BalanceOfERC20Function = Nethereum.Contracts.Standards.ERC20.ContractDefinition.BalanceOfFunction;
using TransferFunction = Nethereum.Contracts.Standards.ERC20.ContractDefinition.TransferFunction;

namespace IDosGames
{
    public static class WalletBlockchainService
    {
        public static async Task<decimal> GetNativeTokenBalance(string walletAddress)
        {
            try
            {
                // Формируем данные JSON-RPC запроса  
                var data = new
                {
                    jsonrpc = "2.0",
                    method = "eth_getBalance",
                    @params = new object[]
                    {
                        walletAddress,
                        "latest"
                    },
                    id = 1
                };
                var jsonData = JsonConvert.SerializeObject(data);

                // Отправка запроса через UnityWebRequest  
                using (UnityWebRequest webRequest = new UnityWebRequest(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet), "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    await webRequest.SendWebRequest();

                    // Проверка результата запроса  
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning("UnityWebRequest Error: " + webRequest.error);
                        return 0;
                    }
                    else
                    {
                        string responseText = webRequest.downloadHandler.text;
                        if (string.IsNullOrEmpty(responseText))
                        {
                            return 0;
                        }

                        var jsonRpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(responseText);
                        if (jsonRpcResponse.Error != null)
                        {
                            Debug.LogWarning("JSON-RPC Error: " + jsonRpcResponse.Error.Message);
                            return 0;
                        }

                        if (BigInteger.TryParse(jsonRpcResponse.Result.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out BigInteger balanceResult))
                        {
                            decimal balanceInEther = ConvertFromWei(balanceResult);

                            return balanceInEther;
                        }
                        else
                        {
                            Debug.LogWarning("Failed to parse balance result.");
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Get BalanceOf Error: " + ex.Message);
                return 0;
            }
        }

        public static decimal ConvertFromWei(BigInteger weiValue)
        {
            const decimal etherConversionFactor = 1000000000000000000m; // 1 ether = 10^18 wei  
            return (decimal)weiValue / etherConversionFactor;
        }

        public static async Task<decimal> GetERC20TokenBalance(string walletAddress, VirtualCurrencyID virtualCurrencyID)
        {
            try
            {
                // Получаем параметры контракта  
                string contractABI = BlockchainSettings.GetTokenContractABI(virtualCurrencyID);
                string contractAddress = BlockchainSettings.GetTokenContractAddress(virtualCurrencyID);
                var web3 = new Web3(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));

                // Настройка функции balanceOf  
                var balanceOfFunction = new BalanceOfERC20Function
                {
                    Owner = walletAddress
                };

                // Создаем CallInput для функции balanceOf  
                var callInput = balanceOfFunction.CreateCallInput(contractAddress);

                // Формируем данные JSON-RPC запроса  
                var data = new
                {
                    jsonrpc = "2.0",
                    method = "eth_call",
                    @params = new object[]
                    {
                new { to = contractAddress, data = callInput.Data },
                "latest"
                    },
                    id = 1
                };
                var jsonData = JsonConvert.SerializeObject(data);

                // Отправка запроса через UnityWebRequest  
                using (UnityWebRequest webRequest = new UnityWebRequest(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet), "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");

                    await webRequest.SendWebRequest();

                    // Проверка результата запроса  
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning("UnityWebRequest Error: " + webRequest.error);
                        return 0;
                    }
                    else
                    {
                        string responseText = webRequest.downloadHandler.text;
                        if (string.IsNullOrEmpty(responseText))
                        {
                            return 0;
                        }

                        var jsonRpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(responseText);
                        if (jsonRpcResponse.Error != null)
                        {
                            Debug.LogWarning("JSON-RPC Error: " + jsonRpcResponse.Error.Message);
                            return 0;
                        }

                        // Парсинг результата  
                        if (BigInteger.TryParse(jsonRpcResponse.Result.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out BigInteger balanceResult))
                        {
                            return Web3.Convert.FromWei(balanceResult);
                        }
                        else
                        {
                            Debug.LogWarning("Failed to parse balance result.");
                            return 0;
                        }
                    }
                }
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
                var balanceOfBatchFunction = new BalanceOfBatchFunction
                {
                    Accounts = Enumerable.Repeat(walletAddress, nftIDs.Count).ToList(),
                    Ids = nftIDs
                };

                // Создаем CallInput для функции balanceOfBatch  
                var callInput = balanceOfBatchFunction.CreateCallInput(contractAddress);

                // Формируем данные JSON-RPC запроса  
                var data = new
                {
                    jsonrpc = "2.0",
                    method = "eth_call",
                    @params = new object[]
                    {
                        new
                        {
                            to = contractAddress,
                            data = callInput.Data
                        },
                        "latest"
                    },
                    id = 1
                };

                var jsonData = JsonConvert.SerializeObject(data);

                using (UnityWebRequest webRequest = new UnityWebRequest(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet), "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    await webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        return new List<BigInteger>();
                    }
                    else
                    {
                        string responseText = webRequest.downloadHandler.text;
                        if (string.IsNullOrEmpty(responseText))
                        {
                            return new List<BigInteger>();
                        }

                        var jsonRpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(responseText);
                        if (jsonRpcResponse.Error != null)
                        {
                            return new List<BigInteger>();
                        }

                        var balances = ParseBalancesFromResponse(jsonRpcResponse.Result, nftIDs.Count);
                        if (balances == null)
                        {
                            return new List<BigInteger>();
                        }

                        return balances;
                    }
                }
            }
            catch (Exception)
            {
                return new List<BigInteger>();
            }
        }

        private static List<BigInteger> ParseBalancesFromResponse(string responseText, int count)
        {
            if (string.IsNullOrEmpty(responseText))
            {
                return null;
            }

            var cleanedResponse = responseText.Replace("0x", "");

            if (cleanedResponse.Length < (count + 2) * 64)
            {
                return null;
            }

            List<BigInteger> balances = new List<BigInteger>();

            // We start with the third block (index 2), since the first two blocks are metadata
            for (int i = 2; i < count + 2; i++)
            {
                string hexValue = cleanedResponse.Substring(i * 64, 64);
                if (BigInteger.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, null, out BigInteger balance))
                {
                    balances.Add(balance);
                }
            }

            return balances;
        }

        public static async Task<string> TransferERC20TokenAndGetHash(string fromAddress, string toAddress, VirtualCurrencyID tokenID, int amount, string privateKey)
        {
            try
            {
                // Получаем параметры контракта и создаем объект Web3  
                var account = new Account(privateKey);
                var web3 = new Web3(account, BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));
                var contractAddress = BlockchainSettings.GetTokenContractAddress(tokenID);

                // Настраиваем функцию transfer  
                var transferFunction = new TransferFunction
                {
                    FromAddress = fromAddress,
                    To = toAddress,
                    Value = new HexBigInteger(Web3.Convert.ToWei(amount, UnitConversion.EthUnit.Ether)),
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GAS_PRICE, UnitConversion.EthUnit.Gwei)),
                    AmountToSend = new HexBigInteger(BlockchainSettings.DEFAULT_VALUE_IN_NATIVE_TOKEN)
                };

                // Оцениваем газ  
                var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
                var estimateGas = await transferHandler.EstimateGasAsync(contractAddress, transferFunction);
                transferFunction.Gas = estimateGas;

                // Создаем транзакцию  
                var transactionInput = transferFunction.CreateTransactionInput(contractAddress);

                // Подписываем транзакцию с использованием TransactionSigner  
                var transactionSigner = new LegacyTransactionSigner();
                var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(fromAddress);
                var signedTransaction = transactionSigner.SignTransaction(privateKey, transactionInput.To, transactionInput.Value, nonce, transactionInput.GasPrice, transactionInput.Gas, transactionInput.Data);

                // Формируем данные JSON-RPC запроса  
                var data = new
                {
                    jsonrpc = "2.0",
                    method = "eth_sendRawTransaction",
                    @params = new object[]
                    {
                "0x" + signedTransaction
                    },
                    id = 1
                };
                var jsonData = JsonConvert.SerializeObject(data);

                // Отправка запроса через UnityWebRequest  
                using (UnityWebRequest webRequest = new UnityWebRequest(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet), "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");

                    await webRequest.SendWebRequest();

                    // Проверка результата запроса  
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning("UnityWebRequest Error: " + webRequest.error);
                        return null;
                    }
                    else
                    {
                        string responseText = webRequest.downloadHandler.text;
                        if (string.IsNullOrEmpty(responseText))
                        {
                            return null;
                        }

                        var jsonRpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(responseText);
                        if (jsonRpcResponse.Error != null)
                        {
                            Debug.LogWarning("JSON-RPC Error: " + jsonRpcResponse.Error.Message);
                            return null;
                        }

                        // Возвращаем хэш транзакции  
                        return jsonRpcResponse.Result;
                    }
                }
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