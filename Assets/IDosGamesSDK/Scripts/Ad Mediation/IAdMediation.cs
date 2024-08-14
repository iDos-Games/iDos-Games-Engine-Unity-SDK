using System;

namespace IDosGames
{
	public interface IAdMediation
	{
		public void Initialize();

		public void ShowBanner();

		public void HideBanner();

		public void RemoveAds();

		public bool ShowInterstitialAd();

		public bool ShowRewardedVideo(Action<bool> onRewardedVideoClosed);

		public void SetConsent(bool consent);


		public event Action<AdRevenueData> AdRevenueDataReceived;
	}
}