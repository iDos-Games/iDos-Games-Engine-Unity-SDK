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
        public static event Action<ClaimQuestRewardResponse> OnQuestRewardClaimed;
        public static event Action<ClaimMilestoneRewardResponse> OnMilestoneRewardClaimed;
        public static event Action<AddProgressResponse> OnProgressAdded;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();

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

            if (result.Success)
            {
                IDosGamesData.Config.PatchQuestDefinitions(result.Data);
                OnQuestDefinitionsUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<GetUserQuestStateResponse>> GetUserQuestState(bool autoRefreshCycles = true)
        {
            var request = CreateBaseRequest();
            request.AutoRefreshCycles = autoRefreshCycles;

            var result = await QuestAPI.GetUserQuestState(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyQuests(result.Data.State);
                OnQuestStateUpdated?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<ClaimQuestRewardResponse>> ClaimQuestReward(string questId, string cycleId = null)
        {
            var request = CreateBaseRequest();
            request.QuestID = questId;

            cycleId = (cycleId ?? "").Trim();
            request.CycleID = string.IsNullOrEmpty(cycleId) ? null : cycleId;

            var result = await QuestAPI.ClaimQuestReward(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchQuestStatus(result.Data.QuestID, result.Data.CycleID, result.Data.NewStatus);
                IDosGamesData.User.GrantResources(result.Data.GrantedRewards);
                OnQuestRewardClaimed?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<ClaimMilestoneRewardResponse>> ClaimMilestoneReward(string cycleId, string milestoneId)
        {
            var request = CreateBaseRequest();
            request.CycleID = cycleId;
            request.MilestoneID = milestoneId;

            var result = await QuestAPI.ClaimMilestoneReward(request);

            if (result.Success)
            {
                IDosGamesData.User.PatchClaimedMilestone(result.Data.CycleID, result.Data.MilestoneID);
                IDosGamesData.User.GrantResources(result.Data.GrantedRewards);
                OnMilestoneRewardClaimed?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<AddProgressResponse>> AddProgress(string metricId, long value)
        {
            var request = CreateBaseRequest();
            request.MetricID = metricId;
            request.Value = value;

            var result = await QuestAPI.AddProgress(request);

            if (result.Success)
            {
                foreach (var update in result.Data.Updates) IDosGamesData.User.PatchQuestObjectiveProgress(update);

                OnProgressAdded?.Invoke(result.Data);
            }

            return result;
        }
    }
}
