using System;
using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// Per-rarity data used by <see cref="EquipmentInfoPopup"/>: the rarity label text,
    /// the colors that tint the popup's Top + Glow Images, and the star count for the
    /// grade-stars row.
    /// </summary>
    [Serializable]
    public class ItemRarityLabel
    {
        public string RarityID;
        public string DisplayName;

        [Header("Popup colors")]
        public Color TopColor = Color.white;
        public Color GlowColor = Color.white;
        public Color TextColor = Color.white;

        [Header("Stars")]
        public int StarCount = 1;
        public bool UseSpecialStars;
    }
}
