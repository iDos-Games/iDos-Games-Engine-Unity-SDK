using System;
using System.Threading.Tasks;

namespace IDosGames
{
    public static class UserService
    {
        public static event Action<ClientState> OnClientStateReceived;
        public static event Action<UserInventoryState> OnUserInventoryReceived;
        public static event Action<UserEventTokensState> OnEventTokensReceived;
        public static event Action<UsageTimeStats> OnUsageTimeReceived;
        public static event Action<SuccessResponse> OnUserAccountDeleted;

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static UserRequest CreateBaseRequest()
        {
            return new UserRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
                RelatedEntityID = Guid.NewGuid().ToString(),
            };
        }

        public static async Task<OperationResult<ClientState>> GetClientState()
        {
            var request = CreateBaseRequest();

            var result = await UserAPI.GetClientState(request);

            if (result.Success)
            {
                IDosGamesData.Config.ApplyTitlePublicConfiguration(result.Data.Title);
                IDosGamesData.User.ApplyUserState(result.Data.User);
                OnClientStateReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UserInventoryState>> GetUserInventory()
        {
            var request = CreateBaseRequest();
            var result = await UserAPI.GetInventory(request);

            if (result.Success)
            {
                //IDosGamesData.User.ApplyInventory(result.Data.InventoryV2);
                OnUserInventoryReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UserEventTokensState>> GetEventTokens()
        {
            var request = CreateBaseRequest();
            var result = await UserAPI.GetEventTokens(request);

            if (result.Success)
            {
                //IDosGamesData.User.ApplyEventToken(result.Data);
                OnEventTokensReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UsageTimeStats>> GetUsageTime()
        {
            var request = CreateBaseRequest();

            var result = await UserAPI.GetUsageTime(request);

            if (result.Success)
            {
                OnUsageTimeReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UsageTimeStats>> AddUsageTime(int usageTime, bool isNewSession, int sessionDurationSeconds)
        {
            var request = CreateBaseRequest();
            request.UsageTime = usageTime;
            request.IsNewSession = isNewSession;
            request.SessionDurationSeconds = sessionDurationSeconds;

            var result = await UserAPI.AddUsageTime(request);

            if (result.Success)
            {
                OnUsageTimeReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> DeleteUserAccount()
        {
            var request = CreateBaseRequest();
            var result = await UserAPI.DeleteUserAccount(request);

            if (result.Success)
            {
                OnUserAccountDeleted?.Invoke(result.Data);
            }

            return result;
        }
    }
}
