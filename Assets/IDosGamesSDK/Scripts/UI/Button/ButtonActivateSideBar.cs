using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ButtonActivateSideBar : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private CanvasGroup _sideBar;
		[SerializeField] private GameObject _alarmObject;

		[Range(1, 10)][SerializeField] private int _animationSpeed = 5;
		[SerializeField] private bool _hideOnAwake;

		public bool IsSideBarActive => _sideBar.gameObject.activeSelf;

		private void Awake()
		{
			ResetButton();

			if (_hideOnAwake)
			{
				SetActiveSideBarObject(false);
			}
		}

		private void ResetButton()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(OnClick);
		}

		public void OnClick()
		{
			SetActiveSideBar(!_sideBar.gameObject.activeSelf);
		}

		public void SetActiveSideBar(bool active)
		{
			if (active)
			{
				SetActiveSideBarObject(true);
			}
			else
			{
				StartCoroutine(nameof(HideWithAnimation));
			}
		}

		private IEnumerator HideWithAnimation()
		{
			float t = 1;

			while (t > 0)
			{
				t -= _animationSpeed * Time.unscaledDeltaTime;
				_sideBar.alpha = t;

				yield return null;
			}

			SetActiveSideBarObject(false);
		}

		private void SetActiveSideBarObject(bool active)
		{
			_sideBar.alpha = active ? 1 : 0;
			_sideBar.gameObject.SetActive(active);
			_alarmObject.SetActive(!active);
		}
	}
}