using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace IDosGames
{
    public static class WebSDK
    {
        private static string startAppParameter;
        public static string initDataUnsafe;
        public static string platform;
        public static string webAppLink;

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern string GetPlatform();

        public static void FetchPlatform()
        {
            platform = GetPlatform();
        }

        [DllImport("__Internal")]
        private static extern string GetStartAppParameter();

        public static void FetchStartAppParameter()
        {
            startAppParameter = GetStartAppParameter();
        }

        public static string GetStartAppParameterValue()
        {
            return startAppParameter;
        }

        [DllImport("__Internal")]
        private static extern void ShareAppLink(string appUrl);

        public static void ShareLink(string appUrl)
        {
            ShareAppLink(appUrl);
        }

        [DllImport("__Internal")]
        private static extern void OpenInvoice(string invoiceUrl);

        public static void OpenInvoiceLink(string invoiceUrl)
        {
            OpenInvoice(invoiceUrl);
        }

        [DllImport("__Internal")]
        private static extern string GetInitDataUnsafe();

        public static void FetchInitDataUnsafe()
        {
            initDataUnsafe = GetInitDataUnsafe();
        }

        public static InitData ParseInitDataUnsafe()
        {
            if (string.IsNullOrEmpty(initDataUnsafe))
            {
                throw new Exception("initDataUnsafe is null or empty");
            }

            return JsonUtility.FromJson<InitData>(initDataUnsafe);
        }

        [DllImport("__Internal")]
        private static extern void ShowAd(string blockId);
        public static void ShowAdInternal(string blockId)
        {
            ShowAd(blockId);
        }

        [DllImport("__Internal")]
        private static extern void CopyToClipboard(string text);
        public static void CopyTextToClipboard(string text)
        {
            CopyToClipboard(text);
        }

        [DllImport("__Internal")]
        private static extern void PasteFromClipboard();
        public static void PasteTextFromClipboard()
        {
            PasteFromClipboard();
        }

        [DllImport("__Internal")]
        private static extern string GetFullURL();
        public static void FetchFullURL()
        {
            webAppLink = GetFullURL();
        }

        [DllImport("__Internal")]
        private static extern void CacheSaveData(string key, byte[] data, int length);

        public static void SaveDataToCache(string key, byte[] data)
        {
            CacheSaveData(key, data, data.Length);
        }

        [DllImport("__Internal")]
        private static extern void CacheLoadData(string key, IntPtr callback);

        [AOT.MonoPInvokeCallback(typeof(Action<IntPtr, int>))]
        private static void OnCacheLoadData(IntPtr dataPtr, int length)
        {
            byte[] data = null;
            if (length > 0)
            {
                data = new byte[length];
                Marshal.Copy(dataPtr, data, 0, length);
            }
            cacheLoadDataCallback?.Invoke(data);
        }

        private static Action<byte[]> cacheLoadDataCallback;

        public static void LoadDataFromCache(string key, Action<byte[]> callback)
        {
            cacheLoadDataCallback = callback;
            CacheLoadData(key, Marshal.GetFunctionPointerForDelegate((Action<IntPtr, int>)OnCacheLoadData));
        }

        [DllImport("__Internal")]
        private static extern void CacheDeleteData(string key);

        public static void DeleteDataFromCache(string key)
        {
            CacheDeleteData(key);
        }

        [DllImport("__Internal")]
        private static extern void CacheClear();

        public static void ClearCache()
        {
            CacheClear();
        }

#endif

    }
}
