#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class ServerDataUploader : Editor
    {
        private static List<string> localFilePaths = new List<string>
        {
            "Assets/IDosGamesSDK/Editor/Data/TITLE_DATA.json",
            "Assets/IDosGamesSDK/Editor/Data/Currency.json",
            "Assets/IDosGamesSDK/Editor/Data/Catalog-Chest.json",
            "Assets/IDosGamesSDK/Editor/Data/Catalog-Primary.json",
            "Assets/IDosGamesSDK/Editor/Data/Catalog-Skin.json",
            "Assets/IDosGamesSDK/Editor/Data/Catalog-Spin.json"
        };
        private static List<string> remoteFileName = new List<string>
        {
            "TITLE_DATA.json",
            "Currency.json",
            "Catalog-Chest.json",
            "Catalog-Primary.json",
            "Catalog-Skin.json",
            "Catalog-Spin.json"
        };

        public static async void UploadData()
        {
            Debug.Log("Upload Start ...");

            List<FileUpload> filesToUpload = new List<FileUpload>();

            for (int i = 0; i < localFilePaths.Count; i++)
            {
                string localFilePath = localFilePaths[i];
                string remoteFilePath = remoteFileName[i];

                if (!File.Exists(localFilePath))
                {
                    Debug.LogWarning($"File not found: {localFilePath}");
                    continue;
                }

                byte[] fileData = File.ReadAllBytes(localFilePath);

                if (fileData == null || fileData.Length == 0)
                {
                    Debug.LogWarning($"Failed to read file or file is empty: {localFilePath}");
                    continue;
                }

                filesToUpload.Add(new FileUpload(remoteFilePath, fileData));
            }

            await IGSAdminApi.UploadTitleDataFiles(filesToUpload);

            Debug.Log("Upload Successfully!");
        }

        public static async void UploadDataFromDirectory(string directoryPath, int batchSize = 1)
        {
            Debug.Log("Uploading Start ...");

            if(IDosGamesSDKSettings.Instance.ClearDirectory)
            {
                await IGSAdminApi.ClearWebGL();
            }
            
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var allFiles = await AddFilesFromDirectory(directoryPath, directoryPath);

            for (int i = 0; i < allFiles.Count; i += batchSize)
            {
                var batch = allFiles.GetRange(i, Math.Min(batchSize, allFiles.Count - i));
                foreach (var file in batch)
                {
                    //Debug.Log($"Uploading file: {file.FilePath}");
                }
                await IGSAdminApi.UploadWebGL(batch);
                Debug.Log($"... Uploaded batch {i / batchSize + 1}");
            }

            Debug.Log("All Files Upload Completed!");

            string appName = ExtractAppName(IDosGamesSDKSettings.Instance.ServerLink);

            if (IDosGamesSDKSettings.Instance.DevBuild)
            {
                IDosGamesSDKSettings.Instance.WebGLUrl = "https://igc" + appName + ".blob.core.windows.net/public-data/" + IDosGamesSDKSettings.Instance.TitleID + "-dev/index.html";
            }
            else
            {
                IDosGamesSDKSettings.Instance.WebGLUrl = "https://igc" + appName + ".blob.core.windows.net/public-data/" + IDosGamesSDKSettings.Instance.TitleID + "/index.html";
            }
            
            Debug.Log("WebGL URL: " + IDosGamesSDKSettings.Instance.WebGLUrl);
        }

        private static async Task<List<FileUpload>> AddFilesFromDirectory(string currentDirectory, string rootDirectory)
        {
            List<FileUpload> filesToUpload = new List<FileUpload>();
            string[] files = Directory.GetFiles(currentDirectory);

            foreach (string file in files)
            {
                if (Path.GetExtension(file).Equals(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    //Debug.Log($"Skipping .meta file: {file}");
                    continue;
                }

                byte[] fileData = await ReadFileAsync(file);
                if (fileData == null || fileData.Length == 0)
                {
                    Debug.LogWarning($"Failed to read file or file is empty: {file}");
                    continue;
                }

                string relativePath = Path.GetRelativePath(rootDirectory, file);
                filesToUpload.Add(new FileUpload(relativePath, fileData));
                //Debug.Log($"File added: {relativePath}");
            }

            string[] directories = Directory.GetDirectories(currentDirectory);
            foreach (string directory in directories)
            {
                var subDirectoryFiles = await AddFilesFromDirectory(directory, rootDirectory);
                filesToUpload.AddRange(subDirectoryFiles);
            }

            return filesToUpload;
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

        public static async void RegisterTelegramWebhook()
        {
            Debug.Log("Register Started ...");

            await IGSAdminApi.RegisterTelegramWebhook();
        }

        public static string ExtractAppName(string serverLink)
        {
            string prefix = "app-";
            string suffix = ".azurewebsites.net";

            if (serverLink.Contains(prefix))
            {
                int startIndex = serverLink.IndexOf(prefix) + prefix.Length;
                int endIndex = serverLink.IndexOf(suffix);
                return serverLink.Substring(startIndex, endIndex - startIndex);
            }
            else
            {
                return serverLink.Replace("https://", string.Empty);
            }
        }

        public static void DeleteAllSettings()
        {
            IDosGamesSDKSettings.Instance.DeveloperSecretKey = null;
            IDosGamesSDKSettings.Instance.WebGLUrl = null;
            IDosGamesSDKSettings.Instance.TitleID = "";
        }
    }
}
#endif