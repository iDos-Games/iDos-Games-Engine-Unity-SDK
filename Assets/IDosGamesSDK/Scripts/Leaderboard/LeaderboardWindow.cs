using IDosGames.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            var titleData = UserDataService.GetTitleData(TitleDataKey.leaderboards);
            var leaderboardsArray = JsonConvert.DeserializeObject<JArray>(titleData);
            if (leaderboardsArray == null || !leaderboardsArray.Any())
            {
                return;
            }

            foreach (var leaderboardData in leaderboardsArray)
            {
                if (leaderboardData == null)
                {
                    continue;
                }

                var leaderboardObject = leaderboardData as JObject;
                if (leaderboardObject == null)
                {
                    continue;
                }

                _view.SetTitle($"{leaderboardObject[JsonProperty.NAME]}");
                _view.SetStatValueName($"{leaderboardObject[JsonProperty.VALUE_NAME]}");
                _view.SetTimer($"{leaderboardObject[JsonProperty.FREQUENCY]}");

                var leaderboardID = $"{leaderboardObject[JsonProperty.DEFAULT_STATISTIC_NAME]}";
                OnSuccessGetLeaderboard(IGSUserData.Leaderboard);

                _description.Initialize(leaderboardObject);
            }
        }

        public void Refresh()
        {
            var titleData = UserDataService.GetTitleData(TitleDataKey.leaderboards);
            var leaderboardsArray = JsonConvert.DeserializeObject<JArray>(titleData);
            if (leaderboardsArray == null || !leaderboardsArray.Any())
            {
                return;
            }

            foreach (var leaderboardData in leaderboardsArray)
            {
                if (leaderboardData == null)
                {
                    continue;
                }

                var leaderboardObject = leaderboardData as JObject;
                if (leaderboardObject == null)
                {
                    continue;
                }

                _view.SetTitle($"{leaderboardObject[JsonProperty.NAME]}");
                _view.SetStatValueName($"{leaderboardObject[JsonProperty.VALUE_NAME]}");
                _view.SetTimer($"{leaderboardObject[JsonProperty.FREQUENCY]}");

                var leaderboardID = $"{leaderboardObject[JsonProperty.DEFAULT_STATISTIC_NAME]}";
                RequestLeaderboard(leaderboardID);

                _description.Initialize(leaderboardObject);
            }
        }

        private void RequestLeaderboard(string leaderboardID)
		{
			Loading.ShowTransparentPanel();

            IGSClientAPI.GetUserAllData(resultCallback: (result) => { OnSuccessGetLeaderboard(result.LeaderboardResult); UserDataService.ProcessingAllData(result); }, notConnectionErrorCallback: OnErrorGetLeaderboard, connectionErrorCallback: null);

            /*
            IGSClientAPI.GetLeaderboard
			(
				leaderboardID: leaderboardID,
				//maxResultCount: MAX_DISPLAY_PLACES_COUNT,
				resultCallback: OnSuccessGetLeaderboard,
				notConnectionErrorCallback: OnErrorGetLeaderboard,
				connectionErrorCallback: () => RequestLeaderboard(leaderboardID)
			);
            */
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