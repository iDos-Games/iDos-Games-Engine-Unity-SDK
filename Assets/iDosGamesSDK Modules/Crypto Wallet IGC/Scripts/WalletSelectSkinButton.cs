using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0414
#pragma warning disable 0067
namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class WalletSelectSkinButton : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private SkinInventoryItem _skinInfo;
		[SerializeField] private WalletSkinInventoryPopUp _inventoryPopUp;
		[SerializeField] private WalletTransferDirection _transferDirection;
		[SerializeField] private PanelCryptoWalletTokenBalance _cryptoBalancePanel;

		public event Action ValueChanged;

		public SkinCatalogItem SelectedSkin { get; private set; }

#if IDOSGAMES_CRYPTO_WALLET
		private void Awake()
		{
			ResetListener();
		}

		private void OnEnable()
		{
			_transferDirection.ValueChanged += ResetButton;

			ResetButton();
		}

		private void OnDisable()
		{
			_transferDirection.ValueChanged -= ResetButton;
		}

		private void ResetButton()
		{
			_skinInfo.gameObject.SetActive(false);
			SelectedSkin = null;
			ValueChanged?.Invoke();
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(OpenInventory);
		}

		private void OpenInventory()
		{
			var isFromCryptoWallet = _transferDirection.Direction == TransactionDirection.Game || _transferDirection.Direction == TransactionDirection.ExternalWalletAddress;

			List<SkinCatalogItem> skins = new();

			foreach (var skin in UserDataService.AllSkinsInCatalog)
			{
				if (skin.NFTID == 0)
				{
					continue;
				}

				if (isFromCryptoWallet)
				{
					bool alreadyContainsItem = skins.Any(x => x.NFTID == skin.NFTID);

					if (alreadyContainsItem)
					{
						continue;
					}
				}

				var amount = isFromCryptoWallet ? _cryptoBalancePanel.GetNFTAmount(skin.NFTID) : UserInventory.GetItemAmount(skin.ItemID);

				if (amount > 0)
				{
					skins.Add(skin);
				}
			}

			_inventoryPopUp.Show(skins, OnSelect, isFromCryptoWallet);
		}

		private void OnSelect(SkinCatalogItem skin, bool isFromCryptoWallet)
		{
			SelectedSkin = skin;

			_skinInfo.Fill(null, skin);

			if (isFromCryptoWallet)
			{
				_skinInfo.UpdateAmount(_cryptoBalancePanel.GetNFTAmount(skin.NFTID));
			}

			_skinInfo.gameObject.SetActive(true);

			ValueChanged?.Invoke();
		}
#endif

    }
}
