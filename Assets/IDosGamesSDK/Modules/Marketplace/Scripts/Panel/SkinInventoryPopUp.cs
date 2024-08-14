using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class SkinInventoryPopUp : MonoBehaviour
	{
		[SerializeField] private SkinInventoryItem _inventoryItemPrefab;
		[SerializeField] private Transform _itemsParent;
		[SerializeField] private TMP_Text _voidText;

		public void Show(List<SkinCatalogItem> skins, Action<SkinCatalogItem> itemOnClickAction)
		{
			foreach (Transform child in _itemsParent)
			{
				Destroy(child.gameObject);
			}

			_voidText.gameObject.SetActive(skins.Count < 1);

			if (skins.Count < 1)
			{
				gameObject.SetActive(true);
				return;
			}

			skins = skins.OrderByDescending(o => o.Rarity).ToList();

			foreach (var skin in skins)
			{
				int amount = UserInventory.GetItemAmount(skin.ItemID);

				if (amount == 0)
				{
					continue;
				}

				var skinItem = Instantiate(_inventoryItemPrefab, _itemsParent);

				Action clickAction = () =>
				{
					itemOnClickAction?.Invoke(skin);
					gameObject.SetActive(false);
				};

				skinItem.Fill(clickAction, skin);
			}

			gameObject.SetActive(true);
		}
	}
}
