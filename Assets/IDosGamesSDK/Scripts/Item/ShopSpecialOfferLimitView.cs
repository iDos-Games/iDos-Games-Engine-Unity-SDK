using System;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class ShopSpecialOfferLimitView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _textLimited;
		[SerializeField] private ShopOfferQuantityInfo _quantityLimitInfo;
		[SerializeField] private ShopSpecialOfferTimeLimitInfo _timeLimitInfo;

		public void Set(string quantityLeft, DateTime endDate, SpecialOfferType type)
		{
			HideAllObjects();

			switch (type)
			{
				case SpecialOfferType.TimeLimited:
					SetActivateTimeLimit(endDate);
					break;
				case SpecialOfferType.QuantityLimitedForPlayer:
					SetActivateQuantityLimit(quantityLeft);
					break;
				case SpecialOfferType.TimeQuantityLimitedForPlayer:
					SetActivateTimeLimit(endDate);
					SetActivateQuantityLimit(quantityLeft);
					break;
			}
		}

		public void HideAllObjects()
		{
			_textLimited.gameObject.SetActive(false);
			_quantityLimitInfo.gameObject.SetActive(false);
			_timeLimitInfo.gameObject.SetActive(false);
		}

		private void SetActivateTimeLimit(DateTime endDate)
		{
			_timeLimitInfo.gameObject.SetActive(true);
			_textLimited.gameObject.SetActive(true);
			_timeLimitInfo.Set(endDate);
		}

		private void SetActivateQuantityLimit(string quantityLeft)
		{
			_quantityLimitInfo.Set(quantityLeft);
			_quantityLimitInfo.gameObject.SetActive(true);
			_textLimited.gameObject.SetActive(true);
		}
	}
}