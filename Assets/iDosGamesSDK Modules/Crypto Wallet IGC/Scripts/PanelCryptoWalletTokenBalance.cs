using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class PanelCryptoWalletTokenBalance : MonoBehaviour
	{
		public const string AMOUNT_LOADING_TEXT = "...";

		public const int TOKEN_DIGITS_AMOUNT_AFTER_DOT = 0;
		public readonly string TOKEN_AMOUNT_FORMAT = $"N{TOKEN_DIGITS_AMOUNT_AFTER_DOT}";
		public const int Native_TOKEN_DIGITS_AMOUNT_AFTER_DOT = 8;

		public BigInteger HardTokenBalanceInWei { get; private set; }
		public BigInteger SoftTokenBalanceInWei { get; private set; }
		public BigInteger NativeTokenBalanceInWei { get; private set; }
        public int BalanceOfNFT { get; private set; }

        private readonly Dictionary<int, int> _eachNFTAmount = new();

		[SerializeField] private GameObject _loading;
		[SerializeField] private GameObject _buttonRefresh;
		[SerializeField] private TMP_Text _amountIGT;
		[SerializeField] private TMP_Text _amountIGC;
		[SerializeField] private TMP_Text _amountNFT;
		[SerializeField] private TMP_Text _amountNativeToken;

#if IDOSGAMES_CRYPTO_WALLET
		private void OnEnable()
		{
			//Refresh();
		}

		public async void Refresh()
		{
			SetActivateLoading(true);

			HardTokenBalanceInWei = await WalletService.GetTokenBalance(VirtualCurrencyID.IG);
			SoftTokenBalanceInWei = await WalletService.GetTokenBalance(VirtualCurrencyID.CO);
			NativeTokenBalanceInWei = await WalletService.GetNativeTokenBalanceInWei();

			var balanceNFTList = await WalletService.GetNFTBalance(new(UserDataService.NFTIDs));
			UpdateNFTBalance(balanceNFTList);

			UpdateUI();
			SetActivateLoading(false);
		}

		private void UpdateNFTBalance(List<BigInteger> nftIDs)
		{
			_eachNFTAmount.Clear();

			int sum = 0;

			for (int i = 0; i < UserDataService.NFTIDs.Count; i++)
			{
				if (nftIDs.Count <= i)
				{
					continue;
				}

				_eachNFTAmount[(int)UserDataService.NFTIDs[i]] = (int)nftIDs[i];
				sum += (int)nftIDs[i];
			}

			BalanceOfNFT = sum;
		}

		public int GetNFTAmount(int nftID)
		{
			_eachNFTAmount.TryGetValue(nftID, out int amount);

			return amount;
		}

		private void SetActivateLoading(bool active)
		{
			_loading.SetActive(active);
			_buttonRefresh.SetActive(!active);

			if (active)
			{
				UpdateIGTAmountUI(AMOUNT_LOADING_TEXT);
				UpdateIGCAmountUI(AMOUNT_LOADING_TEXT);
				UpdateNFTAmountUI(AMOUNT_LOADING_TEXT);
				UpdateNativeTokenAmountUI(AMOUNT_LOADING_TEXT);
			}
		}

		private void UpdateUI()
		{
			UpdateIGTAmountUI(GetTokenAmountInEth(HardTokenBalanceInWei, TOKEN_DIGITS_AMOUNT_AFTER_DOT));
			UpdateIGCAmountUI(GetTokenAmountInEth(SoftTokenBalanceInWei, TOKEN_DIGITS_AMOUNT_AFTER_DOT));
			UpdateNFTAmountUI(BalanceOfNFT.ToString(TOKEN_AMOUNT_FORMAT));
			UpdateNativeTokenAmountUI(GetTokenAmountInEth(NativeTokenBalanceInWei, Native_TOKEN_DIGITS_AMOUNT_AFTER_DOT));
		}

        private string GetTokenAmountInEth(BigInteger tokenBalance, int decimalPlaces)
        {
            const int decimals = 18;
            BigInteger divisor = BigInteger.Pow(10, decimals - decimalPlaces);

            if (divisor == 0)
                return "0".PadRight(decimalPlaces + 2, '0');

            BigInteger scaledValue = tokenBalance / divisor;
            string result = scaledValue.ToString();

            if (result.Length <= decimalPlaces)
            {
                result = result.PadLeft(decimalPlaces + 1, '0');
            }

            string withDot = result.Insert(result.Length - decimalPlaces, ".");

            string[] parts = withDot.Split('.');
            string integerPart = parts[0].TrimStart('0');
            string fractionalPart = parts[1].TrimEnd('0');

            if (string.IsNullOrEmpty(integerPart))
            {
                integerPart = "0";
            }

            if (fractionalPart.Length > 0)
            {
                return $"{integerPart}.{fractionalPart}";
            }
            else
            {
                return integerPart;
            }
        }

        public int GetTokenAmount(VirtualCurrencyID currencyID)
		{
            if (currencyID == VirtualCurrencyID.IG)
            {
				return int.Parse(GetTokenAmountInEth(HardTokenBalanceInWei, TOKEN_DIGITS_AMOUNT_AFTER_DOT));
            }
            else if (currencyID == VirtualCurrencyID.CO)
            {
                return int.Parse(GetTokenAmountInEth(SoftTokenBalanceInWei, TOKEN_DIGITS_AMOUNT_AFTER_DOT));
            }
			else
			{
				return 0;
			}
        }

        private void UpdateIGTAmountUI(string text)
		{
			_amountIGT.text = text;
		}

		private void UpdateIGCAmountUI(string text)
		{
			_amountIGC.text = text;
		}

		private void UpdateNFTAmountUI(string text)
		{
			_amountNFT.text = text;
		}

		private void UpdateNativeTokenAmountUI(string text)
		{
			_amountNativeToken.text = text;
		}
#endif

    }
}
