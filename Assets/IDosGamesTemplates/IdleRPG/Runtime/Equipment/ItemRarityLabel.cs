using System;
using UnityEngine;

namespace IDosGames
{
    [Serializable]
    public class ItemRarityLabel
    {
        public string RarityID;
        public string DisplayName;

        [Header("Label")]
        public Sprite LabelSprite;

        [Header("Frame colors")]
        public Color BgColor = Color.white;
        public Color GradientColor = Color.white;
        public Color BasicFrameColor = Color.white;

        [Header("Stars")]
        public int StarCount = 1;
        public bool UseSpecialStars;
    }
}
