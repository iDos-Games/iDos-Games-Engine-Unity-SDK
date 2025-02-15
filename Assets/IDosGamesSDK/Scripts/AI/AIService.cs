using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    public class AIService : MonoBehaviour
    {
        public static event Action<Action> ConnectionError;

        public static string URL_AI_SERVICE = IDosGamesSDKSettings.Instance.AILink;

        public static async Task<string> CreateThreadAndRun(AIRequest req)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                WebAppLink = WebSDK.webAppLink,
                UserID = AuthService.UserID,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                AIRequest = req,
            };

            string responseString = await SendPostRequest(URL_AI_SERVICE + nameof(CreateThreadAndRun), requestBody);
            if (responseString != null) { IDosGamesSDKSettings.Instance.PlayTime = 0; }
            return responseString;
        }

        public static async Task<string> CreateThread()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                WebAppLink = WebSDK.webAppLink,
                UserID = AuthService.UserID,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
            };

            string responseString = await SendPostRequest(URL_AI_SERVICE + nameof(CreateThread), requestBody);
            if (responseString != null) { IDosGamesSDKSettings.Instance.PlayTime = 0; }
            return responseString;
        }

        public static async Task<string> CreateMessage(AIRequest req)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                WebAppLink = WebSDK.webAppLink,
                UserID = AuthService.UserID,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                AIRequest = req,
            };

            string responseString = await SendPostRequest(URL_AI_SERVICE + nameof(CreateMessage), requestBody);
            var response = JsonConvert.DeserializeObject<string>(responseString);

            if (response != null) { IDosGamesSDKSettings.Instance.PlayTime = 0; }

            return response;
        }

        public static async Task<string> RetrieveResponse(AIRequest req)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                WebAppLink = WebSDK.webAppLink,
                UserID = AuthService.UserID,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                AIRequest = req,
            };

            string responseString = await SendPostRequest(URL_AI_SERVICE + nameof(RetrieveResponse), requestBody);
            var response = JsonConvert.DeserializeObject<string>(responseString);

            if (response != null) { IDosGamesSDKSettings.Instance.PlayTime = 0; }

            return response;
        }

        public static async Task<string> SendPostRequest(string functionURL, IGSRequest request)
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

                    if (result.Contains(MessageCode.SESSION_EXPIRED.ToString()) || result.Contains(MessageCode.INVALID_SESSION_TICKET.ToString()))
                    {
                        AuthService.Instance.AutoLogin();
                    }

                    if (IDosGamesSDKSettings.Instance.DebugLogging)
                    {
                        Debug.Log(result);
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
