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
    public class MockMilestoneData
    {
        public string milestoneId  = "ms_3";
        [Min(1)] public int requiredQuests = 3;
        public long reward = 500;
    }

    [Serializable]
    public class MockCycleSettings
    {
        public string cycleName = "Ежедневные";
        [Min(1)] public float hoursUntilReset = 18f;
        [Min(0)] public float hoursSinceStart = 6f;
        public List<MockMilestoneData> milestones = new()
        {
            new() { milestoneId = "ms_3", requiredQuests = 3, reward = 300  },
            new() { milestoneId = "ms_5", requiredQuests = 5, reward = 600  },
            new() { milestoneId = "ms_7", requiredQuests = 7, reward = 1000 },
        };
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
            // --- Активные (в процессе) ---
            new() { questId = "daily_kill_10",   title = "Уничтожь 10 врагов",      baseReward = 100,
                    objectives = new() { new() { label = "Убийства",    currentValue = 3,  targetValue = 10 } } },
            new() { questId = "daily_collect_5", title = "Собери 5 ресурсов",        baseReward = 80,
                    objectives = new() { new() { label = "Ресурсы",     currentValue = 2,  targetValue = 5  } } },
            new() { questId = "daily_win_3",     title = "Победи в 3 матчах",        baseReward = 150,
                    objectives = new() { new() { label = "Победы",      currentValue = 1,  targetValue = 3  } } },
            new() { questId = "daily_dmg_500",   title = "Нанеси 500 урона",         baseReward = 120,
                    objectives = new() { new() { label = "Урон",        currentValue = 210, targetValue = 500 } } },
            new() { questId = "daily_use_skill", title = "Используй умение 8 раз",   baseReward = 90,
                    objectives = new() { new() { label = "Умения",      currentValue = 0,  targetValue = 8  } } },

            // --- Выполнены (можно забрать награду) ---
            new() { questId = "daily_open_chest",  title = "Открой 2 сундука",       baseReward = 200,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Сундуки",     currentValue = 2,  targetValue = 2, completed = true } } },
            new() { questId = "daily_play_5",      title = "Сыграй 5 игр",           baseReward = 130,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Игры",        currentValue = 5,  targetValue = 5, completed = true } } },

            // --- Заклеймлено ---
            new() { questId = "daily_login",       title = "Войди в игру сегодня",   baseReward = 50,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Вход",        currentValue = 1,  targetValue = 1, completed = true } } },

            // --- Истёк срок ---
            new() { questId = "daily_expired",     title = "Улучши снаряжение",      baseReward = 175,
                    status = QuestStatus.Expired,
                    objectives = new() { new() { label = "Улучшения",   currentValue = 1,  targetValue = 3  } } },
        };

        [SerializeField] private List<MockQuestData> _permanentQuests = new()
        {
            // --- Долгосрочные достижения ---
            new() { questId = "perm_login_7",      title = "Войди 7 дней подряд",    baseReward = 500,
                    objectives = new() { new() { label = "Дни входа",   currentValue = 4,  targetValue = 7  } } },
            new() { questId = "perm_kill_100",     title = "Уничтожь 100 врагов",    baseReward = 1000,
                    objectives = new() { new() { label = "Убийства",    currentValue = 63, targetValue = 100 } } },
            new() { questId = "perm_reach_lvl10",  title = "Достигни 10 уровня",     baseReward = 800,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Уровень",     currentValue = 10, targetValue = 10, completed = true } } },
            new() { questId = "perm_earn_1000",    title = "Заработай 1000 монет",   baseReward = 300,
                    objectives = new() { new() { label = "Монеты",      currentValue = 740, targetValue = 1000 } } },
            new() { questId = "perm_first_win",    title = "Одержи первую победу",   baseReward = 250,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Победы",      currentValue = 1,  targetValue = 1, completed = true } } },
            new() { questId = "perm_craft_10",     title = "Скрафти 10 предметов",   baseReward = 600,
                    objectives = new() { new() { label = "Крафт",       currentValue = 3,  targetValue = 10 } } },
            new() { questId = "perm_join_guild",   title = "Вступи в гильдию",       baseReward = 400,
                    objectives = new() { new() { label = "Гильдия",     currentValue = 0,  targetValue = 1  } } },
            new() { questId = "perm_pvp_10",       title = "Проведи 10 PvP боёв",    baseReward = 750,
                    objectives = new() { new() { label = "PvP бои",     currentValue = 7,  targetValue = 10 } } },
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
            var q = AllQuests().FirstOrDefault(x => x.questId == questId);
            if (q != null) { q.status = QuestStatus.Claimed; q.forceClaimed = true; _player.coins += q.baseReward; }
            RefreshModelAndUI();
            return Task.CompletedTask;
        }

        private IEnumerable<MockQuestData> AllQuests() => _quests.Concat(_permanentQuests);

        private Task SimulateClaimMilestone(string milestoneId)
        {
            var ms = _cycle.milestones.FirstOrDefault(m => m.milestoneId == milestoneId);
            if (ms != null) _player.coins += ms.reward;
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
            foreach (var q in AllQuests())
            {
                if (q.status == QuestStatus.Claimed) continue;
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
            foreach (var q in AllQuests()) { q.status = QuestStatus.Claimed; q.forceClaimed = true; _player.coins += q.baseReward; }
            foreach (var ms in _cycle.milestones) _player.coins += ms.reward;
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

            defs.Cycles.Add(new QuestCycleDefinition
            {
                CycleID     = _cycle.cycleName,
                DisplayName = _cycle.cycleName,
                Milestones  = _cycle.milestones.Select(m => new QuestCycleMilestoneDefinition
                {
                    MilestoneID             = m.milestoneId,
                    RequiredCompletedQuests = m.requiredQuests,
                    Rewards                 = new List<ItemOrCurrency> { new() { Amount = m.reward } },
                }).ToList(),
            });

            foreach (var q in AllQuests())
            {
                defs.Quests.Add(new QuestDefinition
                {
                    QuestID     = q.questId,
                    DisplayName = q.title,
                    Rewards     = new List<ItemOrCurrency> { new() { Amount = q.baseReward } },
                    Objectives  = q.objectives.Select(o => new QuestObjectiveDefinition
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
                CycleID = _cycle.cycleName,
                CycleStartUtc = DateTime.UtcNow.AddHours(-_cycle.hoursSinceStart),
                CycleEndUtc = DateTime.UtcNow.AddHours(_cycle.hoursUntilReset),
                Quests = new Dictionary<string, UserQuestProgress>(),
                ClaimedMilestoneIDs = new List<string>(),
            };

            int completed = 0;
            foreach (var q in _quests)
            {
                cycle.Quests[q.questId] = BuildProgress(q);
                if (q.status is QuestStatus.Completed or QuestStatus.Claimed) completed++;
            }
            cycle.CompletedQuestsCount = completed;

            var permanentDict = new Dictionary<string, UserQuestProgress>();
            foreach (var q in _permanentQuests)
                permanentDict[q.questId] = BuildProgress(q);

            return new UserQuestState
            {
                Cycles = new Dictionary<string, UserQuestCycleState> { [_cycle.cycleName] = cycle },
                PermanentQuests = permanentDict,
                LastUpdatedUtc = DateTime.UtcNow,
            };
        }

        private static UserQuestProgress BuildProgress(MockQuestData q)
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
            return progress;
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