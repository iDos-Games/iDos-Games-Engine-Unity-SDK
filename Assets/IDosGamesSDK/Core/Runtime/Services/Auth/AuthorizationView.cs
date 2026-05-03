using System.Threading.Tasks;
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
            AuthenticationService.OnLoggedIn += UpdateView;
		}

		private void OnDisable()
		{
            AuthenticationService.OnLoggedIn -= UpdateView;
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
            AuthenticationService.LogOut();
		}

		private void UpdateView()
		{
			_AuthorizationBtn.gameObject.SetActive(!AuthenticationService.IsLoggedIn);
			_logOutBtn.gameObject.SetActive(AuthenticationService.IsLoggedIn);
			_deleteAccountButton.gameObject.SetActive(IsActiveDeleteAccountButton());
		}

		private bool IsActiveDeleteAccountButton()
		{
#if UNITY_IOS
			return AuthenticationService.IsLoggedIn && IDosGamesSDKSettings.Instance.IOSAccountDeletionEnabled;
#elif UNITY_ANDROID
			return AuthenticationService.IsLoggedIn && IDosGamesSDKSettings.Instance.AndroidAccountDeletionEnabled;
#else
            return false;
#endif
		}

		public async Task DeleteTitlePlayerAccountAsync()
		{
            var result = await UserService.DeleteUserAccount();
			if (result.Success)
			{
				OnSuccessDeleteTitlePlayerAccount();
            }
        }

		private void OnSuccessDeleteTitlePlayerAccount()
		{
			Message.Show(MessageCode.ACCOUNT_SUCCESS_DELETED);
			SetActiveDeleteAccountPopUp(false);
            AuthenticationService.LogOut();
		}

		private void SetActiveDeleteAccountPopUp(bool active)
		{
			_popUpDeleteAccount.SetActive(active);
		}
	}
}