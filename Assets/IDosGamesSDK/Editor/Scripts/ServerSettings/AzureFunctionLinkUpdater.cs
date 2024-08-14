#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    public class AzureFunctionLinkUpdater
    {
        private static readonly string containerName = "azure-webjobs-secrets";
        public static string storageAccountName;
        private static string storageAccountKey;

        public static void UpdateJsonFiles()
        {
            GameObject tempGameObject = new GameObject("AzureJsonUpdaterTemp");
            MonoBehaviour monoBehaviour = tempGameObject.AddComponent<AzureFunctionLinkFetcherMonoHelper>();
            Debug.Log("Loading ...");
            monoBehaviour.StartCoroutine(UpdateJsonFilesAsync(() =>
            {
                GameObject.DestroyImmediate(tempGameObject);

                AzureFunctionLinkSave.GetFunctionLinks();
            }, tempGameObject));
        }

        private static string GetAuthorizationHeader(string method, DateTime now, string accountName, string accountKey, string resource)
        {
            string stringToSign = $"{method}\n\n\n\n\n\n\n\n\n\n\n\nx-ms-date:{now:R}\nx-ms-version:2020-06-12\n/{accountName}/{resource}";
            byte[] keyByteArray = Convert.FromBase64String(accountKey);

            using (HMACSHA256 hmac = new HMACSHA256(keyByteArray))
            {
                byte[] stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);
                byte[] signatureBytes = hmac.ComputeHash(stringToSignBytes);
                string signature = Convert.ToBase64String(signatureBytes);
                string authHeader = $"SharedKey {accountName}:{signature}";
                return authHeader;
            }
        }

        private static string GetAuthorizationHeaderForPut(string method, DateTime now, string accountName, string accountKey, string resource, long contentLength)
        {
            string formattedDate = now.ToString("R");

            string stringToSign = string.Join("\n", new string[]
            {
                method,
                "", // Content-Encoding empty
                "", // Content-Language empty
                contentLength.ToString(), // Content-Length
                "", // Content-MD5 empty
                "application/octet-stream", // Content-Type
                "", // Date empty
                "", // If-Modified-Since empty
                "", // If-Match empty
                "", // If-None-Match empty
                "", // If-Unmodified-Since empty
                "", // Range empty
                "x-ms-blob-type:BlockBlob",
                $"x-ms-date:{formattedDate}",
                "x-ms-version:2020-06-12",
                $"/{accountName}/{resource}"
            });

            byte[] keyByteArray = Convert.FromBase64String(accountKey);

            using (HMACSHA256 hmac = new HMACSHA256(keyByteArray))
            {
                byte[] stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);
                byte[] signatureBytes = hmac.ComputeHash(stringToSignBytes);
                string signature = Convert.ToBase64String(signatureBytes);
                string authHeader = $"SharedKey {accountName}:{signature}";
                return authHeader;
            }
        }

        private static IEnumerator ExecutePutRequest(string blobUri, byte[] jsonToBytes, string storageAccountName, string storageAccountKey, string containerName, string blobName)
        {
            UnityWebRequest putRequest = UnityWebRequest.Put(blobUri, jsonToBytes);
            DateTime now = DateTime.UtcNow;
            string formattedDate = now.ToString("R");
            putRequest.SetRequestHeader("x-ms-blob-type", "BlockBlob");
            putRequest.SetRequestHeader("x-ms-date", formattedDate);
            putRequest.SetRequestHeader("x-ms-version", "2020-06-12");

            string resource = $"{containerName}/{blobName}";
            long contentLength = jsonToBytes.Length;
            string authorizationHeader = GetAuthorizationHeaderForPut("PUT", now, storageAccountName, storageAccountKey, resource, contentLength);
            putRequest.SetRequestHeader("Authorization", authorizationHeader);

            yield return putRequest.SendWebRequest();

            if (putRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error updating JSON: {putRequest.error}, Response: {putRequest.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"JSON updated successfully: {blobName}");
            }
        }

        private static IEnumerator UpdateJsonFilesAsync(Action onComplete, GameObject tempGameObject)
        {
            try
            {
                ParseConnectionString(IDosGamesSDKSettings.Instance.ServerConnectionString);

                if (string.IsNullOrEmpty(storageAccountName) || string.IsNullOrEmpty(storageAccountKey))
                {
                    Debug.LogError("Failed to parse connection string.");
                    yield break;
                }

                string listUri = $"https://{storageAccountName}.blob.core.windows.net/{containerName}?restype=container&comp=list";
                UnityWebRequest listRequest = UnityWebRequest.Get(listUri);
                DateTime now = DateTime.UtcNow;
                string formattedDate = now.ToString("R");
                listRequest.SetRequestHeader("x-ms-date", formattedDate);
                listRequest.SetRequestHeader("x-ms-version", "2020-06-12");
                string authorizationHeader = GetAuthorizationHeader("GET", now, storageAccountName, storageAccountKey, $"{containerName}\ncomp:list\nrestype:container");
                listRequest.SetRequestHeader("Authorization", authorizationHeader);

                yield return listRequest.SendWebRequest();

                if (listRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error: {listRequest.error}, Response: {listRequest.downloadHandler.text}");
                    yield break;
                }

                string listXml = listRequest.downloadHandler.text;
                listXml = listXml.TrimStart('\uFEFF');
                List<string> blobNames = ParseBlobNamesFromXml(listXml);

                foreach (var blobName in blobNames)
                {
                    if (!blobName.EndsWith("host.json"))
                    {
                        yield return UpdateJsonBlobAsync(blobName);
                    }
                }

                onComplete?.Invoke();
            }
            finally
            {
                Debug.Log("Finished!");
                GameObject.DestroyImmediate(tempGameObject);

            }
        }

        private static IEnumerator UpdateJsonBlobAsync(string blobName)
        {
            string blobUri = $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{blobName}";
            UnityWebRequest request = UnityWebRequest.Get(blobUri);
            DateTime now = DateTime.UtcNow;
            string formattedDate = now.ToString("R");
            request.SetRequestHeader("x-ms-date", formattedDate);
            request.SetRequestHeader("x-ms-version", "2020-06-12");
            string authorizationHeader = GetAuthorizationHeader("GET", now, storageAccountName, storageAccountKey, $"{containerName}/{blobName}");
            request.SetRequestHeader("Authorization", authorizationHeader);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}, Response: {request.downloadHandler.text}");
            }
            else
            {
                string jsonContent = request.downloadHandler.text;
                JObject functionConfig = JObject.Parse(jsonContent);

                if (functionConfig["keys"][0]["encrypted"].Value<bool>())
                {
                    string value = functionConfig["keys"][0]["value"].Value<string>();
                    string shortenedValue = value.Substring(value.Length - 54) + "==";
                    functionConfig["keys"][0]["value"] = shortenedValue;
                    functionConfig["keys"][0]["encrypted"] = false;

                    string updatedJsonContent = functionConfig.ToString();
                    byte[] jsonToBytes = Encoding.UTF8.GetBytes(updatedJsonContent);

                    yield return ExecutePutRequest(blobUri, jsonToBytes, storageAccountName, storageAccountKey, containerName, blobName);
                }
            }
        }

        public static void ParseConnectionString(string connString)
        {
            var parts = connString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var keyValue = part.Split(new[] { '=' }, 2);
                if (keyValue.Length != 2)
                {
                    continue;
                }
                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();
                switch (key)
                {
                    case "AccountName":
                        storageAccountName = value;
                        break;
                    case "AccountKey":
                        storageAccountKey = value;
                        break;
                }
            }
            if (string.IsNullOrEmpty(storageAccountName) || string.IsNullOrEmpty(storageAccountKey))
            {
                Debug.LogError("Invalid connection string. AccountName or AccountKey is missing.");
            }
        }

        private static List<string> ParseBlobNamesFromXml(string xml)
        {
            List<string> blobNames = new List<string>();
            try
            {
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(xml);
                var nodes = xmlDoc.GetElementsByTagName("Name");
                foreach (System.Xml.XmlNode node in nodes)
                {
                    blobNames.Add(node.InnerText);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse XML. Error: " + ex.Message);
            }
            return blobNames;
        }
    }

    public class AzureFunctionLinkFetcherMonoHelper : MonoBehaviour
    {
        // This class helps to run coroutines on a temporary GameObject
    }
}
#endif