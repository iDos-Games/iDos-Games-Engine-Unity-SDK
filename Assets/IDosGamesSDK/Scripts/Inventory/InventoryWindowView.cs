using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class InventoryWindowView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _currentProfit;

		[SerializeField] private TabItem _myItemsTab;
		[SerializeField] private TabItem _catalogTab;

		[SerializeField] private TMP_Text _voidInventoryText;
		[SerializeField] private TMP_Text _voidCatalogText;

		[SerializeField] private ScrollRect _scrollRect;

		[SerializeField] private SkinItemMenu _skinMenu;

		public void UpdateCurrentProfitAmountText(int amount)
		{
			_currentProfit.text = amount.ToString();
		}

		public void ShowSkinMenu(Transform transform, SkinCatalogItem item)
		{
			_skinMenu.ShowPopUp(transform, item);
		}

		public void ResetScrollParentVerticalPosition()
		{
			_scrollRect.verticalNormalizedPosition = 1f;
		}

		public void HideAllVoidTexts()
		{
			SetActivateInventoryVoidText(false);
			SetActivateCatalogVoidText(false);
		}

		public void SelectMyItemsTab()
		{
			_myItemsTab.Select();
			_catalogTab.Deselect();
		}

		public void SelectCatalogTab()
		{
			_myItemsTab.Deselect();
			_catalogTab.Select();
		}

		public void SetActivateInventoryVoidText(bool active)
		{
			_voidCatalogText.gameObject.SetActive(false);
			_voidInventoryText.gameObject.SetActive(active);
		}

		public void SetActivateCatalogVoidText(bool active)
		{
			_voidInventoryText.gameObject.SetActive(false);
			_voidCatalogText.gameObject.SetActive(active);
		}
	}
}
