using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class DotPageObject : MonoBehaviour
	{
		[SerializeField] private Image _selectedIcon;

		public void Select()
		{
			SetActivateSelection(true);
		}

		public void Deselect()
		{
			SetActivateSelection(false);
		}
		private void SetActivateSelection(bool active)
		{
			_selectedIcon.gameObject.SetActive(active);
		}
	}
}