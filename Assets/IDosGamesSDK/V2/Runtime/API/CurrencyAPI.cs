using System.Threading.Tasks;

namespace IDosGames
{
    public static class CurrencyAPI
    {
        private static string GetEndpoint(CurrencyAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Currency/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(CurrencyAction action, CurrencyRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<ConvertResponse>> Convert(CurrencyRequest request)
            => SendRequest<ConvertResponse>(CurrencyAction.Convert, request);

        public static Task<OperationResult<CryptoConvertResponse>> CryptoConvert(CurrencyRequest request)
            => SendRequest<CryptoConvertResponse>(CurrencyAction.CryptoConvert, request);
    }
}
