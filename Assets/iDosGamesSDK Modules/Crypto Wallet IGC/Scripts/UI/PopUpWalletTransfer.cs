using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class PopUpWalletTransfer : MonoBehaviour
	{
		[SerializeField] private WalletWindow _walletWindow;
		[SerializeField] private PopUpWalletTransferView _view;
		[SerializeField] private Button _transferButton;
		[SerializeField] private PopUpTransferConfirmation _confirmationPopUp;

#if IDOSGAMES_CRYPTO_WALLET
		private void Start()
		{
			_transferButton.onClick.RemoveAllListeners();
			_transferButton.onClick.AddListener(OnClickTransfer);
		}

		public void OpenTokenTransferPopUp()
		{
			_view.SetTransactionType(CryptoTransactionType.Token);
			gameObject.SetActive(true);
		}

		public void OpenNFTTransferPopUp()
		{
			_view.SetTransactionType(CryptoTransactionType.NFT);
			gameObject.SetActive(true);
		}

		public async void TryTransferToken()
		{
			var amountInput = _view.GetAmountInput();
			var virtualCurrencyID = _view.GetTokenInput();
			var direction = _view.GetTransferDirection();

			int.TryParse(amountInput, out int amount);

			await _walletWindow.TransferToken(direction, virtualCurrencyID, amount);
		}

		public async void TryTransferNFT()
		{
			var amountInput = _view.GetAmountInput();
			var direction = _view.GetTransferDirection();
			var skin = _view.GetSkinInput();
			int.TryParse(amountInput, out int amount);

			await _walletWindow.TransferNFT(direction, skin.ItemID, amount);
		}

		private async void OnClickTransfer()
		{
			var amountInput = _view.GetAmountInput();

			if (amountInput == string.Empty)
			{
				Message.Show(MessageCode.AMOUNT_IS_REQUIRED_FIELD);
				_view.ChangeAmountFieldColorToIncorrect();
				return;
			}

			bool isAmountCorrect = _view.GetAmountInputStatus();

			int.TryParse(amountInput, out int amount);

			if (!isAmountCorrect)
			{
				if (amount <= 0)
				{
					Message.Show(MessageCode.AMOUNT_MUST_BE_GREATER_GREATER_ZERO);
				}
				else
				{
					Message.Show(MessageCode.NOT_ENOUGH_FUNDS);
				}

				return;
			}

			if (_view.GetTransferDirection() == TransactionDirection.UsersCryptoWallet)
			{
				if (UserInventory.GetVirtualCurrencyAmount(VirtualCurrencyID.WK.ToString()) < 1)
				{
					Message.Show(MessageCode.TO_WITHDRAW_FROM_GAME_YOU_NEED_WK);
					ShopSystem.PopUpSystem.ShowWKPopUp();

					return;
				}
			}

			Sprite itemIcon = null;
			string itemDescription = null;

			if (_view.TransactionType == CryptoTransactionType.Token)
			{
				var tokenInput = _view.GetTokenInput();
				var tokenName = tokenInput == VirtualCurrencyID.IG ? JsonProperty.IGT.ToUpper() : JsonProperty.IGC.ToUpper();
                var tokenTicker = tokenInput == VirtualCurrencyID.IG ? BlockchainSettings.HardTokenTicker.ToUpper() : BlockchainSettings.SoftTokenTicker.ToUpper();

                string imagePath = $"Sprites/Currency/{tokenName}";
                string iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;

                itemIcon = await ImageLoader.GetSpriteAsync(iconPath);

                itemDescription = $"<color=white>{tokenTicker}</color> {_view.TransactionType}";
			}
			else if (_view.TransactionType == CryptoTransactionType.NFT)
			{
				var skin = _view.GetSkinInput();

				if (skin == null)
				{
					Message.Show(MessageCode.YOU_MUST_SELECT_SKIN);
					return;
				}

				itemIcon = await ImageLoader.GetSpriteAsync(skin.ImagePath);
                itemDescription = $"<color=white>{skin.DisplayName}</color> {_view.TransactionType}";
			}

			_confirmationPopUp.Set(OnClickConfirm, _view.GetTransferDirection(), itemIcon, amount, itemDescription);
			_confirmationPopUp.gameObject.SetActive(true);
		}

		private void OnClickConfirm()
		{
			if (_view.TransactionType == CryptoTransactionType.Token)
			{
				TryTransferToken();
			}
			else
			{
				TryTransferNFT();
			}

			gameObject.SetActive(false);
		}
#endif

    }
}
