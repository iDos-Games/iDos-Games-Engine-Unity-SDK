using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    public static class IGSwapAPI
    {
        public static string URL_SWAP_API = IDosGamesSDKSettings.Instance.SwapApiLink;
        public static event Action<Action> ConnectionError;

#if UNITY_EDITOR
        public static async Task<string> CreateBuyOrderForSolana(string takerPubkey, string tokenMint, ulong amountLamports)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.CreateBuyOrderForSolana.ToString(),
                WebAppLink = WebSDK.webAppLink,
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                TakerPubkey = takerPubkey,
                TokenMint = tokenMint,
                AmountLamports = amountLamports,
            };

            Debug.Log(URL_SWAP_API);
            return await SendPostRequest(URL_SWAP_API + ServerFunctionHandlers.CreateBuyOrderForSolana.ToString(), requestBody);
        }

        public static async Task<string> CreateSellOrderForSolana(string takerPubkey, string tokenMint, ulong tokenAmountAtomic)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.CreateSellOrderForSolana.ToString(),
                WebAppLink = WebSDK.webAppLink,
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                TakerPubkey = takerPubkey,
                TokenMint = tokenMint,
                TokenAmountAtomic = tokenAmountAtomic,
            };
            return await SendPostRequest(URL_SWAP_API + ServerFunctionHandlers.CreateSellOrderForSolana.ToString(), requestBody);
        }

        public static async Task<string> ExecuteOrderForSolana(string requestId, string signedTxBase64)
        {
            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.ExecuteOrderForSolana.ToString(),
                WebAppLink = WebSDK.webAppLink,
                SecretKey = IDosGamesSDKSettings.Instance.DeveloperSecretKey,
                RequestId = requestId,
                SignedTxBase64 = signedTxBase64,
            };
            return await SendPostRequest(URL_SWAP_API + ServerFunctionHandlers.ExecuteOrderForSolana.ToString(), requestBody);
        }
#endif

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
