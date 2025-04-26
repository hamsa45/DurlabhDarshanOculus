using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;

public class ImageLoader : MonoBehaviour
{
    private RawImage targetRawImage;
    private string imageUrl;
    private CancellationTokenSource cts;  // For cancelling async tasks
    private string CachePath => Application.persistentDataPath + "/ArthiDocumentaryThumbnails/";

    private string cacheFileName;

    public string CacheFileName
    {
        get
        {
            return cacheFileName;
        }
        set
        {
            cacheFileName = value;
        }
    }

    internal void setUp(RawImage targetRawImage, string imageUrl)
    {
        this.targetRawImage = targetRawImage;
        this.imageUrl = imageUrl;
        cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        // Cancel any ongoing async tasks to avoid dangling callbacks
        cts?.Cancel();
        cts?.Dispose();
    }

    public async Task StartLoadingImage()
    {
        if (string.IsNullOrEmpty(imageUrl) || targetRawImage == null)
        {
            Debug.LogWarning("Invalid setup: missing imageUrl or targetRawImage.");
            return;
        }

        string filePath = string.Empty;
        // Step 1: Load from cache if available
        if(string.IsNullOrEmpty(cacheFileName)) filePath = GetCachedFilePath(imageUrl);
        else filePath = GetCachedFilePathFromFileName(cacheFileName);

        
        if (File.Exists(filePath))
        {
            //Debug.Log("filePath Exists : " + filePath);
            Texture2D cachedTexture = await LoadTextureFromFileAsync(filePath);

            if (this != null && targetRawImage != null)
                OnImageDownloaded(cachedTexture);

            return;
        }

        try
        {
            await ImageDownloadManager.Instance.GetImageAsync(imageUrl, (texture) =>
            {
                if (this == null || targetRawImage == null) return;  // Ensure no null references
                OnImageDownloaded(texture);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to download image: {ex.Message}");
        }
    }

    private string GetCachedFilePathFromFileName(string fileName)   
    {
        return Path.Combine(CachePath, fileName);
    }

    private async Task<Texture2D> LoadTextureFromFileAsync(string filePath)
    {
        try
        {
            byte[] fileData = await Task.Run(() => File.ReadAllBytesAsync(filePath));

            if (cts.Token.IsCancellationRequested) return null;

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load texture from cache: {ex.Message}");
            return null;
        }
    }

    private string GetCachedFilePath(string url)
    {
        string fileName = Path.GetFileName(new Uri(url).LocalPath);
        return Path.Combine(CachePath, fileName);
    }

    private void OnImageDownloaded(Texture2D texture)
    {
        if (this == null || targetRawImage == null)
        {
            //Debug.LogWarning("GameObject or RawImage destroyed, skipping texture update.");
            return;
        }

        if (texture != null)
        {
            targetRawImage.texture = texture;
            //Debug.Log("Texture applied successfully.");
        }
        else
        {
            Debug.LogError("Failed to load image.");
        }
    }
}
