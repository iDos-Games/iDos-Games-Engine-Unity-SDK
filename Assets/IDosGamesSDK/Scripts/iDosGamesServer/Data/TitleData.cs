using IDosGames.ClientModels;
using System;

namespace IDosGames
{
    public class TitleData
    {
        public GetLeaderboardResult Leaderboard { get; private set; }

        public string MarketplaceGroupedOffers { get; set; }
        public string MarketplaceActiveOffers { get; set; }
        public string MarketplaceHistory { get; set; }

        public event Action OnAnyUpdated;
        public event Action OnLeaderboardUpdated;

        internal void ApplyLeaderboard(GetLeaderboardResult data)
        {
            Leaderboard = data;
            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void Clear()
        {
            Leaderboard = null;
        }
    }
}
