using System;
using System.Collections.Generic;
using System.Linq;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
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
                    var cycle = new ActiveQuestCycle
                    {
                        CycleID      = cycleState.CycleID,
                        StartUtc     = cycleState.CycleStartUtc,
                        EndUtc       = cycleState.CycleEndUtc,
                        CompletedCount = cycleState.CompletedQuestsCount,
                        Quests       = cycleState.Quests?.Values
                                           .Select(q => BuildQuestItem(q, config, cycleState.CycleID))
                                           .ToList() ?? new List<QuestUIItem>(),
                    };

                    BuildMilestonesForCycle(cycleState, config, cycle);
                    AllCycles.Add(cycle);
                }
            }

            BuildPermanentQuests(state, config);
        }

        private static void BuildMilestonesForCycle(
            UserQuestCycleState cycleState,
            QuestDefinitions config,
            ActiveQuestCycle cycle)
        {
            cycle.Milestones.Clear();

            var cycleDef = config?.Cycles?.FirstOrDefault(x => x.CycleID == cycleState.CycleID);
            if (cycleDef?.Milestones == null) return;

            foreach (var ms in cycleDef.Milestones.OrderBy(m => m.RequiredCompletedQuests))
            {
                cycle.Milestones.Add(new MilestoneUIItem
                {
                    MilestoneID   = ms.MilestoneID,
                    CycleID       = cycleState.CycleID,
                    DisplayName   = $"Выполни {ms.RequiredCompletedQuests} квестов",
                    RequiredCount = ms.RequiredCompletedQuests,
                    Rewards       = ms.Rewards,
                    IconPath      = "",
                    IsFreeClaimed = cycleState.ClaimedMilestoneIDs?.Contains(ms.MilestoneID) == true,
                    IsReached     = cycleState.CompletedQuestsCount >= ms.RequiredCompletedQuests,
                });
            }
        }

        private void BuildPermanentQuests(UserQuestState state, QuestDefinitions config)
        {
            PermanentQuests.Clear();
            if (state.PermanentQuests == null) return;

            foreach (var kvp in state.PermanentQuests)
                PermanentQuests.Add(BuildQuestItem(kvp.Value, config, null));
        }

        private static QuestUIItem BuildQuestItem(UserQuestProgress progress, QuestDefinitions config, string cycleId)
        {
            var questDef = config?.Quests?.FirstOrDefault(q => q.QuestID == progress.QuestID);

            var item = new QuestUIItem
            {
                QuestID    = progress.QuestID,
                CycleID    = cycleId,
                Status     = progress.Status,
                Title      = questDef?.DisplayName ?? progress.QuestID,
                CanClaim   = progress.Status == QuestStatus.Completed,
                IsExpired  = progress.Status == QuestStatus.Expired,
                Rewards    = questDef?.Rewards ?? new List<ItemOrCurrency>()
            };

            if (progress.Objectives?.Count > 0)
            {
                foreach (var kvp in progress.Objectives)
                {
                    var objDef = questDef?.Objectives?.FirstOrDefault(o => o.ObjectiveID == kvp.Key);
                    long target = objDef?.TargetValue > 0
                        ? objDef.TargetValue
                        : kvp.Value.Completed ? kvp.Value.CurrentValue : System.Math.Max(kvp.Value.CurrentValue, 1L);

                    item.Objectives.Add(new ObjectiveUIItem
                    {
                        Label     = kvp.Key,
                        Current   = kvp.Value.CurrentValue,
                        Target    = target,
                        Completed = kvp.Value.Completed,
                    });
                }

                // Keep aggregate totals for sorting
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
        public List<ItemOrCurrency>  Rewards    = new();
        public List<ObjectiveUIItem> Objectives = new();
    }

    public class MilestoneUIItem
    {
        public string MilestoneID;
        public string CycleID;
        public string DisplayName;
        public long RequiredCount;
        public List<ItemOrCurrency> Rewards;
        public bool IsFreeClaimed;
        public bool IsReached;
        public string IconPath;
    }
}
