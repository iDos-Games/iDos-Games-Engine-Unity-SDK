#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace IDosGames
{
    public static class AzureFunctionLinkSave
    {
        private static readonly string containerName = "azure-webjobs-secrets";
        private static string storageAccountName;
        private static string storageAccountKey;

        public static void GetFunctionLinks()
        {
            GameObject tempGameObject = new GameObject("AzureFunctionLinkFetcherTemp");
            MonoBehaviour monoBehaviour = tempGameObject.AddComponent<AzureFunctionLinkFetcherMonoHelper>();
            monoBehaviour.StartCoroutine(GetFunctionLinksAsync(() => {
                GameObject.DestroyImmediate(tempGameObject);
            }));
        }

        private static void ParseConnectionString(string connString)
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

        private static IEnumerator GetFunctionLinksAsync(Action onComplete)
        {
            ParseConnectionString(IDosGamesSDKSettings.Instance.ServerConnectionString);
            if (string.IsNullOrEmpty(storageAccountName) || string.IsNullOrEmpty(storageAccountKey))
            {
                Debug.LogError("Failed to parse connection string.");
                onComplete?.Invoke();
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
                Debug.LogError("Error: " + listRequest.error);
                Debug.LogError("Response: " + listRequest.downloadHandler.text);
                onComplete?.Invoke();
                yield break;
            }

            string listXml = listRequest.downloadHandler.text;
            listXml = listXml.TrimStart('\uFEFF');
            var blobNames = ParseBlobNamesFromXml(listXml);
            foreach (var blobName in blobNames)
            {
                yield return GetFunctionLinkAsync(blobName);
            }
            onComplete?.Invoke();
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

        private static IEnumerator GetFunctionLinkAsync(string blobName)
        {
            if (blobName.EndsWith("host.json"))
            {
                yield break;
            }

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
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
            else
            {
                string jsonContent = request.downloadHandler.text;
                JObject functionConfig = JObject.Parse(jsonContent);
                JToken keyToken = functionConfig.SelectToken("keys[?(@.name == 'default' && @.encrypted == false)].value");

                if (keyToken != null)
                {
                    string functionCode = keyToken.ToString();
                    string functionName = blobName.Split('/').Last().Replace(".json", "").ToLower();
                    string functionLink = $"https://{functionConfig["hostName"]}/api/{functionName}?code={functionCode}";

                    if (Enum.TryParse(typeof(ServerFunctionNames), functionName, true, out var functionEnum))
                    {
                        switch ((ServerFunctionNames)functionEnum)
                        {
                            case ServerFunctionNames.IGSAdminApi: IDosGamesSDKSettings.Instance.IgsAdminApiLink = functionLink; break;
                            case ServerFunctionNames.IGSClientApi: IDosGamesSDKSettings.Instance.IgsClientApiLink = functionLink; break;
                            case ServerFunctionNames.UserDataSystem: IDosGamesSDKSettings.Instance.UserDataSystemLink = functionLink; break;
                            case ServerFunctionNames.SpinSystem: IDosGamesSDKSettings.Instance.SpinSystemLink = functionLink; break;
                            case ServerFunctionNames.ChestSystem: IDosGamesSDKSettings.Instance.ChestSystemLink = functionLink; break;
                            case ServerFunctionNames.RewardAndProfitSystem: IDosGamesSDKSettings.Instance.RewardAndProfitSystemLink = functionLink; break;
                            case ServerFunctionNames.ReferralSystem: IDosGamesSDKSettings.Instance.ReferralSystemLink = functionLink; break;
                            case ServerFunctionNames.EventSystem: IDosGamesSDKSettings.Instance.EventSystemLink = functionLink; break;
                            case ServerFunctionNames.ShopSystem: IDosGamesSDKSettings.Instance.ShopSystemLink = functionLink; break;
                            case ServerFunctionNames.DealOfferSystem: IDosGamesSDKSettings.Instance.DealOfferSystemLink = functionLink; break;
                            case ServerFunctionNames.TryMakeTransaction: IDosGamesSDKSettings.Instance.TryMakeTransactionLink = functionLink; break;
                            case ServerFunctionNames.TryDoMarketplaceAction: IDosGamesSDKSettings.Instance.TryDoMarketplaceActionLink = functionLink; break;
                            case ServerFunctionNames.GetDataFromMarketplace: IDosGamesSDKSettings.Instance.GetDataFromMarketplaceLink = functionLink; break;
                            case ServerFunctionNames.ValidateIAPSubscription: IDosGamesSDKSettings.Instance.ValidateIAPSubscriptionLink = functionLink; break;
                            case ServerFunctionNames.FriendSystem: IDosGamesSDKSettings.Instance.FriendSystemLink = functionLink; break;
                            case ServerFunctionNames.LoginSystem: IDosGamesSDKSettings.Instance.LoginSystemLink = functionLink; break;
                            default: Debug.LogWarning($"Unexpected function name: {functionName}"); break;
                        }

                        Debug.Log("Function Link: " + functionLink);
                    }
                    else
                    {
                        Debug.LogWarning($"Unexpected function name: {functionName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"No unencrypted default key found in {blobName}");
                }
            }
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

    }
}
#endif