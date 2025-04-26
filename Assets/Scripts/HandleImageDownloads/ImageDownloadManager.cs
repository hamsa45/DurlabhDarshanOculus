using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ImageDownloadManager : MonoBehaviour
{
    private static ImageDownloadManager _instance;
    private const string thumbnailImageCacheFolder = "ArthiDocumentaryThumbnails";

	private ConcurrentDictionary<string, Task<Texture2D>> ongoingDownloads = new ConcurrentDictionary<string, Task<Texture2D>>();
    private Dictionary<string, TextureWithShowId> textureCache = new Dictionary<string, TextureWithShowId>();
    private Dictionary<string, System.Action<TextureWithShowId>> downloadCallbacks = new Dictionary<string, System.Action<TextureWithShowId>>();
    //private ConcurrentDictionary<string, System.Action<TextureWithShowId>> downloadCallbacks = new ConcurrentDictionary<string, System.Action<TextureWithShowId>>();


    private const int timeLimitToStoreThumbnails = 365;

    public static ImageDownloadManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerObj = new GameObject("ImageDownloadManager");
                _instance = managerObj.AddComponent<ImageDownloadManager>();
                DontDestroyOnLoad(managerObj); // Ensures persistence across scenes
            }
            return _instance;
        }
    }

    private string CachePath => Application.persistentDataPath + "/ArthiDocumentaryThumbnails/";
    private string CachePath_thumbnailImage => Application.persistentDataPath + "/ArthiDocumentaryThumbnails/Thumbnails";
    

	private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureCacheDirectoryExists();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadImage(string url, string ShowId, System.Action<TextureWithShowId> callback)
    {
        if (string.IsNullOrEmpty(url) || ShowId == null )
            return;

        //look at the cached folder
        string filePath = GetCachedFilePath(url, CachePath_thumbnailImage);
        if (File.Exists(filePath))
		{
            //Debug.Log("<<<<111Thumbnail image exist in the Cachefolder."+ filePath);
            Texture2D texture = LoadTextureFromFileAsync(filePath);
            callback?.Invoke(new TextureWithShowId(ShowId, texture));
            return;
		}

        if (textureCache.ContainsKey(url))
        {
            callback?.Invoke(textureCache[url]);
        }
        else
        {
            downloadCallbacks[ShowId] = callback;
            StartCoroutine(DownloadThumbnailImage(url,filePath, ShowId));
        }
    }

	private void OnDestroy()
	{
        StopAllCoroutines();
	}

	private IEnumerator DownloadThumbnailImage(string url, string filePath, string showID)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                textureCache[url] = new TextureWithShowId(showID, texture);

                if (downloadCallbacks.ContainsKey(showID))
                {
                    downloadCallbacks[showID]?.Invoke(textureCache[url]);
                    downloadCallbacks.Remove(showID);
                }

                // Optional: Save texture to disk cache here
                // SaveTextureToDisk(url, texture);

                //Debug.LogWarning(showID + " <<<<<<<22222is downloaded and attempting to save to cache");
                // Step 2: Encode on the **main thread**
                byte[] pngBytes = texture.EncodeToPNG();

                // Step 3: Save in the background (non-blocking)
                _ = Task.Run(() => SaveTextureToFile(pngBytes, filePath));
            }
            else
            {
                Debug.LogError("Error downloading image: " + webRequest.error);
            }
        }
    }

    /// <summary>
    /// Downloads an image asynchronously, updates the UI immediately with a callback, and caches it.
    /// Prevents redundant downloads using ConcurrentDictionary.
    /// Includes proper error handling.
    /// </summary>
    public async Task GetImageAsync(string url, Action<Texture2D> weakCallBack)
    {
        string filePath = GetCachedFilePath(url, CachePath);

        // Step 1: Prevent redundant downloads using ConcurrentDictionary
        Task<Texture2D> downloadTask = ongoingDownloads.GetOrAdd(url, _ => DownloadImageAsync(url, filePath, weakCallBack));

        try
        {
            // Step 2: Await the download task
            Texture2D texture = await downloadTask;

            // Invoke the callback to update the UI immediately
            weakCallBack?.Invoke(texture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to download image: {ex.Message}");

            // Invoke the callback with null on failure to avoid breaking the UI
            weakCallBack?.Invoke(null);
        }
        finally
        {
            // Step 3: Remove the task from the dictionary
            ongoingDownloads.TryRemove(url, out _);
        }
    }


    /// <summary>
    /// Downloads an image asynchronously, updates UI immediately with a callback, and caches it.
    /// Prevents redundant downloads using ConcurrentDictionary.
    /// Includes proper error handling and returns the texture.
    /// </summary>
    public async Task<Texture2D> GetImageAsyncOld(string url, Action<Texture2D> weakCallBack)
    {
        string filePath = GetCachedFilePath(url, CachePath);

        // Step 1: Prevent redundant downloads using ConcurrentDictionary
        Task<Texture2D> downloadTask = ongoingDownloads.GetOrAdd(url, _ => DownloadImageAsync(url, filePath, weakCallBack));

        Texture2D texture = null;

        try
        {
            // Step 2: Await the download task
            texture = await downloadTask;

            // Invoke the callback to update the UI
            weakCallBack?.Invoke(texture);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to download image: {ex.Message}");

            // Use a fallback image or invoke callback with null to avoid breaking the UI
            weakCallBack?.Invoke(null);
        }
        finally
        {
            // Step 3: Remove the task from the dictionary once download completes or fails
            ongoingDownloads.TryRemove(url, out _);
        }

        // Step 4: Return the downloaded texture
        return texture;
    }


    /// <summary>
    /// Handles the actual image download and caching.
    /// </summary>
    private async Task<Texture2D> DownloadImageAsync(string url, string filePath, Action<Texture2D> weakCallback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            var operation = request.SendWebRequest();
            var tcs = new TaskCompletionSource<bool>();

            operation.completed += _ => tcs.SetResult(true);
            await tcs.Task;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download {url}: {request.error}");
                return null;
            }

            //Step 1: Display downloaded image immediately
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            //Debug.Log("Download-----Completed");
            weakCallback?.Invoke(texture);

            // Step 2: Encode on the **main thread**
            byte[] pngBytes = texture.EncodeToPNG();

            // Step 3: Save in the background (non-blocking)
            await SaveTextureToFileAsync(pngBytes, filePath);
            //_ = Task.Run(() => SaveTextureToFile(pngBytes, filePath));

            return texture;
        }
    }

    private void SaveTextureToFile(byte[] pngBytes, string filePath)
    {
        try
        {
            // Ensure directory exists
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Write file asynchronously
            File.WriteAllBytes(filePath, pngBytes);
            //Debug.Log($"Image saved: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($" Error saving image to cache: {ex.Message}");
        }
    }

    private async Task SaveTextureToFileAsync(byte[] pngBytes, string filePath)
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllBytesAsync(filePath, pngBytes);
            Debug.Log($"Image saved: {Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving image to cache: {ex.Message}");
            SaveTextureToFile(pngBytes, filePath);
        }
    }


    /// <summary>
    /// Generates a unique file path for caching an image.
    /// </summary>
    private string GetCachedFilePath(string url, string cachePath)
    {
        string fileName = Path.GetFileName(new Uri(url).LocalPath);
        return Path.Combine(cachePath, fileName);
    }

    /// <summary>
    /// Ensures that the cache directory exists.
    /// </summary>
    private void EnsureCacheDirectoryExists()
    {
        if (!Directory.Exists(CachePath))
        {
            Directory.CreateDirectory(CachePath);
        }

        if (!Directory.Exists(CachePath_thumbnailImage))
		{
            Directory.CreateDirectory(CachePath_thumbnailImage);
		}
    }

    /// <summary>
    /// Clears old cached images (older than 365 days).
    /// </summary>
    public void ClearOldCache()
    {
        if (!Directory.Exists(CachePath)) return;

        string[] files = Directory.GetFiles(CachePath);
        foreach (string file in files)
        {
            DateTime creationTime = File.GetCreationTime(file);
            if ((DateTime.Now - creationTime).TotalDays > timeLimitToStoreThumbnails)
            {
                File.Delete(file);
            }
        }
    }

    private Texture2D LoadTextureFromFileAsync(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
}

public class TextureWithShowId
{
    public string showId;
    public Texture2D texture;

    public TextureWithShowId(string showId, Texture2D texture)
    {
        this.showId = showId;
        this.texture = texture;
    }
}