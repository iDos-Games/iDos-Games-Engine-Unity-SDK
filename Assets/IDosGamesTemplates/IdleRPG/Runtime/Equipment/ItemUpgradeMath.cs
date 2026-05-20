using UnityEngine;

namespace IDosGames
{
    public static class ItemUpgradeMath
    {
        public static bool IsMaxLevel(int level, ItemUpgrade upgrade)
        {
            if (upgrade == null) return true;
            return upgrade.MaxLevel > 0 && level >= upgrade.MaxLevel;
        }

        public static long ComputeNextLevelCost(ItemUpgrade upgrade, int currentLevel)
        {
            var entries = upgrade?.BaseCostResource?.Standard?.Entries;
            if (entries == null || entries.Count == 0) return 0;

            int nextLevel = Mathf.Max(1, currentLevel + 1);
            double scale = 1.0 + upgrade.CostScalingFactor * (nextLevel - 1);

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null || !e.Amount.HasValue) continue;
                long scaled = (long)System.Math.Ceiling(e.Amount.Value * scale);
                if (scaled > 0) return scaled;
            }
            return 0;
        }

        public static double ApplyFlatScaling(double baseValue, ItemUpgrade upgrade, int level)
        {
            if (upgrade == null || level <= 1) return baseValue;
            return baseValue + upgrade.FlatScalingFactor * (level - 1);
        }

        public static double ApplyPercentScaling(double baseValue, ItemUpgrade upgrade, int level)
        {
            if (upgrade == null || level <= 1) return baseValue;
            return baseValue * (1.0 + upgrade.PercentScalingFactor * (level - 1));
        }

        public static (double flat, double percent) GetScaledBonuses(ItemStats stats, string statKey, ItemUpgrade upgrade, int level)
        {
            double flatBase = 0;
            double percentBase = 0;

            if (stats?.FlatBonuses != null && stats.FlatBonuses.TryGetValue(statKey, out var f))
                flatBase = f;
            if (stats?.PercentBonuses != null && stats.PercentBonuses.TryGetValue(statKey, out var p))
                percentBase = p;

            return (ApplyFlatScaling(flatBase, upgrade, level), ApplyPercentScaling(percentBase, upgrade, level));
        }
    }
}
