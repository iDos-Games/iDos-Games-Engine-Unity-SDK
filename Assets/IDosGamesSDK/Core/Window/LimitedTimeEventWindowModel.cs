using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames.UI.LimitedTimeEvent
{
    public class LimitedTimeEventWindowModel
    {
        public ActiveEventInfo Event       { get; private set; }
        public bool            HasPremium  { get; private set; }
        public long            CoinBalance { get; private set; }
        public long            GemBalance  { get; private set; }

        public event Action OnChanged;

        public bool IsLoaded => Event != null;

        public float TokenProgress      => ComputeProgress(out _, out _);
        public float SegmentProgress    => ComputeProgress(out float seg, out _) == 0f ? 0f : seg;
        public int CompletedMilestones
        {
            get
            {
                ComputeProgress(out _, out int completed);
                return completed;
            }
        }

        private float ComputeProgress(out float segmentProgress, out int completedCount)
        {
            segmentProgress = 0f;
            completedCount  = 0;
            if (Event?.Progress == null) return 0f;

            var milestoneDict = Event.Content?.Milestones;
            if (milestoneDict == null || milestoneDict.Count == 0) return 0f;

            var milestones = milestoneDict.Values.OrderBy(m => m.RequiredTokensEarned).ToList();

            int  total         = milestones.Count;
            long earned        = Event.Progress?.Balance?.TotalEarned ?? 0;
            long lastThreshold = 0L;

            foreach (var m in milestones)
            {
                if (earned >= m.RequiredTokensEarned)
                {
                    completedCount++;
                    lastThreshold = m.RequiredTokensEarned;
                }
            }

            if (completedCount >= total) { segmentProgress = 1f; return 1f; }

            var next = Event.NextMilestone;
            if (next == null) { segmentProgress = 1f; return 1f; }

            long segmentSize = next.RequiredTokensEarned - lastThreshold;
            if (segmentSize <= 0) return (float)completedCount / total;

            segmentProgress = Mathf.Clamp01((float)(earned - lastThreshold) / segmentSize);
            float result = (completedCount + segmentProgress) / total;

#if UNITY_EDITOR
            Debug.Log($"[TokenProgress]={result} \n" +
                      $"TokensEarnedTotal={earned} | \n" +
                      $"TokenBalance={Event.Progress?.Balance?.Current ?? 0} | \n" +
                      $"lastThreshold={lastThreshold} (порог последнего пройденного milestone) | \n" +
                      $"nextThreshold={next.RequiredTokensEarned} (порог следующего milestone) | \n" +
                      $"segmentSize={segmentSize} (nextThreshold - lastThreshold) | \n" +
                      $"segmentProgress={segmentProgress:F3} (прогресс внутри текущего сегмента 0..1) | \n" +
                      $"completed={completedCount}/{total} (пройдено/всего milestone) | \n" +
                      $"fillAmount={result:F3}\n");
#endif

            return result;
        }

        public string ProgressText
        {
            get
            {
                if (Event?.Progress == null) return "0 / 0";
                long balance = Event.Progress?.Balance?.TotalEarned ?? 0;
                long target  = Event.NextMilestone?.RequiredTokensEarned
                               ?? (Event.Progress?.Balance?.TotalEarned ?? 0);
                return $"{balance:N0} / {target:N0}";
            }
        }

        public string BalanceText =>
            (Event?.Progress?.Balance?.Current ?? 0).ToString("N0");

        private HashSet<string> _premiumClaimed = new HashSet<string>();

        public void Apply(ActiveEventInfo evt, bool hasPremium, long coins, long gems)
        {
            Event       = evt;
            HasPremium  = hasPremium;
            CoinBalance = coins;
            GemBalance  = gems;
            _premiumClaimed.Clear();
            OnChanged?.Invoke();
        }

        public void SetPremiumClaimed(IEnumerable<string> ids)
        {
            _premiumClaimed = ids != null ? new HashSet<string>(ids) : new HashSet<string>();
        }

        public void UpdateCurrencies(long coins, long gems)
        {
            CoinBalance = coins;
            GemBalance  = gems;
            OnChanged?.Invoke();
        }

        public bool IsMilestoneFreeClaimed(string milestoneId) =>
            Event?.Progress?.Milestone?.ClaimedIDs?.Contains(milestoneId) ?? false;

        public bool IsMilestonePremiumClaimed(string milestoneId) =>
            _premiumClaimed.Contains(milestoneId);

        public bool IsMilestoneClaimed(string milestoneId) =>
            IsMilestoneFreeClaimed(milestoneId);

        public bool IsMilestoneReached(long required) =>
            (Event?.Progress?.Balance?.TotalEarned ?? 0) >= required;

        public bool IsMilestonePendingEventEnd(long required, bool isFeatured)
        {
            if (!IsMilestoneReached(required)) return false;

            var claimMode = Event?.Content?.ClaimMode ?? EventClaimMode.Instant;
            bool eventEnded = DateTime.UtcNow >= (Event?.ComputedEndUtc ?? DateTime.MaxValue);

            return claimMode switch
            {
                EventClaimMode.AfterEventEnd    => !eventEnded,
                EventClaimMode.FeaturedAfterEnd => isFeatured && !eventEnded,
                _                               => false,
            };
        }

        public bool CanClaimMilestone(string milestoneId, long required, bool isFeatured = false) =>
            IsMilestoneReached(required)
            && !IsMilestoneFreeClaimed(milestoneId)
            && !IsMilestonePendingEventEnd(required, isFeatured)
            && (Event?.CanClaim ?? false);
    }
}
