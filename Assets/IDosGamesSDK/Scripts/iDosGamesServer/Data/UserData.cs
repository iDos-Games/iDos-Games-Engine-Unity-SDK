using System;
using System.Collections.Generic;
using System.Linq;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class UserData
    {
        public Dictionary<string, long> VirtualCurrency { get; private set; }
        public Dictionary<string, VirtualCurrencyRechargeTime> VirtualCurrencyRechargeTimes { get; private set; }
        public List<ItemInstance> Inventory { get; private set; }

        public UserLimitedTimeEventsState LimitedTimeEvents { get; private set; }
        public GetCustomUserDataResult CustomUserData { get; private set; }

        public UserSocialState Social { get; private set; } = new ();
        public UserPublicDataModel UserPublicData { get; private set; }
        public BoardLoopState Board { get; private set; }
        public UserQuestState Quests { get; private set; }
        public Dictionary<string, CharacterModel> Characters { get; private set; }
        public Dictionary<string, DailyRewardState> DailyRewards { get; private set; }
        //public UserPremiumState Premium { get; private set; }
        public Dictionary<string, PlayerLeaderboardData> LeaderboardData { get; private set; }
        public List<BattleStepConfig> PvPBattleStrategy { get; private set; }

        public event Action OnVirtualCurrencyUpdated;
        public event Action OnVirtualCurrencyRechargeTimesUpdated;
        public event Action OnInventoryUpdated;
        public event Action OnLimitedTimeEventsUpdated;
        public event Action OnCustomUserDataUpdated;

        public event Action OnSocialUpdated;
        public event Action OnUserPublicDataUpdated;
        public event Action OnBoardUpdated;
        public event Action OnQuestsUpdated;
        public event Action OnCharactersUpdated;
        public event Action OnDailyRewardsUpdated;
        //public event Action OnPremiumUpdated;
        public event Action OnLeaderboardDataUpdated;
        public event Action OnAnyUpdated;
        public event Action OnPvPBattleStrategyUpdated;

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

        internal void ApplyLimitedTimeEvents(UserLimitedTimeEventsState data)
        {
            LimitedTimeEvents = data;
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyLimitedTimeEventsFromActive(GetActiveEventsResponse data)
        {
            if (data?.ActiveEvents == null) return;

            LimitedTimeEvents ??= new UserLimitedTimeEventsState
            {
                ScheduledEvents = new(),
                EventChains = new(),
            };

            foreach (var info in data.ActiveEvents)
            {
                if (info.Progress == null) continue;

                if (info.EventType == ActiveEventType.Chained && !string.IsNullOrEmpty(info.ID))
                {
                    if (!LimitedTimeEvents.EventChains.TryGetValue(info.ID, out var chain))
                    {
                        chain = new UserEventChainProgress { EventChainID = info.ID };
                        LimitedTimeEvents.EventChains[info.ID] = chain;
                    }
                    chain.CurrentEventProgress = info.Progress;
                    if (info.CurrentCycleIndex.HasValue) chain.CurrentCycleIndex = info.CurrentCycleIndex.Value;
                    if (info.CurrentEventOrder.HasValue) chain.CurrentEventOrder = info.CurrentEventOrder.Value;
                }
                else if (!string.IsNullOrEmpty(info.ID))
                {
                    LimitedTimeEvents.ScheduledEvents[info.ID] = info.Progress;
                }
            }

            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCustomUserData(GetCustomUserDataResult data)
        {
            CustomUserData = data;
            OnCustomUserDataUpdated?.Invoke();
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

        internal void ApplySocial(UserSocialState data)
        {
            Social = data;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

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

        internal void ApplyPvPBattleStrategy(List<BattleStepConfig> data)
        {
            PvPBattleStrategy = data;
            OnPvPBattleStrategyUpdated?.Invoke();
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
            UserPublicData = null;
            Board = null;
            Quests = null;
            Characters = null;
            Inventory = null;
            VirtualCurrency = null;
            VirtualCurrencyRechargeTimes = null;
            DailyRewards = null;
            Social = null;
            //Premium = null;
            LeaderboardData = null;
            PvPBattleStrategy = null;
            IsLoggedIn = false;
        }

        internal void PatchVirtualCurrency(string currencyId, long newBalance)
        {
            VirtualCurrency ??= new();
            VirtualCurrency[currencyId] = newBalance;
            OnVirtualCurrencyUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchVirtualCurrency(Dictionary<string, long> updatedCurrencies)
        {
            VirtualCurrency ??= new();
            foreach (var pair in updatedCurrencies)
                VirtualCurrency[pair.Key] = pair.Value;
            OnVirtualCurrencyUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCharacter(string characterId, Action<CharacterModel> patch)
        {
            Characters ??= new();
            if (!Characters.TryGetValue(characterId, out var character))
            {
                character = new CharacterModel { CharacterID = characterId };
                Characters[characterId] = character;
            }
            patch(character);
            OnCharactersUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

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

        internal void PatchCharacterEquipment(string characterId, string slotId, EquippedItem item)
        {
            if (Characters == null || !Characters.TryGetValue(characterId, out var character)) return;

            character.Equipment ??= new();

            if (item == null)
                character.Equipment.Remove(slotId);
            else
                character.Equipment[slotId] = item;

            OnCharactersUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ClearAllCharactersEquipment()
        {
            if (Characters == null) return;

            foreach (var character in Characters.Values)
                character.Equipment?.Clear();

            OnCharactersUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchQuestStatus(string questId, string cycleId, QuestStatus newStatus)
        {
            if (Quests == null) return;

            var progress = FindQuestProgress(questId, cycleId);
            if (progress == null) return;

            progress.Status = newStatus;
            if (newStatus == QuestStatus.Claimed)
                progress.ClaimedAtUtc = DateTime.UtcNow;

            OnQuestsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchClaimedMilestone(string cycleId, string milestoneId)
        {
            if (Quests?.Cycles == null) return;
            if (!Quests.Cycles.TryGetValue(cycleId, out var cycle)) return;

            cycle.ClaimedMilestoneIDs ??= new List<string>();
            if (!cycle.ClaimedMilestoneIDs.Contains(milestoneId))
                cycle.ClaimedMilestoneIDs.Add(milestoneId);

            OnQuestsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchQuestObjectiveProgress(QuestProgressUpdate update)
        {
            if (Quests == null) return;

            var progress = FindQuestProgress(update.QuestID, update.CycleID);
            if (progress == null) return;

            progress.Objectives ??= new Dictionary<string, UserQuestObjectiveProgress>();
            if (!progress.Objectives.TryGetValue(update.ObjectiveID, out var obj))
            {
                obj = new UserQuestObjectiveProgress { ObjectiveID = update.ObjectiveID };
                progress.Objectives[update.ObjectiveID] = obj;
            }

            obj.CurrentValue = update.NewValue;
            obj.Completed = update.ObjectiveCompleted;
            if (update.ObjectiveCompleted && obj.CompletedAtUtc == null)
                obj.CompletedAtUtc = DateTime.UtcNow;

            progress.Status = update.QuestStatus;

            OnQuestsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        private UserQuestProgress FindQuestProgress(string questId, string cycleId)
        {
            if (string.IsNullOrEmpty(cycleId))
            {
                if (Quests.PermanentQuests != null &&
                    Quests.PermanentQuests.TryGetValue(questId, out var p))
                    return p;

                return null;
            }

            if (Quests.Cycles != null &&
                Quests.Cycles.TryGetValue(cycleId, out var cycle) &&
                cycle.Quests != null &&
                cycle.Quests.TryGetValue(questId, out var cp))
                return cp;

            return null;
        }

        internal void PatchDailyReward(string calendarId, DailyRewardState data)
        {
            DailyRewards ??= new();
            DailyRewards[calendarId] = data;
            OnDailyRewardsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

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

        internal void PatchConsumedItemInstance(string itemInstanceId, long consumedAmount)
        {
            if (Inventory == null) return;
            var item = Inventory.FirstOrDefault(i => i.ItemInstanceId == itemInstanceId);
            if (item != null)
            {
                item.RemainingUses -= (int)consumedAmount;
                if (item.RemainingUses <= 0) Inventory.Remove(item);
            }
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCustomUserData(string key, string value)
        {
            CustomUserData ??= new GetCustomUserDataResult { Data = new() };
            CustomUserData.Data ??= new();
            CustomUserData.Data[key] = new UserDataRecord
            {
                Value = value,
                LastUpdated = DateTime.UtcNow
            };
            OnCustomUserDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchBoard(Action<BoardLoopState> patch)
        {
            Board ??= new BoardLoopState();
            patch(Board);
            OnBoardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Scheduled — increment token balance ----------
        internal void PatchScheduledEventTokenBalance(string eventId, long grantAmount)
        {
            LimitedTimeEvents ??= new UserLimitedTimeEventsState { ScheduledEvents = new(), EventChains = new() };
            LimitedTimeEvents.ScheduledEvents ??= new();

            if (!LimitedTimeEvents.ScheduledEvents.TryGetValue(eventId, out var progress))
            {
                progress = new UserEventProgress { EventID = eventId };
                LimitedTimeEvents.ScheduledEvents[eventId] = progress;
            }
            progress.TokenBalance += grantAmount;
            progress.TokensEarnedTotal += grantAmount;
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Scheduled — set absolute token balance (after Spend) ----------
        internal void PatchScheduledEventTokenBalanceDirect(string eventId, long newBalance)
        {
            if (LimitedTimeEvents?.ScheduledEvents == null) return;
            if (!LimitedTimeEvents.ScheduledEvents.TryGetValue(eventId, out var progress)) return;
            progress.TokenBalance = newBalance;
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Scheduled — push claimed milestone ----------
        internal void PatchScheduledEventClaimedMilestone(string eventId, string milestoneId)
        {
            if (LimitedTimeEvents?.ScheduledEvents == null) return;
            if (!LimitedTimeEvents.ScheduledEvents.TryGetValue(eventId, out var progress)) return;
            progress.ClaimedMilestoneIDs ??= new();
            if (!progress.ClaimedMilestoneIDs.Contains(milestoneId))
                progress.ClaimedMilestoneIDs.Add(milestoneId);
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Scheduled — push claimed streak day ----------
        internal void PatchScheduledEventClaimedStreakDay(string eventId, int streakDay)
        {
            if (LimitedTimeEvents?.ScheduledEvents == null) return;
            if (!LimitedTimeEvents.ScheduledEvents.TryGetValue(eventId, out var progress)) return;
            progress.ClaimedStreakDays ??= new();
            if (!progress.ClaimedStreakDays.Contains(streakDay))
                progress.ClaimedStreakDays.Add(streakDay);
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Chain — increment token balance ----------
        internal void PatchEventChainTokenBalance(string chainId, long grantAmount)
        {
            LimitedTimeEvents ??= new UserLimitedTimeEventsState { ScheduledEvents = new(), EventChains = new() };
            LimitedTimeEvents.EventChains ??= new();

            if (!LimitedTimeEvents.EventChains.TryGetValue(chainId, out var chain))
            {
                chain = new UserEventChainProgress { EventChainID = chainId };
                LimitedTimeEvents.EventChains[chainId] = chain;
            }
            chain.CurrentEventProgress ??= new UserEventProgress();
            chain.CurrentEventProgress.TokenBalance += grantAmount;
            chain.CurrentEventProgress.TokensEarnedTotal += grantAmount;
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Chain — set absolute token balance (after Spend) ----------
        internal void PatchEventChainTokenBalanceDirect(string chainId, long newBalance)
        {
            if (LimitedTimeEvents?.EventChains == null) return;
            if (!LimitedTimeEvents.EventChains.TryGetValue(chainId, out var chain)) return;
            if (chain.CurrentEventProgress == null) return;
            chain.CurrentEventProgress.TokenBalance = newBalance;
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Chain — push claimed milestone ----------
        internal void PatchEventChainClaimedMilestone(string chainId, string milestoneId)
        {
            if (LimitedTimeEvents?.EventChains == null) return;
            if (!LimitedTimeEvents.EventChains.TryGetValue(chainId, out var chain)) return;
            var progress = chain.CurrentEventProgress;
            if (progress == null) return;
            progress.ClaimedMilestoneIDs ??= new();
            if (!progress.ClaimedMilestoneIDs.Contains(milestoneId))
                progress.ClaimedMilestoneIDs.Add(milestoneId);
            OnLimitedTimeEventsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        // ---------- Patch: Chain — push claimed streak day ----------
        internal void PatchEventChainClaimedStreakDay(string chainId, int streakDay)
        {
            if (LimitedTimeEvents?.EventChains == null) return;
            if (!LimitedTimeEvents.EventChains.TryGetValue(chainId, out var chain)) return;
            var progress = chain.CurrentEventProgress;
            if (progress == null) return;
            progress.ClaimedStreakDays ??= new();
            if (!progress.ClaimedStreakDays.Contains(streakDay))
                progress.ClaimedStreakDays.Add(streakDay);
            OnLimitedTimeEventsUpdated?.Invoke();
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
