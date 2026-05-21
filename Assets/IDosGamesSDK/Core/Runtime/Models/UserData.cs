using System;
using System.Collections.Generic;
using System.Linq;

namespace IDosGames
{
    public class UserData
    {
        public UserState State { get; private set; } = new();
        
        public event Action OnAnyUpdated;
        public event Action OnUserStateUpdated;
        public event Action OnInventoryUpdated;
        public event Action OnVirtualCurrencyUpdated;
        public event Action OnEventTokenUpdated;
        public event Action OnSocialUpdated;

        public event Action OnCharacterUpdated;
        public event Action OnTimedEventUpdated;
        public event Action OnQuestUpdated;
        public event Action OnSeasonUpdated;
        public event Action OnLeaderboardUpdated;
        public event Action OnCollectionUpdated;
        public event Action OnCoopEventUpdated;
        public event Action OnDealOfferUpdated;
        public event Action OnGameLoopUpdated;
        public event Action OnLootboxUpdated;
        public event Action OnMatchUpdated;
        public event Action OnPremiumUpdated;
        public event Action OnReferralUpdated;
        public event Action OnRewardUpdated;
        public event Action OnStoreUpdated;
        public event Action OnTimedBoostUpdated;
        public event Action OnUserCustomDataUpdated;

        private GetActiveEventsResponse _activeEventsCache;
        public GetActiveEventsResponse GetCachedActiveEvents() => _activeEventsCache;

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

        // ----- Character -----

