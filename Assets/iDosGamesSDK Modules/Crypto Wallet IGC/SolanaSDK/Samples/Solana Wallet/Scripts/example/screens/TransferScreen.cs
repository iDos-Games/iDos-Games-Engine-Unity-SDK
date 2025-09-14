using System;
using System.Globalization;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace

namespace Solana.Unity.SDK.Example
{
    public class TransferScreen : SimpleScreen
    {
        public TextMeshProUGUI ownedAmountTxt;
        public TextMeshProUGUI nftTitleTxt;
        public TextMeshProUGUI errorTxt;
        public TMP_InputField toPublicTxt;
        public TMP_InputField amountTxt;
        public Button transferBtn;
        public RawImage nftImage;
        public Button closeBtn;

        private TokenAccount _transferTokenAccount;
        private Nft.Nft _nft;
        private double _ownedSolAmount;
        
        private const long SolLamports = 1000000000;

        private void Start()
        {
            transferBtn.onClick.AddListener(TryTransfer);

            closeBtn.onClick.AddListener(() =>
            {
                manager.ShowScreen(this, "wallet_screen");
            });
        }

        private void TryTransfer()
        {
            if (_nft != null)
            {
                TransferNft();
            }
            else if (_transferTokenAccount == null)
            {
                if (CheckInput())
                    TransferSol();
            }
            else
            {
                if (CheckInput())
                    TransferToken();
            }
        }

        private async void TransferSol()
        {
            RequestResult<string> result = await Web3.Instance.WalletBase.Transfer(
                new PublicKey(toPublicTxt.text),
                Convert.ToUInt64(float.Parse(amountTxt.text)*SolLamports));
            HandleResponse(result);
        }

        private async void TransferNft()
        {
            RequestResult<string> result = await Web3.Instance.WalletBase.Transfer(
                new PublicKey(toPublicTxt.text),
                new PublicKey(_nft.metaplexData.data.mint),
                1);
            HandleResponse(result);
        }

        bool CheckInput()
        {
            if (string.IsNullOrWhiteSpace(toPublicTxt.text))
            {
                errorTxt.text = "Please enter receiver public key";
                return false;
            }

            if (string.IsNullOrWhiteSpace(amountTxt.text))
            {
                errorTxt.text = "Please input transfer amount";
                return false;
            }

            // Нормализуем разделитель для парсинга
            var normalized = amountTxt.text.Trim().Replace(',', '.');

            if (_transferTokenAccount == null && _nft == null)
            {
                // Режим SOL
                if (!decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var solAmount))
                {
                    errorTxt.text = "Invalid SOL amount";
                    return false;
                }

                if (solAmount <= 0 || (double)solAmount > _ownedSolAmount)
                {
                    errorTxt.text = "Not enough funds for transaction.";
                    return false;
                }
            }
            else if (_nft != null)
            {
                // Режим NFT — количество фиксировано = 1, поле отключено
                // Доп. проверок не требуется
            }
            else
            {
                // Режим SPL-токена
                var info = _transferTokenAccount.Account.Data.Parsed.Info.TokenAmount;

                if (!decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var uiAmount))
                {
                    errorTxt.text = "Invalid token amount";
                    return false;
                }

                var uiBalance = Convert.ToDecimal(info.AmountDecimal, CultureInfo.InvariantCulture);

                if (uiAmount <= 0 || uiAmount > uiBalance)
                {
                    errorTxt.text = "Not enough funds for transaction.";
                    return false;
                }
            }

