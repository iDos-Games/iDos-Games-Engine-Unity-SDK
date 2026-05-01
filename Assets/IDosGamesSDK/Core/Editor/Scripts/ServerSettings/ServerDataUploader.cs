#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class ServerDataUploader : Editor
    {
        private const int UPLOAD_TIMEOUT_SEC = 600;
        private const int MAX_RETRIES = 5;
        private const int BASE_BACKOFF_MS = 800;

        private static bool ShouldRetry(long code, string errorText, string respBody)
        {
            if (code == 0) return true;
            if (code == 429) return true;
            if (code == 500 || code == 502 || code == 503 || code == 504) return true;

            if (code == 403 && (respBody?.IndexOf("Expired", StringComparison.OrdinalIgnoreCase) >= 0
                             || respBody?.IndexOf("SlowDown", StringComparison.OrdinalIgnoreCase) >= 0
                             || respBody?.IndexOf("Signature", StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            if (!string.IsNullOrEmpty(errorText) && (
                errorText.IndexOf("Recv failure", StringComparison.OrdinalIgnoreCase) >= 0 ||
                errorText.IndexOf("Connection was reset", StringComparison.OrdinalIgnoreCase) >= 0 ||
                errorText.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            return false;
        }

        private static int BackoffDelayMs(int attempt)
        {
            var rand = UnityEngine.Random.Range(0, 250);
            return (int)(Mathf.Min(32000, BASE_BACKOFF_MS * Mathf.Pow(2, attempt))) + rand;
        }

        public static async void UploadDataFromDirectory(string directoryPath, int batchSize = 1)
        {
            Debug.Log("Uploading Start ...");

            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError("Directory does not exist: " + directoryPath);
                return;
            }

            var allFiles = await AddFilesFromDirectory(directoryPath, directoryPath);

            string[] requiredEndings = { ".loader.js", ".data.unityweb", ".framework.js.unityweb", ".wasm.unityweb" };

            var uploadedFiles = new Dictionary<string, FileUpload>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in allFiles)
            {
                var key = requiredEndings.FirstOrDefault(ending => f.FilePath.EndsWith(ending, StringComparison.OrdinalIgnoreCase));
                if (key != null && !uploadedFiles.ContainsKey(key))
                    uploadedFiles[key] = f;
            }

            if (requiredEndings.Any(ext => !uploadedFiles.ContainsKey(ext)))
            {
                Debug.LogError("Missing required WebGL build files. Required: .loader.js, .data.unityweb, .framework.js.unityweb, .wasm.unityweb");
                return;
            }

            // StreamingAssets
            var projectRoot = Directory.GetParent(directoryPath)?.FullName ?? directoryPath;
            var streamingAssetsPath = Path.Combine(projectRoot, "StreamingAssets");

            List<FileUpload> streamingFiles = new List<FileUpload>();
            if (Directory.Exists(streamingAssetsPath))
            {
                streamingFiles = await AddStreamingAssetsRecursively(streamingAssetsPath);
                Debug.Log($"StreamingAssets found: {streamingFiles.Count} files");
            }
            else
            {
                Debug.Log("StreamingAssets directory not found (optional): " + streamingAssetsPath);
            }

            var result = await IGSAdminApi.GetWebGLUploadUrls(allFiles);

            var localNameToFullPath = allFiles
                .Select(f => new
                {
                    Name = Path.GetFileName(f.FilePath),
                    Full = Path.Combine(directoryPath, f.FilePath.Replace('/', Path.DirectorySeparatorChar))
                })
                .ToDictionary(x => x.Name, x => x.Full, StringComparer.OrdinalIgnoreCase);

            try
            {
                await UploadFilesToServer(result, localNameToFullPath, Math.Max(1, batchSize));
                Debug.Log("Upload (build files) completed.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Uploader] Build files upload failed: {ex.Message}\n{ex}");
                throw;
            }

            if (streamingFiles.Count > 0)
            {
                var streamingJson = await IGSAdminApi.GetUploadUrls(streamingFiles);

                var localNameToFullPathSA = streamingFiles.ToDictionary(
                    f => f.FilePath,
                    f =>
                    {
                        var relativeInsideSA = f.FilePath.StartsWith("StreamingAssets/", StringComparison.OrdinalIgnoreCase)
                            ? f.FilePath.Substring("StreamingAssets/".Length)
                            : f.FilePath;
                        return Path.Combine(streamingAssetsPath, relativeInsideSA.Replace('/', Path.DirectorySeparatorChar));
                    },
                    StringComparer.OrdinalIgnoreCase
                );

                try
                {
                    await UploadFilesToServer(streamingJson, localNameToFullPathSA, Math.Max(1, batchSize));
                    Debug.Log("StreamingAssets Upload Completed!");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Uploader] StreamingAssets upload failed: {ex.Message}\n{ex}");
                    throw;
                }
            }

            if (IDosGamesSDKSettings.Instance.DevBuild)
            {
                IDosGamesSDKSettings.Instance.WebGLUrl = "https://cloud.idosgames.com/drive/app/" + IDosGamesSDKSettings.Instance.TitleID + "-dev/index.html";
            }
            else
            {
                IDosGamesSDKSettings.Instance.WebGLUrl = "https://cloud.idosgames.com/drive/app/" + IDosGamesSDKSettings.Instance.TitleID + "/index.html";
            }

            Debug.Log("WebGL URL: " + IDosGamesSDKSettings.Instance.WebGLUrl);
        }

        private static async Task UploadFilesToServer(string serverResponseJson, Dictionary<string, string> localNameToFullPath, int batchSize = 1)
        {
            if (string.IsNullOrEmpty(serverResponseJson))
                throw new ArgumentException("serverResponseJson is empty");
            if (localNameToFullPath == null || localNameToFullPath.Count == 0)
                throw new ArgumentException("localNameToFullPath is empty");

            JObject root;
            try
            {
                root = JObject.Parse(serverResponseJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Server JSON parse error: {ex.Message}\nJSON:\n{serverResponseJson}");
                throw;
            }

            JToken filesNode = root["files"] ?? root;

            var map = new Dictionary<string, (string url, string contentType)>(StringComparer.OrdinalIgnoreCase);

            if (filesNode is JObject objMap)
            {
                if (objMap.TryGetValue("presigned", StringComparison.OrdinalIgnoreCase, out var presignedTok)
                    && presignedTok is JObject presignedObj)
                {
                    objMap = presignedObj;
                }

                foreach (var prop in objMap.Properties())
                {
                    var entry = prop.Value as JObject;
                    if (entry == null) continue;

                    var url = (string)entry["url"];
                    var ct = (string)(entry["contentType"] ?? "application/octet-stream");
                    if (!string.IsNullOrEmpty(url))
                        map[prop.Name] = (url, ct);
                }
            }
            else if (filesNode is JArray arr)
            {
                foreach (var item in arr.OfType<JObject>())
                {
                    var name = (string)(item["name"] ?? item["path"] ?? item["key"]);
                    var url = (string)item["url"];
                    var ct = (string)(item["contentType"] ?? "application/octet-stream");
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                    {
                        map[name] = (url, ct);
                    }
                }
            }
            else
            {
                Debug.LogError("Unsupported server JSON shape for presigned URLs.");
                Debug.Log(serverResponseJson);
                throw new Exception("Unsupported server JSON shape.");
            }

            // Диагностика, если что-то не сходится
            if (map.Count == 0)
            {
                Debug.LogError("No presigned URLs found in server response.");
                Debug.Log(serverResponseJson);
                throw new Exception("No presigned URLs");
            }

            var names = localNameToFullPath.Keys.ToList();

            var serverKeysPreview = string.Join("\n", map.Keys.Take(10));
            var clientKeysPreview = string.Join("\n", names.Take(10));
            Debug.Log($"Server keys sample:\n{serverKeysPreview}");
            Debug.Log($"Client keys sample:\n{clientKeysPreview}");

            var urlByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var typeByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in names)
            {
                if (!map.TryGetValue(name, out var info))
                {
                    var baseName = System.IO.Path.GetFileName(name);
                    var candidates = map.Keys.Where(k => string.Equals(System.IO.Path.GetFileName(k), baseName, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (candidates.Count == 1)
                    {
                        info = map[candidates[0]];
                        Debug.LogWarning($"Key '{name}' not found. Fallback to unique basename match: '{candidates[0]}'");
                    }
                    else
                    {
                        var serverList = string.Join("\n", map.Keys.Take(50));
                        throw new Exception($"Presigned URL not found in server response for file '{name}'.\n" +
                                            $"TIP: убедись, что ключ в запросе и ответе совпадает 1:1 (относительный путь с '/').\n" +
                                            $"Server keys (first 50):\n{serverList}");
                    }
                }

                urlByName[name] = info.url;
                typeByName[name] = string.IsNullOrEmpty(info.contentType) ? "application/octet-stream" : info.contentType;
            }

            int step = Math.Max(1, batchSize);
            for (int i = 0; i < names.Count; i += step)
            {
                var batch = names.Skip(i).Take(step).ToList();
                var tasks = new List<Task>(batch.Count);

                foreach (var name in batch)
                {
                    if (!localNameToFullPath.TryGetValue(name, out var localPath) || string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                        throw new FileNotFoundException($"Local file not found for '{name}'", localPath);

                    string url = urlByName[name];
                    string ct = typeByName[name];

                    tasks.Add(PutOneFile(url, localPath, ct));
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception)
                {
                    // Пробегаемся и печатаем фейлы поимённо
                    for (int ti = 0; ti < tasks.Count; ti++)
                    {
                        if (tasks[ti].IsFaulted && tasks[ti].Exception != null)
                        {
                            Debug.LogError($"[Uploader] File failed: {batch[ti]} :: {tasks[ti].Exception.GetBaseException().Message}");
                        }
                    }
                    throw; // пусть общий флоу тоже знает, что партия упала
                }

                await Task.Delay(500);
                Debug.Log($"Upload batch done: {Math.Min(i + step, names.Count)}/{names.Count}");
            }
        }

        private static async Task PutOneFile(string url, string localPath, string contentType)
        {
            // Читаем файл
            byte[] data;
            using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                data = new byte[fs.Length];
#if UNITY_2021_2_OR_NEWER
                int read = await fs.ReadAsync(data, 0, data.Length);
#else
        int read = await Task.Factory.StartNew(() => fs.Read(data, 0, data.Length));
#endif
                if (read != data.Length) Array.Resize(ref data, read);
            }

            string fileName = Path.GetFileName(localPath);

            for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
            {
                using (var req = new UnityEngine.Networking.UnityWebRequest(url, "PUT"))
                {
                    req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(data);
                    req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();

                    if (!string.IsNullOrEmpty(contentType))
                        req.SetRequestHeader("Content-Type", contentType);

                    req.useHttpContinue = false;
                    //req.chunkedTransfer = false;

                    req.timeout = UPLOAD_TIMEOUT_SEC;

                    var op = req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                    while (!op.isDone) await Task.Yield();
#else
            while (!req.isDone) await Task.Yield();
#endif
                    long code = req.responseCode;
                    string errorText = req.error;
                    string body = req.downloadHandler?.text;
                    string bodyShort = string.IsNullOrEmpty(body) ? "" :
                                       (body.Length > 600 ? body.Substring(0, 600) + "..." : body);

                    if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success && (code >= 200 && code < 300))
                    {
                        // Успех
                        return;
                    }

                    // ЛОГ: код, ошибка, кусок ответа и ключевые заголовки
                    Debug.LogError(
                        $"[PUT ERROR] {fileName} -> {code} {errorText}\n" +
                        $"Response (short): {bodyShort}\n" +
                        $"Headers: {DumpHeaders(req)}");

                    // Решаем — ретраить или падать
                    if (attempt < MAX_RETRIES - 1 && ShouldRetry(code, errorText, body))
                    {
                        int delay = BackoffDelayMs(attempt);
                        Debug.LogWarning($"Retry {attempt + 1}/{MAX_RETRIES - 1} in {delay} ms for {fileName}...");
                        await Task.Delay(delay);
                        continue;
                    }

                    throw new Exception($"PUT failed: {code} {errorText} ({fileName})");
                }
            }
        }

        private static string DumpHeaders(UnityEngine.Networking.UnityWebRequest req)
        {
            try
            {
                var headers = req.GetResponseHeaders();
                if (headers == null || headers.Count == 0) return "(no headers)";
                return string.Join("; ", headers.Select(kv => kv.Key + "=" + kv.Value).ToArray());
            }
            catch { return "(headers unavailable)"; }
        }

        private static Task<List<FileUpload>> AddFilesFromDirectory(string currentDirectory, string rootDirectory)
        {
            var filesToUpload = new List<FileUpload>();
            string[] files = Directory.GetFiles(currentDirectory);
            string[] requiredEndings = { ".loader.js", ".data.unityweb", ".framework.js.unityweb", ".wasm.unityweb" };

            foreach (string file in files)
            {
                if (requiredEndings.Any(ending => file.EndsWith(ending, StringComparison.OrdinalIgnoreCase)))
                {
                    string relativePath = Path.GetRelativePath(rootDirectory, file);
                    string normalized = NormalizeUploadPath(relativePath);
                    filesToUpload.Add(new FileUpload(normalized, Array.Empty<byte>()));
                    Debug.Log($"File registered (metadata only): {normalized}");
                }
                else
                {
                    Debug.Log($"Skipping file: {file}");
                }
            }
            return Task.FromResult(filesToUpload);
        }

        private static Task<List<FileUpload>> AddStreamingAssetsRecursively(string streamingAssetsDirectory)
        {
            var filesToUpload = new List<FileUpload>();

            foreach (var file in Directory.EnumerateFiles(streamingAssetsDirectory, "*", SearchOption.AllDirectories))
            {
                if (ShouldSkip(file)) continue;

                string relative = Path.GetRelativePath(streamingAssetsDirectory, file);
                string uploadPath = NormalizeUploadPath(Path.Combine("StreamingAssets", relative));

                filesToUpload.Add(new FileUpload(uploadPath, Array.Empty<byte>()));
                Debug.Log($"StreamingAsset registered (metadata only): {uploadPath}");
            }

            return Task.FromResult(filesToUpload);
        }

        private static bool ShouldSkip(string filePath)
        {
            return filePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith(".DS_Store", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith("Thumbs.db", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeUploadPath(string path)
        {
            return path.Replace('\\', '/');
        }

        private static async Task<byte[]> ReadFileAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1024, useAsync: true))
            {
                byte[] buffer = new byte[sourceStream.Length];
                int numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static void DeleteAllSettings()
        {
            IDosGamesSDKSettings.Instance.DeveloperSecretKey = null;
            IDosGamesSDKSettings.Instance.WebGLUrl = null;
            IDosGamesSDKSettings.Instance.TitleID = "0";
            IDosGamesSDKSettings.Instance.TitleTemplateID = "default";
            IDosGamesSDKSettings.Instance.BuildKey = "";
            PlayerPrefs.DeleteAll();
        }
    }
}
#endif