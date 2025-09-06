using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC1155;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Signer;
using Nethereum.ABI.Model;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors;

using BalanceOfERC20Function = Nethereum.Contracts.Standards.ERC20.ContractDefinition.BalanceOfFunction;
using TransferFunction = Nethereum.Contracts.Standards.ERC20.ContractDefinition.TransferFunction;

#if !UNITY_6000_0_OR_NEWER
using Nethereum.Unity.Util;
#endif

namespace IDosGames
{
    public static class WalletBlockchainService
    {
        public static async Task<BigInteger> GetERC20Allowance(string tokenAddress, string ownerAddress, string spenderAddress)
        {
            try
            {
                string functionSelector = "0xdd62ed3e";

                string ownerPadded = ownerAddress.Substring(2).PadLeft(64, '0');
                string spenderPadded = spenderAddress.Substring(2).PadLeft(64, '0');

                string data = functionSelector + ownerPadded + spenderPadded;

                var callData = new
                {
                    to = tokenAddress,
                    data = data
                };

                var request = new
                {
                    jsonrpc = "2.0",
                    method = "eth_call",
                    @params = new object[] { callData, "latest" },
                    id = 1
                };

                var jsonData = JsonConvert.SerializeObject(request);
                string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

                if (string.IsNullOrEmpty(responseText))
                {
                    Debug.LogWarning("Empty response from RPC");
                    return BigInteger.Zero;
                }

                var jsonRpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(responseText);
                if (jsonRpcResponse.Error != null)
                {
                    Debug.LogWarning($"JSON-RPC Error: {jsonRpcResponse.Error.Message}");
                    return BigInteger.Zero;
                }

                //Debug.Log($"Raw allowance response: {jsonRpcResponse.Result}");

                string hexResult = jsonRpcResponse.Result;
                if (hexResult.StartsWith("0x"))
                {
                    hexResult = hexResult.Substring(2);
                }

                byte[] bytes = HexStringToByteArray(hexResult);

                BigInteger allowance = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);

                Debug.Log($"Parsed allowance: {allowance}");

                return allowance;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Get ERC20 Allowance Error: {ex.Message}");
                return BigInteger.Zero;
            }
        }

        public static async Task<string> ApproveERC20Token(string tokenAddress, string spenderAddress, string amountInWei, string privateKey, int chainID)
        {
            try
            {
                var account = new Account(privateKey);
                var fromAddress = account.Address;
                if (!BigInteger.TryParse(amountInWei, out BigInteger tokenAmountInWei))
                {
                    Debug.LogWarning("Invalid amount format for approval.");
                    return null;
                }

                var approveFunction = new ApproveFunction
                {
                    Spender = spenderAddress,
                    Value = tokenAmountInWei,
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GasPrice, UnitConversion.EthUnit.Gwei)),
                    FromAddress = fromAddress
                };

                approveFunction.Gas = new HexBigInteger(50000);

                var nonce = await GetTransactionCountAsync(fromAddress);

                var callData = approveFunction.GetCallData().ToHex();
                var gasPrice = approveFunction.GasPrice.Value;
                var gasLimit = approveFunction.Gas.Value;

                var signer = new LegacyTransactionSigner();

                BigInteger chainIdBigInt = new BigInteger(chainID);

                string signedTx = signer.SignTransaction(
                    privateKey,
                    chainIdBigInt,
                    tokenAddress,
                    BigInteger.Zero,
                    nonce.Value,
                    gasPrice,
                    gasLimit,
                    callData
                );

                var txHash = await SendRawTransaction("0x" + signedTx);
                return txHash;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Approve ERC20 Error: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> DepositERC20Token(string tokenAddress, string contractAddress, string amountInWei, string userID, string privateKey, int chainID)
        {
            try
            {
                var account = new Account(privateKey);
                var fromAddress = account.Address;
                if (!BigInteger.TryParse(amountInWei, out BigInteger tokenAmountInWei))
                {
                    Debug.LogWarning("Invalid amount format for approval.");
                    return null;
                }

                var depositMessage = new DepositERC20Function
                {
                    Token = tokenAddress,
                    Amount = tokenAmountInWei,
                    UserID = userID,
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GasPrice, UnitConversion.EthUnit.Gwei)),
                    FromAddress = fromAddress
                };

