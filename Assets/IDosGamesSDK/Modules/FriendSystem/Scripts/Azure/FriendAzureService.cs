using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames.Friends
{
    public static class FriendAzureService
    {
        public static event Action<Action> ConnectionError;
        public static string URL = IDosGamesSDKSettings.Instance.FriendSystemLink;

        public static async Task<List<string>> GetMyFriend()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = FriendActionType.GetMyFriends.ToString(),
                WebAppLink = WebSDK.webAppLink,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                UserID = AuthService.UserID
            };

            string result = await SendRequest(URL + FriendActionType.GetMyFriends.ToString(), requestBody);

            return JsonConvert.DeserializeObject<List<string>>(result) ?? new List<string>();
        }

        public static async Task<List<string>> GetRecommendedFriends()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = FriendActionType.GetRecommendedFriends.ToString(),
                WebAppLink = WebSDK.webAppLink,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                UserID = AuthService.UserID
            };

            string result = await SendRequest(URL + FriendActionType.GetRecommendedFriends.ToString(), requestBody);

            return JsonConvert.DeserializeObject<List<string>>(result) ?? new List<string>();
        }

        public static async Task<List<string>> GetPendingFriendRequests()
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = FriendActionType.GetPendingFriendRequests.ToString(),
                WebAppLink = WebSDK.webAppLink,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                UserID = AuthService.UserID
            };

            string result = await SendRequest(URL + FriendActionType.GetPendingFriendRequests.ToString(), requestBody);

            return JsonConvert.DeserializeObject<List<string>>(result) ?? new List<string>();
        }

        public static async Task<string> SendRequestToAdd(IGSRequest request)
        {
            request.ClientSessionTicket = AuthService.ClientSessionTicket;
            request.BuildKey = IDosGamesSDKSettings.Instance.BuildKey;
            request.UserID = AuthService.UserID;
            request.FunctionName = FriendActionType.AdditionRequest.ToString();
            request.TitleID = IDosGamesSDKSettings.Instance.TitleID;
            request.WebAppLink = WebSDK.webAppLink;

            return await SendRequest(URL + FriendActionType.AdditionRequest.ToString(), request);
        }

        public static async Task<string> AcceptRequest(IGSRequest request)
        {
            request.ClientSessionTicket = AuthService.ClientSessionTicket;
            request.BuildKey = IDosGamesSDKSettings.Instance.BuildKey;
            request.UserID = AuthService.UserID;
            request.FunctionName = FriendActionType.AcceptRequest.ToString();
            request.TitleID = IDosGamesSDKSettings.Instance.TitleID;
            request.WebAppLink = WebSDK.webAppLink;

            return await SendRequest(URL + FriendActionType.AcceptRequest.ToString(), request);
        }

        public static async Task<string> RejectRequest(IGSRequest request)
        {
            request.ClientSessionTicket = AuthService.ClientSessionTicket;
            request.BuildKey = IDosGamesSDKSettings.Instance.BuildKey;
            request.UserID = AuthService.UserID;
            request.FunctionName = FriendActionType.RejectRequest.ToString();
            request.TitleID = IDosGamesSDKSettings.Instance.TitleID;
            request.WebAppLink = WebSDK.webAppLink;

            return await SendRequest(URL + FriendActionType.RejectRequest.ToString(), request);
        }

        public static async Task<string> DeleteFriend(IGSRequest request)
        {
            request.ClientSessionTicket = AuthService.ClientSessionTicket;
            request.BuildKey = IDosGamesSDKSettings.Instance.BuildKey;
            request.UserID = AuthService.UserID;
            request.FunctionName = FriendActionType.RemoveFriend.ToString();
            request.TitleID = IDosGamesSDKSettings.Instance.TitleID;
            request.WebAppLink = WebSDK.webAppLink;

            return await SendRequest(URL + FriendActionType.RemoveFriend.ToString(), request);
        }

        private static async Task<string> SendRequest(string functionURL, IGSRequest request)
        {
            var requestBody = JObject.FromObject(request);
            byte[] bodyRaw = new UTF8Encoding(true).GetBytes(requestBody.ToString());

            UnityWebRequest webRequest = new UnityWebRequest()
            {
                url = functionURL,
                method = RequestMethodType.POST.ToString(),
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer(),
            };

            webRequest.SetRequestHeader("Content-Type", "application/json");

            UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string result = webRequest.downloadHandler.text;
                webRequest?.Dispose();

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
                    webRequest?.Dispose();
                    return "ConnectionError: " + errorDetail;
                }

                webRequest?.Dispose();
                return "Error: " + errorDetail;
            }
        }
    }
}