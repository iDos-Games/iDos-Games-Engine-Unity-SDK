using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class QuestService
    {
        // ---------- Typed events ----------

        /// <summary>Fired after quest definitions are loaded and patched into TitleConfig.</summary>
        public static event Action<QuestDefinitions> OnQuestDefinitionsLoaded;

        /// <summary>Fired after user quest state is loaded and patched into UserData.</summary>
        public static event Action<UserQuestState> OnUserQuestStateLoaded;

        /// <summary>Fired after cycle windows are refreshed (server returned success).</summary>
        public static event Action OnQuestCyclesRefreshed;

        /// <summary>Fired after a quest reward is claimed; carries the full server response.</summary>
        public static event Action<ClaimQuestRewardResponse> OnQuestRewardClaimed;

        /// <summary>Fired after a milestone reward is claimed; carries the full server response.</summary>
        public static event Action<ClaimMilestoneRewardResponse> OnMilestoneRewardClaimed;

        /// <summary>Fired after quest progress is successfully added; carries the full server response.</summary>
        public static event Action<AddQuestProgressResponse> OnQuestProgressAdded;

        // ---------- Helpers ----------

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static QuestRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// Loads quest definitions from the server and patches them into TitleConfig.
        /// Fires OnQuestDefinitionsLoaded on success.
        /// </summary>
        public static async Task<OperationResult<QuestDefinitions>> GetQuestDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await QuestAPI.GetQuestDefinitions(request);
            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchQuest(result.Data);
                OnQuestDefinitionsLoaded?.Invoke(result.Data);
            }
            return result;
        }

        /// <summary>
        /// Loads the player's current quest state and patches it into UserData.
        /// Pass autoRefreshCycles=true (default) to let the server update cycle windows first.
        /// Fires OnUserQuestStateLoaded on success.
        /// </summary>
        public static async Task<OperationResult<GetUserQuestStateResponse>> GetUserQuestState(bool autoRefreshCycles = true)
        {
            var request = CreateBaseRequest();
            request.AutoRefreshCycles = autoRefreshCycles;
            var result = await QuestAPI.GetUserQuestState(request);
            if (result.Success && result.Data != null)
            {
                IDosGamesData.User.ApplyQuest(result.Data.State ?? new UserQuestState());
                OnUserQuestStateLoaded?.Invoke(result.Data.State);
            }
            return result;
        }

        /// <summary>
        /// Asks the server to refresh cycle windows (open new windows, remove stale ones).
        /// Fires OnQuestCyclesRefreshed on success.
        /// </summary>
        public static async Task<OperationResult<SuccessResponse>> RefreshQuestCycles()
        {
            var request = CreateBaseRequest();
            var result = await QuestAPI.RefreshQuestCycles(request);
            if (result.Success)
                OnQuestCyclesRefreshed?.Invoke();
            return result;
        }

        /// <summary>
        /// Claims the reward for a completed quest.
        /// For cyclic quests provide cycleID; pass null for permanent quests.
        /// Locally applies resources and patches quest status to Claimed.
        /// Fires OnQuestRewardClaimed on success.
        /// </summary>
        public static async Task<OperationResult<ClaimQuestRewardResponse>> ClaimQuestReward(string questID, string cycleID = null)
        {
            if (string.IsNullOrWhiteSpace(questID))
            {
                Debug.LogWarning("[QuestService] ClaimQuestReward: QuestID is required.");
                return OperationResult<ClaimQuestRewardResponse>.Fail("QuestID is required");
            }

            var request = CreateBaseRequest();
            request.QuestID = questID.Trim();
            request.CycleID = string.IsNullOrWhiteSpace(cycleID) ? null : cycleID.Trim();
            request.RelatedEntityID = $"quest_claim_{request.CycleID}_{request.QuestID}_{Guid.NewGuid():N}";

            var result = await QuestAPI.ClaimQuestReward(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchQuestStatus(data.QuestID, data.CycleID, QuestStatus.Claimed);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnQuestRewardClaimed?.Invoke(data);
            }
            return result;
        }

        /// <summary>
        /// Claims a milestone reward for a cycle.
        /// Locally applies resources and records the milestone as claimed.
        /// Fires OnMilestoneRewardClaimed on success.
        /// </summary>
        public static async Task<OperationResult<ClaimMilestoneRewardResponse>> ClaimMilestoneReward(string cycleID, string milestoneID)
        {
            if (string.IsNullOrWhiteSpace(cycleID))
            {
                Debug.LogWarning("[QuestService] ClaimMilestoneReward: CycleID is required.");
                return OperationResult<ClaimMilestoneRewardResponse>.Fail("CycleID is required");
            }
            if (string.IsNullOrWhiteSpace(milestoneID))
            {
                Debug.LogWarning("[QuestService] ClaimMilestoneReward: MilestoneID is required.");
                return OperationResult<ClaimMilestoneRewardResponse>.Fail("MilestoneID is required");
            }

            var request = CreateBaseRequest();
            request.CycleID = cycleID.Trim();
            request.MilestoneID = milestoneID.Trim();
            request.RelatedEntityID = $"milestone_claim_{request.CycleID}_{request.MilestoneID}_{Guid.NewGuid():N}";

            var result = await QuestAPI.ClaimMilestoneReward(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchMilestoneClaimed(data.CycleID, data.MilestoneID, data.CompletedQuestsCount);

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnMilestoneRewardClaimed?.Invoke(data);
            }
            return result;
        }

        /// <summary>
        /// Submits progress for a ClientApi-sourced quest metric.
        /// ProgressValue must be >= 0. Server validates the MetricID and bans abusers.
        /// Fires OnQuestProgressAdded on success.
        /// </summary>
        public static async Task<OperationResult<AddQuestProgressResponse>> AddQuestProgress(string metricID, long progressValue)
        {
            if (string.IsNullOrWhiteSpace(metricID))
            {
                Debug.LogWarning("[QuestService] AddQuestProgress: MetricID is required.");
                return OperationResult<AddQuestProgressResponse>.Fail("MetricID is required");
            }
            if (progressValue < 0)
            {
                Debug.LogWarning("[QuestService] AddQuestProgress: ProgressValue must be >= 0.");
                return OperationResult<AddQuestProgressResponse>.Fail("ProgressValue must be >= 0");
            }

            var request = CreateBaseRequest();
            request.MetricID = metricID.Trim();
            request.ProgressValue = progressValue;

            var result = await QuestAPI.AddQuestProgress(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchQuestProgressUpdates(data.Updates);

                OnQuestProgressAdded?.Invoke(data);
            }
            return result;
        }
    }
}
