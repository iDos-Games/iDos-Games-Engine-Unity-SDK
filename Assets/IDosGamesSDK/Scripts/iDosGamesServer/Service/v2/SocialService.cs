using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class SocialService
    {
        public static event Action<List<FriendPublicProfile>> OnFriendsListUpdated;
        public static event Action<List<FriendPublicProfile>> OnIncomingRequestsUpdated;
        public static event Action<List<FriendPublicProfile>> OnRecommendedFriendsUpdated;

        public static event Action<FriendActionResponse> OnSendFriendRequestSuccess;
        public static event Action<FriendActionResponse> OnAcceptFriendRequestSuccess;
        public static event Action<FriendActionResponse> OnDeclineFriendRequestSuccess;
        public static event Action<FriendActionResponse> OnRemoveFriendSuccess;

        public static event Action<List<SocialTimelineEventDocument>> OnTimelineUpdated;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        /// <summary>
        /// Creates a basic request with required authorization fields
        /// </summary>
        private static SocialRequest CreateBaseRequest()
        {
            return new SocialRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        // =================================================================================
        // FRIENDS
        // =================================================================================

        public static async Task<OperationResult<List<FriendPublicProfile>>> GetFriendsList()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetFriendsList(request);

            if (result.Success)
            {
                OnFriendsListUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<List<FriendPublicProfile>>> GetIncomingRequests()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetIncomingRequests(request);

            if (result.Success)
            {
                OnIncomingRequestsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<List<FriendPublicProfile>>> GetRecommendedFriends(int limit = 5)
        {
            var request = CreateBaseRequest();
            request.Limit = limit;

            var result = await SocialAPI.GetRecommendedFriends(request);

            if (result.Success)
            {
                OnRecommendedFriendsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<FriendActionResponse>> SendFriendRequest(string targetUserId)
        {
            var request = CreateBaseRequest();
            request.TargetUserID = targetUserId;

            var result = await SocialAPI.SendFriendRequest(request);

            if (result.Success)
            {
                OnSendFriendRequestSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<FriendActionResponse>> AcceptFriendRequest(string requesterUserId)
        {
            var request = CreateBaseRequest();
            request.TargetUserID = requesterUserId;

            var result = await SocialAPI.AcceptFriendRequest(request);

            if (result.Success)
            {
                OnAcceptFriendRequestSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<FriendActionResponse>> DeclineFriendRequest(string requesterUserId)
        {
            var request = CreateBaseRequest();
            request.TargetUserID = requesterUserId;

            var result = await SocialAPI.DeclineFriendRequest(request);

            if (result.Success)
            {
                OnDeclineFriendRequestSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<FriendActionResponse>> RemoveFriend(string friendUserId)
        {
            var request = CreateBaseRequest();
            request.TargetUserID = friendUserId;

            var result = await SocialAPI.RemoveFriend(request);

            if (result.Success)
            {
                OnRemoveFriendSuccess?.Invoke(result.Data);
            }

            return result;
        }

        // =================================================================================
        // TIMELINE
        // =================================================================================

        public static async Task<OperationResult<List<SocialTimelineEventDocument>>> GetTimeline()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetTimeline(request);

            if (result.Success)
            {
                OnTimelineUpdated?.Invoke(result.Data);
            }

            return result;
        }
    }
}
