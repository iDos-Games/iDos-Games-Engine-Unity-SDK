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
                if (result.Data.GrantedBaseRewards != null && result.Data.GrantedBaseRewards.Count > 0) IDosGamesData.User.GrantResources(result.Data.GrantedBaseRewards);
                if (result.Data.GrantedPremiumRewards != null && result.Data.GrantedPremiumRewards.Count > 0) IDosGamesData.User.GrantResources(result.Data.GrantedPremiumRewards);
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
                if (result.Data.GrantedBaseRewards != null && result.Data.GrantedBaseRewards.Count > 0) IDosGamesData.User.GrantResources(result.Data.GrantedBaseRewards);
                if (result.Data.GrantedPremiumRewards != null && result.Data.GrantedPremiumRewards.Count > 0) IDosGamesData.User.GrantResources(result.Data.GrantedPremiumRewards);
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
