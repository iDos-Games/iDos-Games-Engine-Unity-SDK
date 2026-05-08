using System.Threading.Tasks;

namespace IDosGames
{
    public static class SocialAPI
    {
        private static string GetEndpoint(SocialAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Social/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(SocialAction action, SocialRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<FriendsListResponse>> GetFriendsList(SocialRequest request)
            => SendRequest<FriendsListResponse>(SocialAction.GetFriendsList, request);

        public static Task<OperationResult<FriendsListResponse>> GetIncomingRequests(SocialRequest request)
            => SendRequest<FriendsListResponse>(SocialAction.GetIncomingRequests, request);

        public static Task<OperationResult<FriendsListResponse>> GetRecommendedFriends(SocialRequest request)
            => SendRequest<FriendsListResponse>(SocialAction.GetRecommendedFriends, request);

        public static Task<OperationResult<FriendActionResponse>> SendFriendRequest(SocialRequest request)
            => SendRequest<FriendActionResponse>(SocialAction.SendFriendRequest, request);

        public static Task<OperationResult<FriendActionResponse>> AcceptFriendRequest(SocialRequest request)
            => SendRequest<FriendActionResponse>(SocialAction.AcceptFriendRequest, request);

        public static Task<OperationResult<FriendActionResponse>> DeclineFriendRequest(SocialRequest request)
            => SendRequest<FriendActionResponse>(SocialAction.DeclineFriendRequest, request);

        public static Task<OperationResult<FriendActionResponse>> RemoveFriend(SocialRequest request)
            => SendRequest<FriendActionResponse>(SocialAction.RemoveFriend, request);

        public static Task<OperationResult<TimelineResponse>> GetTimeline(SocialRequest request)
            => SendRequest<TimelineResponse>(SocialAction.GetTimeline, request);
    }
}
