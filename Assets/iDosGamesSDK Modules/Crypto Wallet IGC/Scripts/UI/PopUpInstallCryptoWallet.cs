using UnityEngine;

namespace IDosGames
{
	public class PopUpInstallCryptoWallet : MonoBehaviour
	{
		public const string GOOGLE_PLAY_STORE_LINK = "https://play.google.com/store/apps/details?id=com.wallet.crypto.trustapp";
		public const string APPLE_APP_STORE_LINK = "https://apps.apple.com/app/apple-store/id1288339409";

		public void OpenURLMetamaskFromStore()
		{
			string url = string.Empty;

#if UNITY_ANDROID
			url = GOOGLE_PLAY_STORE_LINK;
#elif UNITY_IOS
			url = APPLE_APP_STORE_LINK;
#endif
			Application.OpenURL(url);
		}
	}
}