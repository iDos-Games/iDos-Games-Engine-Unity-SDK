using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	public class PopUpConfirmation : PopUp
	{
		[SerializeField] private Button _confirmButton;

		[SerializeField] private TMP_Text _productName;
		[SerializeField] private TMP_Text _price;
		[SerializeField] private Image _currencyIcon;

		public override void Set(Action confirmAction)
		{
			_confirmButton.onClick.RemoveAllListeners();
			_confirmButton.onClick.AddListener(() => gameObject.SetActive(false));
			_confirmButton.onClick.AddListener(new UnityAction(confirmAction));
		}

		public void FullSet(Action confirmAction, string productName, string price, Sprite currencyIcon)
		{
			Set(confirmAction);

			_productName.text = productName;
			_price.text = price;
			_currencyIcon.sprite = currencyIcon;
		}
	}
}