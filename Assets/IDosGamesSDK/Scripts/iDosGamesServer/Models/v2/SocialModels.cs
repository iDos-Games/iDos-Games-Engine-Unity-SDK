using System;
using System.Collections.Generic;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum SocialAction
    {
        GetFriendsList,
        GetIncomingRequests,
        GetRecommendedFriends,
        SendFriendRequest,
        AcceptFriendRequest,
        DeclineFriendRequest,
        RemoveFriend,
        GetTimeline,
    }

    public enum TimelineEventType
    {
        Attack,
        Raid,
        FriendAdd
    }

    public enum FriendActionStatus
    {
        RequestSent,
        Accepted,
        Declined,
        Removed
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class SocialRequest : IGSRequest
    {
        public string TargetUserID { get; set; }
        public int Limit { get; set; } = 5;
    }

    // =================================================================================
    // RESPONSES / MODELS
    // =================================================================================

    [Serializable]
    public class FriendPublicProfile
    {
        public string UserID { get; set; }
        public UserPublicDataModel PublicData { get; set; }
    }

    [Serializable]
    public class FriendActionResponse
    {
        // Универсальное поле для ID целевого пользователя (кто принял, кого удалили и т.д.)
        public string TargetUserID { get; set; }
        public FriendActionStatus Status { get; set; }
    }

    [Serializable]
    public class SocialTimelineEventDocument
    {
        public string ID { get; set; }

        public string OwnerUserID { get; set; } // Чья лента
        public string ActorUserID { get; set; } // Кто сделал действие
        public UserPublicDataModel ActorProfile { get; set; } // Снапшот профиля

        public TimelineEventType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public long Amount { get; set; }
        public bool IsBlocked { get; set; }
        public string TargetObjectName { get; set; }
    }

    [Serializable]
    public class FriendsListResponse
    {
        public List<FriendPublicProfile> Friends { get; set; } = new();
    }

    [Serializable]
    public class TimelineResponse
    {
        public List<SocialTimelineEventDocument> Events { get; set; } = new();
    }
}
