using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class MarketplaceCommissionPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text _minPrice;
		[SerializeField] private TMP_Text _commission;
		[SerializeField] private TMP_Text _youWillGet;

#if IDOSGAMES_MARKETPLACE
		private void Start()
		{
			UpdateMinPriceText();
			UpdateCommisionText();
		}

		public void UpdatePlayerGetText(int value)
		{
			_youWillGet.text = ((int)((float)value * (100 - MarketplaceWindow.SumOfAllCommissions) / 100)).ToString();
		}

		private void UpdateMinPriceText()
		{
			_minPrice.text = MarketplaceWindow.MIN_OFFER_PRICE.ToString();
		}

		private void UpdateCommisionText()
		{
			_commission.text = $"{MarketplaceWindow.SumOfAllCommissions}%";
		}
#endif

    }
}
