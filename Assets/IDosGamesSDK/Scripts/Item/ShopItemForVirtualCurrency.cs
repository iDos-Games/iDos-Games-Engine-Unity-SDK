using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ShopItemForVirtualCurrency : ShopItem
	{
		[SerializeField] private Image _currencyImage;

		public override void Fill(Action action, string title, string price, Sprite icon, Sprite currencyIcon)
		{
			base.Fill(action, title, price, icon);
			_currencyImage.sprite = currencyIcon;
		}
	}
}