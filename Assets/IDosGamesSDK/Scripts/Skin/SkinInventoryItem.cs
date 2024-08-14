using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class SkinInventoryItem : Item
	{
		[SerializeField] private Button _button;
		[SerializeField] private Image _rarityBackground;
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _amount;

		[SerializeField] private GameObject _amountObject;

		public virtual void Fill(Action action, SkinCatalogItem item)
		{
			ResetButton(action);
			_icon.sprite = Resources.Load<Sprite>(item.ImagePath);
			_rarityBackground.color = Rarity.GetColor(item.Rarity);

			var amount = UserInventory.GetItemAmount(item.ItemID);

			UpdateAmount(amount);
		}

		public void UpdateAmount(int amount)
		{
			_amount.text = amount.ToString();

			if (_amountObject != null)
			{
				_amountObject.SetActive(amount > 0);
			}
		}

		private void ResetButton(Action action)
		{
			if (action == null)
			{
				return;
			}

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(new UnityAction(action));
		}
	}
}