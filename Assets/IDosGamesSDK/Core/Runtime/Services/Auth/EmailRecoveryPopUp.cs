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
            if (CheckEmailInput())
                Send();
            else
                ShowErrorMessage();
        }

        private void ShowErrorMessage()
        {
            Message.Show(MessageCode.INCORRECT_EMAIL);
        }

        public void ShowResetPasswordPopup()
        {
            if (CheckEmailInput())
                _resetPasswordPopup.SetActive(true);
            else
                ShowErrorMessage();
        }

        private async void Send()
        {
            var result = await AuthenticationService.ForgotPassword(GetEmailInput());
            if (result.Success)
                OnSendSuccess();
        }

        public async void SendResetPassword()
        {
            if (!CheckPasswordInput())
            {
                Message.Show(MessageCode.PASSWORD_VERY_SHORT);
                return;
            }

            var result = await AuthenticationService.ResetPassword(GetEmailInput(), GetResetToken(), GetNewPassword());
            if (result.Success)
                OnResetPasswordSuccess();
        }

        private bool CheckEmailInput()
        {
            return AuthenticationService.IsValidEmail(GetEmailInput());
        }

        private bool CheckPasswordInput()
        {
            return AuthenticationService.IsValidPasswordLength(GetNewPassword());
        }

        private void OnSendSuccess()
        {
            Message.Show(MessageCode.PASSWORD_RECOVERY_SENT);
            _resetPasswordPopup.SetActive(true);
        }

        private void OnResetPasswordSuccess()
        {
            _resetPasswordPopup.SetActive(false);
            _authorizationPopUpView.CloseRecoveryPopUp();
            Message.Show(MessageCode.PASSWORD_UPDATED);
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