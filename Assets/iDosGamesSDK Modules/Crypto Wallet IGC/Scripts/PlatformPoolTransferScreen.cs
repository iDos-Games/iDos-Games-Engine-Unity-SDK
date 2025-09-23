#if IDOSGAMES_CRYPTO_WALLET
using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Solana.Unity.SDK.Example;
using Solana.Unity.SDK.Nft;
using Solana.Unity.SDK;
using Solana.Unity.Rpc.Types;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace

namespace IDosGames
{
    public class PlatformPoolTransferScreen : SimpleScreen
    {
        private string programIdBase58 = "FWvDZMpUy9SPgRV6rJSa6fju1VtYejPNqshXpgA9BzsG";
        private string userID;
        private string mintAddress = "4zMMC9srt5Ri5X14GAgXhaHii3GnPAEERYPJgZJDncDU";

        [Header("UI")]
        public TextMeshProUGUI titleTxt;
        public TextMeshProUGUI tokenTitleTxt;   // название токена/NFT
        public TextMeshProUGUI balanceTxt;      // баланс токена (UI, в человекочитаемом виде)
        public RawImage tokenImage;

        private bool isWithdraw = false;

        public TMP_InputField amountTxt;        // сумма в UI-единицах (учитывая decimals)

        public TextMeshProUGUI errorTxt;
        public Button actionBtn;
        public Button closeBtn;

        private TokenAccount _tokenAccount;
        private Nft _nft;

        private IDosGames.SolanaPlatformPoolService _svc;
        private IRpcClient _rpcClient;
        private PublicKey _mintPk;
        private int _decimals = 0;

        private void Start()
        {
            programIdBase58 = string.IsNullOrWhiteSpace(BlockchainSettings.PlatformPoolContractAddress) ? "FWvDZMpUy9SPgRV6rJSa6fju1VtYejPNqshXpgA9BzsG" : BlockchainSettings.PlatformPoolContractAddress;
            mintAddress = BlockchainSettings.HardTokenContractAddress;
            userID = AuthService.UserID;

            _rpcClient = Web3.Instance.WalletBase.ActiveRpcClient;
            _svc = new IDosGames.SolanaPlatformPoolService(_rpcClient, programIdBase58);

            actionBtn.onClick.AddListener(OnAction);
            closeBtn.onClick.AddListener(() => manager.ShowScreen(this, "wallet_screen"));
        }

        public void OpenAsDeposit()
        {
            isWithdraw = false;
            titleTxt.text = "Deposit in Game";
            manager.ShowScreen(this, "platform_pool_transfer_screen");
        }

        public void OpenAsWithdraw()
        {
            isWithdraw = true;
            titleTxt.text = "Withdrawal from Game";
            manager.ShowScreen(this, "platform_pool_transfer_screen");
        }

        public override async void ShowScreen(object data = null)
        {
            base.ShowScreen();
            ResetState();
            await PopulateFromData(data);
            gameObject.SetActive(true);
        }

        public override void HideScreen()
        {
            base.HideScreen();
            _nft = null;
            _tokenAccount = null;
            gameObject.SetActive(false);
        }

        private void ResetState()
        {
            errorTxt.text = "";
            tokenTitleTxt.text = "";
            balanceTxt.text = "";
            amountTxt.text = "";

            if (tokenImage != null)
            {
                tokenImage.texture = null;
                tokenImage.color = new Color(1, 1, 1, 0); // прозрачно до загрузки
            }

            _nft = null;
            _tokenAccount = null;
            _mintPk = default;
            _decimals = 0;
        }

