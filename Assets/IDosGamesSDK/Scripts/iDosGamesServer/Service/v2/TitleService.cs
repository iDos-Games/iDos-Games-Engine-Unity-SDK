using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class TitleService
    {
        public static event Action<Dictionary<string, object>> OnTitleDataReceived;
        public static event Action<TitlePublicConfigurationModel> OnConfigReceived;
        public static event Action<GetCatalogItemsResult> OnCatalogReceived;
        public static event Action<GetLeaderboardResult> OnLeaderboardReceived;
        public static event Action<PlatformSettingsModel> OnPlatformSettingsReceived;
        public static event Action<Currencies> OnCurrencyDataReceived;
        public static event Action<SuccessResponse> OnServerTimeReceived;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
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
                IDosGamesData.Config?.ApplyTitlePublicData(result.Data);
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

        public static async Task<OperationResult<GetCatalogItemsResult>> GetCatalogItems(string catalogVersion)
        {
            var request = CreateBaseRequest();
            request.CatalogVersion = catalogVersion;

            var result = await TitleAPI.GetCatalogItems(request);

            if (result.Success)
            {
                IDosGamesData.Config?.ApplyCatalog(catalogVersion, result.Data);
                OnCatalogReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<GetLeaderboardResult>> GetLeaderboard(string statisticName)
        {
            var request = CreateBaseRequest();
            request.StatisticName = statisticName;

            var result = await TitleAPI.GetLeaderboard(request);

            if (result.Success)
            {
                OnLeaderboardReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<PlatformSettingsModel>> GetPlatformSettings()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetPlatformSettings(request);

            if (result.Success)
            {
                IDosGamesData.Config?.ApplyPlatformSettings(result.Data);
                OnPlatformSettingsReceived?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<Currencies>> GetCurrencyData()
        {
            var request = CreateBaseRequest();
            var result = await TitleAPI.GetCurrencyData(request);

            if (result.Success)
            {
                IDosGamesData.Config?.ApplyCurrencies(result.Data);
                OnCurrencyDataReceived?.Invoke(result.Data);
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
