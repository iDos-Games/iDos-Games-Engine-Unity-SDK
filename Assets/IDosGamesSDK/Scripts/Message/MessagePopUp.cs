using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class MessagePopUp : PopUp
	{
		[SerializeField] private TMP_Text _message;

		public override void Set(string message)
		{
			SetMessage(message);
		}

		private void SetMessage(string message)
		{
			_message.text = message;
		}
	}
}