using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace IDosGames
{
    public class UIImageUpdater : MonoBehaviour
    {
        public ImageType imageType;
        private Image _image;
        private static Dictionary<string, string> serverImageUrls;

        private async void Awake()
        {
            _image = GetComponent<Image>();
            if (_image == null)
            {
                return;
            }

            // �������� �� ������� �������� ������ � �������
            UserDataService.DataUpdated += OnServerImageDataLoaded;

            UpdateImage();
        }

        private void OnDestroy()
        {
            // ������� �� ������� ��� ����������� �������
            UserDataService.DataUpdated -= OnServerImageDataLoaded;
        }

        private void OnServerImageDataLoaded()
        {
            // ��������� ���������� URL �� ������ �������
            serverImageUrls = IGSUserData.ImageData;
            UpdateImage();
        }

        public async void UpdateImage()
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

            // ��������� ������� ������ � �������, ����� ������ imageUrl, � ���� �� ���, ���������� imageSprite
            if (serverImageUrls != null && serverImageUrls.TryGetValue(imageType.ToString(), out var serverImageUrl) && !string.IsNullOrEmpty(serverImageUrl))
            {
                await LoadImageFromUrl(serverImageUrl);
            }
            else if (!string.IsNullOrEmpty(imagePair.imageUrl))
            {
                await LoadImageFromUrl(imagePair.imageUrl);
            }
            else if (imagePair.imageSprite != null)
            {
                _image.sprite = imagePair.imageSprite;
            }
        }

        private async Task LoadImageFromUrl(string url)
        {
            // ��������� ���� ��� ���������� �������� ��������
            string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(url));

            // ���������, ���������� �� ����
            if (File.Exists(localPath))
            {
                // ��������� ����������� � �����
                byte[] imageBytes = File.ReadAllBytes(localPath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                return;
            }

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                var asyncOperation = www.SendWebRequest();

                while (!asyncOperation.isDone)
                    await Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error downloading image: " + www.error);
                }
                else
                {
                    // ��������� �����������
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                    // ��������� ����������� �� ����
                    byte[] imageBytes = texture.EncodeToPNG();
                    File.WriteAllBytes(localPath, imageBytes);
                }
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
