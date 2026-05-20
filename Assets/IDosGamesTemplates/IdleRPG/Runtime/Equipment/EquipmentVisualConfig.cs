using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// Shared visual configuration for the Equipment UI. Holds the per-RarityID
    /// frame palette + label, and the per-ItemClass icon. Used by both
    /// <see cref="EquipmentPanel"/> and <see cref="EquipmentInfoPopup"/> so the
    /// data lives in one place.
    /// Create via Assets → Create → iDos Games → Equipment Visual Config.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EquipmentVisualConfig",
        menuName = "iDos Games/Equipment Visual Config",
        order = 100)]
    public class EquipmentVisualConfig : ScriptableObject
    {
        [Header("Rarities (frame colors for tiles / slots)")]
        public List<ItemRarityVisual> RarityVisuals;

        [Header("Rarities (label + grade stars for popup)")]
        public List<ItemRarityLabel> RarityLabels;

        [Header("Item classes (icons for tabs, slots, popup)")]
        public List<ItemClassIcon> ClassIcons;

        public ItemRarityVisual ResolveRarityVisual(string rarityID)
        {
            if (RarityVisuals == null || RarityVisuals.Count == 0) return null;

            if (!string.IsNullOrEmpty(rarityID))
            {
                for (int i = 0; i < RarityVisuals.Count; i++)
                {
                    var v = RarityVisuals[i];
                    if (v != null && string.Equals(v.RarityID, rarityID, StringComparison.OrdinalIgnoreCase))
                        return v;
                }
            }
            return RarityVisuals[0];
        }

        public ItemRarityLabel ResolveRarityLabel(string rarityID)
        {
            if (RarityLabels == null || RarityLabels.Count == 0) return null;

            if (!string.IsNullOrEmpty(rarityID))
            {
                for (int i = 0; i < RarityLabels.Count; i++)
                {
                    var l = RarityLabels[i];
                    if (l != null && string.Equals(l.RarityID, rarityID, StringComparison.OrdinalIgnoreCase))
                        return l;
                }
            }
            return RarityLabels[0];
        }

        public Sprite ResolveClassIcon(string itemClass)
        {
            if (ClassIcons == null || string.IsNullOrEmpty(itemClass)) return null;
            for (int i = 0; i < ClassIcons.Count; i++)
            {
                var c = ClassIcons[i];
                if (c != null && string.Equals(c.ItemClass, itemClass, StringComparison.OrdinalIgnoreCase))
                    return c.Icon;
            }
            return null;
        }
    }
}
