using IDosGames.ClientModels;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    public class TitleData
    {
        // v1
        public GetLeaderboardResult Leaderboard { get; private set; }
        public string MarketplaceGroupedOffers { get; set; }
        public string MarketplaceActiveOffers { get; set; }
        public string MarketplaceHistory { get; set; }

        public event Action OnAnyUpdated;
        public event Action OnLeaderboardUpdated;

        // v2
        public Dictionary<string, GetLeaderboardResponse> LeaderboardResponses { get; private set; }

        
        // v1
        internal void ApplyLeaderboard(GetLeaderboardResult data)
        {
            Leaderboard = data;
            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

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
    }
}
