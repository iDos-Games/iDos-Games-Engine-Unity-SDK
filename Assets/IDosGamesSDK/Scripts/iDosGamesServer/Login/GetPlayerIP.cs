using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    public static class IPUtility
    {
        private const string ipApiUrl = "https://api.ipify.org?format=json";
        public static string Ip { get; private set; }

        public static async Task GetIPAddressAsync()
        {
            UnityWebRequest ipRequest = UnityWebRequest.Get(ipApiUrl);

            var asyncOperation = ipRequest.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (ipRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error getting IP address: " + ipRequest.error);
            }
            else
            {
                var jsonResponse = ipRequest.downloadHandler.text;
                var ipStart = jsonResponse.IndexOf(":\"") + 2;
                var ipEnd = jsonResponse.IndexOf("\"", ipStart);
                Ip = jsonResponse.Substring(ipStart, ipEnd - ipStart);
                Debug.Log($"Player IP: {Ip}");
            }
        }
    }
}