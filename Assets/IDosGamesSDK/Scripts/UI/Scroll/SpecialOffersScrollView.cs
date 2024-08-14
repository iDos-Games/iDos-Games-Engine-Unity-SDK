using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class SpecialOffersScrollView : MonoBehaviour
	{
		[SerializeField] private DotsNavigation _dotsNavigation;
		[SerializeField] private RectTransform _content;
		[SerializeField] private Button _switchToNextButton;
		[SerializeField] private Button _switchToPreviousButton;
		[SerializeField] private int _oneStepWidth;

		[SerializeField] private bool _autoScroll = true;
		[Range(1, 10)][SerializeField] private int _autoScrollInterval = 5;

		private const int MIN_CHILDS_TO_SCROLL = 2;

		private float _switchSpeed = 7.5f;

		private int _currentPageIndex => _dotsNavigation.CurrentPageIndex;

		private bool _canScroll => _content.childCount >= MIN_CHILDS_TO_SCROLL;

		private void OnEnable()
		{
			SpecialOfferShopPanel.PanelInitialized += Initialize;
			StartAutoScroll();
		}

		private void OnDisable()
		{
			SpecialOfferShopPanel.PanelInitialized -= Initialize;
		}

		public void Initialize()
		{
			_content.anchoredPosition = Vector2.zero;

			var childCount = GetChildCount(_content);

			_dotsNavigation.Initialize(childCount);
			SetSwitchButtons();
			StartAutoScroll();
		}

		private int GetChildCount(Transform _content)
		{
			var childCount = 0;

			foreach (Transform child in _content)
			{
				if (child.gameObject.activeSelf)
				{
					childCount++;
				}
			}

			return childCount;
		}

		private void SetSwitchButtons()
		{
			_switchToNextButton.gameObject.SetActive(_canScroll);
			_switchToPreviousButton.gameObject.SetActive(_canScroll);

			if (_canScroll)
			{
				_switchToNextButton.onClick.RemoveAllListeners();
				_switchToNextButton.onClick.AddListener(ClickSwitchToNext);

				_switchToPreviousButton.onClick.RemoveAllListeners();
				_switchToPreviousButton.onClick.AddListener(ClickSwitchToPrevious);
			}
		}

		private void StartAutoScroll()
		{
			if (_autoScroll && _canScroll)
			{
				StopCoroutine(nameof(AutoScroll));
				StartCoroutine(nameof(AutoScroll));
			}
		}

		private void StopAutoScroll()
		{
			if (_autoScroll)
			{
				StopCoroutine(nameof(AutoScroll));
			}
		}

		private IEnumerator AutoScroll()
		{
			while (_autoScroll)
			{
				yield return new WaitForSecondsRealtime(_autoScrollInterval);
				SwitchToNext();
			}
		}

		private IEnumerator Switch()
		{
			bool finishedMoving = false;

			while (!finishedMoving)
			{
				float decelerate = Mathf.Min(_switchSpeed * Time.unscaledDeltaTime, 1f);

				var lerpTo = new Vector2(-(_currentPageIndex * _oneStepWidth), 0);

				_content.anchoredPosition = Vector2.Lerp(_content.anchoredPosition, lerpTo, decelerate);

				if (Vector2.SqrMagnitude(_content.anchoredPosition - lerpTo) < 1f)
				{
					_content.anchoredPosition = lerpTo;
					finishedMoving = true;
				}

				yield return null;
			}
		}

		private void SwitchToNext()
		{
			_dotsNavigation.SwitchToNext();
			StartCoroutine(Switch());
		}

		private void SwitchToPrevious()
		{
			_dotsNavigation.SwitchToPrevious();
			StartCoroutine(Switch());
		}

		private void ClickSwitchToNext()
		{
			SwitchToNext();
			StopAutoScroll();
		}

		private void ClickSwitchToPrevious()
		{
			SwitchToPrevious();
			StopAutoScroll();
		}
	}
}