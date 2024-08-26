using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class PopUpWalletTransferView : MonoBehaviour
	{
		[SerializeField] private TMP_Dropdown _tokenDropdown;
		[SerializeField] private AmountInputField _amountInputField;
		[SerializeField] private WalletTransferDirection _transferDirection;
		[SerializeField] private PanelCryptoWalletTokenBalance _cryptoTokenBalance;
		[SerializeField] private ButtonWithOptionalIcon _transferButton;
		[SerializeField] private WalletSelectSkinButton _selectSkinButton;

		[SerializeField] private GameObject _tokenAsset;
		[SerializeField] private GameObject _nftAsset;

		public CryptoTransactionType TransactionType { get; private set; }

#if IDOSGAMES_CRYPTO_WALLET
		private void OnEnable()
		{
			ResetAmountInputField();

			UpdateAssetType();

			UpdateIconVisibilityOnTransferBtn();
			_transferDirection.ValueChanged += UpdateAvailableAmount;
			_transferDirection.ValueChanged += UpdateIconVisibilityOnTransferBtn;

			if (TransactionType == CryptoTransactionType.Token)
			{
				ResetTokenDropdown();
				_tokenDropdown.onValueChanged.AddListener((text) => UpdateAvailableAmount());
			}
			else if (TransactionType == CryptoTransactionType.NFT)
			{
				_selectSkinButton.ValueChanged += UpdateAvailableAmount;
			}
		}

		private void OnDisable()
		{
			if (TransactionType == CryptoTransactionType.Token)
			{
				_transferDirection.ValueChanged -= UpdateAvailableAmount;
				_tokenDropdown.onValueChanged.RemoveAllListeners();
			}
			else if (TransactionType == CryptoTransactionType.NFT)
			{
				_selectSkinButton.ValueChanged -= UpdateAvailableAmount;
			}
		}

		public void SetTransactionType(CryptoTransactionType transactionType)
		{
			TransactionType = transactionType;
		}

		public SkinCatalogItem GetSkinInput()
		{
			return _selectSkinButton.SelectedSkin;
		}

		public string GetAmountInput()
		{
			return _amountInputField.GetInput().Trim();
		}

		public bool GetAmountInputStatus()
		{
			return _amountInputField.IsAmountCorrect;
		}

		public void ChangeAmountFieldColorToIncorrect()
		{
			_amountInputField.ChangeOuterFrameColor(false);
		}

		public VirtualCurrencyID GetTokenInput()
		{
			if (_tokenDropdown.captionText.text.Trim().ToUpper() == JsonProperty.IGT.ToUpper())
			{
				return VirtualCurrencyID.IG;
			}
			else
			{
				return VirtualCurrencyID.CO;
			}
		}

		public TransactionDirection GetTransferDirection()
		{
			return _transferDirection.Direction;
		}

		private void UpdateAssetType()
		{
			_tokenAsset.SetActive(TransactionType == CryptoTransactionType.Token);
			_nftAsset.SetActive(TransactionType == CryptoTransactionType.NFT);
		}

		private void ResetTokenDropdown()
		{
			_tokenDropdown.value = 0;
		}

		private void ResetAmountInputField()
		{
			_amountInputField.ResetInput();
			UpdateAvailableAmount();
		}

		private void UpdateAvailableAmount()
		{
			if (TransactionType == CryptoTransactionType.Token)
			{
				UpdateTokenAvailableAmount();
			}
			else if (TransactionType == CryptoTransactionType.NFT)
			{
				UpdateNFTAvailableAmount();
			}
		}

		private void UpdateTokenAvailableAmount()
		{
			int amount = 0;

			var direction = GetTransferDirection();
			var tokenInput = GetTokenInput();

			if (direction == TransactionDirection.UsersCryptoWallet)
			{
				amount = UserInventory.GetVirtualCurrencyAmount(tokenInput.ToString());
			}
			else if (direction == TransactionDirection.Game)
			{
				if (tokenInput == VirtualCurrencyID.IG)
				{
					amount = _cryptoTokenBalance.BalanceOfIGT;
				}
				else if (tokenInput == VirtualCurrencyID.CO)
				{
					amount = _cryptoTokenBalance.BalanceOfIGC;
				}
			}

			_amountInputField.UpdateAvailableAmount(amount);
		}

		private void UpdateNFTAvailableAmount()
		{
			int amount = 0;

			var direction = GetTransferDirection();

			var skinInput = GetSkinInput();

			if (skinInput != null)
			{
				if (direction == TransactionDirection.UsersCryptoWallet)
				{
					amount = UserInventory.GetItemAmount(skinInput.ItemID);
				}
				else if (direction == TransactionDirection.Game)
				{
					amount = _cryptoTokenBalance.GetNFTAmount(skinInput.NFTID);
				}
			}

			_amountInputField.UpdateAvailableAmount(amount);
		}

		private void UpdateIconVisibilityOnTransferBtn()
		{
			_transferButton.SetActivateIcon(GetTransferDirection() == TransactionDirection.UsersCryptoWallet);
		}
#endif

    }
}
