using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public abstract class ShopItem : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private TMP_Text _title;
		[SerializeField] private TMP_Text _price;
		[SerializeField] private Image _image;

		public virtual void Fill(Action action, string title, string price, Sprite icon)
		{
			ResetButton(action);
			_title.text = title;
			_price.text = price;
			_image.sprite = icon;
		}

		public virtual void Fill(Action action, string title, string price, Sprite icon, Sprite currencyIcon)
		{
			Fill(action, title, price, icon);
		}

		private void ResetButton(Action action)
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(new UnityAction(action));
		}
	}
}