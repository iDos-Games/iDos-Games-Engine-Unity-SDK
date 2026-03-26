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
            IDosGamesData.User.OnLeaderboardDataUpdated += CheckData;
        }

        private void OnDisable()
        {
            IDosGamesData.User.OnLeaderboardDataUpdated -= CheckData;
        }

        private void CheckData()
        {
            _leaderboardID = $"{IDosGamesSDKSettings.Instance.TitleID}_{_statisticName}";
            if (IDosGamesData.User.LeaderboardData != null && IDosGamesData.User.LeaderboardData.TryGetValue(_leaderboardID, out var leaderboardData))
            {
                if (leaderboardData.PendingRewardVersion > 0)
                {
                    _popUpRewards.SetActivatePopUp(true);
                }
            }
        }
    }
}
