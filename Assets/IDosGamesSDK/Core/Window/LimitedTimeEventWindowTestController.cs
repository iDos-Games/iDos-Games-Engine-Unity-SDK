using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDosGames.UI.LimitedTimeEvent
{
    // ─── Serializable data objects ───────────────────────────────────────────

    [Serializable]
    public class MockRewardData
    {
        public int amount = 100;
    }

    [Serializable]
    public class MockMilestoneData
    {
        [Tooltip("Editor label only — not shown in-game")]
        public string label = "Milestone";

        [Tooltip("Tokens earned threshold required to reach this milestone")]
        [Min(1)] public int requiredTokens = 100;

        [Tooltip("Free rewards for this milestone (shown in the left/free column)")]
        public List<MockRewardData> rewards = new List<MockRewardData>();

        [Tooltip("Premium rewards for this milestone (shown in the right/premium column)")]
        public List<MockRewardData> premiumRewards = new List<MockRewardData>();

        [Tooltip("Force the FREE reward of this milestone to show as Claimed")]
        [FormerlySerializedAs("forceClaimed")]
        public bool freeClaimed = false;

        [Tooltip("Force the PREMIUM reward of this milestone to show as Claimed")]
        public bool premiumClaimed = false;
    }

    [Serializable]
    public class MockTokenSettings
    {
        public string displayName  = "Festival Coin";
        [Min(0)] public int maxBalance   = 1500;
        [Min(0)] public int dailyEarnCap = 300;
        [Min(0)] public int maxPerGrant  = 50;

        [Tooltip("Tokens in current balance (shown in progress bar)")]
        [Min(0)] public int tokenBalance = 300;

        [Tooltip("Total tokens ever earned — must be >= tokenBalance. Determines milestone unlock state.")]
        [Min(0)] public int tokensEarnedTotal = 300;

        [Tooltip("Total tokens spent (cosmetic info only)")]
        [Min(0)] public int tokensSpentTotal  = 0;
    }

    [Serializable]
    public class MockEventSettings
    {
        public string eventName    = "Summer Festival";
        public string description  = "Collect tokens to earn rewards!";

        [Tooltip("Hours from now until the event ends (negative = already ended)")]
        public float hoursUntilEnd   = 72f;

        [Tooltip("Hours since the event started")]
        [Min(0)] public float hoursSinceStart = 48f;

        public bool canEarn  = true;
        public bool canClaim = true;
    }

    [Serializable]
    public class MockPlayerSettings
    {
        public bool hasPremium = false;
        [Min(0)] public int coins = 1500;
        [Min(0)] public int gems  = 250;
    }

    // ─── Test Controller ─────────────────────────────────────────────────────

    /// <summary>
    /// Drop this on the prefab instead of the real controller to preview the UI
    /// without a server. Tweak any field and the view refreshes immediately in the Editor.
    /// Right-click the component header for Randomize / Render actions.
    /// </summary>
    [RequireComponent(typeof(LimitedTimeEventWindowView))]
    public class LimitedTimeEventWindowTestController : MonoBehaviour
    {
        [SerializeField] private LimitedTimeEventWindowView _view;

        [Header("Event")]
        [SerializeField] private MockEventSettings _event = new MockEventSettings();

        [Header("Token")]
        [SerializeField] private MockTokenSettings _token = new MockTokenSettings();

        [Header("Player")]
        [SerializeField] private MockPlayerSettings _player = new MockPlayerSettings();

        [Header("Milestones  (order = SortOrder)")]
        [SerializeField] private List<MockMilestoneData> _milestones = new List<MockMilestoneData>
        {
            new MockMilestoneData { label = "Milestone 1 (1 free / 1 premium)", requiredTokens = 100,
                rewards        = new List<MockRewardData> { new MockRewardData { amount = 300 } },
                premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 10 } } },
            new MockMilestoneData { label = "Milestone 2 (2 free / 1 premium)", requiredTokens = 250,
                rewards        = new List<MockRewardData> { new MockRewardData { amount = 600 }, new MockRewardData { amount = 150 } },
                premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 25 } } },
            new MockMilestoneData { label = "Milestone 3 (1 free / 2 premium)", requiredTokens = 500,
                rewards        = new List<MockRewardData> { new MockRewardData { amount = 1200 } },
                premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 50 }, new MockRewardData { amount = 500 } } },
            new MockMilestoneData { label = "Milestone 4 (2 free / 2 premium)", requiredTokens = 800,
                rewards        = new List<MockRewardData> { new MockRewardData { amount = 2500 }, new MockRewardData { amount = 300 } },
                premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 100 }, new MockRewardData { amount = 1000 } } },
            new MockMilestoneData { label = "Milestone 5 (3 free / 3 premium)", requiredTokens = 1200,
                rewards        = new List<MockRewardData> { new MockRewardData { amount = 5000 }, new MockRewardData { amount = 500 }, new MockRewardData { amount = 100 } },
                premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 200 }, new MockRewardData { amount = 50 }, new MockRewardData { amount = 2000 } } },
        };

        // ── Runtime ──────────────────────────────────────────────────────────

        private LimitedTimeEventWindowModel _model;
        private float _timerTick;

        private void Awake()
        {
            _model = new LimitedTimeEventWindowModel();
            if (_view == null)
                _view = GetComponent<LimitedTimeEventWindowView>();
        }

        private Task SimulateClaim(string milestoneId, bool isPremium)
        {
            for (int i = 0; i < _milestones.Count; i++)
            {
                if ($"ms_{i + 1:00}" == milestoneId)
                {
                    if (isPremium) _milestones[i].premiumClaimed = true;
                    else           _milestones[i].freeClaimed    = true;
                    break;
                }
            }
            Render();
            return Task.CompletedTask;
        }

        private void Start()  => Render();

        private void Update()
        {
            if (_model?.Event == null) return;
            _timerTick += Time.deltaTime;
            if (_timerTick < 1f) return;
            _timerTick = 0f;
            var remaining = _model.Event.ComputedEndUtc - DateTime.UtcNow;
            _view.UpdateTimer(remaining.TotalSeconds > 0 ? FormatCountdown(remaining) : "Ended");
        }

        private void OnEnable()
        {
            _view.ActivateButton?.onClick.AddListener(OnActivateClicked);
            _view.BackButton?.onClick.AddListener(OnBackClicked);
        }

        private void OnDisable()
        {
            _view.ActivateButton?.onClick.RemoveListener(OnActivateClicked);
            _view.BackButton?.onClick.RemoveListener(OnBackClicked);
        }
        private void OnActivateClicked()
        {
            // TODO: открыть экран покупки Premium Pass
            Message.Show("Premium pass activation coming soon!");
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
        }
        private void OnValidate()
        {
            if (_model == null) _model = new LimitedTimeEventWindowModel();
            if (_view   == null) _view  = GetComponent<LimitedTimeEventWindowView>();
            if (_view   == null) return;
            _model.Apply(BuildMockEvent(), _player.hasPremium, _player.coins, _player.gems);
            _view.RenderUI(_model);
        }

        // ── Context-menu actions ──────────────────────────────────────────────

        [ContextMenu("Render Mock")]
        public void Render()
        {
            if (_model == null) _model = new LimitedTimeEventWindowModel();
            _model.Apply(BuildMockEvent(), _player.hasPremium, _player.coins, _player.gems);

            var premiumIds = new List<string>();
            for (int i = 0; i < _milestones.Count; i++)
                if (_milestones[i].premiumClaimed)
                    premiumIds.Add($"ms_{i + 1:00}");
            _model.SetPremiumClaimed(premiumIds);

            _view?.Render(_model, (id, isPremium) => () => SimulateClaim(id, isPremium));
        }

        [ContextMenu("Randomize Everything")]
        public void RandomizeEverything()
        {
            var rng = new System.Random();

            _event.eventName      = RandomEventName(rng);
            _event.hoursUntilEnd  = rng.Next(1, 168);
            _event.hoursSinceStart = rng.Next(1, 72);
            _event.canEarn        = rng.Next(4) != 0;
            _event.canClaim       = rng.Next(4) != 0;

            _token.maxBalance      = rng.Next(500, 5001);
            _token.dailyEarnCap    = rng.Next(100, 1001);
            _token.maxPerGrant     = rng.Next(10, 201);
            _token.tokensEarnedTotal = rng.Next(0, _token.maxBalance + 1);
            _token.tokenBalance      = rng.Next(0, _token.tokensEarnedTotal + 1);
            _token.tokensSpentTotal  = _token.tokensEarnedTotal - _token.tokenBalance;

            _player.hasPremium = rng.Next(2) == 0;
            _player.coins      = rng.Next(0, 50001);
            _player.gems       = rng.Next(0, 5001);

            RandomizeMilestones(rng);
            Render();
        }

        [ContextMenu("Randomize Token Progress Only")]
        public void RandomizeTokenProgress()
        {
            var rng = new System.Random();
            _token.tokensEarnedTotal = rng.Next(0, _token.maxBalance + 1);
            _token.tokenBalance      = rng.Next(0, _token.tokensEarnedTotal + 1);
            _token.tokensSpentTotal  = _token.tokensEarnedTotal - _token.tokenBalance;
            Render();
        }

        [ContextMenu("Claim All Milestones")]
        public void ClaimAllMilestones()
        {
            foreach (var ms in _milestones) { ms.freeClaimed = true; ms.premiumClaimed = true; }
            _token.tokensEarnedTotal = _token.maxBalance;
            _token.tokenBalance      = _token.maxBalance;
            Render();
        }

        [ContextMenu("Reset All Milestones")]
        public void ResetAllMilestones()
        {
            foreach (var ms in _milestones) { ms.freeClaimed = false; ms.premiumClaimed = false; }
            _token.tokensEarnedTotal = 0;
            _token.tokenBalance      = 0;
            Render();
        }

        // ── Mock builders ─────────────────────────────────────────────────────

        private ActiveEventInfo BuildMockEvent()
        {
            ClampTokenValues();

            var milestones = BuildMilestones();
            var progress   = BuildProgress(milestones);
            var nextMs     = FindNextMilestone(milestones, progress.Balance?.TotalEarned ?? 0);

            return new ActiveEventInfo
            {
                Type                  = TimedEventType.Scheduled,
                TimedEventID          = "mock_event_001",
                ComputedStartUtc      = DateTime.UtcNow.AddHours(-_event.hoursSinceStart),
                ComputedEndUtc        = DateTime.UtcNow.AddHours(_event.hoursUntilEnd),
                CanEarn               = _event.canEarn,
                CanClaim              = _event.canClaim,
                NextMilestone         = nextMs,
                Content = new EventContent
                {
                    DisplayName = _event.eventName,
                    Description = _event.description,
                    ClaimMode   = EventClaimMode.Instant,
                    Milestones  = milestones.ToDictionary(m => m.MilestoneID),
                    Token = new EventTokenDefinition
                    {
                        DisplayName  = _token.displayName,
                        MaxBalance   = _token.maxBalance,
                        DailyEarnCap = _token.dailyEarnCap,
                        MaxPerGrant  = _token.maxPerGrant,
                    },
                },
                Progress = progress,
            };
        }

        private List<EventMilestoneDefinition> BuildMilestones()
        {
            var list = new List<EventMilestoneDefinition>();
            for (int i = 0; i < _milestones.Count; i++)
            {
                var m = _milestones[i];

                var freeEntries    = BuildResourceEntries(m.rewards);
                var premiumEntries = BuildResourceEntries(m.premiumRewards);

                List<PremiumTierBundle> premiumTiers = null;
                if (premiumEntries.Count > 0)
                {
                    premiumTiers = new List<PremiumTierBundle>
                    {
                        new PremiumTierBundle
                        {
                            MinPremiumTier = 1,
                            Resources = new ResourceBundle { Entries = premiumEntries }
                        }
                    };
                }

                list.Add(new EventMilestoneDefinition
                {
                    MilestoneID          = $"ms_{i + 1:00}",
                    DisplayName          = m.label,
                    RequiredTokensEarned = m.requiredTokens,
                    SortOrder            = i,
                    Rewards = new ResourceGrant
                    {
                        Standard     = new ResourceBundle { Entries = freeEntries },
                        PremiumTiers = premiumTiers,
                    },
                });
            }
            return list;
        }

        private static List<ResourceEntry> BuildResourceEntries(List<MockRewardData> rewards)
        {
            var entries = new List<ResourceEntry>();
            foreach (var r in rewards)
                entries.Add(new ResourceEntry { Amount = r.amount });
            return entries;
        }

        private UserEventTokenProgress BuildProgress(List<EventMilestoneDefinition> milestones)
        {
            var claimed = new List<string>();
            for (int i = 0; i < milestones.Count; i++)
            {
                // Только явный forceClaimed добавляет в список — достигнутые по токенам
                // остаются в состоянии Reached, кнопка Claim активна и кликабельна.
                if (_milestones[i].freeClaimed)
                    claimed.Add(milestones[i].MilestoneID);
            }

            return new UserEventTokenProgress
            {
                Balance = new EventTokenBalanceData
                {
                    Current     = _token.tokenBalance,
                    TotalEarned = _token.tokensEarnedTotal,
                    TotalSpent  = _token.tokensSpentTotal,
                },
                Daily = new EventTokenDailyData
                {
                    TotalEarned = Math.Min(_token.tokensEarnedTotal, _token.dailyEarnCap),
                },
                Milestone = new EventTokenMilestoneData
                {
                    ClaimedIDs = claimed,
                },
                Meta = new EventTokenMetaData
                {
                    JoinedAtUtc     = DateTime.UtcNow.AddHours(-_event.hoursSinceStart),
                    LastEarnedAtUtc = DateTime.UtcNow.AddMinutes(-5),
                },
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ClampTokenValues()
        {
            _token.tokensEarnedTotal = Math.Max(0, _token.tokensEarnedTotal);
            _token.tokenBalance      = Math.Min(_token.tokenBalance, _token.tokensEarnedTotal);
            _token.tokenBalance      = Math.Max(0, _token.tokenBalance);
            _token.tokensSpentTotal  = _token.tokensEarnedTotal - _token.tokenBalance;
        }

        private static EventMilestoneDefinition FindNextMilestone(
            List<EventMilestoneDefinition> milestones, long earned)
        {
            foreach (var ms in milestones)
            {
                if (earned < ms.RequiredTokensEarned)
                    return ms;
            }
            return null;
        }

        private void RandomizeMilestones(System.Random rng)
        {
            int count = rng.Next(2, 7);
            _milestones.Clear();

            int threshold = 0;
            for (int i = 0; i < count; i++)
            {
                threshold += rng.Next(50, 401);
                _milestones.Add(new MockMilestoneData
                {
                    label          = $"Milestone {i + 1}",
                    requiredTokens = threshold,
                    rewards        = new List<MockRewardData> { new MockRewardData { amount = (i + 1) * rng.Next(100, 1001) } },
                    premiumRewards = new List<MockRewardData> { new MockRewardData { amount = (i + 1) * rng.Next(5, 51) } },
                    freeClaimed    = false,
                    premiumClaimed = false,
                });
            }

            _token.maxBalance = threshold + rng.Next(0, 201);
        }

        private static string FormatCountdown(TimeSpan diff)
        {
            if (diff.TotalSeconds <= 0) return "0м";
            if (diff.TotalDays >= 7)
            {
                int weeks = (int)(diff.TotalDays / 7);
                int days  = (int)diff.TotalDays % 7;
                return $"{weeks}н {days}д";
            }
            if (diff.TotalHours >= 24)
                return $"{(int)diff.TotalDays}д {diff.Hours}ч";
            if (diff.TotalMinutes >= 60)
                return $"{(int)diff.TotalHours}ч {diff.Minutes}м";
            return $"{(int)diff.TotalMinutes}м {diff.Seconds}с";
        }

        private static string RandomEventName(System.Random rng)
        {
            string[] names =
            {
                "Summer Festival", "Winter Blitz", "Golden Rush", "Dragon Hunt",
                "Treasure Hunt", "Crystal Storm", "Inferno Week", "Lunar Carnival",
            };
            return names[rng.Next(names.Length)];
        }
    }
}