            errorTxt.text = "";
            return true;
        }

        private async void TransferToken()
        {
            var info = _transferTokenAccount.Account.Data.Parsed.Info.TokenAmount;
            int decimals = info.Decimals;

            // Берём ввод пользователя и нормализуем разделитель
            var rawInput = (amountTxt.text ?? "").Trim();
            rawInput = rawInput.Replace(',', '.');

            if (!decimal.TryParse(rawInput, NumberStyles.Float, CultureInfo.InvariantCulture, out var uiAmount))
            {
                errorTxt.text = "Invalid token amount";
                return;
            }

            var uiBalance = Convert.ToDecimal(info.AmountDecimal, CultureInfo.InvariantCulture);

            if (uiAmount <= 0 || uiAmount > uiBalance)
            {
                errorTxt.text = "Not enough funds for transaction.";
                return;
            }

            // Пересчёт из UI-количества в «сырые» единицы (лампорты токена)
            var factor = (decimal)Math.Pow(10, decimals);
            var rawAmount = (ulong)Math.Floor(uiAmount * factor);

            var result = await Web3.Instance.WalletBase.Transfer(
                new PublicKey(toPublicTxt.text),
                new PublicKey(_transferTokenAccount.Account.Data.Parsed.Info.Mint),
                rawAmount
            );

            HandleResponse(result);
        }

        private void HandleResponse(RequestResult<string> result)
        {
            errorTxt.text = result.Result == null ? result.Reason : "";
            if (result.Result != null)
            {
                manager.ShowScreen(this, "wallet_screen");
            }
        }

        public override async void ShowScreen(object data = null)
        {
            base.ShowScreen();

            ResetInputFields();
            await PopulateInfoFields(data);
            gameObject.SetActive(true);
        }
        
        public void OnClose()
        {
            var wallet = GameObject.Find("wallet");
            wallet.SetActive(false);
        }

        private async System.Threading.Tasks.Task PopulateInfoFields(object data)
        {
            nftImage.gameObject.SetActive(false);
            nftTitleTxt.gameObject.SetActive(false);
            ownedAmountTxt.gameObject.SetActive(false);

            if (data != null && data.GetType() == typeof(Tuple<TokenAccount, string, Texture2D>))
            {
                var (tokenAccount, tokenDef, texture) = (Tuple<TokenAccount, string, Texture2D>)data;

                // ВАЖНО: сохраняем аккаунт токена, чтобы TryTransfer() пошёл в TransferToken()
                _transferTokenAccount = tokenAccount;

                // Отобразим баланс токена в человекочитаемом виде
                var uiBalance = Convert.ToDecimal(tokenAccount.Account.Data.Parsed.Info.TokenAmount.AmountDecimal, CultureInfo.InvariantCulture);
                ownedAmountTxt.text = uiBalance.ToString(CultureInfo.CurrentCulture);
                ownedAmountTxt.gameObject.SetActive(true);

                nftTitleTxt.gameObject.SetActive(true);
                nftImage.gameObject.SetActive(true);
                nftTitleTxt.text = tokenDef;
                nftImage.texture = texture;
                nftImage.color = Color.white;

                // В этом режиме отправляем SPL-токен, поле количества редактируемо
                amountTxt.interactable = true;
                amountTxt.text = "";
            }
            else if (data != null && data.GetType() == typeof(Nft.Nft))
            {
                // Режим NFT (одно изделие, decimals = 0)
                nftTitleTxt.gameObject.SetActive(true);
                nftImage.gameObject.SetActive(true);
                _nft = (Nft.Nft)data;
                nftTitleTxt.text = $"{_nft.metaplexData.data.offchainData?.name}";
                nftImage.texture = _nft.metaplexData?.nftImage?.file;
                nftImage.color = Color.white;

                amountTxt.text = "1";
                amountTxt.interactable = false;
            }
            else
            {
                // Режим SOL
                _ownedSolAmount = await Web3.Instance.WalletBase.GetBalance();
                ownedAmountTxt.text = $"{_ownedSolAmount}";
                ownedAmountTxt.gameObject.SetActive(true);

                amountTxt.interactable = true;
                amountTxt.text = "";
            }
        }

        private void ResetInputFields()
        {
            errorTxt.text = "";
            amountTxt.text = "";
            toPublicTxt.text = "";
            amountTxt.interactable = true;

            // Сбрасываем прошлое состояние режимов перевода
            _nft = null;
            _transferTokenAccount = null;
        }

        public override void HideScreen()
        {
            base.HideScreen();
            _transferTokenAccount = null;
            gameObject.SetActive(false);
        }
    }

}