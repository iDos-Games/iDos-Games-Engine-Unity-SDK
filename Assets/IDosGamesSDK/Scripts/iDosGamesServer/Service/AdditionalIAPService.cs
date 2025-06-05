using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    public static class AdditionalIAPService
    {
        public static event Action<Action> ConnectionError;

        private static readonly string URL_PURCHASE = IDosGamesSDKSettings.Instance.PurchaseLink;

        public static async Task<string> CreateTelegramInvoice(string title, string description, string payload, string providerToken, string currency, int amount)
        {
            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Title = title,
                Description = description,
                Payload = payload,
                ProviderToken = providerToken,
                Currency = currency,
                Prices = new List<LabeledPrice> { new LabeledPrice { Label = title, Amount = amount } }
            };

            var requestBody = new IGSRequest
            {
                TitleID = IDosGamesSDKSettings.Instance.TitleID,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                FunctionName = ServerFunctionHandlers.CreateTelegramInvoice.ToString(),
                WebAppLink = WebSDK.webAppLink,
                UserID = AuthService.UserID,
                ClientSessionTicket = AuthService.ClientSessionTicket,
                CreateInvoice = createInvoiceRequest
            };

            return await SendPostRequest(URL_PURCHASE + ServerFunctionHandlers.CreateTelegramInvoice.ToString(), requestBody);
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

                    if (result.Contains(MessageCode.SESSION_EXPIRED.ToString()) || result.Contains(MessageCode.INVALID_SESSION_TICKET.ToString()))
                    {
                        AuthService.Instance.AutoLogin();
                    }

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
