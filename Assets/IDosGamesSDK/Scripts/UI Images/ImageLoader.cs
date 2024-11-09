using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

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

        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly Dictionary<string, Sprite> ImageCache = new Dictionary<string, Sprite>();
        private static readonly string CacheFilePath = Path.Combine(Application.persistentDataPath, "ImageCache.json");
        private static Dictionary<string, CachedImageInfo> CachedImageInfos;
        private const long MaxCacheSize = 1024 * 1024 * 1024;

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

        public static async Task<Sprite> LoadImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            if (ImageCache.TryGetValue(url, out var cachedImage))
            {
                UpdateLastAccessedTime(url);
                return cachedImage;
            }

            if (CachedImageInfos.TryGetValue(url, out var cachedInfo) && cachedInfo != null && File.Exists(cachedInfo.LocalPath))
            {
                byte[] imageBytes = await File.ReadAllBytesAsync(cachedInfo.LocalPath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageBytes))
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    ImageCache[url] = sprite;
                    UpdateLastAccessedTime(url);
                    return sprite;
                }
            }

            try
            {
                byte[] imageBytes = await HttpClient.GetByteArrayAsync(url);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageBytes))
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    ImageCache[url] = sprite;

                    string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(url));
                    await File.WriteAllBytesAsync(localPath, imageBytes);

                    CachedImageInfos[url] = new CachedImageInfo
                    {
                        Url = url,
                        LocalPath = localPath,
                        LastAccessed = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };
                    SaveCache();
                    CleanupCache();

                    return sprite;
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"Error downloading image from URL {url}: {e.Message}");
            }

            return null;
        }

        private static void LoadCache()
        {
            if (File.Exists(CacheFilePath))
            {
                var json = File.ReadAllText(CacheFilePath);
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
            File.WriteAllText(CacheFilePath, json);
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
            if (File.Exists(CacheFilePath))
            {
                File.Delete(CacheFilePath);
            }
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
