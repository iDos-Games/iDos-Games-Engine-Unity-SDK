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

// ReSharper disable once CheckNamespace

namespace IDosGames
{
    /// <summary>
    /// Ёкран работы с программой-пулом: депозит/вывод SPL-токенов в/из смарт-контракта.
    /// ќснован на идее TransferScreen, но использует SolanaPlatformPoolService.
    /// </summary>
    public class PlatformPoolTransferScreen : SimpleScreen
    {
        [Header("Pool Service Config")]
        [Tooltip("RPC endpoint, напр. https://api.mainnet-beta.solana.com или Devnet")]
        public string rpcUrl = "https://api.devnet.solana.com";
        [Tooltip("Program ID (base58) вашей Anchor-программы")]
        public string programIdBase58 = "FWvDZMpUy9SPgRV6rJSa6fju1VtYejPNqshXpgA9BzsG";

        [Header("UI")]
        public TextMeshProUGUI tokenTitleTxt;   // название токена/NFT
        public TextMeshProUGUI balanceTxt;      // баланс токена (UI, в человекочитаемом виде)
        public RawImage tokenImage;

        public Toggle depositToggle;             // переключатель "ƒепозит"
        public Toggle withdrawToggle;            // переключатель "¬ывод"

        public TMP_InputField mintTxt;          // mint address (prefill из входных данных)
        public TMP_InputField amountTxt;        // сумма в UI-единицах (учитыва€ decimals)
        public TMP_InputField userIdTxt;        // userId дл€ deposit_spl
        [TextArea(3, 6)]
        public TMP_InputField serverPayloadJsonTxt; // JSON от бэка дл€ withdraw_spl

        public TextMeshProUGUI errorTxt;
        public Button actionBtn;
        public Button closeBtn;

        // ¬ходные данные (как в TransferScreen): либо TokenAccount+meta, либо NFT
        private TokenAccount _tokenAccount;
        private Nft _nft;

        // “ехническое
        private IDosGames.SolanaPlatformPoolService _svc;
        private IRpcClient _rpcClient;
        private PublicKey _mintPk;
        private int _decimals = 0;
        private Texture2D _texture;

        private const long SolLamports = 1_000_000_000; // не нужен напр€мую, но пусть будет дл€ консистентности

        private void Awake()
        {
            // »нициализируем RPC и сервис (можно сделать лениво, но так надЄжнее)
            _rpcClient = ClientFactory.GetClient(rpcUrl);
            _svc = new IDosGames.SolanaPlatformPoolService(_rpcClient, programIdBase58);
        }

        private void Start()
        {
            actionBtn.onClick.AddListener(OnAction);
            closeBtn.onClick.AddListener(() => manager.ShowScreen(this, "wallet_screen"));

            // ѕо умолчанию Ч депозит
            if (depositToggle != null && withdrawToggle != null)
            {
                depositToggle.isOn = true;
                withdrawToggle.isOn = false;
            }
        }

        public override void ShowScreen(object data = null)
        {
            base.ShowScreen();
            ResetState();
            PopulateFromData(data);
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
                // при открытии Ч режим депозит
                depositToggle.isOn = true;
                withdrawToggle.isOn = false;
            }
        }

        private void PopulateFromData(object data)
        {
            // ѕоддерживаем те же формы данных, что и TransferScreen:
            // 1) Tuple<TokenAccount, string, Texture2D> дл€ фанджибл токенов
            // 2) Nft.Nft дл€ NFT
            // 3) string (mint) Ч опционально, если хочетс€ открыть "пустой" экран по mint

            tokenImage?.gameObject.SetActive(true);
            tokenTitleTxt.gameObject.SetActive(true);
            balanceTxt.gameObject.SetActive(true);

            if (data != null && data.GetType() == typeof(Tuple<TokenAccount, string, Texture2D>))
            {
                var (tokenAccount, tokenDef, texture) = (Tuple<TokenAccount, string, Texture2D>)data;
                _tokenAccount = tokenAccount;
                _texture = texture;

                _mintPk = new PublicKey(tokenAccount.Account.Data.Parsed.Info.Mint);
                _decimals = tokenAccount.Account.Data.Parsed.Info.TokenAmount.Decimals;

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

                // NFT: decimals = 0, amount фиксированный = 1
                _decimals = 0;
                _mintPk = new PublicKey(_nft.metaplexData.data.mint);

                balanceTxt.text = "1";
                mintTxt.text = _mintPk.Key;

                amountTxt.text = "1";
                amountTxt.interactable = false; // NFT всегда 1 шт.
            }
            else if (data is string mintStr && !string.IsNullOrWhiteSpace(mintStr))
            {
                // ≈сли открыли экран просто по mint
                _mintPk = new PublicKey(mintStr);
                _decimals = 0; // если нужно Ч можно дозапросить decimals по mint через RPC
                tokenTitleTxt.text = mintStr;
                balanceTxt.text = "-";
                mintTxt.text = mintStr;
                amountTxt.interactable = true;
                amountTxt.text = "";
            }
            else
            {
                // Ѕез входных данных экран смысла не имеет Ч но дадим пользователю самому ввести mint
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
                // если тумблеров нет Ч по умолчанию депозит
                await DoDeposit();
            }
        }

        private async Task DoDeposit()
        {
            try
            {
                // ¬алидации
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

                // ѕересчЄт из UI в "сырые" единицы
                var raw = UiToRaw(uiAmount, _decimals);
                _mintPk = new PublicKey(mintTxt.text.Trim());

                // јккаунт-подписант (кошелЄк игрока) Ч берЄм из Web3
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
                // ƒл€ вывода нам нужен JSON-пейлоад с сервера
                if (string.IsNullOrWhiteSpace(serverPayloadJsonTxt.text))
                {
                    errorTxt.text = "Paste server payload JSON for withdrawal.";
                    return;
                }

                // ќпционально: сверим mint из JSON с полем mintTxt (если введено)
                var payload = JsonConvert.DeserializeObject<IDosGames.ServerWithdrawPayload>(serverPayloadJsonTxt.text.Trim());
                if (payload == null)
                {
                    errorTxt.text = "Invalid payload JSON.";
                    return;
                }

                // јккаунт-подписант (плательщик комиссий)
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
            var factor = (decimal)Math.Pow(10, decimals);
            var raw = (ulong)Math.Floor(uiAmount * factor);
            return raw;
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
                // ”спех Ч закрываем экран в кошелЄк (как в TransferScreen)
                errorTxt.text = "";
                manager.ShowScreen(this, "wallet_screen");
            }
            else
            {
                errorTxt.text = res.Reason ?? "Unknown RPC error.";
            }
        }
    }
}
