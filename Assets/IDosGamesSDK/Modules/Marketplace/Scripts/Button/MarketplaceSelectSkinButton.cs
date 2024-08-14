using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class MarketplaceSelectSkinButton : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private SkinInventoryItem _skinInfo;
		[SerializeField] private SkinInventoryPopUp _inventoryPopUp;

		public event Action ValueChanged;

		public SkinCatalogItem SelectedSkin { get; private set; }

		private void Awake()
		{
			ResetListener();
		}

		private void OnEnable()
		{
			ResetButton();
		}

		public void ResetButton()
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
			List<SkinCatalogItem> skins = new();

			foreach (var skin in UserDataService.AllSkinsInCatalog)
			{
				var amount = UserInventory.GetItemAmount(skin.ItemID);

				if (amount > 0)
				{
					skins.Add(skin);
				}
			}

			_inventoryPopUp.Show(skins, OnSelect);
		}

		private void OnSelect(SkinCatalogItem skin)
		{
			SelectedSkin = skin;

			_skinInfo.Fill(null, skin);
			_skinInfo.gameObject.SetActive(true);

			ValueChanged?.Invoke();
		}
	}
}
