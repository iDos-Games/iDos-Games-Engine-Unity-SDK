using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class SocialService
    {
        public static event Action<FriendsListResponse> OnFriendsListUpdated;
        public static event Action<FriendsListResponse> OnIncomingRequestsUpdated;
        public static event Action<FriendsListResponse> OnRecommendedFriendsUpdated;

        public static event Action<FriendActionResponse> OnSendFriendRequestSuccess;
        public static event Action<FriendActionResponse> OnAcceptFriendRequestSuccess;
        public static event Action<FriendActionResponse> OnDeclineFriendRequestSuccess;
        public static event Action<FriendActionResponse> OnRemoveFriendSuccess;

        public static event Action<TimelineResponse> OnTimelineUpdated;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
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

        public static async Task<OperationResult<FriendsListResponse>> GetFriendsList()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetFriendsList(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchSocialAccepted(result.Data.Friends.Select(f => f.UserID).ToList());
                OnFriendsListUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<FriendsListResponse>> GetIncomingRequests()
        {
            var request = CreateBaseRequest();
            var result = await SocialAPI.GetIncomingRequests(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchSocialIncomingRequests(result.Data.Friends.Select(f => f.UserID).ToList());
                OnIncomingRequestsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<FriendsListResponse>> GetRecommendedFriends(int limit = 5)
        {
            var request = CreateBaseRequest();
            request.Limit = limit;

            var result = await SocialAPI.GetRecommendedFriends(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchSocialRecommended(result.Data.Friends.Select(f => f.UserID).ToList());
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
                IDosGamesData.User.PatchSocialOutgoingAdd(targetUserId);
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
                IDosGamesData.User.PatchSocialAcceptRequest(requesterUserId);
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
                IDosGamesData.User.PatchSocialRemoveIncoming(requesterUserId);
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
                IDosGamesData.User.PatchSocialRemoveFriend(friendUserId);
                OnRemoveFriendSuccess?.Invoke(result.Data);
            }

            return result;
        }

        // =================================================================================
        // TIMELINE
        // =================================================================================

        public static async Task<OperationResult<TimelineResponse>> GetTimeline()
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
