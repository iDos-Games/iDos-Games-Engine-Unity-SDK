using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static bool IsExternalUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static async Task<Sprite> LoadExternalImageAsync(string url)
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
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    ImageCache[url] = sprite;

                    string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(url));
                    await File.WriteAllBytesAsync(localPath, uwr.downloadHandler.data);

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

            return null;
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