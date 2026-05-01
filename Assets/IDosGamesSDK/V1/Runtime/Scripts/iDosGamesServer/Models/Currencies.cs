using System.Collections.Generic;

namespace IDosGames
{
    public class CurrencyModel
    {
        public string CurrencyCode { get; set; }
        public string DisplayName { get; set; }
        public decimal ValueInUSD { get; set; }
        public float ValueInUSDMultiplier { get; set; } = 1.0f;
        public int InitialDeposit { get; set; }
        public int RechargeRate { get; set; }
        public int RechargeMax { get; set; }
        public int RechargePeriod { get; set; }
        public string ImageUrl { get; set; }
    }

    public class Currencies
    {
        public List<CurrencyModel> CurrencyData { get; set; }
    }
}
