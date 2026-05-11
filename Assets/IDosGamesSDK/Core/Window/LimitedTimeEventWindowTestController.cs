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

        [Tooltip("In FeaturedAfterEnd mode: featured milestones are only claimable after event ends")]
        public bool isFeatured = false;
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

        [Tooltip("When milestone rewards can be claimed")]
        public EventClaimMode claimMode = EventClaimMode.Instant;
    }

    [Serializable]
    public class MockPlayerSettings
    {
        public bool hasPremium = false;
        [Min(0)] public int coins = 1500;
        [Min(0)] public int gems  = 250;
    }

    [Serializable]
    public class MockEventData
    {
        public MockEventSettings Event = new MockEventSettings();
        public MockTokenSettings Token = new MockTokenSettings();

        [Tooltip("Milestones in order of SortOrder")]
        public List<MockMilestoneData> Milestones = new List<MockMilestoneData>();
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

        [Header("Player (общие для всех ивентов)")]
        [SerializeField] private MockPlayerSettings _player = new MockPlayerSettings();

        [Header("Events")]
        [SerializeField] private List<MockEventData> _events = new List<MockEventData>
        {
            // ── Event 1: Summer Festival — partial progress ───────────────────
            new MockEventData
            {
                Event  = new MockEventSettings { eventName = "Summer Festival", hoursUntilEnd = 72f, hoursSinceStart = 48f },
                Token  = new MockTokenSettings  { displayName = "Festival Coin", maxBalance = 1500, dailyEarnCap = 300, maxPerGrant = 50, tokenBalance = 300, tokensEarnedTotal = 300 },
                Milestones = new List<MockMilestoneData>
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
                },
            },

            // ── Event 2: Winter Blitz — AfterEventEnd, событие ещё активно → Reached-милестоуны = Pending ──
            new MockEventData
            {
                Event  = new MockEventSettings { eventName = "Winter Blitz", hoursUntilEnd = 24f, hoursSinceStart = 120f, canClaim = false, claimMode = EventClaimMode.AfterEventEnd },
                Token  = new MockTokenSettings  { displayName = "Ice Crystal", maxBalance = 2000, dailyEarnCap = 500, maxPerGrant = 100, tokenBalance = 1600, tokensEarnedTotal = 1600 },
                Milestones = new List<MockMilestoneData>
                {
                    new MockMilestoneData { label = "Stage 1", requiredTokens = 200,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 500 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 30 } } },
                    new MockMilestoneData { label = "Stage 2", requiredTokens = 500,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 1000 }, new MockRewardData { amount = 200 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 60 }, new MockRewardData { amount = 400 } } },
                    new MockMilestoneData { label = "Stage 3", requiredTokens = 900,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 2000 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 120 } } },
                    new MockMilestoneData { label = "Stage 4", requiredTokens = 1400,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 3500 }, new MockRewardData { amount = 500 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 200 }, new MockRewardData { amount = 1500 } } },
                    new MockMilestoneData { label = "Stage 5", requiredTokens = 2000,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 8000 }, new MockRewardData { amount = 1000 }, new MockRewardData { amount = 200 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 400 }, new MockRewardData { amount = 100 }, new MockRewardData { amount = 3000 } } },
                },
            },

            // ── Event 3: Dragon Hunt — FeaturedAfterEnd: обычные = Instant, featured = Pending пока активно ──
            new MockEventData
            {
                Event  = new MockEventSettings { eventName = "Dragon Hunt", hoursUntilEnd = 168f, hoursSinceStart = 2f, canEarn = true, canClaim = true, claimMode = EventClaimMode.FeaturedAfterEnd },
                Token  = new MockTokenSettings  { displayName = "Dragon Scale", maxBalance = 1000, dailyEarnCap = 200, maxPerGrant = 30, tokenBalance = 750, tokensEarnedTotal = 750 },
                Milestones = new List<MockMilestoneData>
                {
                    new MockMilestoneData { label = "Rank 1", requiredTokens = 100, isFeatured = false,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 200 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 15 } } },
                    new MockMilestoneData { label = "Rank 2", requiredTokens = 300, isFeatured = false,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 500 }, new MockRewardData { amount = 100 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 40 } } },
                    new MockMilestoneData { label = "Rank 3 (Featured)", requiredTokens = 600, isFeatured = true,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 1500 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 80 }, new MockRewardData { amount = 600 } } },
                    new MockMilestoneData { label = "Rank 4 (Featured)", requiredTokens = 1000, isFeatured = true,
                        rewards        = new List<MockRewardData> { new MockRewardData { amount = 4000 }, new MockRewardData { amount = 400 } },
                        premiumRewards = new List<MockRewardData> { new MockRewardData { amount = 150 }, new MockRewardData { amount = 2000 } } },
                },
            },
        };

        // ── Runtime ──────────────────────────────────────────────────────────

        private LimitedTimeEventWindowModel _model;
        private float                       _timerTick;
        private int                         _activeEventIndex;

        private void Awake()
        {
            _model = new LimitedTimeEventWindowModel();
            if (_view == null)
                _view = GetComponent<LimitedTimeEventWindowView>();
        }

        private void Start() => Render();

        private void Update()
        {
            if (_model?.Event == null) return;
            _timerTick += Time.deltaTime;
            if (_timerTick < 1f) return;
            _timerTick = 0f;
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            if (_model?.Event == null) return;
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
            Message.Show("Premium pass activation coming soon!");
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            if (_events == null || _events.Count == 0) return;
            if (_model == null) _model = new LimitedTimeEventWindowModel();
            if (_view   == null) _view  = GetComponent<LimitedTimeEventWindowView>();
            if (_view   == null) return;

            int index = Mathf.Clamp(_activeEventIndex, 0, _events.Count - 1);
            var data  = _events[index];
            _model.Apply(BuildMockEvent(data), _player.hasPremium, _player.coins, _player.gems);
            _view.RenderUI(_model);
        }

        // ── Tab switching ─────────────────────────────────────────────────────

        private void OnTabSelected(int index)
        {
            _activeEventIndex = index;
            Render();
        }

        // ── Context-menu actions ──────────────────────────────────────────────

        [ContextMenu("Render Mock")]
        public void Render()
        {
            if (_events == null || _events.Count == 0) return;
            if (_model == null) _model = new LimitedTimeEventWindowModel();

            _activeEventIndex = Mathf.Clamp(_activeEventIndex, 0, _events.Count - 1);

            // Tabs
            var names = _events.Select(e => e.Event.eventName).ToList();
            _view.RenderTabs(names, _activeEventIndex, OnTabSelected);

            // Active event
            var data = _events[_activeEventIndex];
            ClampTokenValues(data.Token);

            _model.Apply(BuildMockEvent(data), _player.hasPremium, _player.coins, _player.gems);

            var premiumIds = new List<string>();
            for (int i = 0; i < data.Milestones.Count; i++)
                if (data.Milestones[i].premiumClaimed)
                    premiumIds.Add($"ms_{i + 1:00}");
            _model.SetPremiumClaimed(premiumIds);

            _view?.Render(_model, (id, isPremium) => () => SimulateClaim(id, isPremium));
            _timerTick = 0f;
            UpdateTimerDisplay();
        }

        [ContextMenu("Randomize Everything")]
        public void RandomizeEverything()
        {
            if (_events == null || _events.Count == 0) return;
            var rng  = new System.Random();
            var data = _events[_activeEventIndex];

            data.Event.eventName      = RandomEventName(rng);
            data.Event.hoursUntilEnd  = rng.Next(1, 168);
            data.Event.hoursSinceStart = rng.Next(1, 72);
            data.Event.canEarn        = rng.Next(4) != 0;
            data.Event.canClaim       = rng.Next(4) != 0;

            data.Token.maxBalance        = rng.Next(500, 5001);
            data.Token.dailyEarnCap      = rng.Next(100, 1001);
            data.Token.maxPerGrant       = rng.Next(10, 201);
            data.Token.tokensEarnedTotal = rng.Next(0, data.Token.maxBalance + 1);
            data.Token.tokenBalance      = rng.Next(0, data.Token.tokensEarnedTotal + 1);
            data.Token.tokensSpentTotal  = data.Token.tokensEarnedTotal - data.Token.tokenBalance;

            _player.hasPremium = rng.Next(2) == 0;
            _player.coins      = rng.Next(0, 50001);
            _player.gems       = rng.Next(0, 5001);

            RandomizeMilestones(data, rng);
            Render();
        }

        [ContextMenu("Randomize Token Progress Only")]
        public void RandomizeTokenProgress()
        {
            if (_events == null || _events.Count == 0) return;
            var rng  = new System.Random();
            var token = _events[_activeEventIndex].Token;
            token.tokensEarnedTotal = rng.Next(0, token.maxBalance + 1);
            token.tokenBalance      = rng.Next(0, token.tokensEarnedTotal + 1);
            token.tokensSpentTotal  = token.tokensEarnedTotal - token.tokenBalance;
            Render();
        }

        [ContextMenu("Claim All Milestones")]
        public void ClaimAllMilestones()
        {
            if (_events == null || _events.Count == 0) return;
            var data = _events[_activeEventIndex];
            foreach (var ms in data.Milestones) { ms.freeClaimed = true; ms.premiumClaimed = true; }
            data.Token.tokensEarnedTotal = data.Token.maxBalance;
            data.Token.tokenBalance      = data.Token.maxBalance;
            Render();
        }

        [ContextMenu("Reset All Milestones")]
        public void ResetAllMilestones()
        {
            if (_events == null || _events.Count == 0) return;
            var data = _events[_activeEventIndex];
            foreach (var ms in data.Milestones) { ms.freeClaimed = false; ms.premiumClaimed = false; }
            data.Token.tokensEarnedTotal = 0;
            data.Token.tokenBalance      = 0;
            Render();
        }

        // ── Claim simulation ──────────────────────────────────────────────────

        private Task SimulateClaim(string milestoneId, bool isPremium)
        {
            if (_events == null || _events.Count == 0) return Task.CompletedTask;
            var milestones = _events[_activeEventIndex].Milestones;
            for (int i = 0; i < milestones.Count; i++)
            {
                if ($"ms_{i + 1:00}" == milestoneId)
                {
                    if (isPremium) milestones[i].premiumClaimed = true;
                    else           milestones[i].freeClaimed    = true;
                    break;
                }
            }
            Render();
            return Task.CompletedTask;
        }

        // ── Mock builders ─────────────────────────────────────────────────────

        private ActiveEventInfo BuildMockEvent(MockEventData data)
        {
            var milestones = BuildMilestones(data.Milestones);
            var progress   = BuildProgress(data);
            var nextMs     = FindNextMilestone(milestones, progress.Balance?.TotalEarned ?? 0);

            return new ActiveEventInfo
            {
                Type             = TimedEventType.Scheduled,
                TimedEventID     = $"mock_{data.Event.eventName.ToLower().Replace(" ", "_")}",
                ComputedStartUtc = DateTime.UtcNow.AddHours(-data.Event.hoursSinceStart),
                ComputedEndUtc   = DateTime.UtcNow.AddHours(data.Event.hoursUntilEnd),
                CanEarn          = data.Event.canEarn,
                CanClaim         = data.Event.canClaim,
                NextMilestone    = nextMs,
                Content = new EventContent
                {
                    DisplayName = data.Event.eventName,
                    Description = data.Event.description,
                    ClaimMode   = data.Event.claimMode,
                    Milestones  = milestones.ToDictionary(m => m.MilestoneID),
                    Token = new EventTokenDefinition
                    {
                        DisplayName  = data.Token.displayName,
                        MaxBalance   = data.Token.maxBalance,
                        DailyEarnCap = data.Token.dailyEarnCap,
                        MaxPerGrant  = data.Token.maxPerGrant,
                    },
                },
                Progress = progress,
            };
        }

        private static List<EventMilestoneDefinition> BuildMilestones(List<MockMilestoneData> milestones)
        {
            var list = new List<EventMilestoneDefinition>();
            for (int i = 0; i < milestones.Count; i++)
            {
                var m = milestones[i];
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
                    IsFeatured           = m.isFeatured,
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

        private UserEventTokenProgress BuildProgress(MockEventData data)
        {
            var claimed = new List<string>();
            for (int i = 0; i < data.Milestones.Count; i++)
                if (data.Milestones[i].freeClaimed)
                    claimed.Add($"ms_{i + 1:00}");

            return new UserEventTokenProgress
            {
                Balance = new EventTokenBalanceData
                {
                    Current     = data.Token.tokenBalance,
                    TotalEarned = data.Token.tokensEarnedTotal,
                    TotalSpent  = data.Token.tokensSpentTotal,
                },
                Daily = new EventTokenDailyData
                {
                    TotalEarned = Math.Min(data.Token.tokensEarnedTotal, data.Token.dailyEarnCap),
                },
                Milestone = new EventTokenMilestoneData
                {
                    ClaimedIDs = claimed,
                },
                Meta = new EventTokenMetaData
                {
                    JoinedAtUtc     = DateTime.UtcNow.AddHours(-data.Event.hoursSinceStart),
                    LastEarnedAtUtc = DateTime.UtcNow.AddMinutes(-5),
                },
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void ClampTokenValues(MockTokenSettings token)
        {
            token.tokensEarnedTotal = Math.Max(0, token.tokensEarnedTotal);
            token.tokenBalance      = Math.Min(token.tokenBalance, token.tokensEarnedTotal);
            token.tokenBalance      = Math.Max(0, token.tokenBalance);
            token.tokensSpentTotal  = token.tokensEarnedTotal - token.tokenBalance;
        }

        private static EventMilestoneDefinition FindNextMilestone(
            List<EventMilestoneDefinition> milestones, long earned)
        {
            foreach (var ms in milestones)
                if (earned < ms.RequiredTokensEarned)
                    return ms;
            return null;
        }

        private static void RandomizeMilestones(MockEventData data, System.Random rng)
        {
            int count = rng.Next(2, 7);
            data.Milestones.Clear();

            int threshold = 0;
            for (int i = 0; i < count; i++)
            {
                threshold += rng.Next(50, 401);
                data.Milestones.Add(new MockMilestoneData
                {
                    label          = $"Milestone {i + 1}",
                    requiredTokens = threshold,
                    rewards        = new List<MockRewardData> { new MockRewardData { amount = (i + 1) * rng.Next(100, 1001) } },
                    premiumRewards = new List<MockRewardData> { new MockRewardData { amount = (i + 1) * rng.Next(5, 51) } },
                    freeClaimed    = false,
                    premiumClaimed = false,
                });
            }

            data.Token.maxBalance = threshold + rng.Next(0, 201);
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
