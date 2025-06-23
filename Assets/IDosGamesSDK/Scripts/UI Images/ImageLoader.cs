using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace IDosGames
{
    [System.Serializable]
    public class CachedImageInfo
    {
        public string Url;
        public string LocalPath;
        public long LastAccessed;
    }

    public static class ImageLoader
    {
        public static event Action ImagesUpdated;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private static readonly Dictionary<string, Sprite> ImageCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, CachedImageInfo> CachedImageInfos;
        private const long MaxCacheSize = 1024 * 1024 * 1024;
        private const string CacheKey = "ImageCache";

        static ImageLoader()
        {
            LoadCache();
            UserDataService.FirstTimeDataUpdated += OnServerDataUpdated;
        }

        private static void OnServerDataUpdated()
        {
            if (IGSUserData.ImageData != null)
            {
                foreach (var kv in IGSUserData.ImageData)
                {
                    if (kv.Value != null && !string.IsNullOrEmpty(kv.Value))
                    {
                        var localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(kv.Value));
                        CachedImageInfos[kv.Key] = new CachedImageInfo
                        {
                            Url = kv.Value,
                            LocalPath = localPath,
                            LastAccessed = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        };
                    }
                }

                SaveCache();
                ImagesUpdated?.Invoke();
            }
        }

        public static bool IsExternalUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static async Task<Sprite> LoadExternalImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            if (ImageCache.TryGetValue(url, out var cachedSprite))
            {
                UpdateLastAccessedTime(url);
                return cachedSprite;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            var sprite = await DownloadAndCacheSpriteWebGL(url);
            return sprite;
#else
            return await DownloadAndCacheSpriteOtherPlatforms(url);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private static async Task<Sprite> DownloadAndCacheSpriteWebGL(string url)
        {
            byte[] imageBytes = await DownloadImageBytes(url);
            if (imageBytes == null) return null;

            var sprite = CreateSpriteFromBytes(url, imageBytes);
            UpdateLastAccessedTime(url);
            CleanupWebGLCache();

            return sprite;
        }

        private static void CleanupWebGLCache()
        {
            long totalMemory = ImageCache.Values.Sum(s =>
                s.texture.width * s.texture.height * 4);

            if (totalMemory <= MaxCacheSize) return;

            var toRemove = CachedImageInfos
                .OrderBy(x => x.Value.LastAccessed)
                .Take(ImageCache.Count / 4)
                .ToList();

            foreach (var item in toRemove)
            {
                ImageCache.Remove(item.Key);
                CachedImageInfos.Remove(item.Key);
            }

            Debug.Log($"WebGL cache cleaned. Removed {toRemove.Count} entries");
            SaveCache();
        }
#else
        private static async Task<Sprite> DownloadAndCacheSpriteOtherPlatforms(string url)
        {
            byte[] imageBytes;

            if (CachedImageInfos.TryGetValue(url, out var cachedInfo) && File.Exists(cachedInfo.LocalPath))
            {
                imageBytes = await File.ReadAllBytesAsync(cachedInfo.LocalPath);
            }
            else
            {
                imageBytes = await DownloadImageBytes(url);
                string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(url));

                await semaphore.WaitAsync();
                try
                {
                    if (!File.Exists(localPath))
                    {
                        await File.WriteAllBytesAsync(localPath, imageBytes);
                    }
                    else
                    {
                        imageBytes = await File.ReadAllBytesAsync(localPath);
                    }
                }
                finally
                {
                    semaphore.Release();
                }

                CachedImageInfos[url] = new CachedImageInfo
                {
                    Url = url,
                    LocalPath = localPath,
                    LastAccessed = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                SaveCache();
                CleanupCache();
            }

            return CreateSpriteFromBytes(url, imageBytes);
        }
#endif

        private static async Task<byte[]> DownloadImageBytes(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                var asyncOp = uwr.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error downloading image from URL {url}: {uwr.error}");
                    return null;
                }
                return uwr.downloadHandler.data;
            }
        }

        private static Sprite CreateSpriteFromBytes(string url, byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;

            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(imageBytes)) return null;

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.zero
            );

            ImageCache[url] = sprite;
            ImagesUpdated?.Invoke();

            return sprite;
        }

        public static Sprite LoadLocalImage(string imagePath)
        {
            return Resources.Load<Sprite>(imagePath);
        }

        public static async Task<Sprite> GetSpriteAsync(string imagePath)
        {
            if (IsExternalUrl(imagePath))
            {
                var sprite = await LoadExternalImageAsync(imagePath);
                if (sprite != null)
                {
                    return sprite;
                }
                Debug.LogError($"Failed to load external image from url: {imagePath}");
            }
            else
            {
                return LoadLocalImage(imagePath);
            }
            return null;
        }

        private static void LoadCache()
        {
            if (PlayerPrefs.HasKey(CacheKey))
            {
                var json = PlayerPrefs.GetString(CacheKey);
                CachedImageInfos = JsonConvert.DeserializeObject<Dictionary<string, CachedImageInfo>>(json);
            }
            else
            {
                CachedImageInfos = new Dictionary<string, CachedImageInfo>();
            }
        }

        private static void SaveCache()
        {
            var json = JsonConvert.SerializeObject(CachedImageInfos, Formatting.Indented);
            PlayerPrefs.SetString(CacheKey, json);
            PlayerPrefs.Save();
        }

        public static void ClearCache()
        {
            ImageCache.Clear();
            foreach (var cachedInfo in CachedImageInfos.Values)
            {
                if (File.Exists(cachedInfo.LocalPath))
                {
                    File.Delete(cachedInfo.LocalPath);
                }
            }
            CachedImageInfos.Clear();
            PlayerPrefs.DeleteKey(CacheKey);
        }

        private static void CleanupCache()
        {
            long totalSize = CachedImageInfos.Values.Sum(info => new FileInfo(info.LocalPath).Length);
            if (totalSize <= MaxCacheSize) return;

            var orderedInfos = CachedImageInfos.Values.OrderBy(info => info.LastAccessed).ToList();
            foreach (var info in orderedInfos)
            {
                if (File.Exists(info.LocalPath))
                {
                    totalSize -= new FileInfo(info.LocalPath).Length;
                    File.Delete(info.LocalPath);
                }
                CachedImageInfos.Remove(info.Url);
                if (totalSize <= MaxCacheSize) break;
            }
            SaveCache();
        }

        private static void UpdateLastAccessedTime(string url)
        {
            if (CachedImageInfos.TryGetValue(url, out var info))
            {
                info.LastAccessed = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                SaveCache();
            }
        }
    }
}