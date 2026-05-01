#if IDOSGAMES_MOBILE_IAP
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace IDosGames
{
	public class IAPService : IDetailedStoreListener
	{
		public const string DEFAULT_ENVIRONMENT = "production";

		private static IAPService _instance;

		public static bool IsInitialized => _instance._storeController != null;

		public static event Action<PurchaseEventArgs> PurchaseCompleted;
		public static event Action<PurchaseFailureDescription> PurchaseFailed;
		public static event Action NotInitialized;

		public static event Action PurchaseInitiated;
		public static event Action PurchaseProcessStarted;

		private IStoreController _storeController;
		private IExtensionProvider _extensionProvider;

		public IAPService()
		{
			_instance = this;

			UserDataService.DataUpdated += InitializeStoreProducts;
		}

		~IAPService()
		{
			UserDataService.DataUpdated -= InitializeStoreProducts;
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			_instance = new();
		}

		[RuntimeInitializeOnLoadMethod]
		private static async void InitializeUnityServices()
		{
			try
			{
				var options = new InitializationOptions().SetEnvironmentName(DEFAULT_ENVIRONMENT);
				await UnityServices.InitializeAsync(options);
			}
			catch (Exception)
			{
				Debug.LogError("An error occurred during Unity services initialization. (IAP)");
			}
		}

		private void InitializeStoreProducts()
		{
			if (IsInitialized)
			{
				return;
			}

			var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

			AddProductsToBuilder(builder);

			UnityPurchasing.Initialize(this, builder);
		}

		private void AddProductsToBuilder(ConfigurationBuilder builder)
		{
			var productsData = UserDataService.GetCachedTitleData(TitleDataKey.products_for_real_money);

			var products = JsonConvert.DeserializeObject<JArray>(productsData);

			products ??= new JArray();

			foreach (var product in products)
			{
				if (Enum.TryParse($"{product[JsonProperty.PRODUCT_TYPE]}", true, out ProductType productType))
				{
					builder.AddProduct($"{product[JsonProperty.ITEM_ID]}", productType);
				}
			}
		}

		public static void PurchaseProductByID(string productID)
		{
			if (!IsInitialized)
			{
				NotInitialized?.Invoke();
				return;
			}

			PurchaseInitiated?.Invoke();
			_instance._storeController.InitiatePurchase(productID);
		}

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			_storeController = controller;
			_extensionProvider = extensions;
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.LogWarning($"UnityPurchasing initialize failed. Error: {error}");
		}

		public void OnInitializeFailed(InitializationFailureReason error, string message)
		{
			Debug.LogWarning($"UnityPurchasing initialize failed. Error: {error} | Message: {message}");
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
		{
			PurchaseProcessStarted?.Invoke();

			if (!IsInitialized)
			{
				return PurchaseProcessingResult.Complete;
			}

			if (purchaseEvent.purchasedProduct == null)
			{
				Debug.LogWarning("Attempted to process purchase with unknown product.");
				return PurchaseProcessingResult.Complete;
			}

			if (string.IsNullOrEmpty(purchaseEvent.purchasedProduct.receipt))
			{
				Debug.LogWarning("Attempted to process purchase with no receipt.");
				return PurchaseProcessingResult.Complete;
			}

			Debug.Log("Processing transaction: " + purchaseEvent.purchasedProduct.transactionID);

			PurchaseCompleted?.Invoke(purchaseEvent);

			return PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
		{
			try
			{
				Debug.LogWarning($"Purchase Failed: Product: {product.definition.storeSpecificId}, Failure Reason: {failureReason}");

				PurchaseFailed?.Invoke(new PurchaseFailureDescription(product.definition.storeSpecificId, failureReason, string.Empty));
			}
			catch
			{
				PurchaseFailed?.Invoke(new PurchaseFailureDescription("id", failureReason, string.Empty));
			}
		}

		public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureDescription failureDescription)
		{
			try
			{
				Debug.LogWarning($"Purchase Failed: Product: {product.definition.storeSpecificId}, Failure Description: {failureDescription}");

				PurchaseFailed?.Invoke(failureDescription);
			}
			catch
			{
				PurchaseFailed?.Invoke(new PurchaseFailureDescription("id", PurchaseFailureReason.Unknown, string.Empty));
			}
		}

		public static void RestorePurchasesIOS(Action<bool, string> callbackAction)
		{
			_instance._extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(callbackAction);
		}
	}
}
#endif