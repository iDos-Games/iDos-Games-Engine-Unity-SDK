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
    /// ����� ������ � ����������-�����: �������/����� SPL-������� �/�� �����-���������.
    /// ������� �� ���� TransferScreen, �� ���������� SolanaPlatformPoolService.
    /// </summary>
    public class PlatformPoolTransferScreen : SimpleScreen
    {
        [Header("Pool Service Config")]
        [Tooltip("RPC endpoint, ����. https://api.mainnet-beta.solana.com ��� Devnet")]
        public string rpcUrl = "https://api.devnet.solana.com";
        [Tooltip("Program ID (base58) ����� Anchor-���������")]
        public string programIdBase58 = "FWvDZMpUy9SPgRV6rJSa6fju1VtYejPNqshXpgA9BzsG";

        [Header("UI")]
        public TextMeshProUGUI tokenTitleTxt;   // �������� ������/NFT
        public TextMeshProUGUI balanceTxt;      // ������ ������ (UI, � ���������������� ����)
        public RawImage tokenImage;

        public Toggle depositToggle;             // ������������� "�������"
        public Toggle withdrawToggle;            // ������������� "�����"

        public TMP_InputField mintTxt;          // mint address (prefill �� ������� ������)
        public TMP_InputField amountTxt;        // ����� � UI-�������� (�������� decimals)
        public TMP_InputField userIdTxt;        // userId ��� deposit_spl
        [TextArea(3, 6)]
        public TMP_InputField serverPayloadJsonTxt; // JSON �� ���� ��� withdraw_spl

        public TextMeshProUGUI errorTxt;
        public Button actionBtn;
        public Button closeBtn;

        // ������� ������ (��� � TransferScreen): ���� TokenAccount+meta, ���� NFT
        private TokenAccount _tokenAccount;
        private Nft _nft;

        // �����������
        private IDosGames.SolanaPlatformPoolService _svc;
        private IRpcClient _rpcClient;
        private PublicKey _mintPk;
        private int _decimals = 0;
        private Texture2D _texture;

        private const long SolLamports = 1_000_000_000; // �� ����� ��������, �� ����� ����� ��� ���������������

        private void Awake()
        {
            // �������������� RPC � ������ (����� ������� ������, �� ��� �������)
            _rpcClient = ClientFactory.GetClient(rpcUrl);
            _svc = new IDosGames.SolanaPlatformPoolService(_rpcClient, programIdBase58);
        }

        private void Start()
        {
            actionBtn.onClick.AddListener(OnAction);
            closeBtn.onClick.AddListener(() => manager.ShowScreen(this, "wallet_screen"));

            // �� ��������� � �������
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
                tokenImage.color = new Color(1, 1, 1, 0); // ��������� �� ��������
            }

            _nft = null;
            _tokenAccount = null;
            _texture = null;
            _mintPk = default;
            _decimals = 0;

            if (depositToggle != null && withdrawToggle != null)
            {
                // ��� �������� � ����� �������
                depositToggle.isOn = true;
                withdrawToggle.isOn = false;
            }
        }

        private void PopulateFromData(object data)
        {
            // ������������ �� �� ����� ������, ��� � TransferScreen:
            // 1) Tuple<TokenAccount, string, Texture2D> ��� �������� �������
            // 2) Nft.Nft ��� NFT
            // 3) string (mint) � �����������, ���� ������� ������� "������" ����� �� mint

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

                // NFT: decimals = 0, amount ������������� = 1
                _decimals = 0;
                _mintPk = new PublicKey(_nft.metaplexData.data.mint);

                balanceTxt.text = "1";
                mintTxt.text = _mintPk.Key;

                amountTxt.text = "1";
                amountTxt.interactable = false; // NFT ������ 1 ��.
            }
            else if (data is string mintStr && !string.IsNullOrWhiteSpace(mintStr))
            {
                // ���� ������� ����� ������ �� mint
                _mintPk = new PublicKey(mintStr);
                _decimals = 0; // ���� ����� � ����� ����������� decimals �� mint ����� RPC
                tokenTitleTxt.text = mintStr;
                balanceTxt.text = "-";
                mintTxt.text = mintStr;
                amountTxt.interactable = true;
                amountTxt.text = "";
            }
            else
            {
                // ��� ������� ������ ����� ������ �� ����� � �� ����� ������������ ������ ������ mint
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
                // ���� ��������� ��� � �� ��������� �������
                await DoDeposit();
            }
        }

        private async Task DoDeposit()
        {
            try
            {
                // ���������
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

                // �������� �� UI � "�����" �������
                var raw = UiToRaw(uiAmount, _decimals);
                _mintPk = new PublicKey(mintTxt.text.Trim());

                // �������-��������� (������ ������) � ���� �� Web3
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
                // ��� ������ ��� ����� JSON-������� � �������
                if (string.IsNullOrWhiteSpace(serverPayloadJsonTxt.text))
                {
                    errorTxt.text = "Paste server payload JSON for withdrawal.";
                    return;
                }

                // �����������: ������ mint �� JSON � ����� mintTxt (���� �������)
                var payload = JsonConvert.DeserializeObject<IDosGames.ServerWithdrawPayload>(serverPayloadJsonTxt.text.Trim());
                if (payload == null)
                {
                    errorTxt.text = "Invalid payload JSON.";
                    return;
                }

                // �������-��������� (���������� ��������)
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
                // ����� � ��������� ����� � ������ (��� � TransferScreen)
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
