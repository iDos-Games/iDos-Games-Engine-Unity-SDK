using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Request for all Referral module actions.
    /// Inherits UserID, ClientSessionTicket, BuildKey, WebAppLink, RelatedEntityID from BaseRequest.
    ///
    /// Field usage by action:
    ///   ActivateReferralCode — ReferralCode required.
    ///   ClaimInviteReward    — InviteRewardID required.
    ///   GetDefinitions       — no extra fields.
    ///   GetUserState         — no extra fields.
    /// </summary>
    [Serializable]
    public class ReferralRequest : BaseRequest
    {
        /// <summary>UserID of the referrer. Used in ActivateReferralCode.</summary>
        public string ReferralCode;

        /// <summary>ID of the invite reward to claim manually. Used in ClaimInviteReward.</summary>
        public string InviteRewardID;
    }

    // ===================== Responses =====================

    [Serializable]
    public class ReferralDefinitionsResponse
    {
        public ReferralDefinitions ReferralDefinitions;
    }

    [Serializable]
    public class UserReferralStateResponse
    {
        public UserReferralState Referral;
    }

    [Serializable]
    public class ActivateReferralCodeResponse
    {
        /// <summary>UserID of the referrer whose code was activated.</summary>
        public string ReferralCode;

        /// <summary>True if this is the first ever activation (activation reward granted).</summary>
        public bool IsFirstActivation;

        /// <summary>Granted resources (activation reward). Null if not first activation.</summary>
        public ResourceOperation Resources;
    }

    [Serializable]
    public class ClaimInviteRewardResponse
    {
        /// <summary>ID of the claimed invite reward.</summary>
        public string RewardID;

        /// <summary>Granted resources for this invite reward.</summary>
        public ResourceOperation Resources;
    }

    // ===================== State =====================

    /// <summary>
    /// Referral state of a user. Stored in UserState.Referral.
    /// </summary>
    [Serializable]
    public class UserReferralState
    {
        /// <summary>UserID of the referrer this user is subscribed to. Null/empty = not subscribed.</summary>
        public string SubscribedToUserID;

        /// <summary>
        /// Whether the one-time activation reward has already been granted.
        /// Stays true even if the user switches referrers.
        /// </summary>
        public bool ActivationRewardGranted = false;

        /// <summary>Number of users subscribed to this user. Atomically incremented/decremented on server.</summary>
        public int FollowersCount = 0;

        /// <summary>List of follower UserIDs. Used to correctly unsubscribe on referrer switch.</summary>
        public List<string> FollowerIDs = new();

        /// <summary>
        /// States of invite rewards (milestone rewards for follower count).
        /// Only contains rewards that have been touched (auto-granted or claimed).
        /// </summary>
        public Dictionary<string, ReferralInviteRewardState> InviteRewardStates = new();

        public DateTime UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>State of a single invite reward for a user.</summary>
    [Serializable]
    public class ReferralInviteRewardState
    {
        /// <summary>Matches InviteRewardDefinition.RewardID.</summary>
        public string RewardID;

        public bool IsClaimed = false;
        public DateTime? ClaimedAt;
    }

    // ===================== Config =====================

    /// <summary>
    /// Referral system configuration for the title. Stored in TitlePublicConfigurationModel.Referral.
    /// </summary>
    [Serializable]
    public class ReferralDefinitions
    {
        /// <summary>Whether the referral system is enabled for this title.</summary>
        public bool IsEnabled = true;

        /// <summary>One-time reward granted to the user on their first activation of any referral code.</summary>
        public ResourceGrant ActivationReward;

        /// <summary>
        /// Milestone invite rewards for the referrer, keyed by RewardID.
        /// AutoGrant=true rewards are granted automatically; AutoGrant=false require manual claim.
        /// </summary>
        public Dictionary<string, InviteRewardDefinition> InviteRewards;

        /// <summary>
        /// Percentage spend-reward configurations per feature.
        /// Determines how much of a subscriber's spend the referrer receives.
        /// </summary>
        public List<SpendRewardDefinition> SpendRewards;
    }

    /// <summary>Definition of a milestone invite reward for the referrer.</summary>
    [Serializable]
    public class InviteRewardDefinition
    {
        /// <summary>Stable unique ID. Used as key in user state.</summary>
        public string RewardID;

        /// <summary>Required follower count to unlock this reward.</summary>
        public int RequiredFollowersCount;

        /// <summary>Resources granted to the referrer.</summary>
        public ResourceGrant Rewards;

        /// <summary>
        /// True = granted automatically when threshold is reached (inside ActivateReferralCode).
        /// False = user must claim manually via ClaimInviteReward.
        /// </summary>
        public bool AutoGrant = false;
    }

    /// <summary>
    /// Configuration for a percentage referral spend-reward for a specific feature.
    /// The calling API is responsible for passing the spend amount and FeatureKey.
    /// </summary>
    [Serializable]
    public class SpendRewardDefinition
    {
        /// <summary>Feature identifier. Examples: "Store", "Marketplace", "Reward", "Gacha".</summary>
        public string FeatureKey;

        /// <summary>Whether spend rewards are enabled for this feature.</summary>
        public bool IsEnabled = true;

        /// <summary>Percentage of subscriber's spend granted to the referrer (0–100).</summary>
        public double Percent;

        /// <summary>Currency ID spent by the subscriber (source for calculation).</summary>
        public string SourceCurrencyID;

        /// <summary>Currency ID received by the referrer. May differ from SourceCurrencyID.</summary>
        public string TargetCurrencyID;
    }

    // ===================== Action Enum =====================

    public enum ReferralAction
    {
        GetDefinitions,
        GetUserState,
        ActivateReferralCode,
        ClaimInviteReward,
    }
}
