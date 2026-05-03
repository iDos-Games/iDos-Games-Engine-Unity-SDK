using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class QuestAPI
    {
        /// <summary>
        /// Generates a URL for the request.
        /// </summary>
        private static string GetEndpoint(QuestAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{templateID}/{titleID}/Client/Quest/{action}/{userID}";
        }

        /// <summary>
        /// A generic method for sending a request that hides the details of HttpService.
        /// </summary>
        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(QuestAction action, QuestRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        public static async Task<OperationResult<QuestDefinitions>> GetQuestDefinitions(QuestRequest request)
        {
            return await SendRequest<QuestDefinitions>(QuestAction.GetQuestDefinitions, request);
        }

        public static async Task<OperationResult<GetUserQuestStateResponse>> GetUserQuestState(QuestRequest request)
        {
            return await SendRequest<GetUserQuestStateResponse>(QuestAction.GetUserQuestState, request);
        }

        public static async Task<OperationResult<SuccessResponse>> RefreshQuestCycles(QuestRequest request)
        {
            return await SendRequest<SuccessResponse>(QuestAction.RefreshQuestCycles, request);
        }

        public static async Task<OperationResult<ClaimQuestRewardResponse>> ClaimQuestReward(QuestRequest request)
        {
            return await SendRequest<ClaimQuestRewardResponse>(QuestAction.ClaimQuestReward, request);
        }

        public static async Task<OperationResult<ClaimMilestoneRewardResponse>> ClaimMilestoneReward(QuestRequest request)
        {
            return await SendRequest<ClaimMilestoneRewardResponse>(QuestAction.ClaimMilestoneReward, request);
        }

        public static async Task<OperationResult<AddProgressResponse>> AddProgress(QuestRequest request)
        {
            return await SendRequest<AddProgressResponse>(QuestAction.AddProgress, request);
        }
    }
}
