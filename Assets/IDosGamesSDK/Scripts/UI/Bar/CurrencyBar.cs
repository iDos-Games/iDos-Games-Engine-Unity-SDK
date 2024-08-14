using TMPro;
using UnityEngine;

namespace IDosGames
{
	public abstract class CurrencyBar : Bar
	{
		[SerializeField] private TMP_Text _amount;

		public int Amount
		{
			get => GetAmount();
			protected set => SetAmount(value);
		}

		private void SetAmount(int amount)
		{
			_amount.text = amount.ToString("N0");
		}

		private int GetAmount()
		{
			int.TryParse(_amount.text, out int amount);
			return amount;
		}

		public abstract void UpdateAmount();
	}
}