using System;
using System.Collections.Generic;

namespace IDosGames
{
    public class UserData
    {
        public UserInventoryState InventoryV2 { get; set; }
        public UserEventTokensState EventToken { get; set; }
        //public UserPremiumState Premium { get; set; }
        public UserPublicDataModel PublicData { get; set; }
        public UserSocialState Social { get; set; }
        //public UserQuestState Quest { get; set; }
        //public UserGameLoopsState GameLoop { get; set; }
        //public UserSeasonsState Season { get; set; }
        //public UserCoopEventState CoopEvent { get; set; }
        //public UserCollectionState Collection { get; set; }
        //public UserLootboxState Lootbox { get; set; }
        //public UserStoreState Store { get; set; }
        //public UserDealOffersState DealOffer { get; set; }
        //public UserReferralState Referral { get; set; }
        //public UserLeaderboardsState Leaderboard { get; set; }
        //public PlayerEconomyTuningState EconomyTuning { get; set; }
        public UserUsageState Usage { get; set; }

        //public UserCustomDataState CustomData { get; set; }
        //public UserBlockchainState Blockchain { get; set; }
        //public UserRewardState Reward { get; set; }
        //public UserCharactersState Character { get; set; }
        //public UserMatchState Match { get; set; }

        public event Action OnAnyUpdated;

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

        public bool IsLoggedIn { get; internal set; }

        internal UserData() { }

        internal void ApplyInventory(UserInventoryState data)
        {
            InventoryV2 = data;
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyVirtualCurrency(Dictionary<string, UserVirtualCurrencyState> data)
        {
            InventoryV2.VirtualCurrencies = data;
            OnVirtualCurrencyUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplySocial(UserSocialState data)
        {
            Social = data;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRecommended(List<string> userIds)
        {
            Social ??= new();
            Social.RecommendedFriends = userIds;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void Clear()
        {
            InventoryV2 = null;
            Social = null;
            IsLoggedIn = false;
        }

        /*
        internal void PatchConsumedResource(ItemOrCurrency resource, long newBalance)
        {
            if (resource == null) return;

            if (resource.Type == ItemType.VirtualCurrency)
            {
                PatchVirtualCurrency(resource.CurrencyID, newBalance);
            }
            else if (resource.Type == ItemType.Item)
            {
                Inventory ??= new();
                var item = Inventory.FirstOrDefault(i => i.ItemId == resource.ItemID
                    && (resource.Catalog == null || i.CatalogVersion == resource.Catalog));
                if (item != null)
                {
                    item.RemainingUses -= (int)resource.Amount.Value;
                    if (item.RemainingUses <= 0)
                        Inventory.Remove(item);
                }
                OnInventoryUpdated?.Invoke();
                OnAnyUpdated?.Invoke();
            }
        }

        internal void ConsumeResources(List<ItemOrCurrency> consumed)
        {
            if (consumed == null) return;

            foreach (var resource in consumed)
            {
                if (resource.Type == ItemType.VirtualCurrency)
                {
                    VirtualCurrency ??= new();
                    if (VirtualCurrency.ContainsKey(resource.CurrencyID))
                        VirtualCurrency[resource.CurrencyID] -= resource.Amount.Value;
                }
                else if (resource.Type == ItemType.Item)
                {
                    Inventory ??= new();
                    var item = Inventory.FirstOrDefault(i => i.ItemId == resource.ItemID
                        && (resource.Catalog == null || i.CatalogVersion == resource.Catalog));
                    if (item != null)
                    {
                        item.RemainingUses -= (int)resource.Amount.Value;
                        if (item.RemainingUses <= 0)
                            Inventory.Remove(item);
                    }
                }
            }

            OnVirtualCurrencyUpdated?.Invoke();
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void GrantResources(List<ItemOrCurrency> granted)
        {
            if (granted == null) return;

            foreach (var resource in granted)
            {
                if (resource.Type == ItemType.VirtualCurrency)
                {
                    VirtualCurrency ??= new();
                    if (VirtualCurrency.ContainsKey(resource.CurrencyID))
                        VirtualCurrency[resource.CurrencyID] += resource.Amount.Value;
                    else
                        VirtualCurrency[resource.CurrencyID] = resource.Amount.Value;
                }
                else if (resource.Type == ItemType.Item)
                {
                    Inventory ??= new();
                    var item = Inventory.FirstOrDefault(i => i.ItemId == resource.ItemID
                        && (resource.Catalog == null || i.CatalogVersion == resource.Catalog));
                    if (item != null)
                    {
                        if (item.RemainingUses.HasValue)
                            item.RemainingUses += (int)resource.Amount.Value;
                    }
                    else
                    {
                        Inventory.Add(new ItemInstance
                        {
                            ItemId = resource.ItemID,
                            CatalogVersion = resource.Catalog,
                            RemainingUses = resource.Amount.HasValue ? (int)resource.Amount.Value : null
                        });
                    }
                }
            }

            OnVirtualCurrencyUpdated?.Invoke();
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchResources(List<ItemOrCurrency> resources)
        {
            if (resources == null) return;

            foreach (var resource in resources)
            {
                if (resource.Type == ItemType.VirtualCurrency)
                {
                    VirtualCurrency ??= new();
                    VirtualCurrency[resource.CurrencyID] = resource.Amount.Value;
                }
                else if (resource.Type == ItemType.Item)
                {
                    Inventory ??= new();
                    var item = Inventory.FirstOrDefault(i => i.ItemId == resource.ItemID
                        && (resource.Catalog == null || i.CatalogVersion == resource.Catalog));
                    if (item != null)
                        item.RemainingUses = resource.Amount.HasValue ? (int)resource.Amount.Value : null;
                }
            }

            OnVirtualCurrencyUpdated?.Invoke();
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
        */

        internal void PatchSocialAccepted(List<string> userIds)
        {
            Social ??= new();
            Social.Accepted = userIds;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialIncomingRequests(List<string> userIds)
        {
            Social ??= new();
            Social.IncomingRequests = userIds;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialOutgoingAdd(string userId)
        {
            Social ??= new();
            Social.OutgoingRequests ??= new();
            if (!Social.OutgoingRequests.Contains(userId))
                Social.OutgoingRequests.Add(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialAcceptRequest(string userId)
        {
            Social ??= new();
            Social.IncomingRequests?.Remove(userId);
            Social.Accepted ??= new();
            if (!Social.Accepted.Contains(userId))
                Social.Accepted.Add(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRemoveIncoming(string userId)
        {
            Social ??= new();
            Social.IncomingRequests?.Remove(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRemoveFriend(string userId)
        {
            Social ??= new();
            Social.Accepted?.Remove(userId);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
    }

    public class UserSocialState
    {
        public List<string> Accepted { get; set; } = new();
        public List<string> IncomingRequests { get; set; } = new();
        public List<string> OutgoingRequests { get; set; } = new();
        public List<string> RecommendedFriends { get; set; } = new();
    }
}
