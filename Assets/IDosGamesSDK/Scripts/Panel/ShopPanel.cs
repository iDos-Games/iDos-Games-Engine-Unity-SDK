using System;
using UnityEngine;

namespace IDosGames
{
	public abstract class ShopPanel : MonoBehaviour
	{
		private void OnEnable()
		{
			InitializePanel();
			UserDataService.CustomUserDataUpdated += InitializePanel;
		}

		private void OnDisable()
		{
			UserDataService.CustomUserDataUpdated -= InitializePanel;
		}

		public abstract void InitializePanel();

		public float GetPriceInRealMoney(string data)
		{
			float.TryParse(data, out float price);

			price /= 100;

			return price;
		}

		public float GetPriceInVirtualCurrency(float price, VirtualCurrencyID currencyID)
		{
			switch (currencyID)
			{
				case VirtualCurrencyID.CO:
					price = VirtualCurrencyPrices.ConverRMtoIGC(price);
					break;
				default:
				case VirtualCurrencyID.IG:
					price = VirtualCurrencyPrices.ConverRMtoIGT(price);
					break;
			}

			return price;
		}

		public VirtualCurrencyID GetVirtualCurrencyID(string currencyID)
		{
			Enum.TryParse(currencyID, true, out VirtualCurrencyID virtualCurrencyID);

			return virtualCurrencyID;
		}
	}
}