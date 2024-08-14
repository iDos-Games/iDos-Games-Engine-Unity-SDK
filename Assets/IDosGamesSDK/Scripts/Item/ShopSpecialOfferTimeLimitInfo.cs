using System;
using UnityEngine;

namespace IDosGames
{
	public class ShopSpecialOfferTimeLimitInfo : MonoBehaviour
	{
		[SerializeField] private SpecialOfferTimer _timer;

		private DateTime _endDate;

		public void Set(DateTime dateTime)
		{
			_endDate = dateTime;
			UpdateTimer();
		}

		private void UpdateTimer()
		{
			_timer.Set(_endDate);
		}
	}
}
