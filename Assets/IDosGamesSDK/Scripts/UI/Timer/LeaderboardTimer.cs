using System;
using UnityEngine;

namespace IDosGames
{
	public class LeaderboardTimer : Timer
	{
		[SerializeField] private LeaderboardWindow _leaderboardWindow;

		protected override void OnEnable()
		{
			base.OnEnable();
			TimerStopped += RefreshLeaderboard;
		}

		private void OnDisable()
		{
			TimerStopped -= RefreshLeaderboard;
		}

		private void RefreshLeaderboard()
		{
			_leaderboardWindow.Refresh();
		}

		public void UpdateTimer(LeaderboardUpdateFrequency frequency)
		{
			DateTime endDate = DateTime.UtcNow;

			switch (frequency)
			{
				default:
				case LeaderboardUpdateFrequency.daily:
					endDate = GetTimeToNextDay();
					break;
				case LeaderboardUpdateFrequency.weekly:
					endDate = GetTimeToNextWeek();
					break;
				case LeaderboardUpdateFrequency.monthly:
					endDate = GetTimeToNextMonth();
					break;
			}

			Set(endDate);
		}

		private DateTime GetTimeToNextDay()
		{
			return DateTime.UtcNow.Date.AddDays(1);
		}

		private DateTime GetTimeToNextWeek()
		{
			int dayOfWeekInt = DateTime.UtcNow.Date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)DateTime.UtcNow.Date.DayOfWeek;
			int daysToAdd = 7 + 1 - dayOfWeekInt;
			return DateTime.UtcNow.Date.AddDays(daysToAdd);
		}

		private DateTime GetTimeToNextMonth()
		{
			DateTime currentMonth = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			return currentMonth.AddMonths(1);
		}
	}
}