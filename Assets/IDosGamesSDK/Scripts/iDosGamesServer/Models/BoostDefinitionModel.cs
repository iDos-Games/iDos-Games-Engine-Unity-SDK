using System.Collections.Generic;

namespace IDosGames
{
    public class BoostDefinitionModel
    {
        public string BoostName { get; set; }
        public int MaxLevel { get; set; }
        public int Weight { get; set; }
        public string BaseCostCurrencyID { get; set; }
        public int BaseCost { get; set; }
        public double CostScalingFactor { get; set; }
        public double BaseStatValue { get; set; }
        public double StatScalingFactor { get; set; }
        public List<BoostRequirement> Requirements { get; set; }
        public string IconPath { get; set; }
        public string Description { get; set; }
    }

    public class BoostRequirement
    {
        public string RequiredBoostName { get; set; }
        public int RequiredLevel { get; set; }
    }
}
