using System.Threading.Tasks;

namespace IDosGames
{
    public static class QuestAPI
    {
        private static string GetEndpoint(QuestAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Quest/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(QuestAction action, QuestRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<QuestDefinitions>> GetQuestDefinitions(QuestRequest request)
            => SendRequest<QuestDefinitions>(QuestAction.GetQuestDefinitions, request);

        public static Task<OperationResult<GetUserQuestStateResponse>> GetUserQuestState(QuestRequest request)
            => SendRequest<GetUserQuestStateResponse>(QuestAction.GetUserQuestState, request);

        public static Task<OperationResult<SuccessResponse>> RefreshQuestCycles(QuestRequest request)
            => SendRequest<SuccessResponse>(QuestAction.RefreshQuestCycles, request);

        public static Task<OperationResult<ClaimQuestRewardResponse>> ClaimQuestReward(QuestRequest request)
            => SendRequest<ClaimQuestRewardResponse>(QuestAction.ClaimQuestReward, request);

        public static Task<OperationResult<ClaimMilestoneRewardResponse>> ClaimMilestoneReward(QuestRequest request)
            => SendRequest<ClaimMilestoneRewardResponse>(QuestAction.ClaimMilestoneReward, request);

        public static Task<OperationResult<AddQuestProgressResponse>> AddQuestProgress(QuestRequest request)
            => SendRequest<AddQuestProgressResponse>(QuestAction.AddQuestProgress, request);
    }
}
