using UnityEngine;

namespace IDosGames
{
	public class ShopFreeItem : ShopItem
	{
		[SerializeField] private ShopDailyOfferView _view;

		public ShopDailyOfferView View => _view;
	}
}