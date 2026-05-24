using System;
using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// Inspector-bound visuals for one RarityID. Used by <see cref="EquipmentItem"/>
    /// (both in the inventory grid and inside <see cref="EquipmentSlotView"/>) to tint
    /// the layered frame Images per rarity.
    /// </summary>
    [Serializable]
    public class ItemRarityVisual
    {
        public string RarityID;

        [Header("Frame colors")]
        public Color BgColor = Color.white;
        public Color CornerDecoColor = Color.white;
        public Color LightColor = Color.white;
        public Color GlowColor = Color.white;
        public Color CircleFrameColor = Color.white;

        [Header("Stars")]
        public int StarCount = 1;
        public bool UseSpecialStars;
    }
}
