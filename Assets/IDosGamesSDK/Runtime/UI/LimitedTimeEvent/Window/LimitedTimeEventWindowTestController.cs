using System;
using System.Collections.Generic;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using UnityEngine;

namespace IDosGames.UI.LimitedTimeEvent
{
    // ─── Serializable data objects ───────────────────────────────────────────

    [Serializable]
    public class MockMilestoneData
    {
        [Tooltip("Editor label only — not shown in-game")]
        public string label = "Milestone";

        [Tooltip("Tokens earned threshold required to reach this milestone")]
        [Min(1)] public int requiredTokens = 100;

        [Tooltip("Amount shown in the reward number text")]
        public int rewardAmount = 500;

        [Tooltip("URL or project path used to load the reward icon (leave empty = no icon)")]
        public string rewardImagePath = "";

        [Tooltip("Force this milestone to show as Claimed regardless of token progress")]
        public bool forceClaimed = false;
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

    [Serializable]
    public class MockBonusWindowSettings
    {
        public bool   isActive       = false;
        [Min(1f)] public double multiplier = 2.0;
        [Tooltip("Minutes until the bonus window closes")]
        [Min(0)] public int minutesLeft  = 30;
    }

    [Serializable]
    public class MockProgressiveMultiplierSettings
    {
        [Min(1f)] public double multiplier = 1.0;
        public string tierName = "";
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
            new MockMilestoneData { label = "Milestone 1", requiredTokens = 100,  rewardAmount = 300  },
            new MockMilestoneData { label = "Milestone 2", requiredTokens = 250,  rewardAmount = 600  },
            new MockMilestoneData { label = "Milestone 3", requiredTokens = 500,  rewardAmount = 1200 },
            new MockMilestoneData { label = "Milestone 4", requiredTokens = 800,  rewardAmount = 2500 },
            new MockMilestoneData { label = "Milestone 5", requiredTokens = 1200, rewardAmount = 5000 },
        };

        [Header("Bonus Window")]
        [SerializeField] private MockBonusWindowSettings _bonusWindow = new MockBonusWindowSettings();

        [Header("Progressive Multiplier")]
        [SerializeField] private MockProgressiveMultiplierSettings _progressiveMult
            = new MockProgressiveMultiplierSettings();

        // ── Runtime ──────────────────────────────────────────────────────────

        private LimitedTimeEventWindowModel _model;
        private float _timerTick;

        private void Awake()
        {
            _model = new LimitedTimeEventWindowModel();
            if (_view == null)
                _view = GetComponent<LimitedTimeEventWindowView>();
        }

        private void Start()  => Render();

        private void Update()
        {
            if (_model?.Event == null) return;
            _timerTick += Time.deltaTime;
            if (_timerTick < 1f) return;
            _timerTick = 0f;
            _view.UpdateTimer(TimeFormatUtil.FormatCountdown(_model.Event.ComputedEndUtc));
        }

        private void OnEnable()
        {
            _view.ActivateButton?.onClick.AddListener(OnActivateClicked);
            _view.BackButton?.onClick.AddListener(OnBackClicked);
        }
        private void OnActivateClicked()
        {
            // TODO: \u043e\u0442\u043a\u0440\u044b\u0442\u044c \u044d\u043a\u0440\u0430\u043d \u043f\u043e\u043a\u0443\u043f\u043a\u0438 Premium Pass
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
            _view?.Render(_model);
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

            _bonusWindow.isActive    = rng.Next(3) == 0;
            _bonusWindow.multiplier  = 1.5 + rng.Next(0, 6) * 0.5;
            _bonusWindow.minutesLeft = rng.Next(5, 121);

            _progressiveMult.multiplier = 1.0 + rng.Next(0, 5) * 0.25;
            _progressiveMult.tierName   = _progressiveMult.multiplier > 1.5 ? "Gold" :
                                          _progressiveMult.multiplier > 1.0 ? "Silver" : "";

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
            foreach (var ms in _milestones) ms.forceClaimed = true;
            _token.tokensEarnedTotal = _token.maxBalance;
            _token.tokenBalance      = _token.maxBalance;
            Render();
        }

        [ContextMenu("Reset All Milestones")]
        public void ResetAllMilestones()
        {
            foreach (var ms in _milestones) ms.forceClaimed = false;
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
            var nextMs     = FindNextMilestone(milestones, progress.TokensEarnedTotal);

            return new ActiveEventInfo
            {
                EventType                    = ActiveEventType.Scheduled,
                ID                           = "mock_event_001",
                ComputedStartUtc             = DateTime.UtcNow.AddHours(-_event.hoursSinceStart),
                ComputedEndUtc               = DateTime.UtcNow.AddHours(_event.hoursUntilEnd),
                CanEarn                      = _event.canEarn,
                CanClaim                     = _event.canClaim,
                BonusWindowActive            = _bonusWindow.isActive,
                BonusWindowMultiplier        = _bonusWindow.isActive ? _bonusWindow.multiplier : 1.0,
                BonusWindowEndUtc            = _bonusWindow.isActive
                    ? DateTime.UtcNow.AddMinutes(_bonusWindow.minutesLeft)
                    : (DateTime?)null,
                CurrentProgressiveMultiplier = _progressiveMult.multiplier,
                CurrentProgressiveTierName   = _progressiveMult.tierName,
                NextMilestone                = nextMs,
                Content = new EventContent
                {
                    DisplayName = _event.eventName,
                    Description = _event.description,
                    ClaimMode   = EventClaimMode.Instant,
                    Milestones  = milestones,
                    Token = new EventTokenDefinition
                    {
                        TokenID      = "MOCK_TOKEN",
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
                var paths = string.IsNullOrEmpty(m.rewardImagePath)
                    ? null
                    : new List<string> { m.rewardImagePath };

                list.Add(new EventMilestoneDefinition
                {
                    MilestoneID          = $"ms_{i + 1:00}",
                    DisplayName          = m.label,
                    RequiredTokensEarned = m.requiredTokens,
                    SortOrder            = i,
                    AssetPaths           = paths,
                    Rewards = new List<ItemOrCurrency>
                    {
                        new ItemOrCurrency
                        {
                            Amount    = m.rewardAmount,
                            ImagePath = m.rewardImagePath,
                        }
                    },
                });
            }
            return list;
        }

        private UserEventProgress BuildProgress(List<EventMilestoneDefinition> milestones)
        {
            var claimed = new List<string>();
            for (int i = 0; i < milestones.Count; i++)
            {
                var ms     = milestones[i];
                var mockMs = _milestones[i];
                bool reachedByTokens = _token.tokensEarnedTotal >= ms.RequiredTokensEarned;
                if (reachedByTokens || mockMs.forceClaimed)
                    claimed.Add(ms.MilestoneID);
            }

            return new UserEventProgress
            {
                EventID             = "mock_event_001",
                TokenBalance        = _token.tokenBalance,
                TokensEarnedTotal   = _token.tokensEarnedTotal,
                TokensSpentTotal    = _token.tokensSpentTotal,
                DailyEarnedTotal    = Math.Min(_token.tokensEarnedTotal, _token.dailyEarnCap),
                ClaimedMilestoneIDs = claimed,
                ClaimedStreakDays   = new List<int>(),
                JoinedAtUtc         = DateTime.UtcNow.AddHours(-_event.hoursSinceStart),
                LastEarnedAtUtc     = DateTime.UtcNow.AddMinutes(-5),
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
                    rewardAmount   = (i + 1) * rng.Next(100, 1001),
                    rewardImagePath = "",
                    forceClaimed   = false,
                });
            }

            _token.maxBalance = threshold + rng.Next(0, 201);
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
