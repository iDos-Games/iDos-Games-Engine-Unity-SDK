using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    [CreateAssetMenu(fileName = "ImageData", menuName = "UI/ImageData")]
    public class ImageData : ScriptableObject
    {
        public List<ImageTypeSpritePair> images;

        [System.Serializable]
        public struct ImageTypeSpritePair
        {
            public ImageType imageType;
            public Sprite imageSprite;
            public string imageUrl;
        }
    }
}