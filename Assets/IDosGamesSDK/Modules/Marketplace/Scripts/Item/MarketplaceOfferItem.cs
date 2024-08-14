using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	public class MarketplaceOfferItem : MonoBehaviour
	{
		[SerializeField] private Button _buyButton;
		[SerializeField] private TMP_Text _sellerID;
		[SerializeField] private TMP_Text _price;
		[SerializeField] private Image _currencyIcon;

		public virtual void Fill(Action action, string sellerID, int price, Sprite currencyIcon)
		{
			_price.text = price.ToString();
			_sellerID.text = sellerID?.ToString();
			_currencyIcon.sprite = currencyIcon;

			if (sellerID == AuthService.UserID)
			{
				_buyButton.interactable = false;
			}
			else
			{
				ResetButton(action);
			}
		}

		private void ResetButton(Action action)
		{
			if (action == null)
			{
				return;
			}

			_buyButton.onClick.RemoveAllListeners();
			_buyButton.onClick.AddListener(new UnityAction(action));
			_buyButton.interactable = true;
		}
	}
}
