using System;

namespace IDosGames.ServerModels
{
    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class RewardClaimRequest : IGSRequest
    {
        public string RewardCurrencyId; // "CO" or "IG"
        public int BaseValue;           // Base value of the reward
        public float Multiplier = 1;    // Multiplier (1, 3, 5)
        public bool IncludeReferral;    // Should referrals be credited (for CO)
        public int Points;              // Event points
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class RewardClaimResponse
    {
        public string RewardCurrencyId; // "CO" / "IG"
        public int RewardGranted;       // How much was accrued
        public int RewardBalanceNew;    // New reward balance

        public string LimitCurrencyId;  // "CL" / "TL"
        public int LimitSpent;          // How much of the limit was written off
        public int LimitBalanceNew;     // New balance limit

        public int ReferralGranted;     // How much was credited to the referral
        public int PointsAdded;         // How many points were added
    }

    public enum RewardAction
    {
        Claim,
        ClaimVip,
        ClaimItemProfit,
        ClaimDailyReward,
    }
}
