using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class IOSRestoreIAPButton : MonoBehaviour
	{
		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
		}

		private void Start()
		{
#if UNITY_IOS
			ResetButton();
#else
			gameObject.SetActive(false);
#endif
		}

		private void Restore()
		{
#if IDOSGAMES_MOBILE_IAP
			IAPService.RestorePurchasesIOS(OnRestored);
#endif
        }

        private void OnRestored(bool status, string result)
		{
			Message.Show(MessageCode.ITEMS_RESTORED);
		}

		private void ResetButton()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(Restore);
		}
	}
}
