using System;
using UnityEngine;

namespace IDosGames
{
    [Serializable]
    public class ItemRarityVisual
    {
        public string RarityID;

        [Header("Frame")]
        public Sprite FrameSprite;
        public Color FrameTint = Color.white;

        [Header("Stars")]
        public int StarCount = 1;
        public bool UseSpecialStars;
    }
}
