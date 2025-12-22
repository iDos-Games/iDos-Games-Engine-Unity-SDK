using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading;

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
        public static event Action OnUnauthorized;

        private static readonly string BASE_URL = "https://api.idosgames.com/api";
        private static int _inFlightRequests = 0;
        public static bool IsBusy => Volatile.Read(ref _inFlightRequests) > 0;

        private static void RaiseBusy()
        {
            int newValue = Interlocked.Increment(ref _inFlightRequests);
            if (newValue == 1)
                OnBusyStateChanged?.Invoke(true);
        }

        private static void LowerBusy()
        {
            int newValue = Interlocked.Decrement(ref _inFlightRequests);

            if (newValue < 0)
            {
                Interlocked.Exchange(ref _inFlightRequests, 0);
                newValue = 0;
            }

            if (newValue == 0)
                OnBusyStateChanged?.Invoke(false);
        }

        // =================================================================================
        // GENERIC POST REQUEST
        // =================================================================================
        public static async Task<OperationResult<T>> Post<T>(string endpoint, object payload, string clientSessionTicket = null, bool silent = false)
        {
            RaiseBusy();

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
                    if (!string.IsNullOrEmpty(clientSessionTicket))
                    {
                        webRequest.SetRequestHeader("Authorization", $"Bearer {clientSessionTicket}");
                    }

                    await webRequest.SendWebRequest();

                    // --- SUCCESS ---
                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        if (IDosGamesSDKSettings.Instance.DebugLogging)
                            Debug.Log($"[HttpService] Success: {url}\nResp: {webRequest.downloadHandler.text}");

                        try
                        {
                            var response = JsonConvert.DeserializeObject<OperationResult<T>>(webRequest.downloadHandler.text);
                            if (response == null) return HandleError<T>("Empty response", silent);

                            if (!response.Success && !string.IsNullOrEmpty(response.Error))
                            {
                                if (!silent) OnGlobalError?.Invoke(response.Error);
                            }
                            return response;
                        }
                        catch (Exception ex)
                        {
                            string err = $"JSON Parse Error: {ex.Message}";
                            Debug.LogError(err);
                            return HandleError<T>(err, silent);
                        }
                    }
                    // --- ERROR ---
                    else
                    {
                        return HandleWebError<T>(webRequest, silent);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleError<T>(ex.Message, silent);
            }
            finally
            {
                LowerBusy();
            }
        }

        private static OperationResult<T> HandleWebError<T>(UnityWebRequest req, bool silent)
        {
            string msg = req.error;
            long code = req.responseCode;

            // 1. First, let's check for critical authorization errors.
            if (code == 401)
            {
                Debug.LogWarning("[HttpService] Token expired (401).");
                OnUnauthorized?.Invoke();
                return new OperationResult<T> { Success = false, Error = "Unauthorized" };
            }

            // 2. Checking network errors
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("[HttpService] Connection Error...");
                msg = "Internet Connection Error";
                if (!silent) ConnectionError?.Invoke(msg);
                return new OperationResult<T> { Success = false, Error = msg };
            }

            // 3. We are trying to extract the error text from the response body (for codes 400, 500, etc.)
            try
            {
                var errObj = JsonConvert.DeserializeObject<OperationResult<T>>(req.downloadHandler.text);
                if (errObj != null && !string.IsNullOrEmpty(errObj.Error)) msg = errObj.Error;
            }
            catch { }

            // 4. Common mistake
            return HandleError<T>(msg, silent);
        }

        private static OperationResult<T> HandleError<T>(string errorMsg, bool silent)
        {
            Debug.LogError($"[HttpService Error] {errorMsg}");
            if (!silent) OnGlobalError?.Invoke(errorMsg);
            return new OperationResult<T> { Success = false, Error = errorMsg };
        }

        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest.Result>();
            reqOp.completed += asyncOp => tcs.TrySetResult(((UnityWebRequestAsyncOperation)asyncOp).webRequest.result);
            return tcs.Task.GetAwaiter();
        }
    }
}
