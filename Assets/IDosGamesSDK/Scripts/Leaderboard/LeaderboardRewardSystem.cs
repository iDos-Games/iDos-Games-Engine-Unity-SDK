using UnityEngine;

namespace IDosGames
{
    public class LeaderboardRewardSystem : MonoBehaviour
    {
        [SerializeField] private PopUpLeaderboardRewards _popUpRewards;

        private string _statisticName = "coin_contest";
        private string _leaderboardID;

        private void OnEnable()
        {
            UserDataService.DataUpdated += CheckData;
        }

        private void OnDisable()
        {
            UserDataService.DataUpdated -= CheckData;
        }

        private void CheckData()
        {
            _leaderboardID = $"{IDosGamesSDKSettings.Instance.TitleID}_{_statisticName}";
            if (IGSUserData.LeaderboardData != null && IGSUserData.LeaderboardData.TryGetValue(_leaderboardID, out var leaderboardData))
            {
                if (leaderboardData.PendingRewardVersion > 0)
                {
                    _popUpRewards.SetActivatePopUp(true);
                }
            }
        }
    }
}
