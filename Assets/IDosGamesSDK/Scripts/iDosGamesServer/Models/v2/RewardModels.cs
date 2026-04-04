using IDosGames.TitlePublicConfiguration;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class RewardRequest : IGSRequest
    {
        public string RewardCurrencyID { get; set; } // "CO" or "IG"
        public int BaseValue { get; set; }           // Base value of the reward
        public float Multiplier { get; set; } = 1;    // Multiplier (1, 3, 5)
        public bool IncludeReferral { get; set; }    // Should referrals be credited (for CO)
        public int Points { get; set; }              // Event points

        public string CalendarID { get; set; }
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class RewardResponse
    {
        public string RewardCurrencyID { get; set; } // "CO" / "IG"
        public int RewardGranted { get; set; }       // How much was accrued
        public int RewardBalanceNew { get; set; }    // New reward balance

        public string LimitCurrencyID { get; set; }  // "CL" / "TL"
        public int LimitSpent { get; set; }          // How much of the limit was written off
        public int LimitBalanceNew { get; set; }     // New balance limit

        public int ReferralGranted { get; set; }     // How much was credited to the referral
        public int PointsAdded { get; set; }         // How many points were added
    }

    [Serializable]
    public class ClaimDailyRewardResponse
    {
        public string CalendarID { get; set; }
        public DailyRewardState DailyState { get; set; } // Updated calendar state (so the client knows what day is next)
        public List<ItemOrCurrency> GrantedRewards { get; set; } // List of what was actually issued (copy from the config of the day)
    }

    [Serializable]
    public class DailyRewardState
    {
        public DateTime LastClaimTime { get; set; }
        public int CollectedDays { get; set; }
        public string CalendarID { get; set; }
    }

    public enum RewardAction
    {
        Claim,
        ClaimVip,
        ClaimItemProfit,
        ClaimDailyReward,
        GetDailyRewardsDefinitions,
    }
}
