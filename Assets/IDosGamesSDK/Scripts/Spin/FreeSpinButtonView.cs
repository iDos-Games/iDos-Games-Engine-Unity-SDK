using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class FreeSpinButtonView : MonoBehaviour
	{
		[SerializeField] private Timer _timer;
		[SerializeField] private ShopOfferQuantityInfo _quantityInfo;
		[SerializeField] private Image _adIcon;

		public void Block(DateTime endDate)
		{
			SetTimer(endDate);
			SetActiveAddIcon(false);
			SetActiveQuantityInfo(false);
		}

		public void UnBlock()
		{
			_timer.gameObject.SetActive(false);
			SetActiveQuantityInfo(true);
		}

		public void SetActiveAddIcon(bool active)
		{
			_adIcon.gameObject.SetActive(active);
		}

		private void SetTimer(DateTime endDate)
		{
			_timer.gameObject.SetActive(true);
			_timer.Set(endDate);
		}

		public void SetQuantity(string quantity)
		{
			_quantityInfo.Set(quantity);
		}

		private void SetActiveQuantityInfo(bool active)
		{
			_quantityInfo.gameObject.SetActive(active);
		}
	}
}