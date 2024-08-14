using UnityEngine;

namespace IDosGames
{
	public class URLOpener : MonoBehaviour
	{
		private const string URL_PRIVACY_POLICY = "https://idosgames.com/privacy/";
		private const string URL_TERMS_OF_USE = "https://idosgames.com/terms/";
		private const string URL_SITE = "https://idosgames.com/";
        private const string URL_SUPPORT = "https://t.me/iDos_Games_bot";

        public void OpenCustomUrl(string url)
        {
            Application.OpenURL(url);
        }

        public static void OpenPrivacyPolicy()
		{
			Application.OpenURL(URL_PRIVACY_POLICY);
		}

		public static void OpenTermsofUse()
		{
			Application.OpenURL(URL_TERMS_OF_USE);
		}

		public static void OpenCryptocurrencySwap()
		{
			Application.OpenURL(URL_SITE);
		}

        public static void OpenSupport()
        {
            Application.OpenURL(URL_SUPPORT + "?start=" + IDosGamesSDKSettings.Instance.TitleID + "-" + AuthService.UserID);
        }

        public static void OpenRateUs()
        {
#if UNITY_ANDROID
			Application.OpenURL("https://play.google.com/store/apps/details?id=" + IDosGamesSDKSettings.Instance.AndroidBundleID);
#endif

#if UNITY_IOS
            Application.OpenURL("https://apps.apple.com/app/id" + IDosGamesSDKSettings.Instance.IosAppStoreID);
#endif
        }
    }
}