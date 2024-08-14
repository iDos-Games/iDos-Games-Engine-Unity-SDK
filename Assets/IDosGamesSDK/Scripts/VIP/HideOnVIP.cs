using UnityEngine;

namespace IDosGames
{
	public class HideOnVIP : MonoBehaviour
	{
		private void OnEnable()
		{
			UpdateView();
			UserInventory.InventoryUpdated += UpdateView;
		}

		private void OnDisable()
		{
			UserInventory.InventoryUpdated -= UpdateView;
		}

		private void UpdateView()
		{
			if (UserInventory.HasVIPStatus)
			{
				gameObject.SetActive(false);
			}
		}
	}
}
