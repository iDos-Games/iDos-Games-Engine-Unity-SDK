using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class RewardItem : Item
	{
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _amount;

		public int Amount
		{
			get => GetAmount();
			protected set => SetAmount(value);
		}

		public void Set(string imagePath, int amount)
		{
			SetIcon(imagePath);
			SetAmount(amount);
		}

		private void SetIcon(string imagePath)
		{
			_icon.sprite = Resources.Load<Sprite>(imagePath);
		}

		private void SetAmount(int amount)
		{
			amount = amount < 0 ? 0 : amount;
			_amount.text = "x" + amount.ToString();
		}

		private int GetAmount()
		{
			int.TryParse(_amount.text, out int amount);
			return amount;
		}
	}
}