using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Collections;
using Unity.Advertisement.IosSupport;
using UnityEngine.iOS;
#endif

namespace IDosGames
{
	public class IOSAppTrackingTransparencySystem : MonoBehaviour
	{

#if UNITY_IOS && !UNITY_EDITOR
		private void Awake()
		{
			StartCoroutine(nameof(CheckIDFAStatus));
		}

		private IEnumerator CheckIDFAStatus()
		{
			var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

			Version currentVersion = new(Device.systemVersion);
			Version ios14 = new("14.0");

			if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED && currentVersion >= ios14)
			{
				ATTrackingStatusBinding.RequestAuthorizationTracking();

				while (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
				{
					status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
					yield return null;
				}

				if (AdMediation.Instance != null)
				{
					bool consent = status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
					AdMediation.Instance.SetConsent(consent);
				}
			}
		}
#endif
	}
}