using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class AccountInfoView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _userID;
		[SerializeField] private TMP_Text _email;

		private void OnEnable()
		{
			UpdateView();

			AuthService.LoggedIn += UpdateView;
			//LocalizationSystem.OnLanguageChanged += UpdateView;
		}

		private void OnDisable()
		{
			AuthService.LoggedIn -= UpdateView;
			//LocalizationSystem.OnLanguageChanged -= UpdateView;
		}

		private void UpdateView()
		{
			SetUserIDText();
			SetEmailText();
		}

		private void SetUserIDText()
		{
			_userID.text = AuthService.UserID;
		}

		private void SetEmailText()
		{
			string email = AuthService.SavedEmail;
			string haveNotLinkedText = MessageCode.NEED_LOGIN_WITH_EMAIL.ToString(); //LocalizationSystem

            _email.text = email != string.Empty ? email : haveNotLinkedText;

		}
	}
}