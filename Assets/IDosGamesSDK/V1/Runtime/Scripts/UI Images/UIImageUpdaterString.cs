using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class ImageUpdaterString : MonoBehaviour
    {
        [Header("Settings")]
        public string imageKey;       // Key (for example "ShopBanner")
        public Sprite defaultSprite;  // Placeholder if nothing came from the server

        private Image _uiImage;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _uiImage = GetComponent<Image>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_uiImage == null && _spriteRenderer == null) return;

            ImageLoader.ImagesUpdated += OnImagesUpdated;
            UpdateImage();
        }

        private void OnDestroy()
        {
            ImageLoader.ImagesUpdated -= OnImagesUpdated;
        }

        private void OnImagesUpdated()
        {
            UpdateImage();
        }

        public void SetKey(string newKey)
        {
            imageKey = newKey;
            UpdateImage();
        }

        public async void UpdateImage()
        {
            if (_uiImage == null && _spriteRenderer == null) return;

            // 1. We immediately take the default sprite as the main one
            Sprite spriteToShow = defaultSprite;

            // 2. If there is a key and data on the server, we try to download it.
            if (!string.IsNullOrEmpty(imageKey) &&
                IDosGamesData.Config.TitlePublicConfiguration.ImageData != null &&
                IDosGamesData.Config.TitlePublicConfiguration.ImageData.TryGetValue(imageKey, out string url) &&
                !string.IsNullOrEmpty(url))
            {
                var downloadedSprite = await ImageLoader.LoadExternalImageAsync(url);

                // If the download is successful, replace the sprite with the downloaded one.
                if (downloadedSprite != null)
                {
                    spriteToShow = downloadedSprite;
                }
            }

            // 3. We apply the final sprite (either the downloaded one or the default one)
            if (spriteToShow != null)
            {
                if (_uiImage != null) _uiImage.sprite = spriteToShow;
                if (_spriteRenderer != null) _spriteRenderer.sprite = spriteToShow;
            }
        }
    }
}
