using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ShopDailyOfferView : MonoBehaviour
	{
		[SerializeField] private ShopOfferQuantityInfo _quantityInfo;
		[SerializeField] private CanvasGroup _canvasGroup;
		[SerializeField] private GameObject _block;
		[SerializeField] private TMP_Text _amountToGrant;
		[SerializeField] private Image _adIcon;
		[SerializeField] private GameObject _alarm;

		public void SetQuantity(string quantity)
		{
			_quantityInfo.Set(quantity);
		}

		public void Block()
		{
			_block.SetActive(true);
			_canvasGroup.interactable = false;
			SetActiveAddIcon(false);
			SetActiveQuantityInfo(false);
			SetActiveAlarm(false);
		}

		public void UnBlock()
		{
			_block.SetActive(false);
			_canvasGroup.interactable = true;
			SetActiveQuantityInfo(true);
			SetActiveAlarm(true);
		}

		public void SetAmountToGrant(string amount)
		{
			_amountToGrant.gameObject.SetActive(true);
			_amountToGrant.text = $"x{amount}";
		}

		public void DisableTextAmountToGrant()
		{
			_amountToGrant.gameObject.SetActive(false);
		}

		public void SetActiveAddIcon(bool active)
		{
			_adIcon.gameObject.SetActive(active);
		}

		public void SetActiveQuantityInfo(bool active)
		{
			_quantityInfo.gameObject.SetActive(active);
		}

		public void SetActiveAlarm(bool active)
		{
			_alarm.SetActive(active);
		}
	}
}