using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames.UI.Leaderboard
{
    // ─────────────────────────────────────────────────────────────────────────
    // Mock data classes — fully configurable in the Inspector
    // ─────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class MockReward
    {
        public ResourceEntryType type = ResourceEntryType.VirtualCurrency;
        public string currencyID = "Coins";
        public string catalogID  = "";
        public string itemID     = "";
        [Min(0)] public long amount = 100;
    }

    [Serializable]
    public class MockLeaderboardMilestone
    {
        public string milestoneId          = "ms_500";
        [Min(1)] public long requiredScore = 500;
        public List<MockReward> rewards    = new() { new() };
        public bool isFeatured             = false;
    }

    [Serializable]
    public class MockLeaderboardRankReward
    {
        public string rankRange        = "1";
        public List<MockReward> rewards = new() { new() };
    }

    [Serializable]
    public class MockLeaderboardPlayer
    {
        public string name                     = "Player";
        [Min(0)] public long score             = 0;
        public int level                       = 1;
        public bool isPremium                  = false;
        public bool isCurrentUser              = false;
    }

    [Serializable]
    public class MockLeaderboardSettings
    {
        public string leaderboardId            = "weekly_pvp";
        public string displayName              = "Weekly PvP";
        public string scoreDisplayName         = "Points";
        public LeaderboardScoreAggregation aggregation = LeaderboardScoreAggregation.Sum;
        public LeaderboardCycleReset cycleReset         = LeaderboardCycleReset.Weekly;
        public EventClaimMode milestoneClaimMode        = EventClaimMode.Instant;
        [Min(0)] public float hoursUntilReset           = 72f;

        public List<MockLeaderboardMilestone> milestones = new()
        {
            new() { milestoneId = "ms_500",  requiredScore = 500,  rewards = new() { new() { currencyID = "Coins", amount = 300  } } },
            new() { milestoneId = "ms_1000", requiredScore = 1000, rewards = new() { new() { currencyID = "Coins", amount = 500  }, new() { currencyID = "Gems", amount = 10 } } },
            new() { milestoneId = "ms_2500", requiredScore = 2500, rewards = new() { new() { currencyID = "Coins", amount = 1000 }, new() { currencyID = "Gems", amount = 25 } } },
        };

        public List<MockLeaderboardRankReward> rankRewards = new()
        {
            new() { rankRange = "1",     rewards = new() { new() { currencyID = "Coins", amount = 5000 }, new() { currencyID = "Gems", amount = 100 } } },
            new() { rankRange = "2-3",   rewards = new() { new() { currencyID = "Coins", amount = 2500 }, new() { currencyID = "Gems", amount = 50  } } },
            new() { rankRange = "4-10",  rewards = new() { new() { currencyID = "Coins", amount = 1000 } } },
            new() { rankRange = "11-50", rewards = new() { new() { currencyID = "Coins", amount = 300  } } },
        };

        public List<MockLeaderboardPlayer> players = new();

        [Header("Current Player State")]
        [Min(0)] public long myCurrentScore         = 0;
        [Min(0)] public long myScoreEarnedThisCycle = 0;
        [Min(0)] public int  myLastKnownRank        = 0;
        public bool hasUnclaimedReward              = false;
    }

    [Serializable]
    public class MockPlayerSettings
    {
        [Min(0)] public long coins = 1500;
        [Min(0)] public long gems  = 250;
        public string userId       = "player_local";
        public string displayName  = "You";
    }

    // ─────────────────────────────────────────────────────────────────────────

    [RequireComponent(typeof(LeaderboardView))]
    public class LeaderboardTestController : MonoBehaviour
    {
        [SerializeField] private LeaderboardView _view;

        [SerializeField] private List<MockLeaderboardSettings> _leaderboards = new()
        {
            // ── Weekly PvP — 50 players, user at rank 12 ─────────────────
            new MockLeaderboardSettings
            {
                leaderboardId          = "weekly_pvp",
                displayName            = "Weekly PvP",
                scoreDisplayName       = "Points",
                aggregation            = LeaderboardScoreAggregation.Sum,
                cycleReset             = LeaderboardCycleReset.Weekly,
                hoursUntilReset        = 54f,
                myCurrentScore         = 1200,
                myScoreEarnedThisCycle = 1200,
                myLastKnownRank        = 8,
                hasUnclaimedReward     = false,
                milestones = new()
                {
                    new() { milestoneId = "w_ms_500",  requiredScore = 500,  rewards = new() { new() { currencyID = "Coins", amount = 300  } } },
                    new() { milestoneId = "w_ms_1000", requiredScore = 1000, rewards = new() { new() { currencyID = "Coins", amount = 500  }, new() { currencyID = "Gems", amount = 10 } } },
                    new() { milestoneId = "w_ms_2500", requiredScore = 2500, rewards = new() { new() { currencyID = "Coins", amount = 1500 }, new() { currencyID = "Gems", amount = 25 }, new() { type = ResourceEntryType.Item, catalogID = "main", itemID = "power_gem", amount = 1 } } },
                },
                rankRewards = new()
                {
                    new() { rankRange = "1",     rewards = new() { new() { currencyID = "Coins", amount = 5000 }, new() { currencyID = "Gems", amount = 100 }, new() { type = ResourceEntryType.Item, catalogID = "main", itemID = "champion_chest", amount = 1 } } },
                    new() { rankRange = "2-3",   rewards = new() { new() { currencyID = "Coins", amount = 2500 }, new() { currencyID = "Gems", amount = 50  } } },
                    new() { rankRange = "4-10",  rewards = new() { new() { currencyID = "Coins", amount = 1000 } } },
                    new() { rankRange = "11-50", rewards = new() { new() { currencyID = "Coins", amount = 300  } } },
                },
                players = new()
                {
                    // Ranks 1-11 (above user)
                    new() { name = "ShadowBlade",  score = 4200, level = 42, isPremium = true  },
                    new() { name = "NightFury",    score = 3800, level = 38, isPremium = true  },
                    new() { name = "IronReaper",   score = 3100, level = 35                    },
                    new() { name = "StormCaller",  score = 2700, level = 31                    },
                    new() { name = "VoidWalker",   score = 2200, level = 29, isPremium = true  },
                    new() { name = "DarkNova",     score = 1650, level = 27                    },
                    new() { name = "CrystalWing",  score = 1380, level = 25                    },
                    new() { name = "ThunderPeak",  score = 1120, level = 24                    },
                    new() { name = "FrostClaw",    score =  980, level = 23                    },
                    new() { name = "PhoenixEdge",  score =  920, level = 23                    },
                    new() { name = "BlazeFang",    score =  875, level = 22                    },
                    // Rank 8 — current user
                    new() { name = "You",          score = 1200, level = 22, isCurrentUser = true },
                    // Ranks 13-50
                    new() { name = "CrimsonEdge",  score =  720, level = 20                    },
                    new() { name = "FrostBite",    score =  640, level = 18                    },
                    new() { name = "BlazePeak",    score =  510, level = 15                    },
                    new() { name = "DuskRaven",    score =  390, level = 12                    },
                    new() { name = "SwiftDrift",   score =  360, level = 11                    },
                    new() { name = "NeonStrike",   score =  330, level = 11                    },
                    new() { name = "GoldRush",     score =  300, level = 10                    },
                    new() { name = "SilverFang",   score =  275, level = 10                    },
                    new() { name = "VenomTide",    score =  250, level =  9                    },
                    new() { name = "DragonWave",   score =  225, level =  9                    },
                    new() { name = "FlameSpike",   score =  200, level =  8                    },
                    new() { name = "GhostHunter",  score =  185, level =  8                    },
                    new() { name = "NebulaEdge",   score =  170, level =  8                    },
                    new() { name = "CosmicBlade",  score =  155, level =  7                    },
                    new() { name = "StarForge",    score =  142, level =  7                    },
                    new() { name = "MoonRider",    score =  130, level =  7                    },
                    new() { name = "SunStrike",    score =  119, level =  6                    },
                    new() { name = "PixelDrift",   score =  109, level =  6                    },
                    new() { name = "ByteHunter",   score =  100, level =  6                    },
                    new() { name = "CodeRunner",   score =   92, level =  5                    },
                    new() { name = "RuneKeeper",   score =   85, level =  5                    },
                    new() { name = "WaveRider",    score =   79, level =  5                    },
                    new() { name = "TideBreaker",  score =   73, level =  5                    },
                    new() { name = "StoneEdge",    score =   68, level =  4                    },
                    new() { name = "IceWalker",    score =   63, level =  4                    },
                    new() { name = "FireDancer",   score =   58, level =  4                    },
                    new() { name = "WindRider",    score =   54, level =  4                    },
                    new() { name = "EarthMover",   score =   50, level =  3                    },
                    new() { name = "LightSeeker",  score =   46, level =  3                    },
                    new() { name = "DarkHunter",   score =   43, level =  3                    },
                    new() { name = "TimeWalker",   score =   40, level =  3                    },
                    new() { name = "SpaceEdge",    score =   37, level =  2                    },
                    new() { name = "VoidChaser",   score =   34, level =  2                    },
                    new() { name = "ChaosEdge",    score =   31, level =  2                    },
                    new() { name = "NullDrift",    score =   28, level =  2                    },
                    new() { name = "ZeroStrike",   score =   25, level =  1                    },
                    new() { name = "TechHunter",   score =   20, level =  1                    },
                    new() { name = "BaseRookie",   score =   15, level =  1                    },
                },
            },

            // ── Daily Speed — 50 игроков, текущий игрок НЕ в топ-50 (rank 73) ──
            new MockLeaderboardSettings
            {
                leaderboardId          = "daily_speed",
                displayName            = "Daily Speed",
                scoreDisplayName       = "ms",
                aggregation            = LeaderboardScoreAggregation.BestTime,
                cycleReset             = LeaderboardCycleReset.Daily,
                milestoneClaimMode     = EventClaimMode.FeaturedAfterEnd,
                hoursUntilReset        = 8.5f,
                myCurrentScore         = 5500,
                myScoreEarnedThisCycle = 5500,
                myLastKnownRank        = 73,
                hasUnclaimedReward     = false,
                // FeaturedAfterEnd: обычные клеймятся сразу, featured — ждёт конца цикла
                // score 5500 → все три майлстоуна достигнуты:
                //   d_ms_2000 → Claimed, d_ms_4000 → Reached (кнопка), d_ms_5000 → Pending ⏳⭐
                milestones = new()
                {
                    new() { milestoneId = "d_ms_2000", requiredScore = 2000, rewards = new() { new() { currencyID = "Coins", amount = 200  } } },
                    new() { milestoneId = "d_ms_4000", requiredScore = 4000, rewards = new() { new() { currencyID = "Coins", amount = 500  } } },
                    new() { milestoneId = "d_ms_5000", requiredScore = 5000, rewards = new() { new() { currencyID = "Coins", amount = 800  }, new() { currencyID = "Gems", amount = 20 }, new() { type = ResourceEntryType.Item, catalogID = "main", itemID = "speed_charm", amount = 1 } }, isFeatured = true },
                },
                rankRewards = new()
                {
                    new() { rankRange = "1",    rewards = new() { new() { currencyID = "Coins", amount = 2000 }, new() { currencyID = "Gems", amount = 50 } } },
                    new() { rankRange = "2-5",  rewards = new() { new() { currencyID = "Coins", amount = 800  } } },
                    new() { rankRange = "6-20", rewards = new() { new() { currencyID = "Coins", amount = 200  } } },
                },
                // Все 50 игроков быстрее текущего (3240ms) — он на rank 73
                players = new()
                {
                    new() { name = "SpeedDemon",   score = 1820, level = 50, isPremium = true  },
                    new() { name = "QuickSilver",  score = 1850, level = 44, isPremium = true  },
                    new() { name = "BlitzKing",    score = 1890, level = 42                    },
                    new() { name = "TurboAce",     score = 1930, level = 40                    },
                    new() { name = "RocketFox",    score = 1970, level = 39                    },
                    new() { name = "DashPro",      score = 2010, level = 38                    },
                    new() { name = "FlashBolt",    score = 2050, level = 37                    },
                    new() { name = "ThunderRun",   score = 2090, level = 36, isPremium = true  },
                    new() { name = "LightFoot",    score = 2130, level = 35                    },
                    new() { name = "WindSprint",   score = 2170, level = 34                    },
                    new() { name = "SlipStream",   score = 2200, level = 33                    },
                    new() { name = "BreezeDash",   score = 2240, level = 32                    },
                    new() { name = "StormSurge",   score = 2280, level = 31                    },
                    new() { name = "GaleForce",    score = 2320, level = 30                    },
                    new() { name = "WhirlWind",    score = 2360, level = 29                    },
                    new() { name = "CycloneFly",   score = 2400, level = 28                    },
                    new() { name = "VortexRun",    score = 2440, level = 27                    },
                    new() { name = "ZephyrRace",   score = 2480, level = 26                    },
                    new() { name = "NimbusDash",   score = 2520, level = 25                    },
                    new() { name = "RushHour",     score = 2560, level = 24, isPremium = true  },
                    new() { name = "CloudBurst",   score = 2600, level = 23                    },
                    new() { name = "SwiftFalcon",  score = 2640, level = 23                    },
                    new() { name = "HailDash",     score = 2680, level = 22                    },
                    new() { name = "SnowBolt",     score = 2720, level = 21                    },
                    new() { name = "FrostRun",     score = 2760, level = 20                    },
                    new() { name = "IceDash",      score = 2800, level = 20                    },
                    new() { name = "ArcticSprint", score = 2830, level = 19                    },
                    new() { name = "PolarDash",    score = 2860, level = 19                    },
                    new() { name = "TundraRun",    score = 2890, level = 18                    },
                    new() { name = "GlacierRace",  score = 2920, level = 18                    },
                    new() { name = "AlpineSprint", score = 2950, level = 17                    },
                    new() { name = "MountainDash", score = 2980, level = 17                    },
                    new() { name = "HillStrike",   score = 3000, level = 16                    },
                    new() { name = "ValleyRun",    score = 3020, level = 16                    },
                    new() { name = "PlainsSprint", score = 3040, level = 15                    },
                    new() { name = "DesertDash",   score = 3060, level = 15                    },
                    new() { name = "SandStorm",    score = 3080, level = 14                    },
                    new() { name = "DustRunner",   score = 3100, level = 14                    },
                    new() { name = "OasisSprint",  score = 3110, level = 13                    },
                    new() { name = "MirageRace",   score = 3120, level = 13                    },
                    new() { name = "DuneDash",     score = 3130, level = 12                    },
                    new() { name = "CanyonRun",    score = 3140, level = 12                    },
                    new() { name = "MesaSprint",   score = 3150, level = 11                    },
                    new() { name = "RiftDash",     score = 3160, level = 11                    },
                    new() { name = "FaultRun",     score = 3170, level = 10                    },
                    new() { name = "TectonicRace", score = 3180, level = 10                    },
                    new() { name = "CraterDash",   score = 3190, level =  9                    },
                    new() { name = "VolcanoRun",   score = 3200, level =  9                    },
                    new() { name = "LavaSprint",   score = 3215, level =  8                    },
                    new() { name = "MagmaDash",    score = 5480, level =  8                    },
                    // Rank 51 — current user (за топ-50)
                    new() { name = "You",          score = 5500, level = 22, isCurrentUser = true },
                },
            },

            // ── All-Time Wins — 50 игроков, текущий игрок на rank 4 ───────
            new MockLeaderboardSettings
            {
                leaderboardId          = "alltime_wins",
                displayName            = "All-Time Wins",
                scoreDisplayName       = "Wins",
                aggregation            = LeaderboardScoreAggregation.Sum,
                cycleReset             = LeaderboardCycleReset.Never,
                milestoneClaimMode     = EventClaimMode.AfterEventEnd,
                hoursUntilReset        = 0f,
                myCurrentScore         = 6000,
                myScoreEarnedThisCycle = 6000,
                myLastKnownRank        = 4,
                hasUnclaimedReward     = false,
                // AfterEventEnd + Never (вечный LB считается завершённым) → все достигнутые клеймятся
                // score 6000:
                //   at_ms_100  → Claimed, at_ms_500 → Claimed,
                //   at_ms_1000 → Reached (кнопка), at_ms_5000 → Reached + featured ⭐,
                //   at_ms_10000 → Locked 🔒
                milestones = new()
                {
                    new() { milestoneId = "at_ms_100",   requiredScore = 100,   rewards = new() { new() { currencyID = "Coins", amount = 500   } } },
                    new() { milestoneId = "at_ms_500",   requiredScore = 500,   rewards = new() { new() { currencyID = "Coins", amount = 1500  } } },
                    new() { milestoneId = "at_ms_1000",  requiredScore = 1000,  rewards = new() { new() { currencyID = "Coins", amount = 3000  }, new() { currencyID = "Gems", amount = 50  } } },
                    new() { milestoneId = "at_ms_5000",  requiredScore = 5000,  rewards = new() { new() { currencyID = "Coins", amount = 10000 }, new() { currencyID = "Gems", amount = 200 }, new() { type = ResourceEntryType.Item, catalogID = "main", itemID = "eternal_crown", amount = 1 } }, isFeatured = true },
                    new() { milestoneId = "at_ms_10000", requiredScore = 10000, rewards = new() { new() { currencyID = "Coins", amount = 25000 }, new() { currencyID = "Gems", amount = 500 }, new() { type = ResourceEntryType.Item, catalogID = "main", itemID = "legend_throne", amount = 1 } }, isFeatured = true },
                },
                rankRewards = new()
                {
                    new() { rankRange = "1",    rewards = new() { new() { currencyID = "Coins", amount = 20000 }, new() { currencyID = "Gems", amount = 500 }, new() { type = ResourceEntryType.Item, catalogID = "main", itemID = "legend_trophy", amount = 1 } } },
                    new() { rankRange = "2-5",  rewards = new() { new() { currencyID = "Coins", amount = 8000  }, new() { currencyID = "Gems", amount = 200 } } },
                    new() { rankRange = "6-25", rewards = new() { new() { currencyID = "Coins", amount = 2000  } } },
                },
                players = new()
                {
                    // Ranks 1-3
                    new() { name = "LegendKing",    score = 12400, level = 99, isPremium = true },
                    new() { name = "WarlordZ",      score =  9800, level = 87, isPremium = true },
                    new() { name = "EternalEdge",   score =  7200, level = 74                   },
                    // Rank 4 — current user
                    new() { name = "You",           score =  6000, level = 22, isCurrentUser = true },
                    // Ranks 5-50
                    new() { name = "PhantomX",      score =  4500, level = 61                   },
                    new() { name = "CrystalGuard",  score =  4100, level = 58                   },
                    new() { name = "IronDuke",      score =  3700, level = 55                   },
                    new() { name = "SteelMage",     score =  3400, level = 52, isPremium = true },
                    new() { name = "GoldKnight",    score =  3100, level = 49                   },
                    new() { name = "SilverSword",   score =  2800, level = 46                   },
                    new() { name = "BronzeShield",  score =  2600, level = 43                   },
                    new() { name = "TitanFist",     score =  2400, level = 40                   },
                    new() { name = "GiantSlayer",   score =  2200, level = 37, isPremium = true },
                    new() { name = "DragonBane",    score =  2000, level = 35                   },
                    new() { name = "CastleBreaker", score =  1850, level = 33                   },
                    new() { name = "WallCrusher",   score =  1700, level = 31                   },
                    new() { name = "TowerSmasher",  score =  1550, level = 29                   },
                    new() { name = "DungeonClear",  score =  1400, level = 27                   },
                    new() { name = "QuestMaster",   score =  1300, level = 25                   },
                    new() { name = "GuildLeader",   score =  1200, level = 24                   },
                    new() { name = "ClanChief",     score =  1100, level = 22                   },
                    new() { name = "TribeElite",    score =  1000, level = 21                   },
                    new() { name = "WarriorSage",   score =   920, level = 20                   },
                    new() { name = "BattleAce",     score =   850, level = 19                   },
                    new() { name = "FightSpirit",   score =   780, level = 18                   },
                    new() { name = "BrawlerX",      score =   710, level = 17                   },
                    new() { name = "DuelistPro",    score =   650, level = 16                   },
                    new() { name = "ChampFall",     score =   590, level = 15                   },
                    new() { name = "GloryHunter",   score =   540, level = 14                   },
                    new() { name = "HonorBound",    score =   490, level = 14                   },
                    new() { name = "ValorSeeker",   score =   445, level = 13                   },
                    new() { name = "CourageX",      score =   400, level = 12                   },
                    new() { name = "HeroX",         score =   360, level = 12                   },
                    new() { name = "PaladinX",      score =   325, level = 11                   },
                    new() { name = "CrusaderX",     score =   290, level = 10                   },
                    new() { name = "KnightX",       score =   260, level = 10                   },
                    new() { name = "SquireX",       score =   230, level =  9                   },
                    new() { name = "MercX",         score =   205, level =  8                   },
                    new() { name = "AdeptX",        score =   180, level =  8                   },
                    new() { name = "NoviceX",       score =   160, level =  7                   },
                    new() { name = "InitiateX",     score =   140, level =  6                   },
                    new() { name = "ApprenticeX",   score =   120, level =  6                   },
                    new() { name = "StudentX",      score =   105, level =  5                   },
                    new() { name = "TraineeX",      score =    90, level =  4                   },
                    new() { name = "RecruitX",      score =    78, level =  4                   },
                    new() { name = "CadetX",        score =    67, level =  3                   },
                    new() { name = "Plebe",         score =    57, level =  3                   },
                    new() { name = "WandererX",     score =    48, level =  2                   },
                    new() { name = "TravelerX",     score =    40, level =  2                   },
                    new() { name = "NewcomerX",     score =    30, level =  1                   },
                },
            },
        };

        [SerializeField] private MockPlayerSettings _player = new();

        // ── claimed milestone tracking ─────────────────────────────────────
        // Key = leaderboardId, Value = set of claimed milestoneIds
        private readonly Dictionary<string, HashSet<string>> _claimedMilestones = new();

        private LeaderboardWindowModel _model;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _model = new LeaderboardWindowModel();
            if (_view == null) _view = GetComponent<LeaderboardView>();

            // weekly_pvp (Instant): 500 claimed → 1000 Reached/кнопка → 2500 Locked
            _claimedMilestones["weekly_pvp"] = new HashSet<string> { "w_ms_500" };

            // daily_speed (FeaturedAfterEnd): 2000 claimed → 4000 Reached/кнопка → 5000 Pending⏳⭐
            _claimedMilestones["daily_speed"] = new HashSet<string> { "d_ms_2000" };

            // alltime_wins (AfterEventEnd+Never): 100,500 claimed → 1000 Reached → 5000 Reached⭐ → 10000 Locked
            _claimedMilestones["alltime_wins"] = new HashSet<string> { "at_ms_100", "at_ms_500" };
        }

        private void Start()     => Render();
        private void OnEnable()  => _view.BackButton?.onClick.AddListener(OnBackClicked);
        private void OnDisable() { if (_view?.BackButton != null) _view.BackButton.onClick.RemoveListener(OnBackClicked); }

        private void OnBackClicked() => gameObject.SetActive(false);

        // ─────────────────────────────────────────────────────────────────

        [ContextMenu("Render Mock")]
        public void Render()
        {
            UpdateModel();
            _view?.Render(
                _model,
                claimMilestoneFactory: (id, ms) => () => SimulateClaimMilestone(id, ms)
            );
        }

        [ContextMenu("Randomize Progress")]
        public void RandomizeProgress()
        {
            _claimedMilestones.Clear();


            var rng = new System.Random();
            foreach (var lb in _leaderboards)
            {
                long maxScore = lb.milestones.Count > 0 ? lb.milestones.Max(m => m.requiredScore) * 2 : 10000;
                lb.myScoreEarnedThisCycle = (long)(rng.NextDouble() * maxScore);
                lb.myCurrentScore         = lb.myScoreEarnedThisCycle;
                lb.myLastKnownRank        = rng.Next(1, 51);
                lb.hasUnclaimedReward     = rng.Next(0, 2) == 1;

                // Randomize player scores
                foreach (var p in lb.players.Where(p => !p.isCurrentUser))
                    p.score = (long)(rng.NextDouble() * maxScore * 1.5);
            }

            Render();
        }

        [ContextMenu("Claim All Milestones")]
        public void ClaimAllMilestones()
        {
            foreach (var lb in _leaderboards)
            {
                if (!_claimedMilestones.TryGetValue(lb.leaderboardId, out var set))
                { set = new HashSet<string>(); _claimedMilestones[lb.leaderboardId] = set; }

                foreach (var ms in lb.milestones)
                {
                    if (set.Add(ms.milestoneId))
                        _player.coins += ms.rewards?.Sum(r => r.amount) ?? 0;
                }
            }
            Render();
        }

        [ContextMenu("Reset All")]
        public void ResetAll()
        {
            _claimedMilestones.Clear();

            foreach (var lb in _leaderboards)
            {
                lb.myCurrentScore          = 0;
                lb.myScoreEarnedThisCycle  = 0;
                lb.myLastKnownRank         = 0;
                lb.hasUnclaimedReward      = false;
            }
            Render();
        }

        // ─────────────────────────────────────────────────────────────────

        private Task SimulateClaimMilestone(string leaderboardId, string milestoneId)
        {
            if (!_claimedMilestones.TryGetValue(leaderboardId, out var set))
            { set = new HashSet<string>(); _claimedMilestones[leaderboardId] = set; }

            if (set.Contains(milestoneId)) return Task.CompletedTask;

            var lb = _leaderboards.FirstOrDefault(x => x.leaderboardId == leaderboardId);
            var ms = lb?.milestones.FirstOrDefault(m => m.milestoneId == milestoneId);
            if (ms != null)
            {
                set.Add(milestoneId);
                _player.coins += ms.rewards?.Sum(r => r.amount) ?? 0;
            }

            Render();
            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────────

        private void UpdateModel()
        {
            if (_model == null) _model = new LeaderboardWindowModel();
            _model.Clear();
            _model.CoinBalance = _player.coins;
            _model.GemBalance  = _player.gems;

            foreach (var lb in _leaderboards)
                _model.AddLeaderboard(
                    BuildDefinition(lb),
                    BuildTopList(lb),
                    BuildMyProgress(lb),
                    _player.userId
                );
        }

        private LeaderboardDefinition BuildDefinition(MockLeaderboardSettings lb)
        {
            var milestonesDict = new Dictionary<string, LeaderboardMilestoneDefinition>();
            for (int i = 0; i < lb.milestones.Count; i++)
            {
                var m = lb.milestones[i];
                milestonesDict[m.milestoneId] = new LeaderboardMilestoneDefinition
                {
                    MilestoneID   = m.milestoneId,
                    DisplayName   = $"Score {FormatScore(m.requiredScore)}",
                    RequiredScore = m.requiredScore,
                    IsFeatured    = m.isFeatured,
                    SortOrder     = i,
                    Rewards = new ResourceGrant
                    {
                        Standard = new ResourceBundle
                        {
                            Entries = m.rewards?.Select(r => new ResourceEntry
                            {
                                Type       = r.type,
                                CurrencyID = r.currencyID,
                                CatalogID  = r.catalogID,
                                ItemID     = r.itemID,
                                Amount     = r.amount,
                            }).ToList() ?? new List<ResourceEntry>()
                        }
                    },
                };
            }

            var rankRewards = lb.rankRewards.Select(rr => new LeaderboardRankReward
            {
                Rank    = rr.rankRange,
                Rewards = new ResourceGrant
                {
                    Standard = new ResourceBundle
                    {
                        Entries = rr.rewards?.Select(r => new ResourceEntry
                        {
                            Type       = r.type,
                            CurrencyID = r.currencyID,
                            CatalogID  = r.catalogID,
                            ItemID     = r.itemID,
                            Amount     = r.amount,
                        }).ToList() ?? new List<ResourceEntry>()
                    }
                },
            }).ToList();

            return new LeaderboardDefinition
            {
                LeaderboardID       = lb.leaderboardId,
                DisplayName         = lb.displayName,
                ScoreDisplayName    = lb.scoreDisplayName,
                ScoreAggregation    = lb.aggregation,
                CycleReset          = lb.cycleReset,
                MilestoneClaimMode  = lb.milestoneClaimMode,
                Milestones          = milestonesDict,
                RankRewards         = rankRewards,
                IsEnabled           = true,
            };
        }

        private GetLeaderboardResponse BuildTopList(MockLeaderboardSettings lb)
        {
            var entries = new List<LeaderboardUserEntry>();
            var ordered = lb.aggregation == LeaderboardScoreAggregation.BestTime
                ? lb.players.OrderBy(p => p.score).ToList()
                : lb.players.OrderByDescending(p => p.score).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var p = ordered[i];
                entries.Add(new LeaderboardUserEntry
                {
                    UserID = p.isCurrentUser ? _player.userId : $"player_{i}",
                    Score  = p.score,
                    Rank   = i + 1,
                    PublicProfile = new UserPublicDataModel
                    {
                        Username  = p.isCurrentUser ? _player.displayName : p.name,
                        Level     = p.level,
                        Premium   = p.isPremium,
                        AvatarUrl = "",
                    },
                });
            }

            return new GetLeaderboardResponse
            {
                LeaderboardID     = lb.leaderboardId,
                CycleVersion      = 1,
                TotalParticipants = lb.players.Count + UnityEngine.Random.Range(50, 500),
                CycleEndUtc       = lb.cycleReset == LeaderboardCycleReset.Never
                                        ? (DateTime?)null
                                        : DateTime.UtcNow.AddHours(lb.hoursUntilReset),
                TopUsers          = entries,
            };
        }

        private GetMyProgressResponse BuildMyProgress(MockLeaderboardSettings lb)
        {
            _claimedMilestones.TryGetValue(lb.leaderboardId, out var claimed);

            // Find next unclaimed milestone
            LeaderboardMilestoneDefinition nextMs = null;
            foreach (var ms in lb.milestones.OrderBy(m => m.requiredScore))
            {
                if (claimed == null || !claimed.Contains(ms.milestoneId))
                {
                    nextMs = new LeaderboardMilestoneDefinition
                    {
                        MilestoneID   = ms.milestoneId,
                        RequiredScore = ms.requiredScore,
                    };
                    break;
                }
            }

            bool hasUnclaimedMilestone = nextMs != null && lb.myScoreEarnedThisCycle >= nextMs.RequiredScore;

            return new GetMyProgressResponse
            {
                LeaderboardID         = lb.leaderboardId,
                CurrentScore          = lb.myCurrentScore,
                ScoreEarnedThisCycle  = lb.myScoreEarnedThisCycle,
                LastKnownRank         = lb.myLastKnownRank,
                HasUnclaimedReward    = false,
                NextMilestone         = nextMs,
                HasUnclaimedMilestone = hasUnclaimedMilestone,
            };
        }

        // ─────────────────────────────────────────────────────────────────

        private static string FormatScore(long score)
        {
            if (score >= 1_000_000) return $"{score / 1_000_000f:0.#}M";
            if (score >= 1_000)     return $"{score / 1_000f:0.#}K";
            return score.ToString();
        }

        private static bool RankInRange(int rank, string range)
        {
            if (string.IsNullOrEmpty(range)) return false;
            if (!range.Contains('-')) return int.TryParse(range, out int single) && rank == single;
            var parts = range.Split('-');
            return parts.Length == 2
                && int.TryParse(parts[0], out int lo)
                && int.TryParse(parts[1], out int hi)
                && rank >= lo && rank <= hi;
        }
    }
}
