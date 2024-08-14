using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class SkinItemMenuPopUp : MonoBehaviour
	{
		[SerializeField] private SkinItemMenu _skinItemMenu;
		[SerializeField] private InventoryWindow _inventoryWindow;
		[SerializeField] private Button _inspectButton;
		[SerializeField] private Button _equipButton;
		[SerializeField] private Button _unequipButton;

		public void SetButtons(SkinCatalogItem item)
		{
			bool isEqipped = _inventoryWindow.IsSkinEquippedInTemp(item.ItemID);

			SetEquipButton(item.ItemID, isEqipped);
			SetUnequipButton(item.ItemID, isEqipped);
			SetInspectButton(item.ItemID);
		}

		public void SetPosition(Transform target)
		{
			transform.position = target.position;
		}

		private void SetInspectButton(string itemID)
		{
			_inspectButton.onClick.RemoveAllListeners();
			_inspectButton.onClick.AddListener(() => _inventoryWindow.InspectSkin(itemID));
			_inspectButton.onClick.AddListener(() => _skinItemMenu.gameObject.SetActive(false));
		}

		private void SetEquipButton(string itemID, bool isEqipped)
		{
			int amountInInventory = UserInventory.GetItemAmount(itemID);

			if (!isEqipped && amountInInventory > 0)
			{
				_equipButton.onClick.RemoveAllListeners();
				_equipButton.onClick.AddListener(() => _inventoryWindow.EquipSkin(itemID));
				_equipButton.onClick.AddListener(() => _skinItemMenu.gameObject.SetActive(false));

				_equipButton.gameObject.SetActive(true);
			}
			else
			{
				_equipButton.gameObject.SetActive(false);
			}
		}

		private void SetUnequipButton(string itemID, bool isEqipped)
		{
			_unequipButton.gameObject.SetActive(isEqipped);

			if (isEqipped)
			{
				_unequipButton.onClick.RemoveAllListeners();
				_unequipButton.onClick.AddListener(() => _inventoryWindow.UnequipSkin(itemID));
				_unequipButton.onClick.AddListener(() => _skinItemMenu.gameObject.SetActive(false));
			}
		}
	}
}
