using System;
using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// Per-rarity config used exclusively by <see cref="CharacterDetailPanel"/>.
    /// Holds everything the detail panel needs for a rarity — label, background, stars —
    /// independent from <see cref="CharacterRarityVisual"/> (which the list card uses).
    /// </summary>
    [Serializable]
    public class CharacterRarityLabel
    {
        public string RarityID;

        [Header("Label")]
        public string DisplayName;          // e.g. "RARE", "LEGENDARY"
        public Sprite LabelSprite;          // background sprite of the label

        [Header("Background")]
        public Color BgColor = Color.white;
        public Color GradientColor = Color.white;
        public Color BasicFrameColor = Color.white;

        [Header("Stars")]
        public int StarCount = 1;
        public bool UseSpecialStars;
    }
}
