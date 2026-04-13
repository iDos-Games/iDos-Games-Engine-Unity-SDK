using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class SocialAPI
    {
        private static string GetEndpoint(SocialAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Social/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(SocialAction action, SocialRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        // =================================================================================
        // PUBLIC METHODS
        // =================================================================================

        public static async Task<OperationResult<FriendsListResponse>> GetFriendsList(SocialRequest request)
        {
            return await SendRequest<FriendsListResponse>(SocialAction.GetFriendsList, request);
        }

        public static async Task<OperationResult<FriendsListResponse>> GetIncomingRequests(SocialRequest request)
        {
            return await SendRequest<FriendsListResponse>(SocialAction.GetIncomingRequests, request);
        }

        public static async Task<OperationResult<FriendsListResponse>> GetRecommendedFriends(SocialRequest request)
        {
            return await SendRequest<FriendsListResponse>(SocialAction.GetRecommendedFriends, request);
        }

        public static async Task<OperationResult<FriendActionResponse>> SendFriendRequest(SocialRequest request)
        {
            return await SendRequest<FriendActionResponse>(SocialAction.SendFriendRequest, request);
        }

        public static async Task<OperationResult<FriendActionResponse>> AcceptFriendRequest(SocialRequest request)
        {
            return await SendRequest<FriendActionResponse>(SocialAction.AcceptFriendRequest, request);
        }

        public static async Task<OperationResult<FriendActionResponse>> DeclineFriendRequest(SocialRequest request)
        {
            return await SendRequest<FriendActionResponse>(SocialAction.DeclineFriendRequest, request);
        }

        public static async Task<OperationResult<FriendActionResponse>> RemoveFriend(SocialRequest request)
        {
            return await SendRequest<FriendActionResponse>(SocialAction.RemoveFriend, request);
        }

        public static async Task<OperationResult<TimelineResponse>> GetTimeline(SocialRequest request)
        {
            return await SendRequest<TimelineResponse>(SocialAction.GetTimeline, request);
        }
    }
}
