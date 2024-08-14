using System;
using UnityEngine;

namespace IDosGames
{
	public class RefreshLeaderboardTimer : Timer
	{
		[SerializeField] private ButtonRefreshLeaderboard _button;

		protected override void OnEnable()
		{
			base.OnEnable();
			TimerStopped += SetButtonInteractable;
		}

		private void OnDisable()
		{
			TimerStopped -= SetButtonInteractable;
		}

		private void SetButtonInteractable()
		{
			_button.SetInteractable(true);
		}

		public void UpdateTimer(double minutes)
		{
			DateTime dateTime = DateTime.UtcNow.AddMinutes(minutes);

			Set(dateTime);
		}
	}
}