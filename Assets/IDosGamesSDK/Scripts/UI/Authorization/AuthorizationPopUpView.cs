using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class AuthorizationPopUpView : MonoBehaviour
	{
		[SerializeField] private AuthorizationPopUp _authorizationPopUp;
		[SerializeField] private EmailRecoveryPopUp _emailRecoveryPopUp;

		[SerializeField] private GameObject[] _logInElements;
		[SerializeField] private GameObject[] _signUpElements;
		[SerializeField] private Button[] _switchButtons;

		private int _currentView = (int)ViewType.SignUp;

		private enum ViewType
		{
			SignUp = 0,
			LogIn = 1
		}

		private void OnEnable()
		{
			AuthService.LoggedIn += CloseMainPopUp;
		}

		private void OnDisable()
		{
			AuthService.LoggedIn -= CloseMainPopUp;
		}

		private void Start()
		{
			ResetSwitchButtons();
			SetActivateCurrentView();

			if (PlayerPrefs.GetInt(AlarmType.OpenedAuthorizationPopUp.ToString(), 0) == 0)
			{
				if (AlarmSystem.Instance != null)
				{
					PlayerPrefs.SetInt(AlarmType.OpenedAuthorizationPopUp.ToString(), 1);
					PlayerPrefs.Save();
					AlarmSystem.Instance.SetAlarmState(AlarmType.OpenedAuthorizationPopUp, false);
				}
			}
		}

		private void ResetSwitchButtons()
		{
			foreach (var button in _switchButtons)
			{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(SwitchView);
			}
		}

		private void SwitchView()
		{
			_currentView = (_currentView == (int)ViewType.SignUp) ? (int)ViewType.LogIn : (int)ViewType.SignUp;
			SetActivateCurrentView();
		}

		private void SetActivateCurrentView()
		{
			foreach (var gameObject in _logInElements)
			{
				gameObject.SetActive(_currentView == (int)ViewType.LogIn);
			}

			foreach (var gameObject in _signUpElements)
			{
				gameObject.SetActive(_currentView == (int)ViewType.SignUp);
			}
		}

		public void CloseMainPopUp()
		{
			gameObject.SetActive(false);
		}

		public void ShowRecoveryPopUp()
		{
			_authorizationPopUp.gameObject.SetActive(false);
			_emailRecoveryPopUp.gameObject.SetActive(true);
			_emailRecoveryPopUp.SetInputFieldText(_authorizationPopUp.GetEmailInput());
		}

		public void CloseRecoveryPopUp()
		{
			_authorizationPopUp.gameObject.SetActive(true);
			_emailRecoveryPopUp.gameObject.SetActive(false);
		}
	}
}