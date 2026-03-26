using System;
using System.Collections.Generic;
using IDosGames.ClientModels;
using IDosGames.ServerModels;

namespace IDosGames
{
    public class UserData
    {
        public Dictionary<string, long> VirtualCurrency { get; private set; }
        public Dictionary<string, VirtualCurrencyRechargeTime> VirtualCurrencyRechargeTimes { get; private set; }
        public List<ItemInstance> Inventory { get; private set; }

        public UserPublicDataModel UserPublicData { get; private set; }
        public BoardLoopState Board { get; private set; }
        public UserQuestState Quests { get; private set; }
        public Dictionary<string, CharacterModel> Characters { get; private set; }
        public Dictionary<string, DailyRewardState> DailyRewards { get; private set; }
        //public UserSocialState Social { get; private set; }
        //public UserPremiumState Premium { get; private set; }
        public Dictionary<string, PlayerLeaderboardData> LeaderboardData { get; private set; }

        public event Action OnUserPublicDataUpdated;
        public event Action OnBoardUpdated;
        public event Action OnQuestsUpdated;
        public event Action OnCharactersUpdated;
        public event Action OnInventoryUpdated;
        public event Action OnVirtualCurrencyUpdated;
        public event Action OnVirtualCurrencyRechargeTimesUpdated;
        public event Action OnDailyRewardsUpdated;
        public event Action OnSocialUpdated;
        public event Action OnPremiumUpdated;
        public event Action OnLeaderboardDataUpdated;
        public event Action OnAnyUpdated;

        public bool IsLoggedIn { get; internal set; }

        internal UserData() { }

        internal void ApplyProfile(UserPublicDataModel data)
        {
            UserPublicData = data;
            OnUserPublicDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyBoard(BoardLoopState data)
        {
            Board = data;
            OnBoardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyQuests(UserQuestState data)
        {
            Quests = data;
            OnQuestsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCharacters(Dictionary<string, CharacterModel> data)
        {
            Characters = data;
            OnCharactersUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyInventory(List<ItemInstance> data)
        {
            Inventory = data;
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyVirtualCurrency(Dictionary<string, long> data)
        {
            VirtualCurrency = data;
            OnVirtualCurrencyUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyVirtualCurrencyRechargeTimes(Dictionary<string, VirtualCurrencyRechargeTime> data)
        {
            VirtualCurrencyRechargeTimes = data;
            OnVirtualCurrencyRechargeTimesUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyDailyRewards(Dictionary<string, DailyRewardState> data)
        {
            DailyRewards = data;
            OnDailyRewardsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        //internal void ApplySocial(UserSocialState data)
        //{
        //    Social = data;
        //    OnSocialUpdated?.Invoke();
        //    OnAnyUpdated?.Invoke();
        //}

        //internal void ApplyPremium(UserPremiumState data)
        //{
        //    Premium = data;
        //    OnPremiumUpdated?.Invoke();
        //    OnAnyUpdated?.Invoke();
        //}

        internal void ApplyLeaderboardData(Dictionary<string, PlayerLeaderboardData> data)
        {
            LeaderboardData = data;
            OnLeaderboardDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void Clear()
        {
            UserPublicData = null;
            Board = null;
            Quests = null;
            Characters = null;
            Inventory = null;
            VirtualCurrency = null;
            VirtualCurrencyRechargeTimes = null;
            DailyRewards = null;
            //Social = null;
            //Premium = null;
            LeaderboardData = null;
            IsLoggedIn = false;
        }
    }
}
