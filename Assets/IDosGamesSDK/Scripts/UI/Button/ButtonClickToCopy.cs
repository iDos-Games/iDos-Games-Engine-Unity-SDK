using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class ButtonClickToCopy : MonoBehaviour
	{
		[SerializeField] private TMP_Text _textToCopy;
		[SerializeField] private PopUpCopied _popUp;
		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(Copy);
		}

		private void Copy()
		{
			GUIUtility.systemCopyBuffer = _textToCopy.text;

			if (_popUp)
			{
				_popUp.gameObject.SetActive(true);
			}

#if UNITY_WEBGL && !UNITY_EDITOR
			WebSDK.CopyTextToClipboard(_textToCopy.text);
#endif
		}
    }
}