        internal void ApplyCharacter(Dictionary<string, CharacterModel> data)
        {
            State ??= new();
            State.Character ??= new UserCharactersState();
            State.Character.Characters = data ?? new Dictionary<string, CharacterModel>();
            OnCharacterUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCharacter(string characterID, Action<CharacterModel> patch)
        {
            if (string.IsNullOrWhiteSpace(characterID) || patch == null) return;

            State ??= new();
            State.Character ??= new UserCharactersState();
            State.Character.Characters ??= new Dictionary<string, CharacterModel>();

            if (!State.Character.Characters.TryGetValue(characterID, out var character) || character == null)
            {
                character = new CharacterModel { CharacterID = characterID };
                State.Character.Characters[characterID] = character;
            }
            patch(character);
            OnCharacterUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCharacterEquipment(string characterID, string slotID, EquippedItem item)
        {
            if (string.IsNullOrWhiteSpace(characterID) || string.IsNullOrWhiteSpace(slotID)) return;
            if (State?.Character?.Characters == null
                || !State.Character.Characters.TryGetValue(characterID, out var character)
                || character == null)
            {
                return;
            }

            character.Equipment ??= new Dictionary<string, EquippedItem>();
            if (item == null) character.Equipment.Remove(slotID);
            else character.Equipment[slotID] = item;

            OnCharacterUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchUnstackableItemEquippedSlot(string itemInstanceID, EquipmentSlot slot)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceID)) return;
            if (State?.InventoryV2?.UnstackableItems == null) return;
            if (!State.InventoryV2.UnstackableItems.TryGetValue(itemInstanceID, out var inst) || inst == null) return;

            inst.EquippedSlot = slot;
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyResourceOperation(ResourceOperation op, ItemDefinitions itemDefs)
        {
            if (op == null || State == null) return;
            State.InventoryV2 ??= new UserInventoryState();
            var now = DateTime.UtcNow;

            var vcEarned = new Dictionary<string, long>();
            var vcSpent = new Dictionary<string, long>();
            var ccEarned = new Dictionary<string, decimal>();
            var ccSpent = new Dictionary<string, decimal>();
            var stackableGrant = new Dictionary<string, long>();
            var stackableConsume = new Dictionary<string, long>();
            var unstackableGrant = new Dictionary<string, long>();
            var unstackableConsume = new Dictionary<string, long>();

            for (int srcIdx = 0; ; srcIdx++)
            {
                List<ResourceEntry> entries;
                bool isConsume;

                if (srcIdx == 0)
                {
                    entries = op.Grant?.Standard?.Entries;
                    isConsume = false;
                }
                else if (op.Grant?.PremiumTiers != null && srcIdx - 1 < op.Grant.PremiumTiers.Count)
                {
                    entries = op.Grant.PremiumTiers[srcIdx - 1]?.Resources?.Entries;
                    isConsume = false;
                }
                else if (srcIdx == 1 + (op.Grant?.PremiumTiers?.Count ?? 0))
                {
                    entries = op.Consume?.Standard?.Entries;
                    isConsume = true;
                }
                else
                {
                    break;
                }

                if (entries == null) continue;

                foreach (var e in entries)
                {
                    if (e?.Type == null) continue;
                    long amt = Math.Max(0L, e.Amount ?? 0L);
                    if (amt <= 0) continue;

                    switch (e.Type)
                    {
                        case ResourceEntryType.VirtualCurrency:
                            if (string.IsNullOrEmpty(e.CurrencyID)) continue;
                            {
                                var bucket = isConsume ? vcSpent : vcEarned;
                                bucket.TryGetValue(e.CurrencyID, out var cur);
                                bucket[e.CurrencyID] = cur + amt;
                            }
                            break;

                        case ResourceEntryType.CryptoCurrency:
                            if (string.IsNullOrEmpty(e.CurrencyID)) continue;
                            {
                                var bucket = isConsume ? ccSpent : ccEarned;
                                bucket.TryGetValue(e.CurrencyID, out var cur);
                                bucket[e.CurrencyID] = cur + (decimal)amt;
                            }
                            break;

                        case ResourceEntryType.Item:
                            if (string.IsNullOrEmpty(e.ItemID)) continue;

                            bool isStackable = true;
                            if (itemDefs?.Catalogs != null)
                            {
                                ItemDefinition def = null;
                                if (!string.IsNullOrEmpty(e.CatalogID)
                                    && itemDefs.Catalogs.TryGetValue(e.CatalogID, out var cat)
                                    && cat?.Items != null)
                                {
                                    cat.Items.TryGetValue(e.ItemID, out def);
                                }
                                if (def == null)
                                {
                                    foreach (var c in itemDefs.Catalogs.Values)
                                    {
                                        if (c?.Items != null && c.Items.TryGetValue(e.ItemID, out var d) && d != null)
                                        {
                                            def = d;
                                            break;
                                        }
                                    }
                                }
                                if (def != null) isStackable = def.IsStackable;
                            }

                            {
                                Dictionary<string, long> bucket = isStackable
                                    ? (isConsume ? stackableConsume : stackableGrant)
                                    : (isConsume ? unstackableConsume : unstackableGrant);
                                bucket.TryGetValue(e.ItemID, out var cur);
                                bucket[e.ItemID] = cur + amt;
                            }
                            break;

                        case ResourceEntryType.UsdCent:
                            continue;
                    }
                }
            }

            bool vcChanged = vcEarned.Count > 0 || vcSpent.Count > 0;
            if (vcChanged)
            {
                State.InventoryV2.VirtualCurrencies ??= new();
                var allVc = new HashSet<string>(vcEarned.Keys);
                allVc.UnionWith(vcSpent.Keys);

                foreach (var id in allVc)
                {
                    vcEarned.TryGetValue(id, out var earned);
                    vcSpent.TryGetValue(id, out var spent);
                    long net = earned - spent;

                    if (!State.InventoryV2.VirtualCurrencies.TryGetValue(id, out var st) || st == null)
                    {
                        State.InventoryV2.VirtualCurrencies[id] = new UserVirtualCurrencyState
                        {
                            Amount = net,
                            CreatedAt = now,
                            UpdatedAt = now,
                            Daily = new UserDailyCounters
                            {
                                PeriodStartUtc = now.Date,
                                Earned = earned,
                                Spent = spent,
                            },
                        };
                    }
                    else
                    {
                        st.Amount += net;
                        st.UpdatedAt = now;

                        bool dailyIsToday = st.Daily != null && st.Daily.PeriodStartUtc.Date == now.Date;
                        if (dailyIsToday)
                        {
                            st.Daily.Earned += earned;
                            st.Daily.Spent += spent;
                        }
                        else
                        {
                            st.Daily = new UserDailyCounters
                            {
                                PeriodStartUtc = now.Date,
                                Earned = earned,
                                Spent = spent,
                            };
                        }
                    }
                }
            }

            bool ccChanged = ccEarned.Count > 0 || ccSpent.Count > 0;
            if (ccChanged)
            {
                State.InventoryV2.CryptoCurrencies ??= new();
                var allCc = new HashSet<string>(ccEarned.Keys);
                allCc.UnionWith(ccSpent.Keys);

                foreach (var id in allCc)
                {
                    ccEarned.TryGetValue(id, out var earned);
                    ccSpent.TryGetValue(id, out var spent);
                    decimal net = earned - spent;

                    if (!State.InventoryV2.CryptoCurrencies.TryGetValue(id, out var st) || st == null)
                    {
                        State.InventoryV2.CryptoCurrencies[id] = new UserCryptoCurrencyState
                        {
                            Amount = net,
                            Frozen = 0m,
                            CreatedAt = now,
                            UpdatedAt = now,
                        };
                    }
                    else
                    {
                        st.Amount += net;
                        st.UpdatedAt = now;
                    }
                }
            }

            bool itemsChanged = stackableGrant.Count > 0 || stackableConsume.Count > 0
                             || unstackableGrant.Count > 0 || unstackableConsume.Count > 0;
            if (itemsChanged)
            {
                State.InventoryV2.Items ??= new();
                State.InventoryV2.UnstackableItems ??= new();

                var allItemIds = new HashSet<string>();
                allItemIds.UnionWith(stackableGrant.Keys);
                allItemIds.UnionWith(stackableConsume.Keys);
                allItemIds.UnionWith(unstackableGrant.Keys);
                allItemIds.UnionWith(unstackableConsume.Keys);

                foreach (var id in allItemIds)
                {
                    stackableGrant.TryGetValue(id, out var sg);
                    stackableConsume.TryGetValue(id, out var sc);
                    unstackableGrant.TryGetValue(id, out var ug);
                    unstackableConsume.TryGetValue(id, out var uc);

                    long stNet = sg - sc;
                    long unNet = ug - uc;
                    long totalNet = stNet + unNet;

                    if (!State.InventoryV2.Items.TryGetValue(id, out var totals) || totals == null)
                    {
                        State.InventoryV2.Items[id] = new ItemTotals
                        {
                            StackableAmount = Math.Max(0, stNet),
                            UnstackableAmount = Math.Max(0, unNet),
                            TotalAmount = Math.Max(0, totalNet),
                        };
                    }
                    else
                    {
                        if (stNet != 0) totals.StackableAmount += stNet;
                        if (unNet != 0) totals.UnstackableAmount += unNet;
                        if (totalNet != 0) totals.TotalAmount += totalNet;
                    }
                }

                foreach (var kv in unstackableConsume)
                {
                    if (kv.Value <= 0) continue;
                    var picked = State.InventoryV2.UnstackableItems.Values
                        .Where(i => i != null
                            && string.Equals(i.ItemID, kv.Key, StringComparison.Ordinal)
                            && i.EquippedSlot == null
                            && (i.ExpiresAt == null || i.ExpiresAt > now))
                        .OrderBy(i => i.AcquiredAt)
                        .ThenBy(i => i.ItemInstanceID, StringComparer.Ordinal)
                        .Take((int)Math.Min(kv.Value, int.MaxValue))
                        .Select(i => i.ItemInstanceID)
                        .ToList();
                    foreach (var iid in picked)
                        State.InventoryV2.UnstackableItems.Remove(iid);
                }

                foreach (var kv in unstackableGrant)
                {
                    string catalogID = null;
                    if (itemDefs?.Catalogs != null)
                    {
                        foreach (var c in itemDefs.Catalogs)
                        {
                            if (c.Value?.Items != null && c.Value.Items.ContainsKey(kv.Key))
                            {
                                catalogID = c.Key;
                                break;
                            }
                        }
                    }

                    for (long i = 0; i < kv.Value; i++)
                    {
                        var iid = Guid.NewGuid().ToString();
                        State.InventoryV2.UnstackableItems[iid] = new UnstackableItemInstanceState
                        {
                            ItemInstanceID = iid,
                            ItemID = kv.Key,
                            CatalogID = catalogID,
                            RemainingUses = 1,
                            AcquiredAt = now,
                        };
                    }
                }
            }

            bool tokensChanged = false;
            State.EventToken ??= new UserEventTokensState();

            for (int srcIdx = 0; ; srcIdx++)
            {
                List<EventTokenOperation> tokens;
                bool isConsume;

                if (srcIdx == 0)
                {
                    tokens = op.Grant?.Standard?.EventTokens;
                    isConsume = false;
                }
                else if (op.Grant?.PremiumTiers != null && srcIdx - 1 < op.Grant.PremiumTiers.Count)
                {
                    tokens = op.Grant.PremiumTiers[srcIdx - 1]?.Resources?.EventTokens;
                    isConsume = false;
                }
                else if (srcIdx == 1 + (op.Grant?.PremiumTiers?.Count ?? 0))
                {
                    tokens = op.Consume?.Standard?.EventTokens;
                    isConsume = true;
                }
                else
                {
                    break;
                }

                if (tokens == null || tokens.Count == 0) continue;

                foreach (var t in tokens)
                {
                    if (t?.Address == null) continue;

                    long applied = t.Amount;
                    if (applied <= 0) continue;

                    Dictionary<string, UserEventTokenProgress> bucket;
                    switch (t.Address.Type)
                    {
                        case EventTokenType.TimedEventScheduled:
                            State.EventToken.TimedEvent ??= new UserTimedEventTokenState();
                            State.EventToken.TimedEvent.Scheduled ??= new();
                            bucket = State.EventToken.TimedEvent.Scheduled;
                            break;
                        case EventTokenType.TimedEventChain:
                            State.EventToken.TimedEvent ??= new UserTimedEventTokenState();
                            State.EventToken.TimedEvent.Chain ??= new();
                            bucket = State.EventToken.TimedEvent.Chain;
                            break;
                        case EventTokenType.Leaderboard:
                            State.EventToken.Leaderboard ??= new();
                            bucket = State.EventToken.Leaderboard;
                            break;
                        case EventTokenType.CoopEvent:
                            State.EventToken.CoopEvent ??= new();
                            bucket = State.EventToken.CoopEvent;
                            break;
                        case EventTokenType.Season:
                            State.EventToken.Season ??= new();
                            bucket = State.EventToken.Season;
                            break;
                        default:
                            continue;
                    }

                    if (!bucket.TryGetValue(t.Address.EntityID, out var progress) || progress == null)
                    {
                        progress = new UserEventTokenProgress();
                        bucket[t.Address.EntityID] = progress;
                    }

                    var src = string.IsNullOrEmpty(t.Source) ? "CustomAction" : t.Source;

                    if (isConsume)
                    {
                        progress.Balance.Current -= applied;
                        progress.Balance.TotalSpent += applied;
                    }
                    else
                    {
                        progress.Balance.Current += applied;
                        progress.Balance.TotalEarned += applied;

                        if (progress.Daily.Date.Date != now.Date)
                            progress.Daily = new EventTokenDailyData { Date = now.Date };

                        progress.Daily.TotalEarned += applied;

                        progress.Daily.EarnedBySource ??= new();
                        progress.Daily.EarnedBySource.TryGetValue(src, out var earnedFromSrc);
                        progress.Daily.EarnedBySource[src] = earnedFromSrc + applied;

                        progress.Daily.TriggersBySource ??= new();
                        progress.Daily.TriggersBySource.TryGetValue(src, out var triggers);
                        progress.Daily.TriggersBySource[src] = triggers + 1;

                        progress.Daily.LastTriggerBySource ??= new();
                        progress.Daily.LastTriggerBySource[src] = now;

                        if (progress.Meta.JoinedAtUtc == default) progress.Meta.JoinedAtUtc = now;
                        progress.Meta.LastEarnedAtUtc = now;
                    }

                    tokensChanged = true;
                }
            }

            if (vcChanged) OnVirtualCurrencyUpdated?.Invoke();
            if (vcChanged || ccChanged || itemsChanged) OnInventoryUpdated?.Invoke();
            if (tokensChanged) OnEventTokenUpdated?.Invoke();
            if (vcChanged || ccChanged || itemsChanged || tokensChanged) OnAnyUpdated?.Invoke();
        }

        internal void ApplySocial(UserSocialState data)
        {
            State.Social = data;
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyActiveEvents(GetActiveEventsResponse data)
        {
            _activeEventsCache = data ?? new GetActiveEventsResponse();
            OnTimedEventUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyUserLteState(UserTimedEventStateResponse data)
        {
            if (data == null) return;

            State ??= new();
            State.EventToken ??= new UserEventTokensState();
            State.EventToken.TimedEvent ??= new UserTimedEventTokenState();

            State.EventToken.TimedEvent.Scheduled = data.Scheduled ?? new();
            State.EventToken.TimedEvent.Chain = data.Chain ?? new();

            OnTimedEventUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchTimedEventMilestoneClaimed(TimedEventType type, string lteID, string milestoneID)
        {
            if (string.IsNullOrWhiteSpace(lteID) || string.IsNullOrWhiteSpace(milestoneID)) return;

            State ??= new();
            State.EventToken ??= new UserEventTokensState();
            State.EventToken.TimedEvent ??= new UserTimedEventTokenState();

            Dictionary<string, UserEventTokenProgress> dict;
            if (type == TimedEventType.Scheduled)
            {
                State.EventToken.TimedEvent.Scheduled ??= new();
                dict = State.EventToken.TimedEvent.Scheduled;
            }
            else
            {
                State.EventToken.TimedEvent.Chain ??= new();
                dict = State.EventToken.TimedEvent.Chain;
            }

            if (!dict.TryGetValue(lteID, out var progress) || progress == null)
            {
                progress = new UserEventTokenProgress();
                dict[lteID] = progress;
            }

            progress.Milestone ??= new EventTokenMilestoneData();
            progress.Milestone.ClaimedIDs ??= new();

            if (!progress.Milestone.ClaimedIDs.Contains(milestoneID))
                progress.Milestone.ClaimedIDs.Add(milestoneID);

            progress.Milestone.UnlockedIDs?.Remove(milestoneID);

            OnTimedEventUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyQuest(UserQuestState data)
        {
            State ??= new();
            State.Quest = data ?? new UserQuestState();
            OnQuestUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchQuestStatus(string questID, string cycleID, QuestStatus status)
        {
            if (string.IsNullOrWhiteSpace(questID)) return;

            State ??= new();
            State.Quest ??= new UserQuestState();

            if (string.IsNullOrWhiteSpace(cycleID))
            {
                State.Quest.PermanentQuests ??= new Dictionary<string, UserQuestProgress>();
                if (!State.Quest.PermanentQuests.TryGetValue(questID, out var progress) || progress == null)
                {
                    progress = new UserQuestProgress { QuestID = questID };
                    State.Quest.PermanentQuests[questID] = progress;
                }
                progress.Status = status;
                if (status == QuestStatus.Claimed)
                    progress.ClaimedAtUtc ??= DateTime.UtcNow;
            }
            else
            {
                State.Quest.Cycles ??= new Dictionary<string, UserQuestCycleState>();
                if (!State.Quest.Cycles.TryGetValue(cycleID, out var cycle) || cycle == null) return;

                cycle.Quests ??= new Dictionary<string, UserQuestProgress>();
                if (!cycle.Quests.TryGetValue(questID, out var progress) || progress == null)
                {
                    progress = new UserQuestProgress { QuestID = questID };
                    cycle.Quests[questID] = progress;
                }
                progress.Status = status;
                if (status == QuestStatus.Claimed)
                    progress.ClaimedAtUtc ??= DateTime.UtcNow;
            }

            OnQuestUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchMilestoneClaimed(string cycleID, string milestoneID, int completedQuestsCount)
        {
            if (string.IsNullOrWhiteSpace(cycleID) || string.IsNullOrWhiteSpace(milestoneID)) return;

            State ??= new();
            State.Quest ??= new UserQuestState();
            State.Quest.Cycles ??= new Dictionary<string, UserQuestCycleState>();

            if (!State.Quest.Cycles.TryGetValue(cycleID, out var cycle) || cycle == null) return;

            cycle.ClaimedMilestoneIDs ??= new List<string>();
            if (!cycle.ClaimedMilestoneIDs.Contains(milestoneID))
                cycle.ClaimedMilestoneIDs.Add(milestoneID);

            cycle.CompletedQuestsCount = completedQuestsCount;

            OnQuestUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchQuestProgressUpdates(List<QuestProgressUpdate> updates)
        {
            if (updates == null || updates.Count == 0) return;

            State ??= new();
            State.Quest ??= new UserQuestState();

            foreach (var upd in updates)
            {
                if (upd == null || string.IsNullOrWhiteSpace(upd.QuestID)) continue;

                UserQuestProgress progress = null;

                if (string.IsNullOrWhiteSpace(upd.CycleID))
                {
                    State.Quest.PermanentQuests ??= new Dictionary<string, UserQuestProgress>();
                    if (!State.Quest.PermanentQuests.TryGetValue(upd.QuestID, out progress) || progress == null)
                    {
                        progress = new UserQuestProgress { QuestID = upd.QuestID };
                        State.Quest.PermanentQuests[upd.QuestID] = progress;
                    }
                }
                else
                {
                    State.Quest.Cycles ??= new Dictionary<string, UserQuestCycleState>();
                    if (!State.Quest.Cycles.TryGetValue(upd.CycleID, out var cycle) || cycle == null) continue;

                    cycle.Quests ??= new Dictionary<string, UserQuestProgress>();
                    if (!cycle.Quests.TryGetValue(upd.QuestID, out progress) || progress == null)
                    {
                        progress = new UserQuestProgress { QuestID = upd.QuestID };
                        cycle.Quests[upd.QuestID] = progress;
                    }
                }

                progress.Status = upd.Status;

                if (!string.IsNullOrWhiteSpace(upd.ObjectiveID))
                {
                    progress.Objectives ??= new Dictionary<string, UserQuestObjectiveProgress>();
                    if (!progress.Objectives.TryGetValue(upd.ObjectiveID, out var obj) || obj == null)
                    {
                        obj = new UserQuestObjectiveProgress { ObjectiveID = upd.ObjectiveID };
                        progress.Objectives[upd.ObjectiveID] = obj;
                    }
                    obj.CurrentValue = upd.NewValue;
                    if (upd.ObjectiveCompleted)
                    {
                        obj.Completed = true;
                        obj.CompletedAtUtc ??= DateTime.UtcNow;
                    }
                }

                if (upd.Status == QuestStatus.Completed)
                    progress.CompletedAtUtc ??= DateTime.UtcNow;
            }

            OnQuestUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplySeasonsState(UserSeasonsState data)
        {
            State ??= new();
            State.Season = data ?? new UserSeasonsState();
            OnSeasonUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSeasonState(string chainID, UserSeasonState state)
        {
            if (string.IsNullOrWhiteSpace(chainID) || state == null) return;

            State ??= new();
            State.Season ??= new UserSeasonsState();
            State.Season.States ??= new Dictionary<string, UserSeasonState>();
            State.Season.States[chainID] = state;
            OnSeasonUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSeasonCurrentTier(string chainID, int newTier)
        {
            if (string.IsNullOrWhiteSpace(chainID)) return;

            State ??= new();
            State.Season ??= new UserSeasonsState();
            State.Season.States ??= new Dictionary<string, UserSeasonState>();

            if (!State.Season.States.TryGetValue(chainID, out var s) || s == null)
            {
                s = new UserSeasonState { SeasonChainID = chainID };
                State.Season.States[chainID] = s;
            }

            s.CurrentTier = newTier;
            OnSeasonUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSeasonClaimedTier(string chainID, int tierNumber)
        {
            if (string.IsNullOrWhiteSpace(chainID)) return;

            State ??= new();
            State.Season ??= new UserSeasonsState();
            State.Season.States ??= new Dictionary<string, UserSeasonState>();

            if (!State.Season.States.TryGetValue(chainID, out var s) || s == null)
            {
                s = new UserSeasonState { SeasonChainID = chainID };
                State.Season.States[chainID] = s;
            }

            s.ClaimedTierRewards ??= new List<int>();
            if (!s.ClaimedTierRewards.Contains(tierNumber))
                s.ClaimedTierRewards.Add(tierNumber);

            OnSeasonUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyLeaderboardsState(UserLeaderboardsState data)
        {
            State ??= new();
            State.Leaderboard = data ?? new UserLeaderboardsState();
            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLeaderboardMyProgress(string cycleDocID, GetMyProgressResponse data)
        {
            if (string.IsNullOrWhiteSpace(cycleDocID) || data == null) return;

            State ??= new();
            State.Leaderboard ??= new UserLeaderboardsState();
            State.Leaderboard.ProgressByCycle ??= new Dictionary<string, UserLeaderboardProgress>();

            if (!State.Leaderboard.ProgressByCycle.TryGetValue(cycleDocID, out var p) || p == null)
            {
                p = new UserLeaderboardProgress();
                State.Leaderboard.ProgressByCycle[cycleDocID] = p;
            }

            p.CurrentScore = data.CurrentScore;
            p.LastKnownRank = data.LastKnownRank;
            p.ScoreEarnedThisCycle = data.ScoreEarnedThisCycle;
            p.UnclaimedRewardCycleVersion = data.HasUnclaimedReward
                ? Math.Max(1, p.UnclaimedRewardCycleVersion)
                : 0;

            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLeaderboardScoreSubmitted(string cycleDocID, long newScore, long submittedScore, int cycleVersion)
        {
            if (string.IsNullOrWhiteSpace(cycleDocID)) return;

            State ??= new();
            State.Leaderboard ??= new UserLeaderboardsState();
            State.Leaderboard.ProgressByCycle ??= new Dictionary<string, UserLeaderboardProgress>();

            if (!State.Leaderboard.ProgressByCycle.TryGetValue(cycleDocID, out var p) || p == null)
            {
                p = new UserLeaderboardProgress();
                State.Leaderboard.ProgressByCycle[cycleDocID] = p;
            }

            if (p.ScoreCycleVersion < cycleVersion)
            {
                p.ScoreEarnedThisCycle = 0;
                p.ClaimedMilestoneIDs = new List<string>();
                p.BracketID = null;
            }

            p.CurrentScore = newScore;
            p.ScoreEarnedThisCycle += submittedScore;
            p.ScoreCycleVersion = cycleVersion;
            p.LastScoreSubmitUtc = DateTime.UtcNow;
            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLeaderboardCycleRewardClaimed(string cycleDocID)
        {
            if (string.IsNullOrWhiteSpace(cycleDocID)) return;

            State ??= new();
            State.Leaderboard ??= new UserLeaderboardsState();
            State.Leaderboard.ProgressByCycle ??= new Dictionary<string, UserLeaderboardProgress>();

            if (!State.Leaderboard.ProgressByCycle.TryGetValue(cycleDocID, out var p) || p == null) return;

            p.UnclaimedRewardCycleVersion = 0;
            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLeaderboardMilestoneClaimed(string cycleDocID, string milestoneID)
        {
            if (string.IsNullOrWhiteSpace(cycleDocID) || string.IsNullOrWhiteSpace(milestoneID)) return;

            State ??= new();
            State.Leaderboard ??= new UserLeaderboardsState();
            State.Leaderboard.ProgressByCycle ??= new Dictionary<string, UserLeaderboardProgress>();

            if (!State.Leaderboard.ProgressByCycle.TryGetValue(cycleDocID, out var p) || p == null)
            {
                p = new UserLeaderboardProgress();
                State.Leaderboard.ProgressByCycle[cycleDocID] = p;
            }

            p.ClaimedMilestoneIDs ??= new List<string>();
            if (!p.ClaimedMilestoneIDs.Contains(milestoneID))
                p.ClaimedMilestoneIDs.Add(milestoneID);

            OnLeaderboardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCollection(UserCollectionState data)
        {
            State ??= new();
            State.Collection = data ?? new UserCollectionState();
            OnCollectionUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCoopEvent(UserCoopEventState data)
        {
            State ??= new();
            State.CoopEvent = data ?? new UserCoopEventState { MyObjectIndex = -1 };
            OnCoopEventUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCoopEventActiveGroup(string groupID, string coopEventID, int myObjectIndex)
        {
            State ??= new();
            State.CoopEvent ??= new UserCoopEventState { MyObjectIndex = -1 };

            State.CoopEvent.ActiveGroupID = groupID;
            State.CoopEvent.ActiveCoopEventID = coopEventID;
            State.CoopEvent.MyObjectIndex = myObjectIndex;

            OnCoopEventUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyDealOffer(UserDealOffersState data)
        {
            State ??= new();
            State.DealOffer = data ?? new UserDealOffersState();
            OnDealOfferUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchDealOfferSlotDismissed(string slotID, DateTime serverTimeUtc)
        {
            if (string.IsNullOrWhiteSpace(slotID)) return;

            State ??= new();
            State.DealOffer ??= new UserDealOffersState();
            State.DealOffer.Slots ??= new();

            if (!State.DealOffer.Slots.TryGetValue(slotID, out var slot) || slot == null)
                return;

            if (slot.ActiveOffer != null)
            {
                slot.ActiveOffer.Status = DealOfferActivationStatus.Dismissed;
                slot.ActiveOffer.DismissedAtUtc = serverTimeUtc;
            }

            slot.LastDismissedAtUtc = serverTimeUtc;
            slot.LastUpdatedUtc = serverTimeUtc;
            State.DealOffer.LastUpdatedUtc = serverTimeUtc;

            OnDealOfferUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchDealOfferNodeExecuted(string slotID, string nodeID, bool nodeCompleted, bool offerExhausted, DateTime serverTimeUtc)
        {
            if (string.IsNullOrWhiteSpace(slotID) || string.IsNullOrWhiteSpace(nodeID)) return;

            State ??= new();
            State.DealOffer ??= new UserDealOffersState();
            State.DealOffer.Slots ??= new();

            if (!State.DealOffer.Slots.TryGetValue(slotID, out var slot) || slot?.ActiveOffer == null)
                return;

            var activation = slot.ActiveOffer;
            activation.Nodes ??= new();

            if (!activation.Nodes.TryGetValue(nodeID, out var nodeState) || nodeState == null)
            {
                nodeState = new UserDealNodeState { NodeID = nodeID };
                activation.Nodes[nodeID] = nodeState;
            }

            nodeState.ExecutionCount++;
            nodeState.LastExecutedAtUtc = serverTimeUtc;

            if (nodeCompleted)
            {
                nodeState.Status = DealNodeRuntimeStatus.Completed;
                nodeState.CompletedAtUtc = serverTimeUtc;
            }
            else
            {
                nodeState.Status = DealNodeRuntimeStatus.Available;
            }

            if (offerExhausted)
            {
                activation.Status = DealOfferActivationStatus.Exhausted;
                activation.ExhaustedAtUtc = serverTimeUtc;
            }

            slot.LastUpdatedUtc = serverTimeUtc;
            State.DealOffer.LastUpdatedUtc = serverTimeUtc;

            OnDealOfferUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchDealOfferShowRecorded(string slotID, DateTime serverTimeUtc)
        {
            if (string.IsNullOrWhiteSpace(slotID)) return;

            State ??= new();
            State.DealOffer ??= new UserDealOffersState();
            State.DealOffer.Slots ??= new();

            if (!State.DealOffer.Slots.TryGetValue(slotID, out var slot) || slot?.ActiveOffer == null)
                return;

            slot.ActiveOffer.ShowCount++;
            slot.ActiveOffer.LastShownAtUtc = serverTimeUtc;
            slot.LastUpdatedUtc = serverTimeUtc;
            State.DealOffer.LastUpdatedUtc = serverTimeUtc;

            OnDealOfferUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyBoardState(BoardLoopState data)
        {
            State ??= new();
            State.GameLoop ??= new UserGameLoopsState();
            State.GameLoop.Board = data ?? new BoardLoopState();
            OnGameLoopUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchBoardState(Action<BoardLoopState> patch)
        {
            if (patch == null) return;
            State ??= new();
            State.GameLoop ??= new UserGameLoopsState();
            State.GameLoop.Board ??= new BoardLoopState();
            patch(State.GameLoop.Board);
            OnGameLoopUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchBoardBuilding(int slotIndex, Action<BuildingState> patch)
        {
            if (patch == null || slotIndex < 0) return;
            State ??= new();
            State.GameLoop ??= new UserGameLoopsState();
            State.GameLoop.Board ??= new BoardLoopState();
            State.GameLoop.Board.BuildingStates ??= new();

            var building = State.GameLoop.Board.BuildingStates.Find(b => b?.SlotIndex == slotIndex);
            if (building == null)
            {
                building = new BuildingState { SlotIndex = slotIndex };
                State.GameLoop.Board.BuildingStates.Add(building);
            }

            patch(building);
            OnGameLoopUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchBoardPendingRaidLayout(List<HeistCell> updatedLayout, int openedIndex)
        {
            State ??= new();
            State.GameLoop ??= new UserGameLoopsState();
            State.GameLoop.Board ??= new BoardLoopState();

            var pending = State.GameLoop.Board.Pending;
            if (pending == null) return;

            if (updatedLayout != null)
                pending.RaidLayout = updatedLayout;

            if (openedIndex >= 0 && !(pending.OpenedIndices ??= new()).Contains(openedIndex))
                pending.OpenedIndices.Add(openedIndex);

            OnGameLoopUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyLootbox(UserLootboxState data)
        {
            State ??= new();
            State.Lootbox = data ?? new UserLootboxState();
            OnLootboxUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyLootboxPityTriggers(string lootboxID, System.Collections.Generic.List<LootboxPityTriggerResponse> triggeredPity, int count)
        {
            if (string.IsNullOrWhiteSpace(lootboxID) || count <= 0) return;

            State ??= new();
            State.Lootbox ??= new UserLootboxState();
            State.Lootbox.Pity ??= new System.Collections.Generic.Dictionary<string, UserLootboxPityCounter>();

            // —чЄтчик дл€ каждого сработавшего правила Ч сбрасываем по модулю.
            // ѕор€док обработки: сначала сработавшие (с точным newCounter = (current + count) % Threshold),
            // затем не сработавшие инкрементируютс€ на count без сброса.
            // “ак как клиент не знает Threshold, дл€ сработавших просто занул€ем счЄтчик
            if (triggeredPity != null)
            {
                var now = System.DateTime.UtcNow;
                foreach (var trigger in triggeredPity)
                {
                    if (string.IsNullOrWhiteSpace(trigger?.RuleID)) continue;
                    string key = $"{lootboxID}:{trigger.RuleID}";
                    State.Lootbox.Pity[key] = new UserLootboxPityCounter
                    {
                        OpensSinceLastTrigger = 0,
                        LastTriggeredAtUtc = now,
                    };
                }
            }

            OnLootboxUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyMatch(UserMatchState data)
        {
            State ??= new();
            State.Match = data ?? new UserMatchState();
            OnMatchUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchMatchStrategy(List<BattleStepConfig> strategy)
        {
            if (strategy == null) return;

            State ??= new();
            State.Match ??= new UserMatchState();
            State.Match.PvPBattleStrategy = strategy;
            OnMatchUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyPremium(UserPremiumState data)
        {
            State ??= new();
            State.Premium = data ?? new UserPremiumState();
            OnPremiumUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchPremiumSubscription(string premiumID, PremiumSubscription subscription)
        {
            if (string.IsNullOrWhiteSpace(premiumID) || subscription == null) return;

            State ??= new();
            State.Premium ??= new UserPremiumState();
            State.Premium.Subscriptions ??= new Dictionary<string, PremiumSubscription>();
            State.Premium.Subscriptions[premiumID] = subscription;

            OnPremiumUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyReferral(UserReferralState data)
        {
            State ??= new();
            State.Referral = data ?? new UserReferralState();
            OnReferralUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchReferralSubscription(string referralCode)
        {
            if (string.IsNullOrWhiteSpace(referralCode)) return;

            State ??= new();
            State.Referral ??= new UserReferralState();
            State.Referral.SubscribedToUserID = referralCode;
            State.Referral.UpdatedAt = DateTime.UtcNow;
            OnReferralUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchReferralInviteRewardClaimed(string rewardID)
        {
            if (string.IsNullOrWhiteSpace(rewardID)) return;

            State ??= new();
            State.Referral ??= new UserReferralState();
            State.Referral.InviteRewardStates ??= new();
            State.Referral.InviteRewardStates[rewardID] = new ReferralInviteRewardState
            {
                RewardID = rewardID,
                IsClaimed = true,
                ClaimedAt = DateTime.UtcNow,
            };
            OnReferralUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyReward(UserRewardState data)
        {
            State ??= new UserState();
            State.Reward = data ?? new UserRewardState();
            OnRewardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchDailyCalendarState(string calendarID, UserDailyCalendarState newState)
        {
            if (string.IsNullOrWhiteSpace(calendarID) || newState == null) return;

            State ??= new UserState();
            State.Reward ??= new UserRewardState();
            State.Reward.DailyCalendars ??= new Dictionary<string, UserDailyCalendarState>();
            State.Reward.DailyCalendars[calendarID] = newState;

            OnRewardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchIdleAccrualState(string accrualID, UserIdleAccrualState newState)
        {
            if (string.IsNullOrWhiteSpace(accrualID) || newState == null) return;

            State ??= new UserState();
            State.Reward ??= new UserRewardState();
            State.Reward.IdleAccruals ??= new Dictionary<string, UserIdleAccrualState>();
            State.Reward.IdleAccruals[accrualID] = newState;

            OnRewardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchComebackState(string comebackID, UserComebackState newState)
        {
            if (string.IsNullOrWhiteSpace(comebackID) || newState == null) return;

            State ??= new UserState();
            State.Reward ??= new UserRewardState();
            State.Reward.Comebacks ??= new Dictionary<string, UserComebackState>();
            State.Reward.Comebacks[comebackID] = newState;

            OnRewardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchClaimRewardState(string claimID, UserClaimRewardState newState)
        {
            if (string.IsNullOrWhiteSpace(claimID) || newState == null) return;

            State ??= new UserState();
            State.Reward ??= new UserRewardState();
            State.Reward.Claims ??= new Dictionary<string, UserClaimRewardState>();
            State.Reward.Claims[claimID] = newState;

            OnRewardUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplySocialFriendsList(List<FriendPublicProfile> friends)
        {
            State ??= new();
            State.Social ??= new UserSocialState();
            State.Social.Accepted = friends == null
                ? new List<string>()
                : friends.ConvertAll(f => f.UserID);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplySocialIncomingRequests(List<FriendPublicProfile> profiles)
        {
            State ??= new();
            State.Social ??= new UserSocialState();
            State.Social.IncomingRequests = profiles == null
                ? new List<string>()
                : profiles.ConvertAll(p => p.UserID);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplySocialTimeline(List<SocialTimelineEvent> events)
        {
            State ??= new();
            State.Social ??= new UserSocialState();
            State.Social.Timeline = events ?? new List<SocialTimelineEvent>();
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialAddOutgoingRequest(string targetUserID)
        {
            if (string.IsNullOrWhiteSpace(targetUserID)) return;
            State ??= new();
            State.Social ??= new UserSocialState();
            State.Social.OutgoingRequests ??= new List<string>();
            if (!State.Social.OutgoingRequests.Contains(targetUserID))
                State.Social.OutgoingRequests.Add(targetUserID);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialAcceptFriend(string requesterUserID)
        {
            if (string.IsNullOrWhiteSpace(requesterUserID)) return;
            State ??= new();
            State.Social ??= new UserSocialState();
            State.Social.IncomingRequests ??= new List<string>();
            State.Social.Accepted ??= new List<string>();
            State.Social.IncomingRequests.Remove(requesterUserID);
            if (!State.Social.Accepted.Contains(requesterUserID))
                State.Social.Accepted.Add(requesterUserID);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRemoveIncomingRequest(string requesterUserID)
        {
            if (string.IsNullOrWhiteSpace(requesterUserID)) return;
            State ??= new();
            State.Social ??= new UserSocialState();
            State.Social.IncomingRequests ??= new List<string>();
            State.Social.IncomingRequests.Remove(requesterUserID);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSocialRemoveFriend(string friendUserID)
        {
            if (string.IsNullOrWhiteSpace(friendUserID)) return;
            State ??= new();
            State.Social ??= new UserSocialState();
            State.Social.Accepted ??= new List<string>();
            State.Social.Accepted.Remove(friendUserID);
            OnSocialUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyStore(UserStoreState data)
        {
            State ??= new();
            State.Store = data ?? new UserStoreState();
            OnStoreUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchStorePurchase(string offerID, int count, DateTime serverTimeUtc)
        {
            if (string.IsNullOrWhiteSpace(offerID) || count < 1) return;

            State ??= new();
            State.Store ??= new UserStoreState();
            State.Store.Purchases ??= new System.Collections.Generic.Dictionary<string, StorePurchaseState>();

            if (!State.Store.Purchases.TryGetValue(offerID, out var state) || state == null)
            {
                state = new StorePurchaseState { OfferID = offerID };
                State.Store.Purchases[offerID] = state;
            }

            state.TotalPurchases += count;
            state.LastPurchasedAt = serverTimeUtc;

            // ƒейли-окно: если сброс уже прошЄл Ч начинаем новое окно.
            if (serverTimeUtc >= state.DailyResetUtc)
            {
                state.DailyPurchases = count;
                state.DailyResetUtc = serverTimeUtc.Date.AddDays(1);
            }
            else
            {
                state.DailyPurchases += count;
            }

            OnStoreUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyTimedBoost(UserTimedBoostsState data)
        {
            State ??= new();
            State.TimedBoost = data ?? new UserTimedBoostsState();
            OnTimedBoostUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchActiveTimedBoost(ActiveTimedBoost boost, TimedBoostStackingPolicy policy)
        {
            if (boost == null) return;

            State ??= new();
            State.TimedBoost ??= new UserTimedBoostsState();
            State.TimedBoost.Active ??= new System.Collections.Generic.Dictionary<string, ActiveTimedBoost>();

            // ƒл€ Replace и KeepBest на клиенте убираем прежние экземпл€ры с тем же BoostID,
            // чтобы локальный кэш не расходилс€ с серверным состо€нием.
            if (policy == TimedBoostStackingPolicy.Replace || policy == TimedBoostStackingPolicy.KeepBest)
            {
                var toRemove = new System.Collections.Generic.List<string>();
                foreach (var kv in State.TimedBoost.Active)
                {
                    if (kv.Value != null &&
                        string.Equals(kv.Value.BoostID, boost.BoostID, StringComparison.Ordinal) &&
                        !string.Equals(kv.Key, boost.InstanceID, StringComparison.Ordinal))
                    {
                        toRemove.Add(kv.Key);
                    }
                }
                foreach (var key in toRemove)
                    State.TimedBoost.Active.Remove(key);
            }

            State.TimedBoost.Active[boost.InstanceID] = boost;
            OnTimedBoostUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyUserCustomData(GetMyUserCustomDataResponse data)
        {
            State ??= new();
            State.CustomData ??= new UserCustomDataState();
            State.CustomData.Version = data.Version;
            State.CustomData.Private = data.Private ?? new Dictionary<string, UserCustomDataRecord>();
            State.CustomData.Public = data.Public ?? new Dictionary<string, UserCustomDataRecord>();
            State.CustomData.ReadOnly = data.ReadOnly ?? new Dictionary<string, UserCustomDataRecord>();
            OnUserCustomDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchUserCustomDataKey(
            CustomDataBucket bucket,
            string keyID,
            string value,
            int version,
            DateTime? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(keyID)) return;

            State ??= new();
            State.CustomData ??= new UserCustomDataState();

            var record = new UserCustomDataRecord
            {
                Value = value,
                UpdatedAt = DateTime.UtcNow,
                Version = version,
                LastWriter = CustomDataWriter.Client,
                ExpiresAt = expiresAt,
            };

            switch (bucket)
            {
                case CustomDataBucket.Private:
                    State.CustomData.Private ??= new Dictionary<string, UserCustomDataRecord>();
                    State.CustomData.Private[keyID] = record;
                    break;
                case CustomDataBucket.Public:
                    State.CustomData.Public ??= new Dictionary<string, UserCustomDataRecord>();
                    State.CustomData.Public[keyID] = record;
                    break;
                default:
                    return; // клиент патчит только Private/Public
            }

            State.CustomData.Version++;
            OnUserCustomDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void RemoveUserCustomDataKey(CustomDataBucket bucket, string keyID)
        {
            if (string.IsNullOrWhiteSpace(keyID)) return;
            if (State?.CustomData == null) return;

            bool removed = false;
            switch (bucket)
            {
                case CustomDataBucket.Private:
                    removed = State.CustomData.Private?.Remove(keyID) ?? false;
                    break;
                case CustomDataBucket.Public:
                    removed = State.CustomData.Public?.Remove(keyID) ?? false;
                    break;
            }

            if (removed)
            {
                State.CustomData.Version++;
                OnUserCustomDataUpdated?.Invoke();
                OnAnyUpdated?.Invoke();
            }
        }

        internal void PatchUnstackableItemLevel(string instanceID, int level)
        {
            if (string.IsNullOrWhiteSpace(instanceID)) return;

            State ??= new();
            State.InventoryV2 ??= new UserInventoryState();
            State.InventoryV2.UnstackableItems ??= new Dictionary<string, UnstackableItemInstanceState>();

            if (!State.InventoryV2.UnstackableItems.TryGetValue(instanceID, out var inst) || inst == null)
                return;

            inst.Level = level;
            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCryptoCurrencyDelta(string currencyID, decimal delta, DateTime updatedAt)
        {
            if (string.IsNullOrWhiteSpace(currencyID) || delta == 0m) return;

            State ??= new();
            State.InventoryV2 ??= new UserInventoryState();
            State.InventoryV2.CryptoCurrencies ??= new Dictionary<string, UserCryptoCurrencyState>();

            if (!State.InventoryV2.CryptoCurrencies.TryGetValue(currencyID, out var slot) || slot == null)
            {
                slot = new UserCryptoCurrencyState
                {
                    Amount = 0m,
                    Frozen = 0m,
                    CreatedAt = updatedAt,
                    UpdatedAt = updatedAt,
                };
                State.InventoryV2.CryptoCurrencies[currencyID] = slot;
            }

            slot.Amount += delta;
            slot.UpdatedAt = updatedAt;

            OnInventoryUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
    }
}
