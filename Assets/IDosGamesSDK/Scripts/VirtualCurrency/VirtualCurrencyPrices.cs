using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using UnityEngine;

namespace IDosGames
{
	public class VirtualCurrencyPrices
	{
		private static VirtualCurrencyPrices _instance;

		public static float IGT { get; private set; }
		public static float IGC { get; private set; }

		private static float _exchangeDivider = 1;

		public static float ExchangeRate { get; private set; }

		public static event Action PricesUpdated;

		private VirtualCurrencyPrices()
		{
			_instance = this;

			UserDataService.TitlePublicConfigurationUpdated += InitializePrices;
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			_instance = new();
		}

		private void InitializePrices()
		{
			var currencyData = IGSUserData.Currency;

            foreach (var currency in currencyData.CurrencyData)
            {
                switch (currency.CurrencyCode)
                {
                    case "CO":
                        IGC = (float)currency.ValueInUSD * currency.ValueInUSDMultiplier;
                        break;

                    case "IG":
                        IGT = (float)currency.ValueInUSD * currency.ValueInUSDMultiplier;
                        _exchangeDivider = currency.ValueInUSDMultiplier;
                        break;

                    default:
                        break;
                }
            }

			ExchangeRate = GetExchangeRate();

			PricesUpdated?.Invoke();
		}

        private float GetExchangeRate()
		{
			if (IGT > 0 && IGC > 0)
			{
				return (float)Math.Round(IGT / IGC, 2);
			}

			return float.MaxValue;
		}

		public static int ConverRMtoIGT(float rm)
		{
			if (rm > 0 && IGT > 0)
			{
				return (int)Math.Round(rm / IGT);
			}

			return int.MaxValue;
		}

		public static int ConverRMtoIGTwithDivider(float rm)
		{
			if (rm > 0 && IGT > 0)
			{
				return (int)Math.Round((rm / IGT) / _exchangeDivider);
			}

			return int.MaxValue;
		}

		public static int ConverRMtoIGC(float rm)
		{
			if (rm > 0 && IGC > 0)
			{
				return (int)Math.Round(rm / IGC);
			}

			return int.MaxValue;
		}

		public static int ConverRMtoIGCwithDivider(float rm)
		{
			if (rm > 0 && IGC > 0)
			{
				return (int)Math.Round((rm / IGC) / _exchangeDivider);
			}

			return int.MaxValue;
		}

		public static int ConverIGCtoIGT(int igc)
		{
			if (igc > 0 && IGT > 0 && IGC > 0)
			{
				return (int)Math.Round((igc * IGC) / IGT, 0);
			}

			return int.MaxValue;
		}

		public static int ConverIGTtoIGC(int igt)
		{
			if (igt > 0 && IGT > 0 && IGC > 0)
			{
				return (int)Math.Round((igt * IGT) / IGC, 0);
			}

			return int.MaxValue;
		}
	}
}