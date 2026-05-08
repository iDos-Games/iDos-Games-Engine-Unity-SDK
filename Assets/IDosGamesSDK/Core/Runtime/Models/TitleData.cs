using System;
using System.Collections.Generic;

namespace IDosGames
{
    public class TitleData
    {
        /*
        public event Action OnAnyUpdated;
        public event Action OnLeaderboardUpdated;
        
        // v2
        public Dictionary<string, GetLeaderboardResponse> LeaderboardResponses { get; private set; }

        // v2
        internal void ApplyLeaderboardResponse(GetLeaderboardResponse data)
        {
            LeaderboardResponses ??= new();
            LeaderboardResponses[data.LeaderboardID] = data;
            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void Clear()
        {
            Leaderboard = null;
            LeaderboardResponses = null;
        }
        */
    }
}
