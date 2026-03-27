using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
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
            IDosGamesData.User.OnAnyUpdated += RefreshData;
            if (IDosGamesData.Title.Leaderboard != null)
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
            IDosGamesData.User.OnAnyUpdated -= RefreshData;
        }

        private List<Leaderboard> GetLeaderboards()
        {
            return IDosGamesData.Config.TitlePublicConfiguration?.Leaderboards;
        }

        public void RefreshData()
        {
            var leaderboards = GetLeaderboards();
            if (leaderboards == null || !leaderboards.Any()) return;

            foreach (var leaderboard in leaderboards)
            {
                if (leaderboard == null) continue;

                _view.SetTitle(leaderboard.Name);
                _view.SetStatValueName(leaderboard.ValueName);
                if (Enum.IsDefined(typeof(StatisticResetFrequency), leaderboard.Frequency))
                {
                    _view.SetTimer(leaderboard.Frequency);
                }
                OnSuccessGetLeaderboard(IDosGamesData.Title.Leaderboard);
                _description.Initialize(leaderboard);
            }
        }

        public async void Refresh()
        {
            var leaderboards = GetLeaderboards();
            if (leaderboards == null || !leaderboards.Any()) return;

            foreach (var leaderboard in leaderboards)
            {
                if (leaderboard == null) continue;

                _view.SetTitle(leaderboard.Name);
                _view.SetStatValueName(leaderboard.ValueName);
                if (Enum.IsDefined(typeof(StatisticResetFrequency), leaderboard.Frequency))
                {
                    _view.SetTimer(leaderboard.Frequency);
                }
                await RequestLeaderboard(leaderboard.StatisticName);
                _description.Initialize(leaderboard);
            }
        }

        private async System.Threading.Tasks.Task RequestLeaderboard(string leaderboardID)
        {
            Loading.ShowTransparentPanel();
            var result = await UserService.GetClientState();
            if (result.Success)
            {
                OnSuccessGetLeaderboard(result.Data.LeaderboardResult);
            }
            else
            {
                OnErrorGetLeaderboard(string.Empty);
            }
        }

        private void OnSuccessGetLeaderboard(GetLeaderboardResult result)
        {
            Loading.HideAllPanels();
            IDosGamesData.Title.ApplyLeaderboard(result);
            if (result?.Leaderboard == null) return;
            _view.SetRows(result.Leaderboard);
        }

        private void OnErrorGetLeaderboard(string error)
        {
            Message.Show(MessageCode.SOMETHING_WENT_WRONG);
        }
    }
}
