using System.Threading.Tasks;

namespace IDosGames
{
    public static class AuthenticationAPI
    {
        private static string GetEndpoint(AuthenticationAction action)
        {
            string titleID = GetTitleID();
            return $"api/v2/{titleID}/Client/Authentication/{action}";
        }

        public static string GetTitleID()
        {
            var settings = IDosGamesSDKSettings.Instance;
            if (settings == null) return "0";

            string titleID = settings.TitleID;

            if (titleID == "0")
            {
#if UNITY_WEBGL
                string fullUrl = WebSDK.webAppLink;
                if (!string.IsNullOrEmpty(fullUrl))
                {
                    int queryStartIndex = fullUrl.IndexOf('?');
                    if (queryStartIndex != -1 && queryStartIndex < fullUrl.Length - 1)
                    {
                        string queryString = fullUrl.Substring(queryStartIndex + 1);
                        string[] queryParams = queryString.Split('&');
                        foreach (string param in queryParams)
                        {
                            string[] keyValue = param.Split('=');
                            if (keyValue.Length == 2 && keyValue[0] == "titleID")
                            {
                                titleID = keyValue[1];
                                break;
                            }
                        }
                    }
                }
#endif
            }

            return titleID;
        }

        public static Task<OperationResult<PlatformLoginResponse>> LoginWithPlatformToken(AuthenticationRequest request)
            => HttpService.Post<PlatformLoginResponse>(GetEndpoint(AuthenticationAction.LoginWithPlatformToken), request);

        public static Task<OperationResult<PlatformLoginResponse>> LoginWithDeviceID(AuthenticationRequest request)
            => HttpService.Post<PlatformLoginResponse>(GetEndpoint(AuthenticationAction.LoginWithDeviceID), request);

        public static Task<OperationResult<PlatformLoginResponse>> LoginWithTelegram(AuthenticationRequest request)
            => HttpService.Post<PlatformLoginResponse>(GetEndpoint(AuthenticationAction.LoginWithTelegram), request);

        public static Task<OperationResult<PlatformLoginResponse>> LoginWithEmail(AuthenticationRequest request)
            => HttpService.Post<PlatformLoginResponse>(GetEndpoint(AuthenticationAction.LoginWithEmail), request);

        public static Task<OperationResult<PlatformLoginResponse>> LoginWithGoogle(AuthenticationRequest request)
            => HttpService.Post<PlatformLoginResponse>(GetEndpoint(AuthenticationAction.LoginWithGoogle), request);

        public static Task<OperationResult<PlatformLoginResponse>> RegisterWithEmail(AuthenticationRequest request)
            => HttpService.Post<PlatformLoginResponse>(GetEndpoint(AuthenticationAction.RegisterWithEmail), request);

        public static Task<OperationResult<SuccessResponse>> ForgotPassword(AuthenticationRequest request)
            => HttpService.Post<SuccessResponse>(GetEndpoint(AuthenticationAction.ForgotPassword), request);

        public static Task<OperationResult<SuccessResponse>> ResetPassword(AuthenticationRequest request)
            => HttpService.Post<SuccessResponse>(GetEndpoint(AuthenticationAction.ResetPassword), request);
    }
}
