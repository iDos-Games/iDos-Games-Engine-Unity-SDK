using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class ButtonSendAnalytic : MonoBehaviour
	{
		[SerializeField] private string _name;

		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
			_button.onClick.AddListener(Send);
		}

		public void Send()
		{
			Analytics.Send(_name);
		}
	}
}