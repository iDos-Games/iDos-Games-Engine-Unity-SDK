using UnityEngine;

namespace IDosGames
{
	public class SettingsPopUpView : MonoBehaviour
	{
		public void CloseMainPopUp()
		{
			gameObject.SetActive(false);
		}
	}
}