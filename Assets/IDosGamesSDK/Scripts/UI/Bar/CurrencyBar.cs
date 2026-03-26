using TMPro;
using UnityEngine;

namespace IDosGames
{
	public abstract class CurrencyBar : Bar
	{
		[SerializeField] private TMP_Text _amount;

		public long Amount
		{
			get => GetAmount();
			protected set => SetAmount(value);
		}

		private void SetAmount(long amount)
		{
			_amount.text = amount.ToString("N0");
		}

		private long GetAmount()
		{
            long.TryParse(_amount.text, out long amount);
			return amount;
		}

		public abstract void UpdateAmount();
	}
}