using UnityEngine;

namespace IDosGames
{
	public class SpinTicketBar : CurrencyBar
	{
		[SerializeField] private SpinTicketType _spinTicketType;

		private void OnEnable()
		{
			UpdateAmount();
			IDosGamesData.User.OnInventoryUpdated += UpdateAmount;
		}

		private void OnDisable()
		{
			IDosGamesData.User.OnInventoryUpdated -= UpdateAmount;
		}

		public override void UpdateAmount()
		{
			Amount = DataService.GetSpinTicketAmount(_spinTicketType);
		}
	}
}