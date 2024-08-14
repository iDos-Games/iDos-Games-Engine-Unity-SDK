using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public static class ImageDataManager
    {
        private static Dictionary<ImageType, ImageData> _imageDataDictionary;

        public static ImageData GetImageData(ImageType imageType)
        {
            if (_imageDataDictionary == null)
            {
                LoadImageData();
            }

            _imageDataDictionary.TryGetValue(imageType, out var imageData);
            return imageData;
        }

        private static void LoadImageData()
        {
            _imageDataDictionary = new Dictionary<ImageType, ImageData>();
            var data = Resources.Load<ImageData>("Data/UIImageData");
            if (data == null)
            {
                Debug.LogError("UIImageData not found in Resources/Data");
                return;
            }

            foreach (var pair in data.images)
            {
                if (!_imageDataDictionary.ContainsKey(pair.imageType))
                {
                    _imageDataDictionary[pair.imageType] = data;
                }
            }
        }
    }
}