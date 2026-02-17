using System;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class QuestService
    {
        public static event Action<QuestDefinitions> OnQuestDefinitionsUpdated;
        public static event Action<GetUserQuestStateResponse> OnQuestStateUpdated;
        public static event Action<SuccessResponse> OnQuestCyclesRefreshed;
        public static event Action<ClaimQuestRewardResponse> OnQuestRewardClaimed;
        public static event Action<ClaimMilestoneRewardResponse> OnMilestoneRewardClaimed;
        public static event Action<AddProgressResponse> OnProgressAdded;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();

        private static QuestRequest CreateBaseRequest()
        {
            return new QuestRequest
            {
                UserID = Ctx.UserID,
                ClientSessionTicket = Ctx.ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        public static async Task<OperationResult<QuestDefinitions>> GetQuestDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await QuestAPI.GetQuestDefinitions(request);

            if (result.Success) OnQuestDefinitionsUpdated?.Invoke(result.Data);

            return result;
        }

        public static async Task<OperationResult<GetUserQuestStateResponse>> GetUserQuestState(bool autoRefreshCycles = true)
        {
            var request = CreateBaseRequest();
            request.AutoRefreshCycles = autoRefreshCycles;

            var result = await QuestAPI.GetUserQuestState(request);

            if (result.Success) OnQuestStateUpdated?.Invoke(result.Data);

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> RefreshQuestCycles()
        {
            var request = CreateBaseRequest();
            var result = await QuestAPI.RefreshQuestCycles(request);

            if (result.Success) OnQuestCyclesRefreshed?.Invoke(result.Data);

            return result;
        }

        public static async Task<OperationResult<ClaimQuestRewardResponse>> ClaimQuestReward(string questId, string cycleId = null)
        {
            var request = CreateBaseRequest();
            request.QuestID = questId;

            cycleId = (cycleId ?? "").Trim();
            request.CycleID = string.IsNullOrEmpty(cycleId) ? null : cycleId;

            var result = await QuestAPI.ClaimQuestReward(request);

            if (result.Success) OnQuestRewardClaimed?.Invoke(result.Data);

            return result;
        }

        public static async Task<OperationResult<ClaimMilestoneRewardResponse>> ClaimMilestoneReward(string cycleId, string milestoneId)
        {
            var request = CreateBaseRequest();
            request.CycleID = cycleId;
            request.MilestoneID = milestoneId;

            var result = await QuestAPI.ClaimMilestoneReward(request);

            if (result.Success) OnMilestoneRewardClaimed?.Invoke(result.Data);

            return result;
        }

        public static async Task<OperationResult<AddProgressResponse>> AddProgress(string metricId, long value)
        {
            var request = CreateBaseRequest();
            request.MetricID = metricId;
            request.Value = value;

            var result = await QuestAPI.AddProgress(request);

            if (result.Success) OnProgressAdded?.Invoke(result.Data);

            return result;
        }
    }
}
