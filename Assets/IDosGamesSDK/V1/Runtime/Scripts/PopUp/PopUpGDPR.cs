using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	public class PopUpGDPR : MonoBehaviour
	{
		[SerializeField] private Button _buttonAccept;
		[SerializeField] private Button _buttonDeny;

		public void ResetButtons(UnityAction acceptAction, UnityAction denyAction)
		{
			_buttonAccept.onClick.RemoveAllListeners();
			_buttonAccept.onClick.AddListener(acceptAction);
			_buttonAccept.onClick.AddListener(DeactivatePopUp);

			_buttonDeny.onClick.RemoveAllListeners();
			_buttonDeny.onClick.AddListener(denyAction);
			_buttonDeny.onClick.AddListener(DeactivatePopUp);
		}

		private void DeactivatePopUp()
		{
			gameObject.SetActive(false);
		}
	}
}