using UnityEngine;

namespace IDosGames
{
	public class VirtualCurrencyBar : CurrencyBar
	{
		[SerializeField] private string _virtualCurrencyID;

		private void OnEnable()
		{
			UpdateAmount();
			UserInventory.InventoryUpdated += UpdateAmount;
		}

		private void OnDisable()
		{
			UserInventory.InventoryUpdated -= UpdateAmount;
		}

		public override void UpdateAmount()
		{
			Amount = UserInventory.GetVirtualCurrencyAmount(_virtualCurrencyID.Trim().ToUpper());
		}
	}
}