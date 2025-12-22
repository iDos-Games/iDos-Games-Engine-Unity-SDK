using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace IDosGames
{
    public static class HttpService
    {
        // =================================================================================
        // GLOBAL EVENTS
        // =================================================================================
        public static event Action<bool> OnBusyStateChanged;
        public static event Action<string> OnGlobalError;
        public static event Action<string> ConnectionError;

        private static readonly string BASE_URL = IDosGamesSDKSettings.Instance.ServerLink;

        // =================================================================================
        // GENERIC POST REQUEST
        // =================================================================================
        public static async Task<IDosGamesApiResult<T>> Post<T>(string endpoint, object payload)
        {
            OnBusyStateChanged?.Invoke(true);

            try
            {
                string url = $"{BASE_URL}/{endpoint}";

                string jsonBody = JsonConvert.SerializeObject(payload);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

                using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
                {
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();

                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    if (!string.IsNullOrEmpty(AuthenticationService.ClientSessionTicket))
                    {
                        webRequest.SetRequestHeader("Authorization", $"Bearer {AuthenticationService.ClientSessionTicket}");
                    }

                    var asyncOp = webRequest.SendWebRequest();
                    while (!asyncOp.isDone) await Task.Yield();

                    // --- SUCCESS ---
                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        if (IDosGamesSDKSettings.Instance.DebugLogging)
                            Debug.Log($"[HttpService] Success: {url}\nResp: {webRequest.downloadHandler.text}");

                        try
                        {
                            var response = JsonConvert.DeserializeObject<IDosGamesApiResult<T>>(webRequest.downloadHandler.text);

                            // Logical error from the server (Success = false)
                            if (!response.Success && !string.IsNullOrEmpty(response.Error))
                            {
                                OnGlobalError?.Invoke(response.Error);
                            }
                            return response;
                        }
                        catch (Exception ex)
                        {
                            string err = $"JSON Parse Error: {ex.Message}";
                            Debug.LogError(err);
                            return HandleError<T>(err);
                        }
                    }
                    // --- ERROR ---
                    else
                    {
                        return HandleWebError<T>(webRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleError<T>(ex.Message);
            }
            finally
            {
                OnBusyStateChanged?.Invoke(false);
            }
        }

        private static IDosGamesApiResult<T> HandleWebError<T>(UnityWebRequest req)
        {
            string msg = req.error;
            long code = req.responseCode;

            if (code == 401)
            {
                Debug.LogWarning("[HttpService] Token expired...");
                AuthenticationService.Logout();
                msg = "Unauthorized";
            }
            else if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("[HttpService] Connection Error...");
                msg = "Internet Connection Error";
                ConnectionError?.Invoke(msg);
            }
            else
            {
                try
                {
                    var errObj = JsonConvert.DeserializeObject<IDosGamesApiResult<T>>(req.downloadHandler.text);
                    if (errObj != null && !string.IsNullOrEmpty(errObj.Error)) msg = errObj.Error;
                }
                catch { }
            }

            return HandleError<T>(msg);
        }

        private static IDosGamesApiResult<T> HandleError<T>(string errorMsg)
        {
            Debug.LogError($"[HttpService Error] {errorMsg}");
            OnGlobalError?.Invoke(errorMsg);
            return new IDosGamesApiResult<T> { Success = false, Error = errorMsg };
        }
    }
}
