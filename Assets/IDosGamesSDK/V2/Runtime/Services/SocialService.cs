using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class SocialService
    {
        // ── Events ───────────────────────────────────────────────────────────
        public static event Action<FriendsListResponse> OnFriendsListLoaded;
        public static event Action<FriendsListResponse> OnIncomingRequestsLoaded;
        public static event Action<FriendsListResponse> OnRecommendedFriendsLoaded;
        public static event Action<FriendActionResponse> OnFriendRequestSent;
        public static event Action<FriendActionResponse> OnFriendRequestAccepted;
        public static event Action<FriendActionResponse> OnFriendRequestDeclined;
        public static event Action<FriendActionResponse> OnFriendRemoved;
        public static event Action<TimelineResponse> OnTimelineLoaded;

        // ── Helpers ──────────────────────────────────────────────────────────
        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static SocialRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ── Actions ──────────────────────────────────────────────────────────

        /// <summary>
        /// Fetches the current user's accepted friends list.
        /// On success, patches local social state and fires <see cref="OnFriendsListLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<FriendsListResponse>> GetFriendsList()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetFriendsList(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplySocialFriendsList(result.Data.Friends);
                OnFriendsListLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches incoming friend requests for the current user.
        /// On success, patches local social state and fires <see cref="OnIncomingRequestsLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<FriendsListResponse>> GetIncomingRequests()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetIncomingRequests(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplySocialIncomingRequests(result.Data.Friends);
                OnIncomingRequestsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches recommended friends for the current user.
        /// On success fires <see cref="OnRecommendedFriendsLoaded"/>. No local state patch (transient list).
        /// </summary>
        public static async Task<OperationResult<FriendsListResponse>> GetRecommendedFriends(int limit = 5)
        {
            var request = CreateBaseRequest();
            request.Limit = limit;
            var result = await SocialAPI.GetRecommendedFriends(request);

            if (result.Success && result.Data != null)
            {
                OnRecommendedFriendsLoaded?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Sends a friend request to <paramref name="targetUserID"/>.
        /// On success, adds target to local OutgoingRequests and fires <see cref="OnFriendRequestSent"/>.
        /// </summary>
        public static async Task<OperationResult<FriendActionResponse>> SendFriendRequest(string targetUserID)
        {
            if (string.IsNullOrWhiteSpace(targetUserID))
            {
                Debug.LogWarning("[SocialService] SendFriendRequest: targetUserID is required.");
                return OperationResult<FriendActionResponse>.Fail("targetUserID is required.");
            }

            if (targetUserID == Ctx.UserID)
            {
                Debug.LogWarning("[SocialService] SendFriendRequest: cannot send request to yourself.");
                return OperationResult<FriendActionResponse>.Fail("Cannot send friend request to yourself.");
            }

            var request = CreateBaseRequest();
            request.TargetUserID = targetUserID;
            var result = await SocialAPI.SendFriendRequest(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchSocialAddOutgoingRequest(targetUserID);
                OnFriendRequestSent?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Accepts an incoming friend request from <paramref name="requesterUserID"/>.
        /// On success, moves requester from IncomingRequests to Accepted locally and fires <see cref="OnFriendRequestAccepted"/>.
        /// </summary>
        public static async Task<OperationResult<FriendActionResponse>> AcceptFriendRequest(string requesterUserID)
        {
            if (string.IsNullOrWhiteSpace(requesterUserID))
            {
                Debug.LogWarning("[SocialService] AcceptFriendRequest: requesterUserID is required.");
                return OperationResult<FriendActionResponse>.Fail("requesterUserID is required.");
            }

            var request = CreateBaseRequest();
            request.TargetUserID = requesterUserID;
            var result = await SocialAPI.AcceptFriendRequest(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchSocialAcceptFriend(requesterUserID);
                OnFriendRequestAccepted?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Declines an incoming friend request from <paramref name="requesterUserID"/>.
        /// On success, removes requester from IncomingRequests locally and fires <see cref="OnFriendRequestDeclined"/>.
        /// </summary>
        public static async Task<OperationResult<FriendActionResponse>> DeclineFriendRequest(string requesterUserID)
        {
            if (string.IsNullOrWhiteSpace(requesterUserID))
            {
                Debug.LogWarning("[SocialService] DeclineFriendRequest: requesterUserID is required.");
                return OperationResult<FriendActionResponse>.Fail("requesterUserID is required.");
            }

            var request = CreateBaseRequest();
            request.TargetUserID = requesterUserID;
            var result = await SocialAPI.DeclineFriendRequest(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchSocialRemoveIncomingRequest(requesterUserID);
                OnFriendRequestDeclined?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Removes an existing friend by <paramref name="friendUserID"/>.
        /// On success, removes friend from local Accepted list and fires <see cref="OnFriendRemoved"/>.
        /// </summary>
        public static async Task<OperationResult<FriendActionResponse>> RemoveFriend(string friendUserID)
        {
            if (string.IsNullOrWhiteSpace(friendUserID))
            {
                Debug.LogWarning("[SocialService] RemoveFriend: friendUserID is required.");
                return OperationResult<FriendActionResponse>.Fail("friendUserID is required.");
            }

            var request = CreateBaseRequest();
            request.TargetUserID = friendUserID;
            var result = await SocialAPI.RemoveFriend(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.PatchSocialRemoveFriend(friendUserID);
                OnFriendRemoved?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Fetches the current user's social timeline.
        /// On success, stores events locally and fires <see cref="OnTimelineLoaded"/>.
        /// </summary>
        public static async Task<OperationResult<TimelineResponse>> GetTimeline()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetTimeline(request);

            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplySocialTimeline(result.Data.Events);
                OnTimelineLoaded?.Invoke(result.Data);
            }

            return result;
        }
    }
}
