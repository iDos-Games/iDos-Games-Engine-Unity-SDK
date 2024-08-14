using UnityEngine;

namespace IDosGames
{
	public class SpinTicketBar : CurrencyBar
	{
		[SerializeField] private SpinTicketType _spinTicketType;

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
			Amount = UserInventory.GetSpinTicketAmount(_spinTicketType);
		}
	}
}