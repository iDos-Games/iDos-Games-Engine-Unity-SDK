using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ButtonWithOptionalIcon : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private Image _icon;

		public Button Button => _button;

		public void SetActivateIcon(bool active)
		{
			_icon.gameObject.SetActive(active);
		}
	}
}