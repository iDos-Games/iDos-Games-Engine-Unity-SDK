using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class ButtonRefreshLeaderboard : MonoBehaviour
	{
		[SerializeField] private LeaderboardWindow _window;
		[SerializeField] private RefreshLeaderboardTimer _timer;

		[SerializeField] private int _timeToNextRefreshInMinutes = 1;

		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(_window.Refresh);
		}

		public void SetInteractable(bool interactable)
		{
			if (interactable)
			{
				_timer.gameObject.SetActive(false);
			}
			else
			{
				_timer.gameObject.SetActive(true);
				_timer.UpdateTimer(_timeToNextRefreshInMinutes);
			}

			_button.interactable = interactable;

		}
	}
}