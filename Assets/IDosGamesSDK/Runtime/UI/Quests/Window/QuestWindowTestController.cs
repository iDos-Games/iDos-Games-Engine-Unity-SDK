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
        public string label              = "Убей врагов";
        [Min(0)] public int currentValue = 3;
        [Min(1)] public int targetValue  = 10;
        public bool completed            = false;
    }

    [Serializable]
    public class MockQuestData
    {
        /// <summary>CycleID цикла к которому принадлежит квест. Пустая строка = перманентный квест.</summary>
        public string cycleId            = "";
        public string questId            = "quest_id";
        public string title              = "Название квеста";
        public QuestStatus status        = QuestStatus.Active;
        public List<MockQuestObjective> objectives = new() { new() { currentValue = 3, targetValue = 10 } };
        public long baseReward           = 100;
        public List<long> extraRewards   = new();
        public bool forceClaimed         = false;
    }

    [Serializable]
    public class MockMilestoneData
    {
        public string milestoneId          = "ms_3";
        [Min(1)] public int requiredQuests = 3;
        public long reward                 = 500;
    }

    [Serializable]
    public class MockCycleSettings
    {
        public string cycleName                  = "Ежедневные";
        [Min(1)] public float hoursUntilReset    = 18f;
        [Min(0)] public float hoursSinceStart    = 6f;
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
        public bool hasPremium        = false;
        [Min(0)] public long coins    = 1500;
        [Min(0)] public long gems     = 250;
    }

    [RequireComponent(typeof(QuestWindowView))]
    public class QuestWindowTestController : MonoBehaviour
    {
        [SerializeField] private QuestWindowView _view;

        [SerializeField] private List<MockCycleSettings> _cycles = new()
        {
            // Таймер: часы и минуты (14ч 32м)
            new MockCycleSettings
            {
                cycleName       = "Ежедневные",
                hoursUntilReset = 14.5f,
                hoursSinceStart = 9.5f,
                milestones      = new()
                {
                    new() { milestoneId = "d_ms_3", requiredQuests = 3, reward = 300  },
                    new() { milestoneId = "d_ms_5", requiredQuests = 5, reward = 600  },
                    new() { milestoneId = "d_ms_7", requiredQuests = 7, reward = 1000 },
                },
            },
            // Таймер: дни и часы (3д 6ч)
            new MockCycleSettings
            {
                cycleName       = "Еженедельные",
                hoursUntilReset = 78f,
                hoursSinceStart = 90f,
                milestones      = new()
                {
                    new() { milestoneId = "w_ms_2", requiredQuests = 2, reward = 500  },
                    new() { milestoneId = "w_ms_4", requiredQuests = 4, reward = 1200 },
                    new() { milestoneId = "w_ms_6", requiredQuests = 6, reward = 2500 },
                },
            },
            // Таймер: недели и дни (2н 3д)
            new MockCycleSettings
            {
                cycleName       = "Сезонные",
                hoursUntilReset = 408f,
                hoursSinceStart = 360f,
                milestones      = new()
                {
                    new() { milestoneId = "s_ms_2", requiredQuests = 2, reward = 1000 },
                    new() { milestoneId = "s_ms_4", requiredQuests = 4, reward = 3000 },
                },
            },
            // Таймер: минуты и секунды (45м хх с) — почти истёк
            new MockCycleSettings
            {
                cycleName       = "Ивент",
                hoursUntilReset = 0.75f,
                hoursSinceStart = 167.25f,
                milestones      = new()
                {
                    new() { milestoneId = "e_ms_1", requiredQuests = 1, reward = 200  },
                    new() { milestoneId = "e_ms_3", requiredQuests = 3, reward = 800  },
                    new() { milestoneId = "e_ms_5", requiredQuests = 5, reward = 2000 },
                },
            },
        };

        /// <summary>
        /// Все квесты. cycleId="" означает перманентный квест.
        /// </summary>
        [SerializeField] private List<MockQuestData> _quests = new()
        {
            // ── Ежедневные: 2 claimed, 2 completed, 3 active, 1 expired ────────
            new() { cycleId = "Ежедневные", questId = "d_login",      title = "Войди в игру сегодня",   baseReward = 50,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Вход",      currentValue = 1,  targetValue = 1,  completed = true } } },
            new() { cycleId = "Ежедневные", questId = "d_chest",      title = "Открой 2 сундука",        baseReward = 200,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Сундуки",   currentValue = 2,  targetValue = 2,  completed = true } } },
            new() { cycleId = "Ежедневные", questId = "d_kill_10",    title = "Уничтожь 10 врагов",      baseReward = 100,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Убийства",  currentValue = 10, targetValue = 10, completed = true } } },
            new() { cycleId = "Ежедневные", questId = "d_play_5",     title = "Сыграй 5 игр",            baseReward = 130,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Игры",      currentValue = 5,  targetValue = 5,  completed = true } } },
            new() { cycleId = "Ежедневные", questId = "d_collect_5",  title = "Собери 5 ресурсов",        baseReward = 80,
                    objectives = new() { new() { label = "Ресурсы",   currentValue = 2,  targetValue = 5   } } },
            new() { cycleId = "Ежедневные", questId = "d_win_3",      title = "Победи в 3 матчах",        baseReward = 150,
                    objectives = new() { new() { label = "Победы",    currentValue = 1,  targetValue = 3   } } },
            new() { cycleId = "Ежедневные", questId = "d_dmg_500",    title = "Нанеси 500 урона",         baseReward = 120,
                    objectives = new() { new() { label = "Урон",      currentValue = 210, targetValue = 500 } } },
            new() { cycleId = "Ежедневные", questId = "d_multi_obj",  title = "Множественные цели",       baseReward = 300,
                    extraRewards = new() { 100 },
                    objectives = new() { 
                        new() { label = "Цель А", currentValue = 5, targetValue = 10 },
                        new() { label = "Цель Б", currentValue = 2, targetValue = 5 },
                        new() { label = "Цель В", currentValue = 1, targetValue = 1, completed = true }
                    } },
            new() { cycleId = "Ежедневные", questId = "d_expired",    title = "Улучши снаряжение",        baseReward = 175,
                    status = QuestStatus.Expired,
                    objectives = new() { new() { label = "Улучшения", currentValue = 1,  targetValue = 3   } } },

            // ── Еженедельные: 1 claimed, 1 completed, 4 active ──────────────────
            new() { cycleId = "Еженедельные", questId = "w_dungeon",  title = "Пройди 3 подземелья",     baseReward = 400,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Подземелья", currentValue = 3, targetValue = 3,  completed = true } } },
            new() { cycleId = "Еженедельные", questId = "w_boss",     title = "Убей 5 боссов",           baseReward = 600,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Боссы",     currentValue = 5, targetValue = 5,   completed = true } } },
            new() { cycleId = "Еженедельные", questId = "w_pvp_10",   title = "Проведи 10 PvP боёв",     baseReward = 350,
                    objectives = new() { new() { label = "PvP бои",   currentValue = 6, targetValue = 10  } } },
            new() { cycleId = "Еженедельные", questId = "w_guild",    title = "Заверши 5 заданий гильдии", baseReward = 450,
                    objectives = new() { new() { label = "Задания",   currentValue = 2, targetValue = 5   } } },
            new() { cycleId = "Еженедельные", questId = "w_craft_20", title = "Скрафти 20 предметов",    baseReward = 300,
                    objectives = new() { new() { label = "Крафт",     currentValue = 7, targetValue = 20  } } },
            new() { cycleId = "Еженедельные", questId = "w_gold",     title = "Заработай 5000 монет",    baseReward = 500,
                    objectives = new() { new() { label = "Монеты",    currentValue = 3100, targetValue = 5000 } } },

            // ── Сезонные: все claimed/completed — все майлстоуны взяты ──────────
            new() { cycleId = "Сезонные", questId = "s_story_1",  title = "Пройди главу 1",          baseReward = 1000,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Главы",    currentValue = 1, targetValue = 1, completed = true } } },
            new() { cycleId = "Сезонные", questId = "s_story_2",  title = "Пройди главу 2",          baseReward = 1000,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Главы",    currentValue = 1, targetValue = 1, completed = true } } },
            new() { cycleId = "Сезонные", questId = "s_story_3",  title = "Пройди главу 3",          baseReward = 1500,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Главы",    currentValue = 1, targetValue = 1, completed = true } } },
            new() { cycleId = "Сезонные", questId = "s_story_4",  title = "Пройди главу 4",          baseReward = 1500,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Главы",    currentValue = 1, targetValue = 1, completed = true } } },
            new() { cycleId = "Сезонные", questId = "s_legend",   title = "Достигни ранга Легенда",   baseReward = 5000,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Ранг",     currentValue = 1, targetValue = 1, completed = true } } },

            // ── Ивент: почти истёк, только 1 active, остальные expired/claimed ─
            new() { cycleId = "Ивент", questId = "e_daily_1",   title = "Собери 10 осколков",       baseReward = 150,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Осколки",  currentValue = 10, targetValue = 10, completed = true } } },
            new() { cycleId = "Ивент", questId = "e_daily_2",   title = "Победи ивент-босса",        baseReward = 300,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Победы",   currentValue = 1,  targetValue = 1,  completed = true } } },
            new() { cycleId = "Ивент", questId = "e_daily_3",   title = "Открой ивент-сундук",       baseReward = 200,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Сундуки",  currentValue = 1,  targetValue = 1,  completed = true } } },
            new() { cycleId = "Ивент", questId = "e_daily_4",   title = "Улучши ивент-броню",        baseReward = 400,
                    status = QuestStatus.Expired,
                    objectives = new() { new() { label = "Улучшения", currentValue = 0, targetValue = 3   } } },
            new() { cycleId = "Ивент", questId = "e_daily_5",   title = "Добудь 500 эфира",          baseReward = 250,
                    objectives = new() { new() { label = "Эфир",     currentValue = 380, targetValue = 500 } } },

            // ── Перманентные квесты (cycleId = "") ──────────────────────────────
            new() { cycleId = "", questId = "perm_complex",     title = "Эпическое приключение",     baseReward = 1500,
                    objectives = new() { 
                        new() { label = "Собрать ресурсы", currentValue = 15, targetValue = 50 },
                        new() { label = "Победить монстров", currentValue = 3, targetValue = 5 },
                        new() { label = "Найти артефакт", currentValue = 0, targetValue = 1 }
                    } },
            new() { cycleId = "", questId = "perm_mega",        title = "Марафон Героя",            baseReward = 2000,
                    extraRewards = new() { 500, 100, 50, 25, 10 },
                    objectives = new() { 
                        new() { label = "Шаги", currentValue = 1200, targetValue = 5000 },
                        new() { label = "Прыжки", currentValue = 45, targetValue = 100 },
                        new() { label = "Удары", currentValue = 89, targetValue = 200 },
                        new() { label = "Блоки", currentValue = 12, targetValue = 50 },
                        new() { label = "Уклонения", currentValue = 5, targetValue = 10 },
                        new() { label = "Прогресс 6", currentValue = 0, targetValue = 100 }
                    } },
            new() { cycleId = "", questId = "perm_login_7",     title = "Войди 7 дней подряд",       baseReward = 500,
                    objectives = new() { new() { label = "Дни входа",  currentValue = 4,  targetValue = 7    } } },
            new() { cycleId = "", questId = "perm_kill_100",    title = "Уничтожь 100 врагов",       baseReward = 1000,
                    objectives = new() { new() { label = "Убийства",   currentValue = 63, targetValue = 100   } } },
            new() { cycleId = "", questId = "perm_reach_lvl10", title = "Достигни 10 уровня",        baseReward = 800,
                    status = QuestStatus.Completed,
                    objectives = new() { new() { label = "Уровень",    currentValue = 10, targetValue = 10, completed = true } } },
            new() { cycleId = "", questId = "perm_earn_1000",   title = "Заработай 1000 монет",      baseReward = 300,
                    objectives = new() { new() { label = "Монеты",     currentValue = 740, targetValue = 1000  } } },
            new() { cycleId = "", questId = "perm_first_win",   title = "Одержи первую победу",      baseReward = 250,
                    status = QuestStatus.Claimed, forceClaimed = true,
                    objectives = new() { new() { label = "Победы",     currentValue = 1,  targetValue = 1, completed = true } } },
            new() { cycleId = "", questId = "perm_craft_10",    title = "Скрафти 10 предметов",      baseReward = 600,
                    objectives = new() { new() { label = "Крафт",      currentValue = 3,  targetValue = 10   } } },
            new() { cycleId = "", questId = "perm_join_guild",  title = "Вступи в гильдию",          baseReward = 400,
                    objectives = new() { new() { label = "Гильдия",    currentValue = 0,  targetValue = 1    } } },
            new() { cycleId = "", questId = "perm_pvp_10",      title = "Проведи 10 PvP боёв",       baseReward = 750,
                    objectives = new() { new() { label = "PvP бои",    currentValue = 7,  targetValue = 10   } } },
        };

        [SerializeField] private MockPlayerQuestSettings _player = new();

        private QuestWindowModel _model;
        // Key = CycleID
        private readonly Dictionary<string, HashSet<string>> _claimedMilestoneIds = new();

        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _model = new QuestWindowModel();
            if (_view == null) _view = GetComponent<QuestWindowView>();

            // Сезонные: оба майлстоуна уже взяты по умолчанию
            _claimedMilestoneIds["Сезонные"] = new HashSet<string> { "s_ms_2", "s_ms_4" };
            // Ивент: первый майлстоун взят
            _claimedMilestoneIds["Ивент"]    = new HashSet<string> { "e_ms_1" };
            // Ежедневные: первый майлстоун взят (2 claimed + 2 completed = 4 >= 3)
            _claimedMilestoneIds["Ежедневные"] = new HashSet<string> { "d_ms_3" };
        }

        private void Start()     => Render();
        private void OnEnable()  => _view.BackButton?.onClick.AddListener(OnBackClicked);
        private void OnDisable() { if (_view.BackButton != null) _view.BackButton.onClick.RemoveListener(OnBackClicked); }
        private void OnBackClicked() => gameObject.SetActive(false);

        // ─────────────────────────────────────────────────────────────

        private Task SimulateClaimQuest(string questId, string cycleId)
        {
            var q = _quests.FirstOrDefault(x => x.questId == questId);
            if (q != null) { q.status = QuestStatus.Claimed; q.forceClaimed = true; _player.coins += q.baseReward; }
            RefreshModelAndUI();
            return Task.CompletedTask;
        }

        private Task SimulateClaimMilestone(string cycleId, string milestoneId)
        {
            if (!_claimedMilestoneIds.TryGetValue(cycleId, out var claimed))
            {
                claimed = new HashSet<string>();
                _claimedMilestoneIds[cycleId] = claimed;
            }
            if (claimed.Contains(milestoneId)) return Task.CompletedTask;

            var cycle = _cycles.FirstOrDefault(c => c.cycleName == cycleId);
            var ms    = cycle?.milestones.FirstOrDefault(m => m.milestoneId == milestoneId);
            if (ms != null) { claimed.Add(milestoneId); _player.coins += ms.reward; }

            RefreshModelAndUI();
            return Task.CompletedTask;
        }

        private void RefreshModelAndUI() => Render();

        // ─────────────────────────────────────────────────────────────

        [ContextMenu("Render Mock")]
        public void Render()
        {
            UpdateModel();
            _view?.Render(_model,
                claimQuestFactory:     (id, cycleId) => () => SimulateClaimQuest(id, cycleId),
                claimMilestoneFactory: (cycleId, msId) => () => SimulateClaimMilestone(cycleId, msId)
            );
        }

        [ContextMenu("Randomize")]
        public void RandomizeProgress()
        {
            _claimedMilestoneIds.Clear();
            var rng = new System.Random();
            foreach (var q in _quests)
            {
                if (q.status == QuestStatus.Claimed) continue;
                foreach (var obj in q.objectives)
                {
                    obj.currentValue = rng.Next(0, obj.targetValue + 1);
                    obj.completed    = obj.currentValue >= obj.targetValue;
                }
                q.status       = q.objectives.All(o => o.completed) ? QuestStatus.Completed : QuestStatus.Active;
                q.forceClaimed = false;
            }
            Render();
        }

        [ContextMenu("Claim All")]
        public void ClaimAll()
        {
            foreach (var q in _quests)
            {
                q.status = QuestStatus.Claimed;
                q.forceClaimed = true;
                _player.coins += q.baseReward;
            }
            foreach (var cycle in _cycles)
                foreach (var ms in cycle.milestones)
                {
                    if (!_claimedMilestoneIds.TryGetValue(cycle.cycleName, out var set))
                    { set = new HashSet<string>(); _claimedMilestoneIds[cycle.cycleName] = set; }
                    if (set.Add(ms.milestoneId)) _player.coins += ms.reward;
                }
            Render();
        }

        // ─────────────────────────────────────────────────────────────

        private void UpdateModel()
        {
            if (_model == null) _model = new QuestWindowModel();
            _model.CoinBalance = _player.coins;
            _model.GemBalance  = _player.gems;
            _model.Apply(BuildMockState(), BuildMockDefinitions());
        }

        private QuestDefinitions BuildMockDefinitions()
        {
            var defs = new QuestDefinitions();

            foreach (var cycle in _cycles)
            {
                defs.Cycles.Add(new QuestCycleDefinition
                {
                    CycleID     = cycle.cycleName,
                    DisplayName = cycle.cycleName,
                    Milestones  = cycle.milestones.Select(m => new QuestCycleMilestoneDefinition
                    {
                        MilestoneID             = m.milestoneId,
                        RequiredCompletedQuests = m.requiredQuests,
                        Rewards                 = new List<ItemOrCurrency> { new() { Amount = m.reward } },
                    }).ToList(),
                });
            }

            foreach (var q in _quests)
            {
                var rewards = new List<ItemOrCurrency> { new() { Amount = q.baseReward, Type = ItemType.VirtualCurrency } };
                if (q.extraRewards != null)
                {
                    foreach (var extra in q.extraRewards)
                    {
                        rewards.Add(new ItemOrCurrency { Amount = extra, Type = ItemType.VirtualCurrency });
                    }
                }

                defs.Quests.Add(new QuestDefinition
                {
                    QuestID     = q.questId,
                    DisplayName = q.title,
                    Rewards     = rewards,
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
            var cyclesDict = new Dictionary<string, UserQuestCycleState>();

            foreach (var cycleSetting in _cycles)
            {
                _claimedMilestoneIds.TryGetValue(cycleSetting.cycleName, out var claimedSet);

                var cycleState = new UserQuestCycleState
                {
                    CycleID             = cycleSetting.cycleName,
                    CycleStartUtc       = DateTime.UtcNow.AddHours(-cycleSetting.hoursSinceStart),
                    CycleEndUtc         = DateTime.UtcNow.AddHours(cycleSetting.hoursUntilReset),
                    Quests              = new Dictionary<string, UserQuestProgress>(),
                    ClaimedMilestoneIDs = claimedSet?.ToList() ?? new List<string>(),
                };

                int completed = 0;
                foreach (var q in _quests.Where(q => q.cycleId == cycleSetting.cycleName))
                {
                    cycleState.Quests[q.questId] = BuildProgress(q);
                    if (q.status is QuestStatus.Completed or QuestStatus.Claimed) completed++;
                }
                cycleState.CompletedQuestsCount = completed;

                cyclesDict[cycleSetting.cycleName] = cycleState;
            }

            var permanentDict = new Dictionary<string, UserQuestProgress>();
            foreach (var q in _quests.Where(q => string.IsNullOrEmpty(q.cycleId)))
                permanentDict[q.questId] = BuildProgress(q);

            return new UserQuestState
            {
                Cycles          = cyclesDict,
                PermanentQuests = permanentDict,
                LastUpdatedUtc  = DateTime.UtcNow,
            };
        }

        private static UserQuestProgress BuildProgress(MockQuestData q)
        {
            var progress = new UserQuestProgress
            {
                QuestID    = q.questId,
                Status     = q.status,
                Objectives = new Dictionary<string, UserQuestObjectiveProgress>(),
            };
            foreach (var obj in q.objectives)
            {
                progress.Objectives[obj.label] = new UserQuestObjectiveProgress
                {
                    ObjectiveID  = obj.label,
                    CurrentValue = obj.currentValue,
                    Completed    = obj.completed,
                };
            }
            return progress;
        }
    }
}
