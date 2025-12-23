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

        public static Task<OperationResult<AuthenticationResponse>> LoginWithDeviceID(string deviceId, string deviceModel, string platform)
        {
            var request = new AuthenticationRequest
            {
                DeviceID = deviceId,
                Device = deviceModel,
                Platform = platform
            };

            return HttpService.Post<AuthenticationResponse>(GetEndpoint(AuthenticationAction.LoginWithDeviceID), request);
        }

        public static Task<OperationResult<AuthenticationResponse>> LoginWithTelegram(string telegramInitData)
        {
            var request = new AuthenticationRequest
            {
                TelegramInitData = telegramInitData
            };

            return HttpService.Post<AuthenticationResponse>(GetEndpoint(AuthenticationAction.LoginWithTelegram), request);
        }

        public static Task<OperationResult<AuthenticationResponse>> LoginWithEmail(string email, string password)
        {
            var request = new AuthenticationRequest
            {
                Email = email,
                Password = password
            };

            return HttpService.Post<AuthenticationResponse>(GetEndpoint(AuthenticationAction.LoginWithEmail), request);
        }

        public static Task<OperationResult<AuthenticationResponse>> RegisterUserByEmail(
            string email,
            string password,
            string deviceId,
            string deviceModel,
            string platform)
        {
            var request = new AuthenticationRequest
            {
                Email = email,
                Password = password,
                DeviceID = deviceId,
                Device = deviceModel,
                Platform = platform
            };

            return HttpService.Post<AuthenticationResponse>(GetEndpoint(AuthenticationAction.RegisterUserByEmail), request);
        }

        public static Task<OperationResult<SuccessResponse>> AddEmailAndPassword(
            string userID,
            string clientSessionTicket,
            string email,
            string password)
        {
            var request = new AuthenticationRequest
            {
                UserID = userID,
                Email = email,
                Password = password,
            };

            return HttpService.Post<SuccessResponse>(GetEndpoint(AuthenticationAction.AddEmailAndPassword), request, clientSessionTicket);
        }

        public static Task<OperationResult<SuccessResponse>> ForgotPassword(string email)
        {
            var request = new AuthenticationRequest
            {
                Email = email
            };

            return HttpService.Post<SuccessResponse>(GetEndpoint(AuthenticationAction.ForgotPassword), request);
        }

        public static Task<OperationResult<SuccessResponse>> ResetPassword(string resetToken, string password)
        {
            var request = new AuthenticationRequest
            {
                ResetToken = resetToken,
                Password = password
            };

            return HttpService.Post<SuccessResponse>(GetEndpoint(AuthenticationAction.ResetPassword), request);
        }
    }
}
