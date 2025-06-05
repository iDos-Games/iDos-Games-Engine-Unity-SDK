using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class AuthorizationPopUp : PopUp
	{
		[SerializeField] private TMP_InputField _emailInputField;
		[SerializeField] private TMP_InputField _passwordInputField;
		[SerializeField] private TMP_InputField _passwordRepeatInputField;
		[SerializeField] private Button _logInButton;
		[SerializeField] private Button _signUpButton;

		private void Start()
		{
			ResetLogInButton();
			ResetSignUpButton();
            _emailInputField.text = AuthService.SavedEmail;
            _passwordInputField.text = AuthService.SavedPassword;
        }

		private void ResetLogInButton()
		{
			_logInButton.onClick.RemoveAllListeners();
			_logInButton.onClick.AddListener(TryLogIn);
		}

		private void ResetSignUpButton()
		{
			_signUpButton.onClick.RemoveAllListeners();
			_signUpButton.onClick.AddListener(TrySignUp);
		}

		private void TryLogIn()
		{
			var isEmailInputCorrect = CheckEmailInput();
			var isPasswordInputCorrect = CheckPasswordLength();

			if (isEmailInputCorrect && isPasswordInputCorrect)
			{
				LogIn();
			}
			else
			{
				ShowErrorMessage(isEmailInputCorrect, isPasswordInputCorrect);
			}
		}

		private void LogIn()
		{
			AuthService.Instance.LoginWithEmailAddress(GetEmailInput(), GetPasswordInput(), OnLogInSuccess, ShowError);
		}

		private void TrySignUp()
		{
			var isEmailInputCorrect = CheckEmailInput();
			var isPasswordInputCorrect = CheckPasswordLength();
			var isPasswordsMatch = CheckPasswordsMatch();

			if (isEmailInputCorrect && isPasswordInputCorrect && isPasswordsMatch)
			{
				//if (AuthService.AuthContext == null)
				//{
				//	Message.Show(MessageCode.MUST_BE_LOGGED_IN_TO_CALL_THIS_FUNCTION);
				//	return;
				//}

				SignUp();
			}
			else
			{
				ShowErrorMessage(isEmailInputCorrect, isPasswordInputCorrect, isPasswordsMatch);
			}
		}

		private void SignUp()
		{
			AuthService.Instance.RegisterUserByEmail(GetEmailInput(), GetPasswordInput(), OnSignUpSuccess, ShowError);
		}

		private void AddEmailAndPassword()
		{
            AuthService.Instance.AddUsernamePassword(GetEmailInput(), GetPasswordInput(), OnEmailAndPasswordAdded, ShowError);
        }

		private void ShowError(string error)
		{
            Message.Show(error);
		}

        private void ShowErrorMessage(bool isEmailInputCorrect, bool isPasswordInputCorrect, bool isPasswordsMatch = true)
		{
			string message = GenerateErrorMessage(isEmailInputCorrect, isPasswordInputCorrect, isPasswordsMatch);
			Message.Show(message);
		}

		private string GenerateErrorMessage(bool isEmailInputCorrect, bool isPasswordInputCorrect, bool isPasswordsMatch = true)
		{
			string message = string.Empty;

			if (isEmailInputCorrect == false)
			{
				message = "Email address is not correct.";
			}
			else if (isPasswordInputCorrect == false)
			{
				message = "Password length must be between 6 and 100 characters.";
			}
			else if (isPasswordsMatch == false)
			{
				message = "Password mismatch.";
			}

			return message;
		}

		private bool CheckPasswordLength()
		{
			var input = GetPasswordInput();

			return AuthService.CheckPasswordLenght(input);
		}

		private bool CheckPasswordsMatch()
		{
			var input1 = GetPasswordInput();
			var input2 = GetRepeatPasswordInput();

			bool isPasswordsMatch = input1 == input2;

			return isPasswordsMatch;
		}

		private bool CheckEmailInput()
		{
			var input = GetEmailInput();
			return AuthService.CheckEmailAddress(input);
		}

		private void OnLogInSuccess(GetAllUserDataResult result)
		{
			//Message.Show(MessageCode.SUCCESS_LOGGED_IN);
			UserDataService.ProcessingAllData(result);
            Loading.SwitchToNextScene();
        }

		private void OnSignUpSuccess(GetAllUserDataResult result)
		{
            UserDataService.ProcessingAllData(result);
            Loading.SwitchToNextScene();
        }

        private void OnEmailAndPasswordAdded(string result)
        {
            Message.Show(MessageCode.SUCCESS_LINK_ACCOUNT_TO_EMAIL);
        }

        public string GetEmailInput()
		{
			return _emailInputField.text;
		}

		private string GetPasswordInput()
		{
			return _passwordInputField.text;
		}

		private string GetRepeatPasswordInput()
		{
			return _passwordRepeatInputField.text;
		}
	}
}