using System;
using UnityEngine;

namespace IDosGames
{
	public class AdMediation : MonoBehaviour, IAdMediation
	{
		public static AdMediation Instance { get; private set; }

		private IAdMediation _selectedMediation;

		public event Action<AdRevenueData> AdRevenueDataReceived;
		public static event Action<AdRevenueData> RevenueDataReceived;

		private void Awake()
		{
			if (IDosGamesSDKSettings.Instance.AdMediationPlatform == AdMediationPlatform.None)
			{
				Destroy(gameObject);
				return;
			}

			if (Instance == null || ReferenceEquals(this, Instance))
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}

#if IDOSGAMES_AD_MEDIATION_LEVELPLAY
            _selectedMediation = gameObject.AddComponent<IronSourceMediation>();
#elif IDOSGAMES_AD_MEDIATION_APPODEAL
			_selectedMediation = gameObject.AddComponent<AppodealMediation>();
#endif

			_selectedMediation.AdRevenueDataReceived += OnAdRevenueDataReceived;

			Initialize();
		}

		private void OnAdRevenueDataReceived(AdRevenueData data)
		{
			AdRevenueDataReceived?.Invoke(data);
			RevenueDataReceived?.Invoke(data);
		}

		public void HideBanner()
		{
			_selectedMediation.HideBanner();
		}

		public void Initialize()
		{
			_selectedMediation.Initialize();
		}

		public void RemoveAds()
		{
			_selectedMediation.RemoveAds();
		}

		public void ShowBanner()
		{
			_selectedMediation.ShowBanner();
		}

		public bool ShowInterstitialAd()
		{
			return _selectedMediation.ShowInterstitialAd();
		}

		public bool ShowRewardedVideo(Action<bool> onRewardedVideoClosed)
		{
			return _selectedMediation.ShowRewardedVideo(onRewardedVideoClosed);
		}

		public void SetConsent(bool consent)
		{
			_selectedMediation.SetConsent(consent);
		}
	}
}