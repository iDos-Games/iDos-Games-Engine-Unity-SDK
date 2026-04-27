using System;
using System.Collections.Generic;
using IDosGames.ClientModels;
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

            var milestones = Event.Content?.Milestones;
            if (milestones == null || milestones.Count == 0) return 0f;

            milestones.Sort((a, b) => a.RequiredTokensEarned.CompareTo(b.RequiredTokensEarned));

            int  total         = milestones.Count;
            long earned        = Event.Progress.TokensEarnedTotal;
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
            Debug.Log($"[TokenProgress]={result} \n" +
                      $"TokensEarnedTotal={earned} | \n" +
                      $"TokenBalance={Event.Progress.TokenBalance} | \n" +
                      $"lastThreshold={lastThreshold} (порог последнего пройденного milestone) | \n" +
                      $"nextThreshold={next.RequiredTokensEarned} (порог следующего milestone) | \n" +
                      $"segmentSize={segmentSize} (nextThreshold - lastThreshold) | \n" +
                      $"segmentProgress={segmentProgress:F3} (прогресс внутри текущего сегмента 0..1) | \n" +
                      $"completed={completedCount}/{total} (пройдено/всего milestone) | \n" +
                      $"fillAmount={result:F3}\n");
            return result;
        }

        public string ProgressText
        {
            get
            {
                if (Event?.Progress == null) return "0 / 0";
                long balance = Event.Progress.TokensEarnedTotal;
                long target  = Event.NextMilestone?.RequiredTokensEarned
                               ?? Event.Progress.TokensEarnedTotal;
                return $"{balance:N0} / {target:N0}";
            }
        }

        public string BalanceText =>
            Event?.Progress?.TokenBalance.ToString("N0") ?? "0";

        public void Apply(ActiveEventInfo evt, bool hasPremium, long coins, long gems)
        {
            Event       = evt;
            HasPremium  = hasPremium;
            CoinBalance = coins;
            GemBalance  = gems;
            OnChanged?.Invoke();
        }

        public void UpdateCurrencies(long coins, long gems)
        {
            CoinBalance = coins;
            GemBalance  = gems;
            OnChanged?.Invoke();
        }

        public bool IsMilestoneClaimed(string milestoneId) =>
            Event?.Progress?.ClaimedMilestoneIDs?.Contains(milestoneId) ?? false;

        public bool IsMilestoneReached(long required) =>
            (Event?.Progress?.TokensEarnedTotal ?? 0) >= required;

        public bool CanClaimMilestone(string milestoneId, long required) =>
            IsMilestoneReached(required)
            && !IsMilestoneClaimed(milestoneId)
            && (Event?.CanClaim ?? false);
    }
}
