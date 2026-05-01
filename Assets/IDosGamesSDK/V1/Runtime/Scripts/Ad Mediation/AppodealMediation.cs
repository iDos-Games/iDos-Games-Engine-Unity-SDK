#if IDOSGAMES_AD_MEDIATION_APPODEAL
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class AppodealMediation : MonoBehaviour, IAdMediation, IAppodealInitializationListener, IRewardedVideoAdListener, IAdRevenueListener
	{
#if UNITY_IOS
		private string APP_KEY => IDosGamesSDKSettings.Instance.MediationAppKeyIOS;
#else
		private string APP_KEY => IDosGamesSDKSettings.Instance.MediationAppKeyAndroid;
#endif
		private BannerPosition _bannerPosition => IDosGamesSDKSettings.Instance.BannerPosition;
		private bool _bannerEnabled => IDosGamesSDKSettings.Instance.BannerEnabled;

		private const int ADD_TYPES = AppodealAdType.Interstitial | AppodealAdType.Banner | AppodealAdType.RewardedVideo;

		private Action<bool> _onRewardedVideoClosed;

		public event Action<AdRevenueData> AdRevenueDataReceived;

		public void Initialize()
		{
			SetCallbacks(); //Must be called before initialization
			Appodeal.Initialize(APP_KEY, ADD_TYPES, this);
		}

		private void SetCallbacks()
		{
			Appodeal.SetRewardedVideoCallbacks(this);
		}

		public void OnInitializationFinished(List<string> errors)
		{
			if (errors == null)
			{
				Debug.Log("Appodeal Initialization Finished.");
			}
			else
			{
				Debug.LogError("Appodeal Initialization Failed. Error(s):");
				if (errors.Count > 0)
				{
					foreach (var error in errors)
					{
						Debug.LogError(error);
					}
				}
			}
		}

		public void ShowBanner()
		{
			if (_bannerEnabled)
			{
				if (Appodeal.IsLoaded(AppodealAdType.Banner))
				{
					ShowBannerByPosition();
				}
				else
				{
					Initialize();

					if (Appodeal.IsLoaded(AppodealAdType.Banner))
					{
						ShowBanner();
					}
				}
			}
		}

		private void ShowBannerByPosition()
		{
			switch (_bannerPosition)
			{
				case BannerPosition.Top:
					Appodeal.Show(AppodealShowStyle.BannerTop);
					break;

				case BannerPosition.Bottom:
					Appodeal.Show(AppodealShowStyle.BannerBottom);
					break;

				case BannerPosition.Left:
					Appodeal.Show(AppodealShowStyle.BannerLeft);
					break;

				case BannerPosition.Right:
					Appodeal.Show(AppodealShowStyle.BannerRight);
					break;

				default:
					Appodeal.Show(AppodealShowStyle.BannerBottom);
					break;
			}
		}

		public void HideBanner()
		{
			Appodeal.Hide(AppodealAdType.Banner);
		}

		public void RemoveAds()
		{
			HideBanner();
		}

		public bool ShowInterstitialAd()
		{
			if (Appodeal.IsLoaded(AppodealAdType.Interstitial))
			{
				Appodeal.Show(AppodealShowStyle.Interstitial);
				return true;
			}
			else
			{
				Initialize();

				if (Appodeal.IsLoaded(AppodealAdType.Interstitial))
				{
					ShowInterstitialAd();
				}
			}
			return false;
		}

		public bool ShowRewardedVideo(Action<bool> onRewardedVideoClosed)
		{
			if (Appodeal.IsLoaded(AppodealAdType.RewardedVideo))
			{
				_onRewardedVideoClosed += onRewardedVideoClosed;
				Appodeal.Show(AppodealShowStyle.RewardedVideo);
				return true;
			}
			else
			{
				Initialize();

				if (Appodeal.IsLoaded(AppodealAdType.RewardedVideo))
				{
					ShowRewardedVideo(onRewardedVideoClosed);
				}
			}
			return false;
		}

		public void SetConsent(bool consent)
		{
		}

		#region Rewarded Video callback handlers

		public void OnRewardedVideoLoaded(bool isPrecache)
		{
		}

		public void OnRewardedVideoFailedToLoad()
		{
		}

		public void OnRewardedVideoShowFailed()
		{
		}

		public void OnRewardedVideoShown()
		{
		}

		public void OnRewardedVideoClicked()
		{
		}

		public void OnRewardedVideoClosed(bool finished)
		{
			_onRewardedVideoClosed.Invoke(finished);
			_onRewardedVideoClosed = null;
		}

		public void OnRewardedVideoFinished(double amount, string name)
		{
		}

		public void OnRewardedVideoExpired()
		{
		}

		public void OnAdRevenueReceived(AppodealAdRevenue ad)
		{
			AdRevenueData revenue = new()
			{
				AdPlatform = "appodeal",
				AdType = ad.AdType,
				AdNetwork = ad.NetworkName,
				AdUnitName = ad.AdUnitName,
				Currency = ad.Currency,
				Revenue = ad.Revenue
			};

			AdRevenueDataReceived?.Invoke(revenue);
		}

		#endregion Rewarded Video callback handlers
	}
}
#endif