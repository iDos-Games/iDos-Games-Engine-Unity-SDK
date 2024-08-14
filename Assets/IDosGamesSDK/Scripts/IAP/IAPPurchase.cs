using Newtonsoft.Json;
using UnityEngine;

namespace IDosGames
{
	public class IAPPurchase
	{
		public string Store;

		public string TransactionID;
		public string Payload;

#if UNITY_ANDROID
		public JsonIAPPayloadData PayloadData;
#endif

		public static IAPPurchase ConvertFromJson(string json)
		{
			try
			{
				var purchase = JsonConvert.DeserializeObject<IAPPurchase>(json);
#if UNITY_ANDROID
				purchase.PayloadData = JsonIAPPayloadData.ConvertFromJson(purchase.Payload);
#endif
				return purchase;
			}
			catch
			{
				Debug.LogWarning("Error converting IAPPurchase. Json:" + json);

				return null;
			}
		}
	}
}