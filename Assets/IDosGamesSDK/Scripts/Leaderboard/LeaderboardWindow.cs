using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
	public class LeaderboardWindow : MonoBehaviour
	{
		public const int MAX_DISPLAY_PLACES_COUNT = 100;

		[SerializeField] private LeaderboardView _view;
		[SerializeField] private LeaderboardDescription _description;

		private void Start()
		{
            UserDataService.DataUpdated += RefreshData;

            //Refresh();
            if (IGSUserData.Leaderboard != null)
			{
                RefreshData();
			}

			if (PlayerPrefs.GetInt(AlarmType.OpenedLeaderboardWindow.ToString(), 0) == 0)
			{
				if (AlarmSystem.Instance != null)
				{
					PlayerPrefs.SetInt(AlarmType.OpenedLeaderboardWindow.ToString(), 1);
					PlayerPrefs.Save();
					AlarmSystem.Instance.SetAlarmState(AlarmType.OpenedLeaderboardWindow, false);
				}
			}
		}

        private void OnDestroy()
        {
            UserDataService.DataUpdated -= RefreshData;
        }

        public void RefreshData()
        {
            var titleData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.Leaderboards);
            var leaderboardsArray = JsonConvert.DeserializeObject<List<Leaderboard>>(titleData);

            if (leaderboardsArray == null || !leaderboardsArray.Any())
            {
                return;
            }

            foreach (var leaderboard in leaderboardsArray)
            {
                if (leaderboard == null)
                {
                    continue;
                }

                _view.SetTitle(leaderboard.Name);
                _view.SetStatValueName(leaderboard.ValueName);

                if (Enum.IsDefined(typeof(StatisticResetFrequency), leaderboard.Frequency))
                {
                    _view.SetTimer(leaderboard.Frequency);
                }

                OnSuccessGetLeaderboard(IGSUserData.Leaderboard);
                _description.Initialize(leaderboard);
            }
        }

        public void Refresh()
        {
            var titleData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.Leaderboards);
            var leaderboardsArray = JsonConvert.DeserializeObject<List<Leaderboard>>(titleData);

            if (leaderboardsArray == null || !leaderboardsArray.Any())
            {
                return;
            }

            foreach (var leaderboard in leaderboardsArray)
            {
                if (leaderboard == null)
                {
                    continue;
                }

                _view.SetTitle(leaderboard.Name);
                _view.SetStatValueName(leaderboard.ValueName);

                if (Enum.IsDefined(typeof(StatisticResetFrequency), leaderboard.Frequency))
                {
                    _view.SetTimer(leaderboard.Frequency);
                }

                var leaderboardID = leaderboard.StatisticName;
                RequestLeaderboard(leaderboardID);
                _description.Initialize(leaderboard);
            }
        }

        private void RequestLeaderboard(string leaderboardID)
		{
			Loading.ShowTransparentPanel();

            IGSClientAPI.GetUserAllData(resultCallback: (result) => { OnSuccessGetLeaderboard(result.LeaderboardResult); UserDataService.ProcessingAllData(result); }, notConnectionErrorCallback: OnErrorGetLeaderboard, connectionErrorCallback: null);

        }

		private void OnSuccessGetLeaderboard(GetLeaderboardResult result)
		{
			Loading.HideAllPanels();

            IGSUserData.Leaderboard = result;

            if (result == null)
			{
				return;
			}

			if (result.Leaderboard == null)
			{
				return;
			}

			_view.SetRows(result.Leaderboard);
		}

		private void OnErrorGetLeaderboard(string error)
		{
			Message.Show(MessageCode.SOMETHING_WENT_WRONG);
		}
	}
}