        private async Task PopulateFromData(object data)
        {
            tokenImage?.gameObject.SetActive(true);
            tokenTitleTxt.gameObject.SetActive(true);
            balanceTxt.gameObject.SetActive(true);

            if (data != null && data.GetType() == typeof(Tuple<TokenAccount, string, Texture2D>))
            {
                var (tokenAccount, tokenDef, texture) = (Tuple<TokenAccount, string, Texture2D>)data;
                _tokenAccount = tokenAccount;

                _mintPk = new PublicKey(tokenAccount.Account.Data.Parsed.Info.Mint);

                _decimals = await FetchMintDecimalsAsync(_mintPk);

                tokenTitleTxt.text = tokenDef;
                balanceTxt.text = tokenAccount.Account.Data.Parsed.Info.TokenAmount.AmountDecimal
                    .ToString(CultureInfo.CurrentCulture);

                if (tokenImage != null && texture != null)
                {
                    tokenImage.texture = texture;
                    tokenImage.color = Color.white;
                }

                amountTxt.interactable = true;
                amountTxt.text = "";
            }
            else if (data != null && data.GetType() == typeof(Nft))
            {
                _nft = (Nft)data;

                var name = _nft.metaplexData.data.offchainData?.name ?? "NFT";
                tokenTitleTxt.text = name;

                var img = _nft.metaplexData?.nftImage?.file;
                if (tokenImage != null && img != null)
                {
                    tokenImage.texture = img;
                    tokenImage.color = Color.white;
                }

                _mintPk = new PublicKey(_nft.metaplexData.data.mint);

                // NFT всегда decimals = 0
                _decimals = 0;

                balanceTxt.text = "1";

                amountTxt.text = "1";
                amountTxt.interactable = false;
            }
            else if (data is string mintStr && !string.IsNullOrWhiteSpace(mintStr))
            {
                _mintPk = new PublicKey(mintStr);

                _decimals = await FetchMintDecimalsAsync(_mintPk);

                tokenTitleTxt.text = mintStr;
                balanceTxt.text = "-";
                amountTxt.interactable = true;
                amountTxt.text = "";
            }
            else
            {
                tokenTitleTxt.text = "Token";
                balanceTxt.text = "-";
                amountTxt.interactable = true;
            }
        }

        private async void OnAction()
        {
            errorTxt.text = "";

            if (isWithdraw) await DoWithdraw();
            else await DoDeposit();
        }

        private async Task DoDeposit()
        {
            try
            {
                // Валидации
                if (string.IsNullOrWhiteSpace(mintAddress))
                {
                    errorTxt.text = "Mint address is required.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(userID))
                {
                    errorTxt.text = "User ID is required for deposit.";
                    return;
                }
                if (!TryParseUiAmount(amountTxt.text, out var uiAmount) || uiAmount <= 0)
                {
                    errorTxt.text = "Invalid amount.";
                    return;
                }

                // Пересчёт из UI в "сырые" единицы
                _mintPk = new PublicKey(mintAddress.Trim());
                _decimals = await FetchMintDecimalsAsync(_mintPk);
                var raw = UiToRaw(uiAmount, _decimals);

                balanceTxt.text = $"Send preview: {uiAmount.ToString(CultureInfo.InvariantCulture)} → {raw} (10^{_decimals})";

                var payer = Web3.Instance.WalletBase.Account;
                if (payer == null)
                {
                    errorTxt.text = "Wallet account is not available.";
                    return;
                }

                Loading.ShowTransparentPanel();

                if (!await HasMinSolAsync(10000)) return;

                RequestResult<string> res = await _svc.DepositSplAsync(
                    payer: payer,
                    mint: _mintPk,
                    amount: raw,
                    userId: userID.Trim()
                );

                // Если RPC неуспешен — показываем причину и выходим
                if (res == null || !res.WasSuccessful || string.IsNullOrWhiteSpace(res.Result))
                {
                    HandleRpcResult(res);
                    return;
                }

                manager.ShowScreen(this, "wallet_screen");
                Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED_PLEASE_WAIT);

                // Транзакция в сети успешна — есть сигнатура
                string signature = res.Result.Trim();
                if (IDosGamesSDKSettings.Instance.DebugLogging) Debug.Log(signature);

                // Сообщаем о транзакции нашему бекенду и ждём подтверждения
                var serverOk = await PostTxToServerWithPolling(signature);

                Loading.HideAllPanels();

                if (serverOk)
                {
                    Message.Show(MessageCode.TRANSACTION_SUCCESS);
                    UserDataService.RequestUserAllData();
                }
                else
                {
                    // Сервер так и не принял хэш за отведённое время — мягкий фолбэк
                    Message.Show("The transaction was sent on-chain, but the server didn’t confirm it in time. Try verifying the transaction using the Tx Hash.");
                }
            }
            catch (Exception ex)
            {
                errorTxt.text = $"Deposit error: {ex.Message}";
            }
        }

