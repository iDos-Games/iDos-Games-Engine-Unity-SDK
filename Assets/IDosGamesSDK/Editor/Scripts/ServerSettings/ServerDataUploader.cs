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

            await IGSAdminApi.UploadWebGLBuild(allFiles);
            Debug.Log("All Files Upload Completed!");

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

        private static async Task<List<FileUpload>> AddFilesFromDirectory(string currentDirectory, string rootDirectory)
        {
            List<FileUpload> filesToUpload = new List<FileUpload>();
            string[] files = Directory.GetFiles(currentDirectory);

            string[] requiredEndings = new string[] { ".loader.js", ".data.unityweb", ".framework.js.unityweb", ".wasm.unityweb" };

            foreach (string file in files)
            {
                if (requiredEndings.Any(ending => file.EndsWith(ending, StringComparison.OrdinalIgnoreCase)))
                {
                    byte[] fileData = await ReadFileAsync(file);
                    if (fileData == null || fileData.Length == 0)
                    {
                        Debug.LogWarning($"Failed to read file or file is empty: {file}");
                        continue;
                    }

                    string relativePath = Path.GetRelativePath(rootDirectory, file);
                    filesToUpload.Add(new FileUpload(relativePath, fileData));
                    Debug.Log($"File is uploading: {relativePath}");
                }
                else
                {
                    Debug.Log($"Skipping file: {file}");
                }
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