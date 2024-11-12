#if UNITY_ANDROID && IDOSGAMES_FIREBASE_ANALYTICS || UNITY_IOS && IDOSGAMES_FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif
using System;
using System.Collections;
using UnityEngine;
#if IDOSGAMES_MOBILE_IAP
using UnityEngine.Purchasing;
#endif

namespace IDosGames
{
	public class Analytics : MonoBehaviour
	{
		private static Analytics _instance;

		private void Awake()
		{
			if (_instance == null || ReferenceEquals(this, _instance))
			{
				_instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}

        void Start()
        {
            IDosGamesSDKSettings.Instance.IsPlaying = true;
            StartCoroutine(TrackPlayTime());
        }

        private void OnEnable()
		{
#if IDOSGAMES_MOBILE_IAP
			IAPService.PurchaseCompleted += ReportIAPRevenue;
#endif
            AdMediation.RevenueDataReceived += ReportAdRevenue;
		}

		private void OnDisable()
		{
#if IDOSGAMES_MOBILE_IAP
			IAPService.PurchaseCompleted -= ReportIAPRevenue;
#endif
            AdMediation.RevenueDataReceived -= ReportAdRevenue;
		}

		public static void Send(string name, string parameterName = "", string parameterValue = "")
		{
#if UNITY_ANDROID && IDOSGAMES_FIREBASE_ANALYTICS || UNITY_IOS && IDOSGAMES_FIREBASE_ANALYTICS
			FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
#endif

#if IDOSGAMES_APP_METRICA
            AppMetricaAnalytics.Instance.ReportEvent(name);
#endif
        }

#if IDOSGAMES_MOBILE_IAP
        private void ReportIAPRevenue(PurchaseEventArgs args)
		{
			if (args.purchasedProduct.receipt == null)
			{
				return;
			}

			var receipt = IAPPurchase.ConvertFromJson(args.purchasedProduct.receipt);
			string currency = args.purchasedProduct.metadata.isoCurrencyCode;
			decimal price = args.purchasedProduct.metadata.localizedPrice;

#if IDOSGAMES_APP_METRICA
			YandexAppMetricaRevenue revenue = new(price, currency);
			YandexAppMetricaReceipt yaReceipt = new();

#if UNITY_ANDROID
			JsonIAPPayloadData payloadData = JsonIAPPayloadData.ConvertFromJson(receipt.Payload);
			yaReceipt.Signature = payloadData.signature;
			yaReceipt.Data = payloadData.json;
#elif UNITY_IOS
			yaReceipt.TransactionID = receipt.TransactionID;
            yaReceipt.Data = receipt.Payload;
#endif
			revenue.Receipt = yaReceipt;

			AppMetricaAnalytics.Instance.ReportRevenue(revenue);
#endif
        }
#endif

        private void ReportAdRevenue(AdRevenueData data)
		{
			ReportAdRevenueFirebase(data);
			ReportAdRevenueAppMetrica(data);
		}

		private void ReportAdRevenueFirebase(AdRevenueData data)
		{
#if UNITY_ANDROID && IDOSGAMES_FIREBASE_ANALYTICS || UNITY_IOS && IDOSGAMES_FIREBASE_ANALYTICS
			Parameter[] AdParameters = {
				new Parameter("ad_platform", data.AdPlatform),
				new Parameter("ad_source", data.AdNetwork),
				new Parameter("ad_unit_name", data.AdUnitName),
				new Parameter("ad_format", data.AdType),
				new Parameter("currency", data.Currency),
				new Parameter("value", $"{data.Revenue}")
			};

			FirebaseAnalytics.LogEvent("ad_revenue", AdParameters);
#endif
        }

        private void ReportAdRevenueAppMetrica(AdRevenueData data)
		{
#if IDOSGAMES_APP_METRICA
			YandexAppMetricaAdRevenue revenue = new((double)data.Revenue, data.Currency)
			{
				AdPlacementName = data.AdPlatform,
				AdNetwork = data.AdNetwork,
				AdUnitName = data.AdUnitName
			};

			Enum.TryParse(data.AdType, out YandexAppMetricaAdRevenue.AdTypeEnum adType);
			revenue.AdType = adType;

			AppMetricaAnalytics.Instance.ReportAdRevenue(revenue);
#endif
        }

        IEnumerator TrackPlayTime()
        {
            while (true)
            {
                if (IDosGamesSDKSettings.Instance.IsPlaying)
                {
                    IDosGamesSDKSettings.Instance.PlayTime += 1;
                }
                yield return new WaitForSecondsRealtime(1);
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                IDosGamesSDKSettings.Instance.IsPlaying = false;
            }
            else
            {
                IDosGamesSDKSettings.Instance.IsPlaying = true;
            }
        }
    }
}