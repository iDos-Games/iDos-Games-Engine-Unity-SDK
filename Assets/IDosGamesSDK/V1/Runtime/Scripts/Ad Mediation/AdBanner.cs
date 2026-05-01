using UnityEngine;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace IDosGames
{
	public class AdBanner : MonoBehaviour
	{
		private AdMediation _adMediation => AdMediation.Instance;

		private bool _bannerShowed = false;

		private void OnEnable()
		{
            IDosGamesData.User.OnInventoryUpdated += OnInventoryUpdated;
		}

		private void OnDisable()
		{
            IDosGamesData.User.OnInventoryUpdated -= OnInventoryUpdated;
		}

		private void OnInventoryUpdated()
		{

#if UNITY_IOS
			bool statusReceived = GetIOSAppTrackingTransparencyStatus();

			if (statusReceived == false)
			{
				return;
			}
#endif

			if (_bannerShowed == false)
			{
				if (!DataService.HasVIPStatus)
				{
					if (_adMediation != null)
					{
						_adMediation.ShowBanner();
						_bannerShowed = true;
					}
				}
			}

			if (_bannerShowed)
			{
				if (DataService.HasVIPStatus)
				{
					if (_adMediation != null)
					{
						_adMediation.HideBanner();
						_bannerShowed = false;
					}
				}
			}
		}

#if UNITY_IOS
    private bool GetIOSAppTrackingTransparencyStatus()
    {
		try
		{
			var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

			return status != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;
		}
		catch 
		{ 
			return false; 
		}
    }
#endif
	}
}