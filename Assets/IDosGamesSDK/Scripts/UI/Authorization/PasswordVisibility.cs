using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class PasswordVisibility : MonoBehaviour
	{
		[SerializeField] private Image _hiddenIcon;
		[SerializeField] private TMP_InputField _inputField;

		private bool _isHidden;
		private Button _button;

		private void Start()
		{
			_button = GetComponent<Button>();
			_button.onClick.AddListener(ChangeState);
		}

		private void ChangeState()
		{
			_isHidden = !_isHidden;

			ChangeInputField();
			ChangeIcon();
		}

		private void ChangeInputField()
		{
			_inputField.contentType = _isHidden ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
			_inputField.ForceLabelUpdate();
		}

		private void ChangeIcon()
		{
			_hiddenIcon.gameObject.SetActive(_isHidden);
		}
	}
}