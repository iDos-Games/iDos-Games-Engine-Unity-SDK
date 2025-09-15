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

// ReSharper disable once CheckNamespace

namespace IDosGames
{
    public class PlatformPoolTransferScreen : SimpleScreen
    {
        [Header("Pool Service Config")]
        public string rpcUrl = "https://api.devnet.solana.com";
        public string programIdBase58 = "FWvDZMpUy9SPgRV6rJSa6fju1VtYejPNqshXpgA9BzsG";

        [Header("UI")]
        public TextMeshProUGUI tokenTitleTxt;   // название токена/NFT
        public TextMeshProUGUI balanceTxt;      // баланс токена (UI, в человекочитаемом виде)
        public RawImage tokenImage;

        public Toggle depositToggle;             // переключатель "Депозит"
        public Toggle withdrawToggle;            // переключатель "Вывод"

        public TMP_InputField mintTxt;          // mint address (prefill из входных данных)
        public TMP_InputField amountTxt;        // сумма в UI-единицах (учитывая decimals)
        public TMP_InputField userIdTxt;        // userId для deposit_spl
        [TextArea(3, 6)]
        public TMP_InputField serverPayloadJsonTxt; // JSON от бэка для withdraw_spl

        public TextMeshProUGUI errorTxt;
        public Button actionBtn;
        public Button closeBtn;

        // Входные данные (как в TransferScreen): либо TokenAccount+meta, либо NFT
        private TokenAccount _tokenAccount;
        private Nft _nft;

        // Техническое
        private IDosGames.SolanaPlatformPoolService _svc;
        private IRpcClient _rpcClient;
        private PublicKey _mintPk;
        private int _decimals = 0;
        private Texture2D _texture;

        private const long SolLamports = 1_000_000_000; // не нужен напрямую, но пусть будет для консистентности

        private void Awake()
        {
            // Инициализируем RPC и сервис (можно сделать лениво, но так надёжнее)
            _rpcClient = ClientFactory.GetClient(rpcUrl);
            _svc = new IDosGames.SolanaPlatformPoolService(_rpcClient, programIdBase58);
        }

        private void Start()
        {
            actionBtn.onClick.AddListener(OnAction);
            closeBtn.onClick.AddListener(() => manager.ShowScreen(this, "wallet_screen"));

            // По умолчанию — депозит
            if (depositToggle != null && withdrawToggle != null)
            {
                depositToggle.isOn = true;
                withdrawToggle.isOn = false;
            }
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
            mintTxt.text = "";
            amountTxt.text = "";
            userIdTxt.text = "";
            serverPayloadJsonTxt.text = "";

            if (tokenImage != null)
            {
                tokenImage.texture = null;
                tokenImage.color = new Color(1, 1, 1, 0); // прозрачно до загрузки
            }

            _nft = null;
            _tokenAccount = null;
            _texture = null;
            _mintPk = default;
            _decimals = 0;

            if (depositToggle != null && withdrawToggle != null)
            {
                // при открытии — режим депозит
                depositToggle.isOn = true;
                withdrawToggle.isOn = false;
            }
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
                _texture = texture;

                _mintPk = new PublicKey(tokenAccount.Account.Data.Parsed.Info.Mint);

                // НЕ доверяем только локальным данным — подтверждаем decimals с блокчейна
                _decimals = await FetchMintDecimalsAsync(_mintPk);

                tokenTitleTxt.text = tokenDef;
                balanceTxt.text = tokenAccount.Account.Data.Parsed.Info.TokenAmount.AmountDecimal
                    .ToString(CultureInfo.CurrentCulture);

                if (tokenImage != null && texture != null)
                {
                    tokenImage.texture = texture;
                    tokenImage.color = Color.white;
                }

                mintTxt.text = _mintPk.Key;
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
                mintTxt.text = _mintPk.Key;

                amountTxt.text = "1";
                amountTxt.interactable = false;
            }
            else if (data is string mintStr && !string.IsNullOrWhiteSpace(mintStr))
            {
                _mintPk = new PublicKey(mintStr);

                // ПОДТЯГИВАЕМ decimals сразу
                _decimals = await FetchMintDecimalsAsync(_mintPk);

                tokenTitleTxt.text = mintStr;
                balanceTxt.text = "-";
                mintTxt.text = mintStr;
                amountTxt.interactable = true;
                amountTxt.text = "";
            }
            else
            {
                tokenTitleTxt.text = "SPL Token";
                balanceTxt.text = "-";
                mintTxt.text = "";
                amountTxt.interactable = true;
            }
        }

