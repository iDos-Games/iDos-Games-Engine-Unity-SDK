using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// Shared math helpers for character stats — value scaling, upgrade cost, and effective
    /// max level (with character-level multiplier). Used by both <see cref="CharacterStatSlot"/>
    /// and <see cref="CharacterStatPopupItem"/>.
    /// </summary>
    internal static class CharacterStatMath
    {
        /// <summary>
        /// Effective stat value at the given stat level.
        /// Formula: <c>BaseStatValue * (1 + StatScalingFactor * (level - 1))</c>.
        /// Level 0/1 returns <c>BaseStatValue</c>.
        /// </summary>
        public static double ComputeValueAtLevel(StatDefinition def, int level)
        {
            if (def == null) return 0.0;
            if (level <= 1) return def.BaseStatValue;
            return def.BaseStatValue * (1.0 + def.StatScalingFactor * (level - 1));
        }

        /// <summary>
        /// Cost (in the first resource entry's amount) to upgrade the stat TO <paramref name="nextLevel"/>.
        /// Formula: <c>BaseAmount * (1 + CostScalingFactor * (nextLevel - 1))</c>, rounded up.
        /// </summary>
        public static long ComputeUpgradeCost(StatDefinition def, int nextLevel)
        {
            var entries = def?.BaseCostResource?.Standard?.Entries;
            if (entries == null || entries.Count == 0) return 0;

            long baseAmount = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].Amount.HasValue)
                {
                    baseAmount = entries[i].Amount.Value;
                    break;
                }
            }
            if (baseAmount <= 0) return 0;

            double scaled = baseAmount * (1.0 + def.CostScalingFactor * (nextLevel - 1));
            if (scaled < 0) scaled = 0;
            return (long)System.Math.Ceiling(scaled);
        }

        /// <summary>
        /// Effective max stat level taking the character's current level multiplier into account.
        /// Equals <c>StatDefinition.MaxLevel * CharacterLevelDefinition.StatMaxLevelMultiplier</c>
        /// rounded up; never below <c>MaxLevel</c> itself.
        /// </summary>
        public static int ComputeEffectiveMaxLevel(StatDefinition def, CharacterDefinition charDef, CharacterModel model)
        {
            if (def == null) return 0;
            int baseMax = def.MaxLevel;
            if (baseMax <= 0) return 0;

            float multiplier = 1.0f;
            if (model != null && charDef?.Levels != null
                && charDef.Levels.TryGetValue(model.Level.ToString(), out var lvl)
                && lvl != null && lvl.StatMaxLevelMultiplier > 0f)
            {
                multiplier = lvl.StatMaxLevelMultiplier;
            }

            int effective = Mathf.CeilToInt(baseMax * multiplier);
            return Mathf.Max(baseMax, effective);
        }
    }
}
