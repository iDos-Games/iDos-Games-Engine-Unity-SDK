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

        public static async Task<string> UploadWebGLBuild(List<FileUpload> files)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.UploadWebGLBuild.ToString(),
                WebAppLink = WebSDK.webAppLink,
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                Files = files,
                DevBuild = IDosGamesSDKSettings.Instance.DevBuild
            };
            return await SendPostRequest(URL_IGS_ADMIN_API + ServerFunctionHandlers.UploadWebGLBuild.ToString(), requestBody);
        }

        public static async Task<string> ClearWebGL()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.ClearWebGL.ToString(),
                WebAppLink = WebSDK.webAppLink,
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                DevBuild = IDosGamesSDKSettings.Instance.DevBuild
            };

            return await SendPostRequest(URL_IGS_ADMIN_API + ServerFunctionHandlers.ClearWebGL.ToString(), requestBody);
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