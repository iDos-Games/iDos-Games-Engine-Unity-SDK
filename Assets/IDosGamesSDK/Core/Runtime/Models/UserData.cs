using System;
using System.Collections.Generic;

namespace IDosGames
{
    public class UserData
    {
        public UserState State { get; private set; } = new();
        
        public event Action OnAnyUpdated;
        public event Action OnUserStateUpdated;
        public event Action OnInventoryUpdated;
        public event Action OnVirtualCurrencyUpdated;
        public event Action OnSocialUpdated;

        //public event Action OnLimitedTimeEventsUpdated;
        //public event Action OnCustomUserDataUpdated;
        //public event Action OnUserPublicDataUpdated;
        //public event Action OnBoardUpdated;
        //public event Action OnQuestsUpdated;
        //public event Action OnCharactersUpdated;
        //public event Action OnDailyRewardsUpdated;
        //public event Action OnPremiumUpdated;
        //public event Action OnLeaderboardDataUpdated;
        //public event Action OnPvPBattleStrategyUpdated;
        //public event Action OnDealOffersUpdated;
        //public event Action OnActiveDealSlotsUpdated;
        //public event Action OnLeaderboardProgressUpdated;
        //public event Action OnReferralUpdated;
        //public event Action OnStoreUpdated;

        internal UserData() { }

        internal void Clear()
        {
            State = null;
        }

        internal void ApplyUserState(UserState data)
        {
            State = data;
            OnUserStateUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyInventory(UserInventoryState data)
        {
            State.InventoryV2 = data;
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyVirtualCurrency(Dictionary<string, UserVirtualCurrencyState> data)
        {
            State.InventoryV2.VirtualCurrencies = data;
            OnVirtualCurrencyUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplySocial(UserSocialState data)
        {
            State.Social = data;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRecommended(List<string> userIds)
        {
            State.Social ??= new();
            State.Social.RecommendedFriends = userIds;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialAccepted(List<string> userIds)
        {
            State.Social ??= new();
            State.Social.Accepted = userIds;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialIncomingRequests(List<string> userIds)
        {
            State.Social ??= new();
            State.Social.IncomingRequests = userIds;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialOutgoingAdd(string userId)
        {
            State.Social ??= new();
            State.Social.OutgoingRequests ??= new();
            if (!State.Social.OutgoingRequests.Contains(userId))
                State.Social.OutgoingRequests.Add(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialAcceptRequest(string userId)
        {
            State.Social ??= new();
            State.Social.IncomingRequests?.Remove(userId);
            State.Social.Accepted ??= new();
            if (!State.Social.Accepted.Contains(userId))
                State.Social.Accepted.Add(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRemoveIncoming(string userId)
        {
            State.Social ??= new();
            State.Social.IncomingRequests?.Remove(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRemoveFriend(string userId)
        {
            State.Social ??= new();
            State.Social.Accepted?.Remove(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
    }
}
