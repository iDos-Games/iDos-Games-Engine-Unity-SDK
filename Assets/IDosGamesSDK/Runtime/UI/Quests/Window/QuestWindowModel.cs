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
        public List<MilestoneUIItem> CycleMilestones { get; } = new();

        public float SegmentProgress => AllCycles.FirstOrDefault()?.SegmentProgress ?? 0f;
        public string ProgressText => AllCycles.FirstOrDefault()?.ProgressText ?? "0/0";
        public DateTime CycleEndUtc => AllCycles.FirstOrDefault()?.EndUtc ?? DateTime.UtcNow;

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
                        CycleID = cycleState.CycleID,
                        StartUtc = cycleState.CycleStartUtc,
                        EndUtc = cycleState.CycleEndUtc,
                        CompletedCount = cycleState.CompletedQuestsCount,
                        Quests = cycleState.Quests?.Values
                            .Select(q => BuildQuestItem(q, config))
                            .ToList() ?? new List<QuestUIItem>(),
                    };
                    AllCycles.Add(cycle);
                }

                var firstCycleState = state.Cycles.Values.FirstOrDefault();
                if (firstCycleState != null)
                    BuildMilestones(firstCycleState, config);
            }

            BuildPermanentQuests(state, config);
        }

        private void BuildMilestones(UserQuestCycleState cycleState, QuestDefinitions config)
        {
            CycleMilestones.Clear();
            // Если в конфиге появятся Milestones, раскомментируй и адаптируй под свою структуру:
            /*
            var cycleDef = config?.Cycles?.FirstOrDefault(x => x.Key == cycleState.CycleID).Value;
            if (cycleDef?.Milestones != null)
            {
                foreach (var ms in cycleDef.Milestones.OrderBy(m => m.RequiredCompletedQuests))
                {
                    CycleMilestones.Add(new MilestoneUIItem
                    {
                        MilestoneID = ms.MilestoneID,
                        DisplayName = ms.DisplayName ?? $"Milestone {ms.MilestoneID}",
                        RequiredCount = ms.RequiredCompletedQuests,
                        Rewards = ms.Rewards,
                        IconPath = ms.AssetPaths?.FirstOrDefault() ?? "",
                        IsFreeClaimed = cycleState.ClaimedMilestoneIDs?.Contains(ms.MilestoneID) == true,
                        IsReached = cycleState.CompletedQuestsCount >= ms.RequiredCompletedQuests,
                    });
                }
            }
            */
        }

        private void BuildPermanentQuests(UserQuestState state, QuestDefinitions config)
        {
            PermanentQuests.Clear();
            if (state.PermanentQuests == null) return;

            foreach (var kvp in state.PermanentQuests)
                PermanentQuests.Add(BuildQuestItem(kvp.Value, config));
        }

        private QuestUIItem BuildQuestItem(UserQuestProgress progress, QuestDefinitions config)
        {
            // 1. Ищем определение квеста в конфиге по ID
            var questDef = config?.Quests?.FirstOrDefault(q => q.QuestID == progress.QuestID);

            var item = new QuestUIItem
            {
                QuestID = progress.QuestID,
                CycleID = null, // для перманентных; для циклических передаётся извне
                Status = progress.Status,
        
                // 2. Берём тексты из конфига, с фоллбэком на ID если не найдено
                Title = questDef?.DisplayName ?? progress.QuestID,
        
                CanClaim = progress.Status == QuestStatus.Completed,
                IsExpired = progress.Status == QuestStatus.Expired,
        
                // 3. Берём награды из конфига (если есть)
                Rewards = questDef?.Rewards ?? new List<ItemOrCurrency>()
            };

            // 4. Прогресс по целям
            if (progress.Objectives?.Count > 0 && questDef?.Objectives?.Count > 0)
            {
                // Считаем общий таргет и текущий прогресс по всем целям
                var total = questDef.Objectives.Sum(obj => obj.TargetValue);
                var current = progress.Objectives.Values
                    .Sum(obj => obj.CurrentValue);
        
                item.CurrentProgress = current;
                item.TargetProgress = total;
                item.ProgressPercent = total > 0 ? Mathf.Clamp01((float)current / total) : 0f;
            }
            else if (progress.Objectives?.Count > 0)
            {
                // Fallback: если в конфиге нет целей, считаем по флагу Completed
                var total = progress.Objectives.Count;
                var current = progress.Objectives.Values.Count(obj => obj.Completed);
        
                item.CurrentProgress = current;
                item.TargetProgress = total;
                item.ProgressPercent = total > 0 ? (float)current / total : 0f;
            }

            return item;
        }

        public bool IsMilestoneFreeClaimed(string milestoneId)
            => CycleMilestones.FirstOrDefault(m => m.MilestoneID == milestoneId)?.IsFreeClaimed == true;

        public bool IsMilestoneReached(long requiredCount)
            => AllCycles.FirstOrDefault()?.CompletedCount >= requiredCount == true;
    }

    public class ActiveQuestCycle
    {
        public string CycleID;
        public DateTime StartUtc;
        public DateTime EndUtc;
        public int CompletedCount;
        public List<QuestUIItem> Quests = new();
        public float SegmentProgress => Quests.Count > 0 
            ? (float)Quests.Count(q => q.Status != QuestStatus.Active) / Quests.Count 
            : 0f;
        public string ProgressText => $"{CompletedCount}/{Quests.Count}";
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
        public List<ItemOrCurrency> Rewards;
    }

    public class MilestoneUIItem
    {
        public string MilestoneID;
        public string DisplayName;
        public long RequiredCount;
        public List<ItemOrCurrency> Rewards;
        public bool IsFreeClaimed;
        public bool IsReached;
        public string IconPath;
    }
}