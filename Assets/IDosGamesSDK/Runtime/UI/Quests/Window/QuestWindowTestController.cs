using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using UnityEngine;

namespace IDosGames.UI.Quest
{
    [Serializable]
    public class MockQuestObjective
    {
        public string label = "Убей врагов";
        [Min(0)] public int currentValue = 3;
        [Min(1)] public int targetValue = 10;
        public bool completed = false;
    }

    [Serializable]
    public class MockQuestData
    {
        public string questId = "daily_kill_10";
        public string title = "Убей 10 врагов";
        public QuestStatus status = QuestStatus.Active;
        public List<MockQuestObjective> objectives = new() { new() { currentValue = 3, targetValue = 10 } };
        public long baseReward = 100;
        public bool forceClaimed = false;
    }

    [Serializable]
    public class MockCycleSettings
    {
        public string cycleName = "Ежедневные";
        [Min(1)] public float hoursUntilReset = 18f;
        [Min(0)] public float hoursSinceStart = 6f;
        [Min(1)] public int milestoneThreshold = 3;
        public long milestoneReward = 500;
    }

    [Serializable]
    public class MockPlayerQuestSettings
    {
        public bool hasPremium = false;
        [Min(0)] public long coins = 1500;
        [Min(0)] public long gems = 250;
    }

    [RequireComponent(typeof(QuestWindowView))]
    public class QuestWindowTestController : MonoBehaviour
    {
        [SerializeField] private QuestWindowView _view;
        [SerializeField] private MockCycleSettings _cycle = new();
        [SerializeField] private List<MockQuestData> _quests = new()
        {
            new() { questId = "q1", title = "Убей 10", baseReward = 100, objectives = new() { new() { currentValue = 7, targetValue = 10 } } },
            new() { questId = "q2", title = "Собери 5", baseReward = 80, status = QuestStatus.Completed, objectives = new() { new() { currentValue = 5, targetValue = 5, completed = true } } },
        };
        [SerializeField] private MockPlayerQuestSettings _player = new();

        private QuestWindowModel _model;

        private void Awake()
        {
            _model = new QuestWindowModel();
            if (_view == null) _view = GetComponent<QuestWindowView>();
        }

        private void Start() => Render();

        private void OnEnable() => _view.BackButton?.onClick.AddListener(OnBackClicked);
        private void OnDisable() { if (_view.BackButton != null) _view.BackButton.onClick.RemoveListener(OnBackClicked); }
        private void OnBackClicked() => gameObject.SetActive(false);

        private Task SimulateClaimQuest(string questId, string cycleId)
        {
            var q = _quests.FirstOrDefault(x => x.questId == questId);
            if (q != null) { q.status = QuestStatus.Claimed; q.forceClaimed = true; _player.coins += q.baseReward; }
            RefreshModelAndUI();
            return Task.CompletedTask;
        }

        private Task SimulateClaimMilestone(string milestoneId)
        {
            _player.coins += _cycle.milestoneReward;
            foreach (var q in _quests) if (q.status == QuestStatus.Completed) q.status = QuestStatus.Claimed;
            RefreshModelAndUI();
            return Task.CompletedTask;
        }

        private void OnValidate() => RefreshModelAndUI();

        [ContextMenu("Render Mock")]
        public void Render()
        {
            UpdateModel();
            _view?.Render(_model,
                claimQuestFactory: (id, cycle) => () => SimulateClaimQuest(id, cycle),
                claimMilestoneFactory: (msId) => () => SimulateClaimMilestone(msId)
            );
        }

        [ContextMenu("Randomize")]
        public void RandomizeProgress()
        {
            var rng = new System.Random();
            foreach (var q in _quests)
            {
                foreach (var obj in q.objectives)
                {
                    obj.currentValue = rng.Next(0, obj.targetValue + 1);
                    obj.completed = obj.currentValue >= obj.targetValue;
                }
                q.status = q.objectives.All(o => o.completed) ? QuestStatus.Completed : QuestStatus.Active;
                q.forceClaimed = false;
            }
            Render();
        }

        [ContextMenu("Claim All")]
        public void ClaimAll()
        {
            foreach (var q in _quests) { q.status = QuestStatus.Claimed; q.forceClaimed = true; _player.coins += q.baseReward; }
            _player.coins += _cycle.milestoneReward;
            Render();
        }

        private void RefreshModelAndUI()
        {
            UpdateModel();
            if (_view != null) _view.RenderUI(_model);
        }

        private void UpdateModel()
        {
            if (_model == null) _model = new QuestWindowModel();
            _model.CoinBalance = _player.coins;
            _model.GemBalance = _player.gems;
            _model.Apply(BuildMockState(), BuildMockDefinitions());
        }

        private QuestDefinitions BuildMockDefinitions()
        {
            var defs = new QuestDefinitions();
            foreach (var q in _quests)
            {
                defs.Quests.Add(new QuestDefinition
                {
                    QuestID = q.questId,
                    DisplayName = q.title,
                    Rewards = new List<ItemOrCurrency> { new ItemOrCurrency { Amount = q.baseReward } },
                    Objectives = q.objectives.Select(o => new QuestObjectiveDefinition
                    {
                        ObjectiveID = o.label,
                        TargetValue = o.targetValue,
                    }).ToList(),
                });
            }
            return defs;
        }

        private UserQuestState BuildMockState()
        {
            var cycle = new UserQuestCycleState
            {
                CycleID = "daily_001",
                CycleStartUtc = DateTime.UtcNow.AddHours(-_cycle.hoursSinceStart),
                CycleEndUtc = DateTime.UtcNow.AddHours(_cycle.hoursUntilReset),
                Quests = new Dictionary<string, UserQuestProgress>(),
                ClaimedMilestoneIDs = new List<string>(),
            };

            int completed = 0;
            foreach (var q in _quests)
            {
                var progress = new UserQuestProgress
                {
                    QuestID = q.questId,
                    Status = q.status,
                    Objectives = new Dictionary<string, UserQuestObjectiveProgress>(),
                };
                foreach (var obj in q.objectives)
                {
                    progress.Objectives[obj.label] = new UserQuestObjectiveProgress
                    {
                        ObjectiveID = obj.label,
                        CurrentValue = obj.currentValue,
                        Completed = obj.completed,
                    };
                }
                cycle.Quests[q.questId] = progress;
                if (q.status is QuestStatus.Completed or QuestStatus.Claimed) completed++;
            }
            cycle.CompletedQuestsCount = completed;
            if (completed >= _cycle.milestoneThreshold) cycle.ClaimedMilestoneIDs.Add("ms_001");

            return new UserQuestState
            {
                Cycles = new Dictionary<string, UserQuestCycleState> { ["daily_001"] = cycle },
                PermanentQuests = new Dictionary<string, UserQuestProgress>(),
                LastUpdatedUtc = DateTime.UtcNow,
            };
        }

        private static string FormatCountdown(DateTime endUtc)
        {
            var diff = endUtc - DateTime.UtcNow;
            if (diff.TotalHours >= 1) return $"{(int)diff.TotalHours}ч {(int)diff.Minutes}м";
            if (diff.TotalMinutes >= 1) return $"{(int)diff.Minutes}м {(int)diff.Seconds}с";
            return $"{(int)diff.Seconds}с";
        }
    }
}