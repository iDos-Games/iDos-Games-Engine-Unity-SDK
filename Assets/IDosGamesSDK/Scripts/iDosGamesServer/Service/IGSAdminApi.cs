#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    public class IGSAdminApi
    {
        public static event Action<Action> ConnectionError;
        public static string URL_IGS_ADMIN_API = IDosGamesSDKSettings.Instance.IgsAdminApiLink;

        public static async Task<string> GetFunctionLinks()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                FunctionName = ServerFunctionHandlers.GetFunctionLinks.ToString(),
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey
            };

            return await SendPostRequest(URL_IGS_ADMIN_API, requestBody);
        }

        public static async Task<string> UploadTitleDataFiles(List<FileUpload> files)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                FunctionName = ServerFunctionHandlers.UploadTitleData.ToString(),
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                Files = files
            };
            return await SendPostRequest(URL_IGS_ADMIN_API, requestBody);
        }

        public static async Task<string> UploadWebGL(List<FileUpload> files)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                FunctionName = ServerFunctionHandlers.UploadWebGL.ToString(),
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                Files = files,
                DevBuild = IDosGamesSDKSettings.Instance.DevBuild
            };
            return await SendPostRequest(URL_IGS_ADMIN_API, requestBody);
        }

        public static async Task<string> ClearWebGL()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                FunctionName = ServerFunctionHandlers.ClearWebGL.ToString(),
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                DevBuild = IDosGamesSDKSettings.Instance.DevBuild
            };

            return await SendPostRequest(URL_IGS_ADMIN_API, requestBody);
        }

        public static async Task<string> RegisterTelegramWebhook()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                FunctionName = ServerFunctionHandlers.RegisterTelegramWebhook.ToString(),
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                WebhookLink = IDosGamesSDKSettings.Instance.TelegramWebhookLink
            };

            return await SendPostRequest(URL_IGS_ADMIN_API, requestBody);
        }

        private static async Task<string> SendPostRequest(string functionURL, IGSRequest request)
        {
            var requestBody = JObject.FromObject(request);
            byte[] bodyRaw = new UTF8Encoding(true).GetBytes(requestBody.ToString());

            using (UnityWebRequest webRequest = new UnityWebRequest(functionURL, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string result = webRequest.downloadHandler.text;

                    if (IDosGamesSDKSettings.Instance.DebugLogging)
                    {
                        Debug.Log(result);
                        //LogLongString(result);  
                    }

                    return result;
                }
                else
                {
                    string errorDetail = $"Error request: {webRequest.error}";
                    Debug.LogError(errorDetail);

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                    {
                        ConnectionError?.Invoke(null);
                        return "ConnectionError: " + errorDetail;
                    }

                    return "Error: " + errorDetail;
                }
            }
        }

    }
}
#endif