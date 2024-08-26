using System;
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
		public const int Native_TOKEN_DIGITS_AMOUNT_AFTER_DOT = 5;
		public readonly string NATIVE_TOKEN_AMOUNT_FORMAT = $"N{Native_TOKEN_DIGITS_AMOUNT_AFTER_DOT}";

		public int BalanceOfIGT { get; private set; }
		public int BalanceOfIGC { get; private set; }
		public int BalanceOfNFT { get; private set; }
		public decimal BalanceOfNativeToken { get; private set; }

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

			BalanceOfIGT = (int)await WalletService.GetTokenBalance(VirtualCurrencyID.IG);
			BalanceOfIGC = (int)await WalletService.GetTokenBalance(VirtualCurrencyID.CO);
			BalanceOfNativeToken = await WalletService.GetNativeTokenBalance();

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
			UpdateIGTAmountUI(BalanceOfIGT.ToString(TOKEN_AMOUNT_FORMAT));
			UpdateIGCAmountUI(BalanceOfIGC.ToString(TOKEN_AMOUNT_FORMAT));
			UpdateNFTAmountUI(BalanceOfNFT.ToString(TOKEN_AMOUNT_FORMAT));
			UpdateNativeTokenAmountUI(GetNativeTokenAmountString());
		}

		private string GetNativeTokenAmountString()
		{
			if (BalanceOfNativeToken * (decimal)Math.Pow(10, Native_TOKEN_DIGITS_AMOUNT_AFTER_DOT) <= 0)
			{
				return "0";
			}

			return BalanceOfNativeToken.ToString(NATIVE_TOKEN_AMOUNT_FORMAT);
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
