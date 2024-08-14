using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class VIPButton : MonoBehaviour
	{
		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(ShowPopUp);
		}

		public void ShowPopUp()
		{
			ShopSystem.PopUpSystem.ShowVIPPopUp();
		}
	}
}