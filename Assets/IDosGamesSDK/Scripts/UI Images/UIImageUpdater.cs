using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class UIImageUpdater : MonoBehaviour
    {
        public ImageType imageType;
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image == null)
            {
                return;
            }

            UpdateImage();
        }

        public void UpdateImage()
        {
            if (_image == null)
            {
                return;
            }

            var imageData = ImageDataManager.GetImageData(imageType);
            if (imageData == null)
            {
                return;
            }

            var imagePair = imageData.images.FirstOrDefault(i => i.imageType == imageType);
            if (imagePair.imageSprite != null)
            {
                _image.sprite = imagePair.imageSprite;
            }
        }

        private void OnValidate()
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }

            UpdateImage();
        }
    }
}