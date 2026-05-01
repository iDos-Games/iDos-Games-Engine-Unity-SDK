using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class RewardPopUp : PopUp
	{
		[SerializeField] private TMP_Text _message;
		[SerializeField] private Image _icon;

		public override void Set(string message, string imagePath)
		{
			SetMessage(message);
			SetIcon(imagePath);
		}

		private void SetIcon(string imagePath)
		{
			_icon.sprite = Resources.Load<Sprite>(imagePath);
		}

		private void SetMessage(string message)
		{
			_message.text = message;
		}
	}
}