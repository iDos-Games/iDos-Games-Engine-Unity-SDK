using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{

	public class ShopSpecialOfferItem : ShopItem
	{
		[SerializeField] private Image _currencyImage;

		[SerializeField] private ShopSpecialOfferLimitView _limitView;

		public override void Fill(Action action, string title, string price, Sprite icon, Sprite currencyIcon)
		{
			base.Fill(action, title, price, icon);
			_currencyImage.sprite = currencyIcon;
		}

		public void SetLimitView(string quantityLeftText, DateTime endDate, SpecialOfferType type)
		{
			_limitView.Set(quantityLeftText, endDate, type);
		}
	}
}