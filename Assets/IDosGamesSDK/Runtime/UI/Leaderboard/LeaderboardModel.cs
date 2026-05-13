using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames.UI.Leaderboard
{
    public class LeaderboardWindowModel
    {
        public bool IsLoaded { get; private set; }
        public long CoinBalance { get; set; }
        public long GemBalance { get; set; }

        public List<LeaderboardTabData> Tabs { get; } = new();

        public void AddLeaderboard(
            LeaderboardDefinition definition,
            GetLeaderboardResponse topList,
            GetMyProgressResponse myProgress,
            string currentUserID)
        {
            IsLoaded = true;

            var existing = Tabs.FirstOrDefault(t => t.LeaderboardID == definition.LeaderboardID);
            if (existing != null) Tabs.Remove(existing);

            var tab = new LeaderboardTabData();
            tab.Build(definition, topList, myProgress, currentUserID);
            Tabs.Add(tab);
        }

        public void Clear()
        {
            IsLoaded = false;
            Tabs.Clear();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    public class LeaderboardTabData
    {
        public string LeaderboardID;
        public string DisplayName;
        public string ScoreDisplayName;
        public LeaderboardCycleReset CycleReset;
        public DateTime? CycleEndUtc;
        public long TotalParticipants;
        public bool HasUnclaimedReward;
        public bool HasUnclaimedMilestone;
        public string BracketID;
        public LeaderboardScoreAggregation ScoreAggregation;
        public EventClaimMode MilestoneClaimMode;

        public List<LeaderboardEntryUIItem>      Entries     = new();
public LeaderboardMyProgressUIItem       MyProgress;
        public List<LeaderboardMilestoneUIItem>  Milestones  = new();
        public List<LeaderboardRankRewardUIItem> RankRewards = new();

        // ── build ─────────────────────────────────────────────────────────

        public void Build(
            LeaderboardDefinition definition,
            GetLeaderboardResponse topList,
            GetMyProgressResponse myProgress,
            string currentUserID)
        {
            LeaderboardID     = definition.LeaderboardID;
            DisplayName       = definition.DisplayName ?? definition.LeaderboardID;
            ScoreDisplayName  = definition.ScoreDisplayName ?? "Score";
            CycleReset        = definition.CycleReset;
            CycleEndUtc       = topList?.CycleEndUtc;
            TotalParticipants = topList?.TotalParticipants ?? 0;
            BracketID         = topList?.BracketID;
            ScoreAggregation  = definition.ScoreAggregation;
            MilestoneClaimMode = definition.MilestoneClaimMode;

            HasUnclaimedReward = myProgress?.HasUnclaimedReward ?? false;

            BuildEntries(topList, currentUserID);
            BuildMyProgress(myProgress, currentUserID);
            BuildMilestones(definition, myProgress);
            BuildRankRewards(definition);

            // Recompute after milestones are built — only count milestones that are
            // actually claimable right now (excludes Pending ones)
            bool cycleEnded = CycleEndUtc.HasValue
                ? DateTime.UtcNow >= CycleEndUtc.Value
                : CycleReset == LeaderboardCycleReset.Never;

            HasUnclaimedMilestone = Milestones.Any(ms =>
                ms.IsReached && !ms.IsClaimed && CanClaimNow(MilestoneClaimMode, ms.IsFeatured, cycleEnded));
        }

        private void BuildEntries(GetLeaderboardResponse topList, string currentUserID)
        {
            Entries.Clear();
            if (topList?.TopUsers == null) return;

            foreach (var e in topList.TopUsers.OrderBy(x => x.Rank))
            {
                Entries.Add(new LeaderboardEntryUIItem
                {
                    UserID        = e.UserID,
                    Name          = e.PublicProfile?.Username ?? e.UserID,
                    AvatarUrl     = e.PublicProfile?.AvatarUrl ?? "",
                    Score         = e.Score,
                    Rank          = e.Rank,
                    Level         = e.PublicProfile?.Level ?? 0,
                    IsPremium     = e.PublicProfile?.Premium ?? false,
                    IsCurrentUser = !string.IsNullOrEmpty(currentUserID) && e.UserID == currentUserID,
                });
            }
        }

        private void BuildMyProgress(GetMyProgressResponse progress, string currentUserID)
        {
            if (progress == null) return;

            MyProgress = new LeaderboardMyProgressUIItem
            {
                UserID                = currentUserID,
                CurrentScore          = progress.CurrentScore,
                ScoreEarnedThisCycle  = progress.ScoreEarnedThisCycle,
                LastKnownRank         = progress.LastKnownRank,
                HasUnclaimedReward    = progress.HasUnclaimedReward,
                HasUnclaimedMilestone = progress.HasUnclaimedMilestone,
            };
        }

        private void BuildMilestones(LeaderboardDefinition definition, GetMyProgressResponse progress)
        {
            Milestones.Clear();
            if (definition.Milestones == null || definition.Milestones.Count == 0) return;

            long scoreEarned   = progress?.ScoreEarnedThisCycle ?? 0;
            string nextMsId    = progress?.NextMilestone?.MilestoneID;
            bool   passedNext  = false;

            var sorted = definition.Milestones.Values
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.RequiredScore)
                .ToList();

            foreach (var ms in sorted)
            {
                bool isReached = scoreEarned >= ms.RequiredScore;
                bool isClaimed;

                if (string.IsNullOrEmpty(nextMsId))
                {
                    isClaimed = isReached;
                }
                else if (ms.MilestoneID == nextMsId)
                {
                    isClaimed  = false;
                    passedNext = true;
                }
                else
                {
                    isClaimed = isReached && !passedNext;
                }

                var rewards = ms.Rewards?.Standard?.Entries ?? new List<ResourceEntry>();

                Milestones.Add(new LeaderboardMilestoneUIItem
                {
                    MilestoneID   = ms.MilestoneID,
                    LeaderboardID = definition.LeaderboardID,
                    DisplayName   = ms.DisplayName ?? ms.MilestoneID,
                    RequiredScore = ms.RequiredScore,
                    Rewards       = rewards,
                    IsReached     = isReached,
                    IsClaimed     = isClaimed,
                    IsFeatured    = ms.IsFeatured,
                    SortOrder     = ms.SortOrder,
                    IconPath      = ms.AssetPaths?.GetValueOrDefault("icon") ?? "",
                });
            }
        }

        internal static bool CanClaimNow(EventClaimMode mode, bool isFeatured, bool cycleEnded) =>
            mode switch
            {
                EventClaimMode.Instant          => true,
                EventClaimMode.AfterEventEnd    => cycleEnded,
                EventClaimMode.FeaturedAfterEnd => !isFeatured || cycleEnded,
                _                               => true,
            };

        private void BuildRankRewards(LeaderboardDefinition definition)
        {
            RankRewards.Clear();
            if (definition.RankRewards == null) return;

            foreach (var rr in definition.RankRewards)
            {
                RankRewards.Add(new LeaderboardRankRewardUIItem
                {
                    RankRange = rr.Rank,
                    Rewards   = rr.Rewards?.Standard?.Entries ?? new List<ResourceEntry>(),
                });
            }
        }

        // ── helpers ───────────────────────────────────────────────────────

        public float MilestoneSegmentProgress(int index)
        {
            if (index < 0 || index >= Milestones.Count) return 0f;
            var ms      = Milestones[index];
            long prev   = index > 0 ? Milestones[index - 1].RequiredScore : 0;
            long segment = ms.RequiredScore - prev;
            if (segment <= 0) return 1f;
            long earned = MyProgress?.ScoreEarnedThisCycle ?? 0;
            return Mathf.Clamp01((float)(earned - prev) / segment);
        }

        public LeaderboardMilestoneUIItem NextUnclaimedMilestone()
        {
            return Milestones.FirstOrDefault(m => !m.IsClaimed && m.IsReached)
                ?? Milestones.FirstOrDefault(m => !m.IsClaimed)
                ?? (Milestones.Count > 0 ? Milestones[^1] : null);
        }

        public string CycleResetLabel => CycleReset switch
        {
            LeaderboardCycleReset.Hourly  => "Hourly",
            LeaderboardCycleReset.Daily   => "Daily",
            LeaderboardCycleReset.Weekly  => "Weekly",
            LeaderboardCycleReset.Monthly => "Monthly",
            LeaderboardCycleReset.Yearly  => "Yearly",
            LeaderboardCycleReset.Never   => "All-Time",
            _                             => "",
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI item models
    // ─────────────────────────────────────────────────────────────────────────

    public class LeaderboardEntryUIItem
    {
        public string UserID;
        public string Name;
        public string AvatarUrl;
        public long   Score;
        public int    Rank;
        public int    Level;
        public bool   IsPremium;
        public bool   IsCurrentUser;
    }

    public class LeaderboardMyProgressUIItem
    {
        public string UserID;
        public long   CurrentScore;
        public long   ScoreEarnedThisCycle;
        public int    LastKnownRank;
        public bool   HasUnclaimedReward;
        public bool   HasUnclaimedMilestone;
    }

    public class LeaderboardMilestoneUIItem
    {
        public string             MilestoneID;
        public string             LeaderboardID;
        public string             DisplayName;
        public long               RequiredScore;
        public List<ResourceEntry> Rewards;
        public bool               IsReached;
        public bool               IsClaimed;
        public bool               IsFeatured;
        public int                SortOrder;
        public string             IconPath;
    }

    public class LeaderboardRankRewardUIItem
    {
        public string             RankRange;
        public List<ResourceEntry> Rewards;
    }
}