                depositMessage.Gas = new HexBigInteger(90000);

                var nonce = await GetTransactionCountAsync(fromAddress);

                var callData = depositMessage.GetCallData().ToHex();
                var gasPrice = depositMessage.GasPrice.Value;
                var gasLimit = depositMessage.Gas.Value;

                var signer = new LegacyTransactionSigner();

                BigInteger chainIdBigInt = new BigInteger(chainID);

                string signedTx = signer.SignTransaction(
                    privateKey,
                    chainIdBigInt,
                    contractAddress,
                    BigInteger.Zero,
                    nonce.Value,
                    gasPrice,
                    gasLimit,
                    callData
                );

                Debug.Log($"ChainID: {chainID}, TokenAddress: {tokenAddress}, TokenAmountInWei: {tokenAmountInWei}, UserID: {userID}, CallData: {callData}");

                var txHash = await SendRawTransaction("0x" + signedTx);
                return txHash;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Deposit ERC20 Error: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> WithdrawERC20Token(WithdrawalSignatureResult withdrawalData, string privateKey, int chainID)
        {
            try
            {
                var account = new Account(privateKey);
                var fromAddress = account.Address;

                var withdrawMessage = new WithdrawERC20Function
                {
                    Token = withdrawalData.TokenAddress,
                    To = withdrawalData.WalletAddress,
                    Amount = BigInteger.Parse(withdrawalData.Amount),
                    Nonce = BigInteger.Parse(withdrawalData.Nonce),
                    Signature = HexStringToByteArray(withdrawalData.Signature),
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GasPrice, UnitConversion.EthUnit.Gwei)),
                    FromAddress = fromAddress
                };

                withdrawMessage.Gas = new HexBigInteger(150000);

                var nonce = await GetTransactionCountAsync(fromAddress);

                var callData = withdrawMessage.GetCallData().ToHex();
                var gasPrice = withdrawMessage.GasPrice.Value;
                var gasLimit = withdrawMessage.Gas.Value;

                var signer = new LegacyTransactionSigner();

                BigInteger chainIdBigInt = new BigInteger(chainID);

                Debug.Log($"ChainID: {chainID}, ContractAddress: {withdrawalData.ContractAddress}, Nonce: {nonce.Value}, GasPrice: {gasPrice}, GasLimit: {gasLimit}, CallData: {callData}");

                string signedTx = signer.SignTransaction(
                    privateKey,
                    chainIdBigInt,
                    withdrawalData.ContractAddress,
                    BigInteger.Zero,
                    nonce.Value,
                    gasPrice,
                    gasLimit,
                    callData
                    );

                //Debug.Log($"Signed Transaction: {signedTx}");

                var txHash = await SendRawTransaction("0x" + signedTx);
                Debug.Log($"txHash: {txHash}");

                return txHash;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Withdraw ERC20 Error: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> WithdrawERC1155Token(WithdrawalSignatureResult withdrawalData, string privateKey, int chainID)
        {
            try
            {
                var account = new Account(privateKey);
                var fromAddress = account.Address;

                var withdrawMessage = new WithdrawERC1155Function
                {
                    Token = withdrawalData.TokenAddress,
                    To = withdrawalData.WalletAddress,
                    Id = BigInteger.Parse(withdrawalData.TokenId),
                    Amount = BigInteger.Parse(withdrawalData.Amount),
                    Nonce = BigInteger.Parse(withdrawalData.Nonce),
                    Signature = HexStringToByteArray(withdrawalData.Signature),
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GasPrice, UnitConversion.EthUnit.Gwei)),
                    FromAddress = fromAddress
                };

                withdrawMessage.Gas = new HexBigInteger(150000);

                var nonce = await GetTransactionCountAsync(fromAddress);

