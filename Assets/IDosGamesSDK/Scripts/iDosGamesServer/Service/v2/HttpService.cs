using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Generic;

namespace IDosGames
{
    public static class HttpService
    {
        public static event Action<bool> OnBusyStateChanged;
        public static event Action<string> OnGlobalError;
        public static event Action<string> ConnectionError;
        public static event Action OnUnauthorized;

        private static readonly string BASE_URL = "https://api.idosgames.com";
        private static int _inFlightRequests = 0;
        public static bool IsBusy => Volatile.Read(ref _inFlightRequests) > 0;
        private const float THROTTLE_SECONDS = 1.2f;
        private static readonly Dictionary<string, float> _lastCallTime = new();

        private static bool TryThrottle(string endpoint)
        {
            float now = Time.realtimeSinceStartup;

            if (_lastCallTime.TryGetValue(endpoint, out float last))
            {
                float elapsed = now - last;
                if (elapsed < THROTTLE_SECONDS)
                {
                    Debug.LogWarning($"[HttpService] Throttled: {endpoint} — wait {THROTTLE_SECONDS - elapsed:F2}s");
                    return false;
                }
            }

            _lastCallTime[endpoint] = now;
            return true;
        }

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

        public static async Task<OperationResult<T>> Post<T>(string endpoint, object payload, string clientSessionTicket = null, bool silent = false, int timeoutSeconds = 12)
        {
            if (!TryThrottle(endpoint)) return OperationResult<T>.Throttled();

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
                    webRequest.timeout = timeoutSeconds;

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
                            Debug.Log($"[HttpService] Success: {endpoint}\nResponse: {webRequest.downloadHandler.text}");

                        try
                        {
                            var response = JsonConvert.DeserializeObject<OperationResult<T>>(webRequest.downloadHandler.text);
                            if (response == null) return HandleError<T>("Empty response", silent, endpoint);

                            if (!response.Success && !string.IsNullOrEmpty(response.Error))
                            {
                                if (IDosGamesSDKSettings.Instance.DebugLogging)
                                    Debug.LogWarning($"[HttpService] Server returned Success=false (HTTP {webRequest.responseCode}) Error={response.Error}");

                                if (!silent) OnGlobalError?.Invoke(response.Error);
                            }
                            return response;
                        }
                        catch (Exception ex)
                        {
                            string err = $"JSON Parse Error: {ex.Message}";
                            return HandleError<T>(err, silent, endpoint);
                        }
                    }
                    // --- ERROR ---
                    else
                    {
                        return HandleWebError<T>(webRequest, silent, endpoint);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleError<T>(ex.Message, silent, endpoint);
            }
            finally
            {
                LowerBusy();
            }
        }

        private static OperationResult<T> HandleWebError<T>(UnityWebRequest req, bool silent, string endpoint)
        {
            long code = req.responseCode;

            // Extended log (code + result + error + body)
            if (IDosGamesSDKSettings.Instance.DebugLogging)
            {
                string debugInfo = BuildHttpErrorMessage(req, endpoint);
                Debug.LogWarning($"[HttpService] Request failed:\n{debugInfo}");
            }

            // 1) Unauthorized
            if (code == 401)
            {
                Debug.LogWarning("[HttpService] Token expired (401).");
                OnUnauthorized?.Invoke();
                return new OperationResult<T> { Success = false, Error = "Unauthorized" };
            }

            // 2) Network error (no internet / DNS / timeout)
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                string connectionMessage = $"Internet Connection Error (HTTP {code})";
                Debug.LogWarning("[HttpService] Connection Error...");
                if (!silent) ConnectionError?.Invoke(connectionMessage);
                return new OperationResult<T> { Success = false, Error = connectionMessage };
            }

            // 3) Data processing error (for example, a response arrived, but Unity couldn't process/read it)
            if (req.result == UnityWebRequest.Result.DataProcessingError)
            {
                string processingMessage = $"Data Processing Error (HTTP {code}): {req.error}";
                return HandleError<T>(processingMessage, silent, endpoint);
            }

            // 4) Default error message (with HTTP code)
            string errorMessage = $"HTTP {code}: {req.error}";

            // 5) Try to extract server error from body (OperationResult<T>)
            try
            {
                string body = req.downloadHandler?.text;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    var errObj = JsonConvert.DeserializeObject<OperationResult<T>>(body);

                    if (errObj != null && !string.IsNullOrWhiteSpace(errObj.Error))
                        errorMessage = $"HTTP {code}: {errObj.Error}"; // server error
                    else
                        errorMessage = $"HTTP {code}: {body}"; // there is a body, but not an OperationResult<T>
                }
            }
            catch
            {
                // leave the errorMessage as is
            }

            return HandleError<T>(errorMessage, silent, endpoint);
        }

        private static OperationResult<T> HandleError<T>(string errorMsg, bool silent, string endpoint = null)
        {
            if (!string.IsNullOrEmpty(endpoint))
                Debug.LogError($"[HttpService Error] Endpoint: {endpoint}\n{errorMsg}");
            else
                Debug.LogError($"[HttpService Error] {errorMsg}");

            if (!silent) OnGlobalError?.Invoke(errorMsg);
            return new OperationResult<T> { Success = false, Error = errorMsg };
        }

        private static string BuildHttpErrorMessage(UnityWebRequest req, string endpoint)
        {
            long code = req.responseCode;
            string result = req.result.ToString();
            string err = string.IsNullOrEmpty(req.error) ? "n/a" : req.error;

            string body = null;
            try { body = req.downloadHandler?.text; } catch { }

            if (!string.IsNullOrWhiteSpace(body) && body.Length > 2000)
                body = body.Substring(0, 2000) + "...(truncated)";

            return $"Endpoint: {endpoint}\nHTTP {code} | Result={result} | Error={err}" + (!string.IsNullOrWhiteSpace(body) ? $"\nBody: {body}" : "");
        }

        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest.Result>();
            reqOp.completed += asyncOp => tcs.TrySetResult(((UnityWebRequestAsyncOperation)asyncOp).webRequest.result);
            return tcs.Task.GetAwaiter();
        }
    }
}
