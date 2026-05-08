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
    }
}
