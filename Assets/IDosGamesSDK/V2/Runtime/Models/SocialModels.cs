using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Request for all Social actions.
    /// <list type="bullet">
    ///   <item><see cref="SocialAction.GetRecommendedFriends"/> — uses <see cref="Limit"/>.</item>
    ///   <item><see cref="SocialAction.SendFriendRequest"/>, <see cref="SocialAction.AcceptFriendRequest"/>,
    ///         <see cref="SocialAction.DeclineFriendRequest"/>, <see cref="SocialAction.RemoveFriend"/> — use <see cref="TargetUserID"/>.</item>
    ///   <item>All other actions — only base fields required.</item>
    /// </list>
    /// </summary>
    [Serializable]
    public class SocialRequest : BaseRequest
    {
        /// <summary>Target user ID for friend actions (Send/Accept/Decline/Remove).</summary>
        public string TargetUserID { get; set; }

        /// <summary>Max results for <see cref="SocialAction.GetRecommendedFriends"/>. Default: 5.</summary>
        public int Limit { get; set; } = 5;
    }

    // ── Enums ────────────────────────────────────────────────────────────────

    [JsonConverter(typeof(StringEnumConverter))]
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

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FriendActionStatus
    {
        RequestSent,
        Accepted,
        Declined,
        Removed,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimelineEventType
    {
        Attack,
        Raid,
        FriendAdd,
    }

    // ── Response models ──────────────────────────────────────────────────────

    /// <summary>Public profile of a friend returned by the server.</summary>
    [Serializable]
    public class FriendPublicProfile
    {
        public string UserID { get; set; }
        public UserPublicDataModel PublicData { get; set; }
    }

    /// <summary>Response for GetFriendsList, GetIncomingRequests, GetRecommendedFriends.</summary>
    [Serializable]
    public class FriendsListResponse
    {
        public List<FriendPublicProfile> Friends { get; set; } = new();
    }

    /// <summary>Response for SendFriendRequest, AcceptFriendRequest, DeclineFriendRequest, RemoveFriend.</summary>
    [Serializable]
    public class FriendActionResponse
    {
        /// <summary>ID of the user who was acted upon.</summary>
        public string TargetUserID { get; set; }
        public FriendActionStatus Status { get; set; }
    }

    /// <summary>Response for GetTimeline.</summary>
    [Serializable]
    public class TimelineResponse
    {
        public List<SocialTimelineEvent> Events { get; set; } = new();
    }

    // ── State / document models ──────────────────────────────────────────────

    /// <summary>
    /// Client-side representation of a single timeline event.
    /// Mirrors <c>SocialTimelineEventDocument</c> on the server (Bson attributes stripped).
    /// </summary>
    [Serializable]
    public class SocialTimelineEvent
    {
        /// <summary>Owner of the timeline (whose feed this event belongs to).</summary>
        public string OwnerUserID { get; set; }

        /// <summary>Actor who performed the action (attacker, raider, friend requester).</summary>
        public string ActorUserID { get; set; }

        /// <summary>Snapshot of the actor's public profile at the time of the event.</summary>
        public UserPublicDataModel ActorProfile { get; set; }

        public TimelineEventType Type { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Impact on the owner caused by this event.
        /// Consume.Standard — resources taken from owner (e.g. stolen coins in Raid).
        /// Grant.Standard   — resources granted to owner (if applicable).
        /// null             — no resource impact (e.g. FriendAdd).
        /// </summary>
        public ResourceOperation OwnerImpact { get; set; }

        /// <summary>Attack only: whether the attack was blocked by the victim's shield.</summary>
        public bool IsBlocked { get; set; }

        /// <summary>Name of the target object (e.g. "Building 3" for Attack). null if not applicable.</summary>
        public string TargetObjectName { get; set; }
    }

    /// <summary>Local social state cached on the client.</summary>
    [Serializable]
    public class UserSocialState
    {
        /// <summary>Accepted friend IDs.</summary>
        public List<string> Accepted { get; set; } = new();

        /// <summary>Incoming friend request user IDs.</summary>
        public List<string> IncomingRequests { get; set; } = new();

        /// <summary>Outgoing friend request user IDs.</summary>
        public List<string> OutgoingRequests { get; set; } = new();

        public List<SocialTimelineEvent> Timeline { get; set; } = new();
    }
}
