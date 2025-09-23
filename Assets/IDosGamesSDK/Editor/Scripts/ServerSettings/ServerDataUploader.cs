#if UNITY_EDITOR
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
        public static async void UploadDataFromDirectory(string directoryPath, int batchSize = 1)
        {
            Debug.Log("Uploading Start ...");

            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError("Directory does not exist: " + directoryPath);
                return;
            }

            var allFiles = await AddFilesFromDirectory(directoryPath, directoryPath);

            string[] requiredEndings = new string[] { ".loader.js", ".data.unityweb", ".framework.js.unityweb", ".wasm.unityweb" };
            var uploadedFiles = allFiles.ToDictionary(
                f => requiredEndings.FirstOrDefault(ending => f.FilePath.EndsWith(ending, StringComparison.OrdinalIgnoreCase)),
                f => f
            );

            if (uploadedFiles.Count != requiredEndings.Length || uploadedFiles.Any(kvp => kvp.Key == null))
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
                    Full = Path.Combine(directoryPath, f.FilePath)
                })
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Full, StringComparer.OrdinalIgnoreCase);

            await UploadFilesToServer(result, localNameToFullPath, Math.Max(1, batchSize));

            Debug.Log("Upload (build files) completed.");

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

                await UploadFilesToServer(streamingJson, localNameToFullPathSA, Math.Max(1, batchSize));
                Debug.Log("StreamingAssets Upload Completed!");
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

            var names = localNameToFullPath.Keys.ToList();
            var urlByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var typeByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in names)
            {
                string escaped = System.Text.RegularExpressions.Regex.Escape(name);
                var m = System.Text.RegularExpressions.Regex.Match(
                    serverResponseJson,
                    $"\"{escaped}\"\\s*:\\s*{{[^}}]*?\"url\"\\s*:\\s*\"([^\"]+)\"[^}}]*?\"contentType\"\\s*:\\s*\"([^\"]+)\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );

                if (!m.Success)
                    throw new Exception($"Presigned URL not found in server response for file '{name}'");

                urlByName[name] = m.Groups[1].Value;
                typeByName[name] = m.Groups[2].Value;
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

                await Task.WhenAll(tasks);
                Debug.Log($"Upload batch done: {Math.Min(i + step, names.Count)}/{names.Count}");
            }
        }

        private static async Task PutOneFile(string url, string localPath, string contentType)
        {
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

            using (var req = new UnityEngine.Networking.UnityWebRequest(url, "PUT"))
            {
                req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(data);
                req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                if (!string.IsNullOrEmpty(contentType))
                    req.SetRequestHeader("Content-Type", contentType);

                var op = req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                while (!op.isDone) await Task.Yield();
#else
            while (!req.isDone) await Task.Yield();
#endif
                if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                    throw new Exception($"PUT failed: {req.responseCode} {req.error} ({Path.GetFileName(localPath)})");
            }
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
                    filesToUpload.Add(new FileUpload(relativePath, Array.Empty<byte>()));
                    Debug.Log($"File registered (metadata only): {relativePath}");
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