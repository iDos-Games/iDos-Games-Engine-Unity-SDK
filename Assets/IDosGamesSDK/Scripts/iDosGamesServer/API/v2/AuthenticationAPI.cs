using System.Threading.Tasks;

namespace IDosGames
{
    public static class AuthenticationAPI
    {
        private static string GetEndpoint(AuthenticationAction action)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = GetTitleID();
            return $"api/v2/{templateID}/{titleID}/Client/Authentication/{action}";
        }

        public static string GetTitleID()
        {
            var settings = IDosGamesSDKSettings.Instance;
            if (settings == null)
            {
                return "0";
            }

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

        public static Task<OperationResult<PlatformLoginResponse>> LoginTokensWithPlatformToken(AuthenticationRequest request)
        {
            return HttpService.Post<PlatformLoginResponse>(GetEndpoint(AuthenticationAction.LoginTokensWithPlatformToken), request);
        }

        public static Task<OperationResult<ClientStateResponse>> LoginWithDeviceID(AuthenticationRequest request)
        {
            return HttpService.Post<ClientStateResponse>(GetEndpoint(AuthenticationAction.LoginWithDeviceID), request);
        }

        public static Task<OperationResult<ClientStateResponse>> LoginWithTelegram(AuthenticationRequest request)
        {
            return HttpService.Post<ClientStateResponse>(GetEndpoint(AuthenticationAction.LoginWithTelegram), request);
        }

        public static Task<OperationResult<ClientStateResponse>> LoginWithEmail(AuthenticationRequest request)
        {
            return HttpService.Post<ClientStateResponse>(GetEndpoint(AuthenticationAction.LoginWithEmail), request);
        }

        public static Task<OperationResult<ClientStateResponse>> RegisterWithEmail(AuthenticationRequest request)
        {
            return HttpService.Post<ClientStateResponse>(GetEndpoint(AuthenticationAction.RegisterWithEmail), request);
        }

        public static Task<OperationResult<SuccessResponse>> ForgotPassword(AuthenticationRequest request)
        {
            return HttpService.Post<SuccessResponse>(GetEndpoint(AuthenticationAction.ForgotPassword), request);
        }

        public static Task<OperationResult<SuccessResponse>> ResetPassword(AuthenticationRequest request)
        {
            return HttpService.Post<SuccessResponse>(GetEndpoint(AuthenticationAction.ResetPassword), request);
        }
    }
}
