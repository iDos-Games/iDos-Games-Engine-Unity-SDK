using System;
using System.Collections.Concurrent;
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

        private static readonly Dictionary<string, Sprite> ImageCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, CachedImageInfo> CachedImageInfos;
        private const long MaxCacheSize = 1024 * 1024 * 1024;
        private const string CacheKey = "ImageCache";

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> UrlLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

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

            var semaphore = UrlLocks.GetOrAdd(url, new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                if (ImageCache.TryGetValue(url, out cachedImage))
                {
                    UpdateLastAccessedTime(url);
                    return cachedImage;
                }

                byte[] imageBytes;

#if UNITY_WEBGL && !UNITY_EDITOR
                var loadTaskSource = new TaskCompletionSource<byte[]>();  
  
                WebSDK.LoadDataFromCache(url, (data) =>  
                {  
                    loadTaskSource.SetResult(data);  
                });  
  
                var cachedData = await loadTaskSource.Task;  
  
                if (cachedData != null)  
                {  
                    imageBytes = cachedData;  
                }  
                else  
                {  
                    imageBytes = await DownloadImageBytes(url);  
                    WebSDK.SaveDataToCache(url, imageBytes);  
                }  
  
                CreateSpriteFromBytes(url, imageBytes);  
#else
                if (CachedImageInfos.TryGetValue(url, out var cachedInfo) && cachedInfo != null && File.Exists(cachedInfo.LocalPath))
                {
                    imageBytes = await File.ReadAllBytesAsync(cachedInfo.LocalPath);
                }
                else
                {
                    imageBytes = await DownloadImageBytes(url);
                    string localPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(url));

                    using (var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        await fs.WriteAsync(imageBytes, 0, imageBytes.Length);
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

                CreateSpriteFromBytes(url, imageBytes);
#endif

                return ImageCache.ContainsKey(url) ? ImageCache[url] : null;
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"IO Exception when accessing file for URL {url}: {ioEx.Message}");
                return null;
            }
            finally
            {
                semaphore.Release();

                if (UrlLocks.TryGetValue(url, out var existingSemaphore) && existingSemaphore.CurrentCount == 1)
                {
                    UrlLocks.TryRemove(url, out _);
                    existingSemaphore.Dispose();
                }
            }
        }

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

        private static void CreateSpriteFromBytes(string url, byte[] imageBytes)
        {
            if (imageBytes != null)
            {
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageBytes))
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    ImageCache[url] = sprite;
                    ImagesUpdated?.Invoke();
                }
            }
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
            long totalSize = CachedImageInfos.Values.Sum(info =>
            {
                try
                {
                    return new FileInfo(info.LocalPath).Length;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error getting file size for {info.LocalPath}: {ex.Message}");
                    return 0;
                }
            });

            if (totalSize <= MaxCacheSize) return;

            var orderedInfos = CachedImageInfos.Values.OrderBy(info => info.LastAccessed).ToList();
            foreach (var info in orderedInfos)
            {
                try
                {
                    if (File.Exists(info.LocalPath))
                    {
                        long fileSize = new FileInfo(info.LocalPath).Length;
                        File.Delete(info.LocalPath);
                        totalSize -= fileSize;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error deleting file {info.LocalPath}: {ex.Message}");
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
