using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Metamask;
using Nethereum.Unity.Rpc;
using Nethereum.Web3;
using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public class MetaMaskWalletService : MonoBehaviour
    {
        private static MetaMaskWalletService _instance;
        public static MetaMaskWalletService Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private bool _isConnected = false;
        private string _selectedAccountAddress;
        private BigInteger _chainId;

        public string TransactionHashAfterTransactionToGame { get; private set; }

        public bool IsConnected => _isConnected && !string.IsNullOrEmpty(_selectedAccountAddress);

        public Task Connect()
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(ConnectCoroutine(tcs));
            return tcs.Task;
        }

        private IEnumerator ConnectCoroutine(TaskCompletionSource<bool> tcs)
        {
#if UNITY_WEBGL
            if (MetamaskWebglInterop.IsMetamaskAvailable())
            {
                MetamaskWebglInterop.EnableEthereum(gameObject.name, nameof(EthereumEnabled), nameof(DisplayError));
                yield return new WaitUntil(() => _isConnected);
                tcs.SetResult(true);
            }
            else
            {
                Debug.LogWarning("Metamask is not available, please install it");
                tcs.SetException(new Exception("Metamask is not available"));
                yield break;
            }
#else
            tcs.SetException(new Exception("MetaMask is only available in WebGL"));
            yield break;
#endif
        }

        public void EthereumEnabled(string addressSelected)
        {
            MetamaskWebglInterop.EthereumInit(gameObject.name, nameof(NewAccountSelected), nameof(ChainChanged));
            MetamaskWebglInterop.GetChainId(gameObject.name, nameof(ChainChanged), nameof(DisplayError));
            NewAccountSelected(addressSelected);
            _isConnected = true;
        }

        public void NewAccountSelected(string accountAddress)
        {
            _selectedAccountAddress = accountAddress;
            Debug.Log($"MetaMask account selected: {accountAddress}");
        }

        public void ChainChanged(string chainId)
        {
            _chainId = new HexBigInteger(chainId).Value;
            Debug.Log($"Chain ID changed to: {_chainId}");
        }

        public void DisplayError(string errorMessage)
        {
            Debug.LogError($"MetaMask Error: {errorMessage}");
        }

        public async Task<string> TransferTokenToGame(VirtualCurrencyID virtualCurrencyID, int amount)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("MetaMask is not connected.");
                return null;
            }

            var tcs = new TaskCompletionSource<string>();
            StartCoroutine(TransferTokenToGameCoroutine(virtualCurrencyID, amount, tcs));
            return await tcs.Task;
        }

        private IEnumerator TransferTokenToGameCoroutine(VirtualCurrencyID virtualCurrencyID, int amount, TaskCompletionSource<string> tcs)
        {
            string platformPoolAddress = BlockchainSettings.PlatformPoolContractAddress;
            string tokenAddress = BlockchainSettings.GetTokenContractAddress(virtualCurrencyID);
            BigInteger requiredAmountWei = Web3.Convert.ToWei(amount);

            // Get current allowance
            var queryRequest = new QueryUnityRequest<AllowanceFunction, AllowanceOutputDTO>(
                GetUnityRpcRequestClientFactory(), _selectedAccountAddress);
            yield return queryRequest.Query(
                new AllowanceFunction { Owner = _selectedAccountAddress, Spender = platformPoolAddress },
                tokenAddress);

            if (queryRequest.Exception != null)
            {
                Debug.LogError($"Failed to get allowance: {queryRequest.Exception.Message}");
                tcs.SetException(queryRequest.Exception);
                yield break;
            }

            BigInteger currentAllowance = queryRequest.Result.Allowance;
            Debug.Log($"CurrentAllowanceWei: {currentAllowance}");

            string maxAllowance = "115792089237316195423570985008687907853269984665640564039457584007913129639935";

            if (currentAllowance < requiredAmountWei)
            {
                var approveFunction = new ApproveFunction
                {
                    Spender = platformPoolAddress,
                    Value = BigInteger.Parse(maxAllowance)
                };
                var transactionRequest = new MetamaskTransactionCoroutineUnityRequest(
                    _selectedAccountAddress, GetUnityRpcRequestClientFactory());
                yield return transactionRequest.SignAndSendTransaction(approveFunction, tokenAddress);

                if (transactionRequest.Exception != null)
                {
                    Debug.LogError($"Approval failed: {transactionRequest.Exception.Message}");
                    tcs.SetException(transactionRequest.Exception);
                    yield break;
                }

                string approveHash = transactionRequest.Result;
                Debug.Log($"Approval transaction hash: {approveHash}");

                var receiptPolling = new TransactionReceiptPollingRequest(GetUnityRpcRequestClientFactory());
                yield return receiptPolling.PollForReceipt(approveHash, 2);

                if (receiptPolling.Result == null)
                {
                    Debug.LogWarning("Approval transaction was not confirmed.");
                    tcs.SetException(new Exception("Approval transaction failed"));
                    yield break;
                }
            }

            // Send deposit transaction
            var depositFunction = new DepositFunction
            {
                Token = tokenAddress,
                Amount = requiredAmountWei,
                UserID = AuthService.UserID
            };
            var depositTransactionRequest = new MetamaskTransactionCoroutineUnityRequest(
                _selectedAccountAddress, GetUnityRpcRequestClientFactory());
            yield return depositTransactionRequest.SignAndSendTransaction(depositFunction, platformPoolAddress);

            if (depositTransactionRequest.Exception != null)
            {
                Debug.LogError($"Deposit failed: {depositTransactionRequest.Exception.Message}");
                tcs.SetException(depositTransactionRequest.Exception);
                yield break;
            }

            string transactionHash = depositTransactionRequest.Result;
            TransactionHashAfterTransactionToGame = transactionHash;
            Debug.Log($"Deposit transaction hash: {transactionHash}");

            if (string.IsNullOrEmpty(TransactionHashAfterTransactionToGame))
            {
                Debug.LogWarning("TransferTokenToGame TransactionHash is null");
                tcs.SetResult(null);
                yield break;
            }

#if IDOSGAMES_CRYPTO_WALLET
            var request = new WalletTransactionRequest
            {
                ChainID = (int)_chainId,
                TransactionType = CryptoTransactionType.Token,
                Direction = TransactionDirection.Game,
                TransactionHash = transactionHash
            };

            Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED);

            var task = IGSService.TryMakeTransaction(request);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogError($"TryMakeTransaction failed: {task.Exception.Message}");
                tcs.SetException(task.Exception);
            }
            else
            {
                tcs.SetResult(task.Result);
            }
#else
            Debug.Log("Crypto wallet not enabled. Transaction hash: " + transactionHash);
            tcs.SetResult(transactionHash);
#endif
        }

        private IUnityRpcRequestClientFactory GetUnityRpcRequestClientFactory()
        {
            return new MetamaskWebglCoroutineRequestRpcClientFactory(_selectedAccountAddress, null, 60000);
        }
    }

    // Function definitions
    public class DepositFunction : FunctionMessage
    {
        [Parameter("address", "token", 1)]
        public string Token { get; set; }

        [Parameter("uint256", "amount", 2)]
        public BigInteger Amount { get; set; }

        [Parameter("string", "userID", 3)]
        public string UserID { get; set; }
    }

    public class AllowanceFunction : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public string Owner { get; set; }

        [Parameter("address", "spender", 2)]
        public string Spender { get; set; }
    }

    public class AllowanceOutputDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "allowance", 1)]
        public BigInteger Allowance { get; set; }
    }
}