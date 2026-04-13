using IDosGames.TitlePublicConfiguration;
using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum ReferralAction
    {
        GetDefinitions,
        GetUserState,
        ActivateReferralCode,
        ClaimInviteReward,
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class ReferralRequest : IGSRequest
    {
        /// <summary>UserID реферера. Используется в ActivateReferralCode.</summary>
        public string ReferralCode { get; set; }

        /// <summary>ID invite-награды для ручного клейма. Используется в ClaimInviteReward.</summary>
        public string InviteRewardID { get; set; }
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class GetReferralDefinitionsResponse
    {
        public ReferralDefinitions ReferralDefinitions { get; set; }
    }

    [Serializable]
    public class GetUserReferralStateResponse
    {
        public UserReferralState Referral { get; set; }
    }

    [Serializable]
    public class ActivateReferralCodeResponse
    {
        public string ReferralCode { get; set; }
        public bool IsFirstActivation { get; set; }
        public List<ItemOrCurrency> GrantedResources { get; set; } = new();
    }

    [Serializable]
    public class ClaimInviteRewardResponse
    {
        public string RewardID { get; set; }
        public List<ItemOrCurrency> GrantedResources { get; set; } = new();
    }

    // =================================================================================
    // MODELS — Config (stored in TitleConfig)
    // =================================================================================

    [Serializable]
    public class ReferralDefinitions
    {
        /// <summary>Включена ли реферальная система для этого тайтла.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Единоразовая награда за первую активацию чужого реферального кода.</summary>
        public List<ItemOrCurrency> ActivationReward { get; set; }

        /// <summary>Поэтапные награды рефереру за достижение порогов подписчиков.</summary>
        public List<InviteRewardDefinition> InviteRewards { get; set; }

        /// <summary>Настройки процентных реферальных отчислений по фичам.</summary>
        public List<SpendRewardDefinition> SpendRewards { get; set; }
    }

    [Serializable]
    public class InviteRewardDefinition
    {
        /// <summary>Уникальный стабильный ID — используется как ключ в user state.</summary>
        public string RewardID { get; set; }

        /// <summary>Порог подписчиков для разблокировки.</summary>
        public int RequiredFollowersCount { get; set; }

        /// <summary>Что выдаётся рефереру.</summary>
        public List<ItemOrCurrency> Rewards { get; set; }

        /// <summary>
        /// true  — выдаётся автоматически при достижении порога.
        /// false — пользователь сам забирает через ClaimInviteReward.
        /// </summary>
        public bool AutoGrant { get; set; } = false;
    }

    [Serializable]
    public class SpendRewardDefinition
    {
        /// <summary>Идентификатор фичи. Примеры: "Store", "Marketplace", "Reward", "Gacha".</summary>
        public string FeatureKey { get; set; }

        /// <summary>Включено ли отчисление для этой фичи.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Процент от потраченной суммы, который получает реферер (0–100).</summary>
        public double Percent { get; set; }

        /// <summary>ID валюты, которую тратит подписчик (источник для расчёта).</summary>
        public string SourceCurrencyID { get; set; }

        /// <summary>ID валюты, которую получает реферер.</summary>
        public string TargetCurrencyID { get; set; }
    }

    // =================================================================================
    // MODELS — User State (stored in UserData)
    // =================================================================================

    [Serializable]
    public class UserReferralState
    {
        /// <summary>UserID реферера, на которого подписан пользователь. null — не подписан.</summary>
        public string SubscribedToUserID { get; set; }

        /// <summary>Была ли уже выдана единоразовая activation reward.</summary>
        public bool ActivationRewardGranted { get; set; } = false;

        /// <summary>Количество пользователей, подписанных на этого пользователя.</summary>
        public int FollowersCount { get; set; } = 0;

        /// <summary>Список UserID подписчиков.</summary>
        public List<string> FollowerIDs { get; set; } = new();

        /// <summary>Состояния invite-наград.</summary>
        public List<ReferralInviteRewardState> InviteRewardStates { get; set; } = new();

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Serializable]
    public class ReferralInviteRewardState
    {
        /// <summary>Совпадает с InviteRewardDefinition.RewardID.</summary>
        public string RewardID { get; set; }

        public bool IsClaimed { get; set; } = false;

        public DateTime? ClaimedAt { get; set; }
    }
}
