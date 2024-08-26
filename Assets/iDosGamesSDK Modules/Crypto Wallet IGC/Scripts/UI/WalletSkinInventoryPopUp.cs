using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class WalletSkinInventoryPopUp : PopUp
	{
		[SerializeField] private SkinInventoryItem _inventoryItemPrefab;
		[SerializeField] private Transform _itemsParent;
		[SerializeField] private TMP_Text _voidInGameText;
		[SerializeField] private TMP_Text _voidCryptoWalletText;

#if IDOSGAMES_CRYPTO_WALLET
		[SerializeField] private PanelCryptoWalletTokenBalance _cryptoBalancePanel;

		public void Show(List<SkinCatalogItem> skins, Action<SkinCatalogItem, bool> itemOnClickAction, bool fromCryptoWallet = false)
		{
			foreach (Transform child in _itemsParent)
			{
				Destroy(child.gameObject);
			}

			_voidInGameText.gameObject.SetActive(skins.Count < 1 && !fromCryptoWallet);
			_voidCryptoWalletText.gameObject.SetActive(skins.Count < 1 && fromCryptoWallet);

			if (skins.Count < 1)
			{
				gameObject.SetActive(true);
				return;
			}

			skins = skins.OrderBy(o => o.Rarity).ToList();

			foreach (var skin in skins)
			{
				int amount = fromCryptoWallet ? _cryptoBalancePanel.GetNFTAmount(skin.NFTID) : UserInventory.GetItemAmount(skin.ItemID);

				if (amount == 0)
				{
					continue;
				}

				var skinItem = Instantiate(_inventoryItemPrefab, _itemsParent);

				Action clickAction = () =>
				{
					itemOnClickAction?.Invoke(skin, fromCryptoWallet);
					gameObject.SetActive(false);
				};

				skinItem.Fill(clickAction, skin);

				if (fromCryptoWallet)
				{
					skinItem.UpdateAmount(amount);
				}
			}

			gameObject.SetActive(true);
		}
#endif

    }
}