        private async Task<bool> PostTxToServerWithPolling(string signature, int maxWaitSeconds = 50, int pollIntervalSeconds = 5)
        {
            var request = new WalletTransactionRequest
            {
                ChainType = "Solana",
                ChainID = 0,
                TransactionType = CryptoTransactionType.Token,
                Direction = TransactionDirection.Game,
                TransactionHash = signature
            };

            var started = DateTime.UtcNow;

            while ((DateTime.UtcNow - started).TotalSeconds < maxWaitSeconds)
            {
                var result = await IGSService.TryMakeTransaction(request);
                var message = Message.MessageResult(result);

                if (message == MessageCode.TRANSACTION_SUCCESS.ToString())
                    return true;

                // Если бэкенд говорит, что хэш ещё невалиден/не найден — подождём и попробуем ещё раз.
                if (message == MessageCode.INCORRECT_HASH.ToString())
                {
                    await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
                    continue;
                }

                // Любая другая ошибка — выходим и показываем её.
                Message.Show(message);
                return false;
            }

            return false; // таймаут
        }

        private async Task DoWithdraw()
        {
            try
            {
                if (!TryParseUiAmount(amountTxt.text, out var uiAmount))
                {
                    errorTxt.text = "Invalid amount.";
                    return;
                }

                decimal floored = Math.Floor(uiAmount);

                if (floored <= 0)
                {
                    errorTxt.text = "Amount must be > 0.";
                    return;
                }
                if (floored > int.MaxValue) floored = int.MaxValue;

                Loading.ShowTransparentPanel();

                int amountInt = (int)floored;

                var userWallet = Web3.Instance.WalletBase.Account.PublicKey;

                if (!await HasMinSolAsync(1500000)) return;

                var request = new WalletTransactionRequest
                {
                    ChainType = BlockchainSettings.ChainType,
                    ChainID = BlockchainSettings.ChainID,
                    TransactionType = CryptoTransactionType.Token,
                    Direction = TransactionDirection.UsersCryptoWallet,
                    CurrencyID = VirtualCurrencyID.IG,
                    Amount = amountInt,
                    ConnectedWalletAddress = userWallet
                };

                var result = await IGSService.TryMakeTransaction(request);

                // Опционально: сверим mint из JSON с полем mintTxt (если введено)
                var payload = JsonConvert.DeserializeObject<IDosGames.ServerWithdrawPayload>(result);
                if (payload == null)
                {
                    errorTxt.text = "Invalid payload JSON.";
                    return;
                }

                // Аккаунт-подписант (плательщик комиссий)
                var payer = Web3.Instance.WalletBase.Account;
                if (payer == null)
                {
                    errorTxt.text = "Wallet account is not available.";
                    return;
                }

                manager.ShowScreen(this, "wallet_screen");
                Message.Show(MessageCode.TRANSACTION_BEING_PROCESSED_PLEASE_WAIT);

                RequestResult<string> res = await _svc.WithdrawSplAsync(
                    payer: payer,
                    srv: payload
                );

                // Если RPC неуспешен — показываем причину и выходим
                if (res == null || !res.WasSuccessful || string.IsNullOrWhiteSpace(res.Result))
                {
                    HandleRpcResult(res);
                    return;
                }

                UserDataService.RequestUserAllData();
            }
            catch (Exception ex)
            {
                errorTxt.text = $"Withdraw error: {ex.Message}";
            }
        }

