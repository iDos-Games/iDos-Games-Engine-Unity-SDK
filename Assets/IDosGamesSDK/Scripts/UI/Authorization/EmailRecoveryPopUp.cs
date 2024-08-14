using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class EmailRecoveryPopUp : PopUp
	{
		[SerializeField] private AuthorizationPopUpView _authorizationPopUpView;
		[SerializeField] private TMP_InputField _emailInputField;
		[SerializeField] private Button _sendButton;

		private void Start()
		{
			ResetSendButton();
		}

		private void ResetSendButton()
		{
			_sendButton.onClick.RemoveAllListeners();
			_sendButton.onClick.AddListener(TrySend);
		}

		private void TrySend()
		{
			bool isEmailInputCorrect = CheckEmailInput();

			if (isEmailInputCorrect)
			{
				Send();
			}
			else
			{
				ShowErrorMessage();
			}
		}

		private void ShowErrorMessage()
		{
			Message.Show(MessageCode.INCORRECT_EMAIL);
		}

		private void Send()
		{
			AuthService.Instance.SendAccountRecoveryEmail(GetEmailInput(), OnSendSuccess, AuthService.ShowErrorMessage);
		}

		private bool CheckEmailInput()
		{
			var email = GetEmailInput();
			return AuthService.CheckEmailAddress(email);
		}

		private void OnSendSuccess(string result)
		{
			_authorizationPopUpView.CloseRecoveryPopUp();
			Message.Show(MessageCode.PASSWORD_RECOVERY_SENT);
		}

		public void SetInputFieldText(string email)
		{
			_emailInputField.text = email;
		}

		public string GetEmailInput()
		{
			return _emailInputField.text;
		}
	}
}