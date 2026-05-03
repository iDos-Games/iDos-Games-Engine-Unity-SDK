using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IDosGames
{
    public static class TitleService
    {
        public static event Action<Dictionary<string, object>> OnTitleDataReceived;
        public static event Action<TitlePublicConfigurationModel> OnConfigReceived;
        public static event Action<SuccessResponse> OnServerTimeReceived;

        private static AuthContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        private static TitleRequest CreateBaseRequest()
        {
            return new TitleRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        public static async Task<OperationResult<Dictionary<string, object>>> GetTitlePublicData()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetTitlePublicData(request);

            if (result.Success)
            {
                //IDosGamesData.Config?.ApplyTitlePublicData(result.Data);
                OnTitleDataReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<TitlePublicConfigurationModel>> GetTitlePublicConfiguration()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetTitlePublicConfiguration(request);

            if (result.Success)
            {
                IDosGamesData.Config?.ApplyTitlePublicConfiguration(result.Data);
                OnConfigReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> GetServerTime()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetServerTime(request);

            if (result.Success)
            {
                OnServerTimeReceived?.Invoke(result.Data);
            }

            return result;
        }
    }
}
