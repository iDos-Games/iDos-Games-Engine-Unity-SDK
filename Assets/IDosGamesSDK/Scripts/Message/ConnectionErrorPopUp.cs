using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ConnectionErrorPopUp : PopUp
	{
		private const string BUTTON_WITHOUT_CALLBACK_TEXT = "OK";
		private const string BUTTON_WITH_CALLBACK_TEXT = "Retry";

		[SerializeField] private Button _button;
		[SerializeField] private TMP_Text _buttonText;

		public override void Set(Action callbackAction = null)
		{
			ResetButtonListener();

			if (callbackAction != null)
			{
				_button.onClick.AddListener(() => callbackAction?.Invoke());
				SetButtonText(BUTTON_WITH_CALLBACK_TEXT);
			}
			else
			{
				SetButtonText(BUTTON_WITHOUT_CALLBACK_TEXT);
			}
		}

		private void ResetButtonListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(Hide);
		}

		private void SetButtonText(string text)
		{
			_buttonText.text = text;
		}

		private void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}