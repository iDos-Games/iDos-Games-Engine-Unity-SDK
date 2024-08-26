using System;
using UnityEngine;
#if IDOSGAMES_MOBILE_IAP
using UnityEngine.Purchasing;
#endif

#pragma warning disable 0067
namespace IDosGames
{
	public class IAPValidator
	{
		private static IAPValidator _instance;

		public static event Action<string> PurchaseValidated;
		public static event Action VIPSubscriptionValidated;

		public IAPValidator()
		{
			_instance = this;

#if IDOSGAMES_MOBILE_IAP
			IAPService.PurchaseCompleted += ValidatePurchase;
#endif
        }

        ~IAPValidator()
		{
#if IDOSGAMES_MOBILE_IAP
			IAPService.PurchaseCompleted -= ValidatePurchase;
#endif
        }

        [RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			_instance = new();
		}

#if IDOSGAMES_MOBILE_IAP
		private void ValidatePurchase(PurchaseEventArgs purchaseEvent)
		{
			if (purchaseEvent.purchasedProduct.definition.type == ProductType.Subscription)
			{
				ValidateVIPSubscriptionPurchase(purchaseEvent);
			}
			else
			{
				ValidateProductPurchase(purchaseEvent);
			}
		}

		public static async void ValidateVIPSubscriptionPurchase(PurchaseEventArgs purchaseEvent)
		{
			var receipt = purchaseEvent.purchasedProduct.receipt;

			await UserDataService.ValidateVIPSubscription(receipt);

			VIPSubscriptionValidated?.Invoke();
		}

		public static void ValidateProductPurchase(PurchaseEventArgs purchaseEvent)
		{
			var receipt = IAPPurchase.ConvertFromJson(purchaseEvent.purchasedProduct.receipt);
			string productID = purchaseEvent.purchasedProduct.definition.id;
			/*
#if UNITY_ANDROID

			PlayFabClientAPI.ValidateGooglePlayPurchase(new ValidateGooglePlayPurchaseRequest()
			{
				CurrencyCode = purchaseEvent.purchasedProduct.metadata.isoCurrencyCode,
				PurchasePrice = (uint)(purchaseEvent.purchasedProduct.metadata.localizedPrice * 100),
				ReceiptJson = receipt.PayloadData.json,
				Signature = receipt.PayloadData.signature
			},
			result =>
			{
				PurchaseValidated?.Invoke(productID);
			},
			error => Debug.LogWarning("Validation failed: " + error.GenerateErrorReport()));

#elif UNITY_IOS

			PlayFabClientAPI.ValidateIOSReceipt(new ValidateIOSReceiptRequest()
			{
				CurrencyCode = purchaseEvent.purchasedProduct.metadata.isoCurrencyCode,
				PurchasePrice = (int)(purchaseEvent.purchasedProduct.metadata.localizedPrice * 100),
				ReceiptData = receipt.Payload
			},
			result =>
			{
				PurchaseValidated?.Invoke(productID);
			},
			error => Debug.LogWarning("Validation failed: " + error.GenerateErrorReport()));

#endif
			*/
		}
#endif

    }
}