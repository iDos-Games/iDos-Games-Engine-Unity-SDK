using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames.UI.Quest
{
    /// <summary>
    /// Чистая модель данных для UI квестов.
    /// Не зависит от Unity, работает только с SDK-моделями.
    /// </summary>
    public class QuestWindowModel
    {
        public bool IsLoaded { get; private set; }
        public long CoinBalance { get; set; }
        public long GemBalance { get; set; }

        public List<ActiveQuestCycle> AllCycles { get; } = new();
        public List<QuestUIItem> PermanentQuests { get; } = new();

        public void Apply(UserQuestState state, QuestDefinitions config)
        {
            IsLoaded = true;

            AllCycles.Clear();
            if (state.Cycles != null)
            {
                foreach (var cycleState in state.Cycles.Values)
                {
                    var questsInCycle = BuildQuestsForCycle(cycleState, config);
                    var cycle = new ActiveQuestCycle
                    {
                        CycleID        = cycleState.CycleID,
                        StartUtc       = cycleState.CycleStartUtc,
                        EndUtc         = cycleState.CycleEndUtc,
                        CompletedCount = cycleState.CompletedQuestsCount,
                        Quests         = questsInCycle,
                    };

                    BuildMilestonesForCycle(cycleState, config, cycle);
                    AllCycles.Add(cycle);
                }
            }

            BuildPermanentQuests(state, config);
        }

        private static List<QuestUIItem> BuildQuestsForCycle(UserQuestCycleState cycleState, QuestDefinitions config)
        {
            var result = new List<QuestUIItem>();
            if (config?.Quests == null) return result;

            foreach (var questDef in config.Quests.Values)
            {
                if (questDef.CycleIDs == null || !questDef.CycleIDs.Contains(cycleState.CycleID))
                    continue;

                UserQuestProgress progress = null;
                cycleState.Quests?.TryGetValue(questDef.QuestID, out progress);
                progress ??= new UserQuestProgress { QuestID = questDef.QuestID };

                result.Add(BuildQuestItem(progress, config, cycleState.CycleID));
            }

            result.Sort((a, b) =>
            {
                var da = config.Quests.GetValueOrDefault(a.QuestID);
                var db = config.Quests.GetValueOrDefault(b.QuestID);
                return (da?.SortOrder ?? 0).CompareTo(db?.SortOrder ?? 0);
            });

            return result;
        }

        private static void BuildMilestonesForCycle(
            UserQuestCycleState cycleState,
            QuestDefinitions config,
            ActiveQuestCycle cycle)
        {
            cycle.Milestones.Clear();

            var cycleDef = config?.Cycles?.GetValueOrDefault(cycleState.CycleID);
            if (cycleDef?.Milestones == null) return;

            foreach (var ms in cycleDef.Milestones.Values.OrderBy(m => m.RequiredCompletedQuests))
            {
                var rewards = ms.Reward?.Standard?.Entries ?? new List<ResourceEntry>();
                cycle.Milestones.Add(new MilestoneUIItem
                {
                    MilestoneID   = ms.MilestoneID,
                    CycleID       = cycleState.CycleID,
                    DisplayName   = $"Выполни {ms.RequiredCompletedQuests} квестов",
                    RequiredCount = ms.RequiredCompletedQuests,
                    Rewards       = rewards,
                    IconPath      = "",
                    IsFreeClaimed = cycleState.ClaimedMilestoneIDs?.Contains(ms.MilestoneID) == true,
                    IsReached     = cycleState.CompletedQuestsCount >= ms.RequiredCompletedQuests,
                });
            }
        }

        private void BuildPermanentQuests(UserQuestState state, QuestDefinitions config)
        {
            PermanentQuests.Clear();
            if (config?.Quests == null) return;

            foreach (var questDef in config.Quests.Values)
            {
                if (questDef.CycleIDs != null && questDef.CycleIDs.Count > 0)
                    continue;

                UserQuestProgress progress = null;
                state.PermanentQuests?.TryGetValue(questDef.QuestID, out progress);
                progress ??= new UserQuestProgress { QuestID = questDef.QuestID };

                PermanentQuests.Add(BuildQuestItem(progress, config, null));
            }

            PermanentQuests.Sort((a, b) =>
            {
                var da = config.Quests.GetValueOrDefault(a.QuestID);
                var db = config.Quests.GetValueOrDefault(b.QuestID);
                return (da?.SortOrder ?? 0).CompareTo(db?.SortOrder ?? 0);
            });
        }

        private static QuestUIItem BuildQuestItem(UserQuestProgress progress, QuestDefinitions config, string cycleId)
        {
            var questDef = config?.Quests?.GetValueOrDefault(progress.QuestID);
            var rewards  = questDef?.Reward?.Standard?.Entries ?? new List<ResourceEntry>();

            var item = new QuestUIItem
            {
                QuestID   = progress.QuestID,
                CycleID   = cycleId,
                Status    = progress.Status,
                Title     = questDef?.DisplayName ?? progress.QuestID,
                CanClaim  = progress.Status == QuestStatus.Completed,
                IsExpired = progress.Status == QuestStatus.Expired,
                Rewards   = rewards,
            };

            if (questDef?.Objectives?.Count > 0)
            {
                foreach (var objDef in questDef.Objectives.Values)
                {
                    UserQuestObjectiveProgress objProgress = null;
                    progress.Objectives?.TryGetValue(objDef.ObjectiveID, out objProgress);
                    long current   = objProgress?.CurrentValue ?? 0;
                    bool completed = objProgress?.Completed ?? false;
                    long target    = objDef.TargetValue > 0 ? objDef.TargetValue
                                     : completed ? current : System.Math.Max(current, 1L);

                    item.Objectives.Add(new ObjectiveUIItem
                    {
                        Label     = objDef.ObjectiveID,
                        Current   = current,
                        Target    = target,
                        Completed = completed,
                    });
                }
            }
            else if (progress.Objectives?.Count > 0)
            {
                foreach (var kvp in progress.Objectives)
                {
                    long target = kvp.Value.Completed ? kvp.Value.CurrentValue
                                  : System.Math.Max(kvp.Value.CurrentValue, 1L);
                    item.Objectives.Add(new ObjectiveUIItem
                    {
                        Label     = kvp.Key,
                        Current   = kvp.Value.CurrentValue,
                        Target    = target,
                        Completed = kvp.Value.Completed,
                    });
                }
            }

            if (item.Objectives.Count > 0)
            {
                item.CurrentProgress = item.Objectives.Sum(o => o.Current);
                item.TargetProgress  = item.Objectives.Sum(o => o.Target);
                item.ProgressPercent = item.TargetProgress > 0
                    ? Mathf.Clamp01((float)item.CurrentProgress / item.TargetProgress)
                    : 0f;
            }

            return item;
        }
    }

    public class ActiveQuestCycle
    {
        public string CycleID;
        public DateTime StartUtc;
        public DateTime EndUtc;
        public int CompletedCount;
        public List<QuestUIItem> Quests = new();
        public List<MilestoneUIItem> Milestones = new();

        public float SegmentProgress => Quests.Count > 0
            ? (float)Quests.Count(q => q.Status != QuestStatus.Active) / Quests.Count
            : 0f;
        public string ProgressText => $"{CompletedCount}/{Quests.Count}";
    }

    public class ObjectiveUIItem
    {
        public string Label;
        public long   Current;
        public long   Target;
        public bool   Completed;
    }

    public class QuestUIItem
    {
        public string QuestID;
        public string CycleID;
        public QuestStatus Status;
        public string Title;
        public long CurrentProgress;
        public long TargetProgress;
        public float ProgressPercent;
        public bool CanClaim;
        public bool IsExpired;
        public List<ResourceEntry>   Rewards    = new();
        public List<ObjectiveUIItem> Objectives = new();
    }

    public class MilestoneUIItem
    {
        public string MilestoneID;
        public string CycleID;
        public string DisplayName;
        public long RequiredCount;
        public List<ResourceEntry> Rewards;
        public bool IsFreeClaimed;
        public bool IsReached;
        public string IconPath;
    }
}
