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

        [SerializeField] private TMP_InputField _resetTokenInputField;
        [SerializeField] private TMP_InputField _newPasswordInputField;
        [SerializeField] private GameObject _resetPasswordPopup;

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

		public void SendResetPassword()
		{
			bool isValid = CheckPasswordInput();

			if (isValid)
			{
                AuthService.Instance.SendResetPassword(GetResetToken(), GetNewPassword(), OnResetPasswordSuccess, AuthService.ShowErrorMessage);
            }
			else
			{
                Message.Show(MessageCode.PASSWORD_VERY_SHORT);
            }
        }

        private bool CheckEmailInput()
		{
			var email = GetEmailInput();
			return AuthService.CheckEmailAddress(email);
		}

        private bool CheckPasswordInput()
        {
            var password = GetNewPassword();
            return AuthService.CheckPasswordLenght(password);
        }

        private void OnSendSuccess(string result)
		{
			if (result == "true")
			{
                Message.Show(MessageCode.PASSWORD_RECOVERY_SENT);
                _resetPasswordPopup.SetActive(true);
            }
			else
			{
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        private void OnResetPasswordSuccess(string result)
        {
			if (result == "true")
			{
                _resetPasswordPopup.SetActive(false);
                _authorizationPopUpView.CloseRecoveryPopUp();
                Message.Show(MessageCode.PASSWORD_UPDATED);
            }
            else
			{
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        public void SetInputFieldText(string email)
		{
			_emailInputField.text = email;
		}

		public string GetEmailInput()
		{
			return _emailInputField.text;
		}

        public string GetResetToken()
        {
            return _resetTokenInputField.text;
        }

        public string GetNewPassword()
        {
            return _newPasswordInputField.text;
        }
    }
}