using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class AuthorizationView : PopUp
	{
		[SerializeField] private AuthorizationPopUpView _popUp;
		[SerializeField] private Button _AuthorizationBtn;
		[SerializeField] private Button _logOutBtn;
		[SerializeField] private Button _deleteAccountButton;
		[SerializeField] private GameObject _popUpDeleteAccount;

		private void OnEnable()
		{
			UpdateView();
			AuthService.LoggedIn += UpdateView;
		}

		private void OnDisable()
		{
			AuthService.LoggedIn -= UpdateView;
		}

		private void Start()
		{
			ResetButtons();
		}

		private void ResetButtons()
		{
			_AuthorizationBtn.onClick.RemoveAllListeners();
			_AuthorizationBtn.onClick.AddListener(ActivateAuthorizationPopUp);

			_logOutBtn.onClick.RemoveAllListeners();
			_logOutBtn.onClick.AddListener(LogOut);

			_deleteAccountButton.onClick.RemoveAllListeners();
			_deleteAccountButton.onClick.AddListener(() => SetActiveDeleteAccountPopUp(true));
		}

		private void ActivateAuthorizationPopUp()
		{
			_popUp.gameObject.SetActive(true);
		}

		private void LogOut()
		{
			AuthService.Instance.LogOut();
		}

		private void UpdateView()
		{
			_AuthorizationBtn.gameObject.SetActive(!AuthService.IsLoggedIn);
			_logOutBtn.gameObject.SetActive(AuthService.IsLoggedIn);
			_deleteAccountButton.gameObject.SetActive(IsActiveDeleteAccountButton());
		}

		private bool IsActiveDeleteAccountButton()
		{
#if UNITY_IOS
			return AuthService.IsLoggedIn && IDosGamesSDKSettings.Instance.IOSAccountDeletionEnabled;
#elif UNITY_ANDROID
			return AuthService.IsLoggedIn && IDosGamesSDKSettings.Instance.AndroidAccountDeletionEnabled;
#else
			return false;
#endif
		}

		public void DeleteTitlePlayerAccount()
		{
			AuthService.Instance.DeleteTitlePlayerAccount(OnSuccessDeleteTitlePlayerAccount);
		}

		private void OnSuccessDeleteTitlePlayerAccount()
		{
			Message.Show(MessageCode.ACCOUNT_SUCCESS_DELETED);
			SetActiveDeleteAccountPopUp(false);
			AuthService.Instance.LogOut();
		}

		private void SetActiveDeleteAccountPopUp(bool active)
		{
			_popUpDeleteAccount.SetActive(active);
		}
	}
}