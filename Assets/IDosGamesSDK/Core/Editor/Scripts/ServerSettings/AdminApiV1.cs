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
    public class AdminApiV1
    {
        public static event Action<Action> ConnectionError;
        public static string URL_IGS_ADMIN_API = IDosGamesSDKSettings.Instance.IgsAdminApiLink;

        public static async Task<string> GetWebGLUploadUrls(List<FileUpload> files)
        {
            var requestBody = new AdminApiV1Request
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                Files = files,
                DevBuild = IDosGamesSDKSettings.Instance.DevBuild
            };
            return await SendPostRequest(URL_IGS_ADMIN_API + "GetWebGLUploadUrls", requestBody);
        }

        public static async Task<string> GetUploadUrls(List<FileUpload> files)
        {
            var requestBody = new AdminApiV1Request
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                Files = files,
                DevBuild = IDosGamesSDKSettings.Instance.DevBuild
            };

            return await SendPostRequest(URL_IGS_ADMIN_API + "GetUploadUrls", requestBody);
        }

        private static async Task<string> SendPostRequest(string functionURL, AdminApiV1Request request)
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

    public class AdminApiV1Request : BaseRequest
    {
        public List<FileUpload> Files {  get; set; }
    }

    public class FileUpload
    {
        public string FilePath { get; set; }
        public byte[] FileContent { get; set; }

        public FileUpload(string filePath, byte[] fileContent)
        {
            FilePath = filePath;
            FileContent = fileContent;
        }
    }
}
#endif