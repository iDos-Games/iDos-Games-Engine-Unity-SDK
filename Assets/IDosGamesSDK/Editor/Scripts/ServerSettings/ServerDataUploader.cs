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
        public static async void UploadDataFromDirectory(string directoryPath, int batchSize = 1)
        {
            Debug.Log("Uploading Start ...");

            await IGSAdminApi.ClearWebGL();

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
                await Task.Delay(1000);
                Debug.Log($"... Uploaded batch {i / batchSize + 1}");
            }

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

        public static void DeleteAllSettings()
        {
            IDosGamesSDKSettings.Instance.ServerLink = null;
            IDosGamesSDKSettings.Instance.DeveloperSecretKey = null;
            IDosGamesSDKSettings.Instance.WebGLUrl = null;
            IDosGamesSDKSettings.Instance.TitleID = "";
            PlayerPrefs.DeleteAll();
        }
    }
}
#endif