        private async void OnAction()
        {
            errorTxt.text = "";

            if (depositToggle != null && depositToggle.isOn)
            {
                await DoDeposit();
            }
            else if (withdrawToggle != null && withdrawToggle.isOn)
            {
                await DoWithdraw();
            }
            else
            {
                // если тумблеров нет — по умолчанию депозит
                await DoDeposit();
            }
        }

        private async Task DoDeposit()
        {
            try
            {
                // Валидации
                if (string.IsNullOrWhiteSpace(mintTxt.text))
                {
                    errorTxt.text = "Mint address is required.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(userIdTxt.text))
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
                _mintPk = new PublicKey(mintTxt.text.Trim());

                // ВСЕГДА подтягиваем decimals из сети перед отправкой
                _decimals = await FetchMintDecimalsAsync(_mintPk);

                // Теперь корректный пересчёт UI → raw (u64)
                var raw = UiToRaw(uiAmount, _decimals);

                // (опционально) быстрое превью в UI, чтобы глазами сверить
                balanceTxt.text = $"Send preview: {uiAmount.ToString(CultureInfo.InvariantCulture)} → {raw} (10^{_decimals})";

                // Аккаунт-подписант (кошелёк игрока) — берём из Web3
                var payer = Web3.Instance.WalletBase.Account;
                if (payer == null)
                {
                    errorTxt.text = "Wallet account is not available.";
                    return;
                }

                RequestResult<string> res = await _svc.DepositSplAsync(
                    payer: payer,
                    mint: _mintPk,
                    amount: raw,
                    userId: userIdTxt.text.Trim()
                );

                HandleRpcResult(res);
            }
            catch (Exception ex)
            {
                errorTxt.text = $"Deposit error: {ex.Message}";
            }
        }

        private async Task DoWithdraw()
        {
            try
            {
                // Для вывода нам нужен JSON-пейлоад с сервера
                if (string.IsNullOrWhiteSpace(serverPayloadJsonTxt.text))
                {
                    errorTxt.text = "Paste server payload JSON for withdrawal.";
                    return;
                }

                // Опционально: сверим mint из JSON с полем mintTxt (если введено)
                var payload = JsonConvert.DeserializeObject<IDosGames.ServerWithdrawPayload>(serverPayloadJsonTxt.text.Trim());
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

                RequestResult<string> res = await _svc.WithdrawSplAsync(
                    payer: payer,
                    srv: payload
                );

                HandleRpcResult(res);
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

            // Вычисляем factor = 10^decimals без double
            decimal factor = 1m;
            for (int i = 0; i < decimals; i++) factor *= 10m;

            // Масштабируем и «обрезаем к нулю» без MidpointRounding.ToZero:
            // положительные → Floor, отрицательные → Ceiling (на всякий случай)
            var scaled = uiAmount * factor;
            scaled = scaled >= 0 ? decimal.Floor(scaled) : decimal.Ceiling(scaled);

            return (ulong)scaled;
        }

        private void HandleRpcResult(RequestResult<string> res)
        {
            if (res == null)
            {
                errorTxt.text = "Null RPC result.";
                return;
            }

            if (!string.IsNullOrEmpty(res.Result))
            {
                // Успех — закрываем экран в кошелёк (как в TransferScreen)
                errorTxt.text = "";
                manager.ShowScreen(this, "wallet_screen");
            }
            else
            {
                errorTxt.text = res.Reason ?? "Unknown RPC error.";
            }
        }

        private async Task<int> FetchMintDecimalsAsync(PublicKey mint)
        {
            // 1) Основной путь: MintInfo → Data.Parsed.Info.Decimals
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

            // 2) Фоллбэк: TokenSupply → Value.Decimals
            try
            {
                var sup = await _rpcClient.GetTokenSupplyAsync(mint.Key, Commitment.Confirmed);
                if (sup != null && sup.WasSuccessful && sup.Result?.Value != null)
                    return (int)sup.Result.Value.Decimals;
            }
            catch { /* ignore */ }

            // 3) Хуже не станет — считаем 0
            return 0;
        }
    }
}