        private static bool TryParseUiAmount(string input, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(input)) return false;
            var norm = input.Trim().Replace(',', '.');
            return decimal.TryParse(norm, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static ulong UiToRaw(decimal uiAmount, int decimals)
        {
            if (decimals < 0) decimals = 0;

            decimal factor = 1m;
            for (int i = 0; i < decimals; i++) factor *= 10m;

            var scaled = uiAmount * factor;
            scaled = scaled >= 0 ? decimal.Floor(scaled) : decimal.Ceiling(scaled);

            return (ulong)scaled;
        }

        private string ExplainRpcRaw(string raw)
        {
            try
            {
                var jo = JObject.Parse(raw);
                var code = (int?)jo["error"]?["code"];
                var msg = (string)jo["error"]?["message"];
                var err = jo["error"]?["data"]?["err"]?.ToString();

                if (err != null && err.Contains("InsufficientFundsForRent"))
                    return "There wasn't enough SOL for rent when creating an account. Top up your wallet and try again.";

                return !string.IsNullOrWhiteSpace(msg) ? msg : "RPC simulation error";
            }
            catch { return null; }
        }

        private void HandleRpcResult(RequestResult<string> res)
        {
            Loading.HideAllPanels();
            if (res == null) { errorTxt.text = "Null RPC result."; Message.Show("Null RPC result"); return; }

            if (res.WasSuccessful && !string.IsNullOrEmpty(res.Result))
            {
                errorTxt.text = "";
                manager.ShowScreen(this, "wallet_screen");
                return;
            }

            var reason = string.IsNullOrWhiteSpace(res.Reason) ? "RPC error" : res.Reason;
            var raw = res.RawRpcResponse;
            var friendly = !string.IsNullOrEmpty(raw) ? ExplainRpcRaw(raw) : null;
            var finalMsg = friendly ?? reason;

            Debug.LogError($"RPC error: {finalMsg}\nRAW: {raw}");
            errorTxt.text = finalMsg;
            Message.Show(finalMsg);
        }

        private async Task<bool> HasMinSolAsync(ulong minLamports = 1_000_000) // 0.001 SOL по умолчанию
        {
            var payerPk = Web3.Instance.WalletBase.Account.PublicKey;
            var balResp = await _rpcClient.GetBalanceAsync(payerPk.Key, Commitment.Confirmed);
            ulong lamports = (ulong)(balResp?.Result?.Value ?? 0UL);

            if (lamports < minLamports)
            {
                const double LAMPORTS_PER_SOL = 1_000_000_000d;
                double needSol = minLamports / LAMPORTS_PER_SOL;
                double haveSol = lamports / LAMPORTS_PER_SOL;

                string error = $"Not enough SOL: needed ≥ {needSol.ToString("0.########", CultureInfo.InvariantCulture)} SOL " +
                                $"(you have ~{haveSol.ToString("0.########", CultureInfo.InvariantCulture)} SOL)";
                Message.Show(error);
                Loading.HideAllPanels();
                return false;
            }
            return true;
        }

        private async Task<int> FetchMintDecimalsAsync(PublicKey mint)
        {
            try
            {
                var resp = await _rpcClient.GetTokenMintInfoAsync(mint.Key, Commitment.Confirmed);
                if (resp != null && resp.WasSuccessful)
                {
                    var v = resp.Result?.Value;
                    var info = v?.Data?.Parsed?.Info;
                    if (info != null)
                        return (int)info.Decimals; // byte → int
                }
            }
            catch { /* ignore */ }

            try
            {
                var sup = await _rpcClient.GetTokenSupplyAsync(mint.Key, Commitment.Confirmed);
                if (sup != null && sup.WasSuccessful && sup.Result?.Value != null)
                    return (int)sup.Result.Value.Decimals;
            }
            catch { /* ignore */ }

            return 0;
        }
    }
}
#endif