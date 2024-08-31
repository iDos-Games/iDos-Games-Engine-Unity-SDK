using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace IDosGames
{
    public static class WebSDK
    {
        private static string startAppParameter;
        private static string initDataUnsafe;
        public static string platform;

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

#endif

    }
}