                var callData = withdrawMessage.GetCallData().ToHex();
                var gasPrice = withdrawMessage.GasPrice.Value;
                var gasLimit = withdrawMessage.Gas.Value;

                var signer = new LegacyTransactionSigner();

                BigInteger chainIdBigInt = new BigInteger(chainID);

                Debug.Log($"ChainID: {chainID}, ContractAddress: {withdrawalData.ContractAddress}, Nonce: {nonce.Value}, GasPrice: {gasPrice}, GasLimit: {gasLimit}, CallData: {callData}");

                string signedTx = signer.SignTransaction(
                    privateKey,
                    chainIdBigInt,
                    withdrawalData.ContractAddress,
                    BigInteger.Zero,
                    nonce.Value,
                    gasPrice,
                    gasLimit,
                    callData
                    );

                //Debug.Log($"Signed Transaction: {signedTx}");

                var txHash = await SendRawTransaction("0x" + signedTx);
                Debug.Log($"txHash: {txHash}");

                return txHash;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Withdraw ERC20 Error: {ex.Message}");
                return null;
            }
        }

        private static async Task<string> SendRawTransaction(string signedTx)
        {
            var data = new
            {
                jsonrpc = "2.0",
                method = "eth_sendRawTransaction",
                @params = new[] { signedTx },
                id = 1
            };

            var response = await SendUnityWebRequest(BlockchainSettings.RpcUrl, JsonConvert.SerializeObject(data));
            Debug.Log("Response Raw Transaction: " + response);
            return JsonConvert.DeserializeObject<JsonRpcResponse<string>>(response).Result;
        }

        private static string ToHex(this byte[] bytes)
        {
            return "0x" + BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.StartsWith("0x") ? hex.Substring(2) : hex;
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static async Task<BigInteger> GetNativeTokenBalance(string walletAddress)
        {
            try
            {
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
                string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

                if (string.IsNullOrEmpty(responseText))
                {
                    return BigInteger.Zero;
                }

                var jsonRpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(responseText);
                if (jsonRpcResponse.Error != null)
                {
                    Debug.LogWarning("JSON-RPC Error: " + jsonRpcResponse.Error.Message);
                    return BigInteger.Zero;
                }

                return HexToBigInteger(jsonRpcResponse.Result);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Get BalanceOf Error: " + ex.Message);
                return BigInteger.Zero;
            }
        }

        private static BigInteger HexToBigInteger(string hex)
        {
            var hexConvertor = new HexBigIntegerBigEndianConvertor();
            return hexConvertor.ConvertFromHex(hex);
        }

        public static async Task<BigInteger> GetERC20TokenBalance(string walletAddress, VirtualCurrencyID virtualCurrencyID)
        {
            try
            {
                string contractAddress = BlockchainSettings.GetTokenContractAddress(virtualCurrencyID);
                var web3 = new Web3(BlockchainSettings.RpcUrl);

                var balanceOfFunction = new BalanceOfERC20Function
                {
                    Owner = walletAddress
                };

                var callInput = balanceOfFunction.CreateCallInput(contractAddress);

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
                string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

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

                return HexToBigInteger(jsonRpcResponse.Result);
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
                var web3 = new Web3(BlockchainSettings.RpcUrl);
                var erc1155Service = new ERC1155Service(web3.Eth);
                var contractAddress = BlockchainSettings.NftContractAddress;
                var balanceOfBatchFunction = new BalanceOfBatchFunction
                {
                    Accounts = Enumerable.Repeat(walletAddress, nftIDs.Count).ToList(),
                    Ids = nftIDs
                };

                var callInput = balanceOfBatchFunction.CreateCallInput(contractAddress);

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
                string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

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
            catch (Exception)
            {
                return new List<BigInteger>();
            }
        }

        public static async Task<string> TransferERC20TokenAndGetHash(string fromAddress, string toAddress, VirtualCurrencyID tokenID, int amount, string privateKey, int chainID)
        {
            try
            {
                var account = new Account(privateKey);
                var web3 = new Web3(account, BlockchainSettings.RpcUrl);
                var contractAddress = BlockchainSettings.GetTokenContractAddress(tokenID);
                var transferFunction = new TransferFunction
                {
                    FromAddress = fromAddress,
                    To = toAddress,
                    Value = new HexBigInteger(Web3.Convert.ToWei(amount, UnitConversion.EthUnit.Ether)),
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GasPrice, UnitConversion.EthUnit.Gwei)),
                    Gas = 100000,
                    AmountToSend = new HexBigInteger(BlockchainSettings.DEFAULT_VALUE_IN_NATIVE_TOKEN)
                };

                var nonce = await GetTransactionCountAsync(fromAddress);
                if (nonce == null)
                {
                    return null;
                }

                var transactionInput = transferFunction.CreateTransactionInput(contractAddress);
                var transactionSigner = new LegacyTransactionSigner();
                var chainIdBigInt = new BigInteger(chainID);
                var signedTransaction = transactionSigner.SignTransaction(privateKey, chainIdBigInt, transactionInput.To, transactionInput.Value, nonce, transactionInput.GasPrice, transactionInput.Gas, transactionInput.Data);

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
                string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

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

                return jsonRpcResponse.Result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Transfer Error: " + ex.Message);
                return null;
            }
        }

        public static async Task<string> TransferNFT1155AndGetHash(string fromAddress, string toAddress, BigInteger nftID, int amount, string privateKey, int chainID)
        {
            try
            {
                var account = new Account(privateKey);
                var web3 = new Web3(account, BlockchainSettings.RpcUrl);
                var contractAddress = BlockchainSettings.NftContractAddress;
                var transferFunction = new SafeTransferFromFunction
                {
                    From = fromAddress,
                    To = toAddress,
                    Id = nftID,
                    Amount = amount,
                    Gas = 100000,
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(BlockchainSettings.GasPrice, UnitConversion.EthUnit.Gwei)),
                    Data = string.IsNullOrEmpty(AuthService.UserID) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(AuthService.UserID)
                };

                var nonce = await GetTransactionCountAsync(fromAddress);
                if (nonce == null)
                {
                    return null;
                }

                var transactionInput = transferFunction.CreateTransactionInput(contractAddress);
                var transactionSigner = new LegacyTransactionSigner();

                transactionInput.Value = new HexBigInteger(BigInteger.Zero);

                var chainIdBigInt = new BigInteger(chainID);
                var signedTransaction = transactionSigner.SignTransaction(
                    privateKey,
                    chainIdBigInt,
                    transactionInput.To,
                    transactionInput.Value.Value,
                    nonce.Value,
                    transactionInput.GasPrice.Value,
                    transactionInput.Gas.Value,
                    transactionInput.Data
                );

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
                string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

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

                return jsonRpcResponse.Result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Transfer Error: " + ex.Message);
                return null;
            }
        }

        private static async Task<string> SendUnityWebRequest(string url, string jsonData)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("UnityWebRequest Error: " + webRequest.error);
                    return null;
                }

                return webRequest.downloadHandler.text;
            }
        }

        public static decimal ConvertFromWei(BigInteger weiValue)
        {
            const decimal etherConversionFactor = 1000000000000000000m; // 1 ether = 10^18 wei  
            return (decimal)weiValue / etherConversionFactor;
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

        public static async Task<HexBigInteger> EstimateGasNFTAsync(string contractAddress, SafeTransferFromFunction transferFunction)
        {
            //var web3 = new Web3(BlockchainSettings.GetProviderAddress(BlockchainNetwork.IgcTestnet));
            var callInput = transferFunction.CreateCallInput(contractAddress);

            var data = new
            {
                jsonrpc = "2.0",
                method = "eth_estimateGas",
                @params = new object[]
                {
                    new
                    {
                        from = transferFunction.From,
                        to = contractAddress,
                        gas = transferFunction.Gas != null ? transferFunction.Gas.Value.ToString("X") : null, // Convert to hex string if not null  
                        gasPrice = transferFunction.GasPrice != null ? transferFunction.GasPrice.Value.ToString("X") : null, // Convert to hex string if not null  
                        value = transferFunction.AmountToSend != null ? transferFunction.AmountToSend.ToString("X") : null, // Convert to hex string if not null  
                        data = callInput.Data
                    }
                },
                id = 1
            };

            var jsonData = JsonConvert.SerializeObject(data);
            string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

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

            return new HexBigInteger(jsonRpcResponse.Result);
        }

        public static async Task<HexBigInteger> GetTransactionCountAsync(string fromAddress)
        {
            var data = new
            {
                jsonrpc = "2.0",
                method = "eth_getTransactionCount",
                @params = new object[]
                {
                    fromAddress,
                    "latest"
                },
                id = 1
            };

            var jsonData = JsonConvert.SerializeObject(data);
            string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

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

            return new HexBigInteger(jsonRpcResponse.Result);
        }

        public static async Task<bool> WaitForTransactionReceipt(string transactionHash)
        {
            const int delayBetweenChecks = 3;
            int maxAttempts = 20;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                var receipt = await GetTransactionReceipt(transactionHash);

                if (receipt != null)
                {
                    return receipt.Status != null && receipt.Status.Value == BigInteger.One;
                }

                attempts++;
                await DelayAsync(delayBetweenChecks);
            }

            Debug.LogWarning($"Transaction {transactionHash} not confirmed after {maxAttempts} attempts.");
            return false;
        }

        private static Task DelayAsync(float seconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            CoroutineRunner.Instance.StartCoroutine(DelayCoroutine(seconds, () => tcs.SetResult(true)));
            return tcs.Task;
        }

        private static IEnumerator DelayCoroutine(float seconds, Action onComplete)
        {
            yield return new WaitForSecondsRealtime(seconds);
            onComplete?.Invoke();
        }

        private static async Task<TransactionReceipt> GetTransactionReceipt(string transactionHash)
        {
            var data = new
            {
                jsonrpc = "2.0",
                method = "eth_getTransactionReceipt",
                @params = new[] { transactionHash },
                id = 1
            };

            var jsonData = JsonConvert.SerializeObject(data);
            string responseText = await SendUnityWebRequest(BlockchainSettings.RpcUrl, jsonData);

            if (string.IsNullOrEmpty(responseText))
            {
                return null;
            }

            var jsonRpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<TransactionReceipt>>(responseText);
            if (jsonRpcResponse.Error != null)
            {
                Debug.LogWarning("JSON-RPC Error: " + jsonRpcResponse.Error.Message);
                return null;
            }

            return jsonRpcResponse.Result;
        }
    }

    [Function("withdrawERC20", "bool")]
    public class WithdrawERC20Function : FunctionMessage
    {
        [Parameter("address", "token", 1)]
        public string Token { get; set; }

        [Parameter("address", "to", 2)]
        public string To { get; set; }

        [Parameter("uint256", "amount", 3)]
        public BigInteger Amount { get; set; }

        [Parameter("uint256", "nonce", 4)]
        public new BigInteger Nonce { get; set; }

        [Parameter("bytes", "signature", 5)]
        public byte[] Signature { get; set; }
    }

    [Function("depositERC20", "bool")]
    public class DepositERC20Function : FunctionMessage
    {
        [Parameter("address", "token", 1)]
        public string Token { get; set; }

        [Parameter("uint256", "amount", 2)]
        public BigInteger Amount { get; set; }

        [Parameter("string", "userID", 3)]
        public string UserID { get; set; }
    }

    [Function("withdrawERC1155", "bool")]
    public class WithdrawERC1155Function : FunctionMessage
    {
        [Parameter("address", "token", 1)]
        public string Token { get; set; }

        [Parameter("address", "to", 2)]
        public string To { get; set; }

        [Parameter("uint256", "id", 3)]
        public BigInteger Id { get; set; }

        [Parameter("uint256", "amount", 4)]
        public BigInteger Amount { get; set; }

        [Parameter("uint256", "nonce", 5)]
        public new BigInteger Nonce { get; set; }

        [Parameter("bytes", "signature", 6)]
        public byte[] Signature { get; set; }
    }
}
