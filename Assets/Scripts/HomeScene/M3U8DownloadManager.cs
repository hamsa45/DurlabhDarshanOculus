using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class CurrentVideoDataInPlay
{
    public static ThumbnailDTO thumbnailDTO = null;
    public static string currentVideoUrl = null;
    public static bool isLocalFile = false;
    public static bool isOptStreamFile = false;
}

public class M3U8DownloadManager : MonoBehaviour
{
    public static M3U8DownloadManager Instance;
    private Dictionary<string, DownloadItemM3u8> ongoingDownloads = new Dictionary<string, DownloadItemM3u8>();
    private List<string> downloadedList = new();
    private Dictionary<string, string> urlFolderMap = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persist across scenes
            CleanupUnfinishedAndOldDownloads(AppConstants.m3u8CacheExpirationDayLimit);
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Deletes all unfinished downloads (folders without 'complete.flag').
    /// </summary>
    private void CleanupUnfinishedAndOldDownloads(int days)
    {
        string cacheRoot = AppConstants.m3u8CacheFolder;
        //Debug.Log("---- checking for clean up");
        try
        {
            if (!Directory.Exists(cacheRoot))
            {
                return;
            }

            string[] folders = Directory.GetDirectories(cacheRoot);
            List<string> onGoingDownloadsList = new();
            foreach (var pair in ongoingDownloads)
			{
                onGoingDownloadsList.Add(pair.Value.FileName);
			}
            
            foreach (string folder in folders)
            {
                //Debug.Log("checking cache integrity for " + folder);
                string completeFlagPath = Path.Combine(folder, "complete.flag");

                // Delete folder if flag does not exist (unfinished download)
                if (!File.Exists(completeFlagPath))
                {
                    if (onGoingDownloadsList.Contains(folder)) continue;
           
                    //Debug.Log($"Deleting unfinished download: {folder}");
                    Directory.Delete(folder, true);
                }
                else
				{
                    string timestamp = File.ReadAllText(completeFlagPath);

                    if (DateTime.TryParse(timestamp, out DateTime completionTime))
                    {
                        TimeSpan age = DateTime.Now - completionTime;

                        if (age.TotalDays > days)
                        {
                            //Debug.Log($"----------Detected & Deleting old cache folder: {folder}");
                            Directory.Delete(folder, true);
                        }
                        else
                        {
                            string folderPath = folder;
                            string fileName = Path.GetFileName(folderPath);
                            //Debug.Log(fileName);  // Output: MaaShardaDeviJaikaraHD

                            if (!downloadedList.Contains(fileName))
							{
                                addToDownloadedList(fileName);
                            }
                            //Debug.Log($"-------{folder} not expired.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logs.LogError($"Error during unfinished download cleanup: {ex.Message}");
        }
    }

    public void updateUrlFileName(string url, string folderName)
	{
        if (!urlFolderMap.ContainsKey(url))
        urlFolderMap.Add(url, folderName);
	}

    internal string GetUrlFolderName(string url)
	{
        return urlFolderMap.GetValueOrDefault(url, null);
	}

    public void StopDownload(string url)//, System.Action<float> onProgress, System.Action onComplete)
    {
        if (!ongoingDownloads.ContainsKey(url))
        {
            Debug.Log("No Download exists in progress.");
            //ReattachDownload(url, onProgress, onComplete);
            return;
        }

        var downloadItem = ongoingDownloads[url];
        downloadItem.CancelDownload();
        downloadItem.DeleteCache();
        ongoingDownloads.Remove(url);
    }

    public void StartDownload(string url)//, System.Action<float> onProgress, System.Action onComplete)
    {
        if (ongoingDownloads.ContainsKey(url))
        {
            //Debug.LogWarning("Download already in progress. Attaching listeners.");
            //ReattachDownload(url, onProgress, onComplete);
            return;
        }

        var downloadItem = new DownloadItemM3u8(GetUrlFolderName(url),url);
        ongoingDownloads[url] = downloadItem;

        downloadItem.DownloadCompleted += downloadComplete;
        downloadItem.DownloadInterrupted += DownloadInterrupted;

        downloadItem.StartDownload();
    }

    private void downloadComplete(string url)
	{
        addToDownloadedList(GetUrlFolderName(url));
        ongoingDownloads[url].DownloadInterrupted -= DownloadInterrupted;
        ongoingDownloads.Remove(url);  // Remove completed download
    }

    // public bool IsDownloaded(string url)
	// {
    //     return downloadedList.Contains(url);
	// }

    // public bool IsDownloadedUrl(string url)
    // {
    //     string fileName = GetUrlFolderName(url);
    //     return downloadedList.Contains(fileName);
    // }

     public bool IsDownloaded(string url)
	{
        return downloadedList.Contains(url) || downloadedList.Contains(GetUrlFolderName(url));
	}

    public bool IsDownloadedUrl(string url)
    {
        return IsDownloaded(url);
    }

    private void addToDownloadedList(string fileName)
	{
        if (!downloadedList.Contains(fileName)) 
        downloadedList.Add(fileName);
    }

    private void removeFromDownloadedListUrl(string url)
	{
        string fileName = GetUrlFolderName(url);
        if (downloadedList.Contains(fileName)) downloadedList.Remove(fileName);
    }

    private void removeFromDownloadedList(string fileName)
    {
        if (downloadedList.Contains(fileName)) downloadedList.Remove(fileName);
    }

    internal void Delete(string url)
    {
        string fileName = GetUrlFolderName(url);
        deleteCache(fileName);
        removeFromDownloadedList(fileName);
    }

    private void deleteCache(string fileName)
    {
        string path = Path.Combine(AppConstants.m3u8CacheFolder, fileName);
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, true);  // Recursively delete
                //Debug.Log($"Deleted directory: {path}");
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"IO Exception: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Debug.LogError($"Access Denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Logs.LogError($"Failed to delete directory: {ex.Message}");
            }
        }
        else
        {
            Logs.LogWarning($"Directory not found.");
        }
    }

    public bool IsPlayingDownloadedFile()
	{
        bool contains = urlFolderMap.ContainsKey(CurrentVideoDataInPlay.currentVideoUrl);
        //Debug.Log("Checking if playing downloaded file, does urlfolder map has current url :"+ contains);
        if (!urlFolderMap.ContainsKey(CurrentVideoDataInPlay.currentVideoUrl)) return false;
        return downloadedList.Contains(urlFolderMap[CurrentVideoDataInPlay.currentVideoUrl]);
    }

    public string GetCurrentPlayingVideoAbsolutePath()
    {
        string manifestFileName = Path.GetFileName(new Uri(CurrentVideoDataInPlay.currentVideoUrl).LocalPath);
        //string absolutePath = Path.Combine(AppData.m3u8CacheFolder, CurrentM3f8FilePlaying.m3u8FileName, manifestFileName);
        if (!urlFolderMap.ContainsKey(CurrentVideoDataInPlay.currentVideoUrl))
		{
            Logs.LogError("fatal error, cache file is missing");
		}
        string absolutePath = Path.Combine(AppConstants.m3u8CacheFolder, urlFolderMap[CurrentVideoDataInPlay.currentVideoUrl], manifestFileName);
        //Debug.Log("------ " + absolutePath);
        return absolutePath;
    }


    private void OnDestroy()
    {
        //save list of unfinished downloads to playerprefs and on nextime scene load delete then if the folder exists.
        //or if the application is closed.
        if (Instance != this)
        {
            return;
        }

        if (ongoingDownloads.Count != 0)
        {
            foreach (var downloadItem in ongoingDownloads.Values)
            {
                downloadItem.CancelDownload();
            }
        }
    }

	internal bool HasDownload(string url)
	{
        return ongoingDownloads.ContainsKey(url);
	}

	internal DownloadItemM3u8 GetDownloadItem(string url)
	{
        return ongoingDownloads[url];
	}

	private void DownloadInterrupted(string mediaUrl)
	{
        var downloadItem = ongoingDownloads[mediaUrl];
        downloadItem.CancelDownload();
        Delete(mediaUrl);
        downloadItem.DownloadCompleted -= downloadComplete;
        ongoingDownloads.Remove(mediaUrl);
    }
}

public enum DownloadState { DOWNLOADING, NOT_DOWNLOADED, DOWNLOADED }

public class DownloadItemM3u8
{
    public string FileName { get; private set; }
    public string Url { get; private set; }

    public DownloadState downloadState;

    private string cacheFolderPath;
    private List<string> fileUrls = new List<string>();

    public event Action<float> DownloadProgress;
    public event Action<string> DownloadCompleted;
    public event Action<string> DownloadInterrupted;

    private CancellationTokenSource cts;
    private List<UnityWebRequest> activeRequests;

    private Coroutine mainCoroutine;          // Track DownloadAllFiles()
    private Coroutine m3u8Coroutine;          // Track DownloadM3U8()
    private Coroutine concurrentCoroutine;    // Track DownloadFilesConcurrently()

    private bool isPaused = false;
    private int downloadedFilesCount = 0;
    private bool isManifestFileDownloaded = false;
    private bool isInterrupted = false;

    public DownloadItemM3u8(string fileName, string url)
    {
        FileName = fileName;
        Url = url;
        cacheFolderPath = Path.Combine(AppConstants.m3u8CacheFolder, FileName);
        EnsureDirectory();
        //Debug.Log("--- Added new download item for m3u8: " + Path.GetFileName(new Uri(Url).LocalPath));

        activeRequests = new List<UnityWebRequest>();

        subscribeInternetConnectionChange();
    }

    private void subscribeInternetConnectionChange()
	{
        if (NetworkMonitor.Instance != null)
        {
            NetworkMonitor.OnInternetStatusChanged.AddListener(onInternetStatusChanged);
        }
    }

    private void unsubscribeInternetConnectionChange()
    {
        if (NetworkMonitor.Instance != null)
        {
            NetworkMonitor.OnInternetStatusChanged.RemoveListener(onInternetStatusChanged);
        }
    }

    private void onInternetStatusChanged(bool isConnected)
    {
        //if (isConnected && isPaused)
        //{
        //    Debug.Log($"Internet restored. Resuming download: {FileName}");
        //    ResumeDownload();
        //}
        //else 
        if(!isConnected)
        {
            Logs.Log($"Internet lost. Pausing download: {FileName}");
            PauseDownload();
        }
    }

	private void PauseDownload()
	{
        isPaused = true;
        OnDownloadInterrupted();
    }

	private void EnsureDirectory()
    {
        //Debug.Log("--- before download ensuring directory.");
        if (Directory.Exists(cacheFolderPath))
        {
            //Debug.Log($"Cache found for: {FileName}. Skipping download." + cacheFolderPath);
            try
            {
                Directory.Delete(cacheFolderPath, true);
                //Debug.Log($"Deleted directory: {cacheFolderPath}");
            }
            catch (IOException ioEx)
            {
                Logs.LogError($"IO Exception: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Logs.LogError($"Access Denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Logs.LogError($"Failed to delete directory: {ex.Message}");
            }

        }

        Directory.CreateDirectory(cacheFolderPath);
    }

    /// <summary>
    /// Start the M3U8 download process.
    /// </summary>
    public void StartDownload()
    {
        //Debug.Log("Starting m3u8 download: " + Path.GetFileName(new Uri(Url).LocalPath));
        OnDownloadStarted();

        cts = new CancellationTokenSource();

        // Track all coroutines separately
        mainCoroutine = PersistanceCoroutineRunner.Instance.StartCoroutine(DownloadAllFiles(cts.Token));
    }


    /// <summary>
    /// Cancels the download gracefully.
    /// </summary>
    public void CancelDownload()
    {
        if (cts != null)
        {
            //Debug.Log("Cancelling M3U8 download...");

            // Cancel the token
            cts.Cancel();

            // Cleanup network requests
            CleanupRequests();

            // Stop all coroutines explicitly
            if (mainCoroutine != null)
            {
                PersistanceCoroutineRunner.Instance.StopCoroutine(mainCoroutine);
                mainCoroutine = null;
            }

            if (m3u8Coroutine != null)
            {
                PersistanceCoroutineRunner.Instance.StopCoroutine(m3u8Coroutine);
                m3u8Coroutine = null;
            }

            if (concurrentCoroutine != null)
            {
                PersistanceCoroutineRunner.Instance.StopCoroutine(concurrentCoroutine);
                concurrentCoroutine = null;
            }

            cts.Dispose();
            cts = null;

            if(!isInterrupted) OnDownloadCanceled();
        }
    }


    /// <summary>
    /// Downloads the .m3u8 file and extracts all URLs, then downloads the files concurrently.
    /// </summary>
    /// 
    private IEnumerator DownloadAllFiles(CancellationToken token)
    {
        // Track the m3u8 coroutine
        m3u8Coroutine = PersistanceCoroutineRunner.Instance.StartCoroutine(DownloadM3U8(token));

        yield return m3u8Coroutine;

        if (token.IsCancellationRequested)
        {
            //Debug.LogWarning("Download cancelled after m3u8 retrieval.");
            yield break;
        }

        // Track the concurrent coroutine
        concurrentCoroutine = PersistanceCoroutineRunner.Instance.StartCoroutine(DownloadFilesConcurrently(token));

        //Debug.Log("completed all files download, concurrently");
        yield return concurrentCoroutine;

        if (!token.IsCancellationRequested && isManifestFileDownloaded && downloadedFilesCount == fileUrls.Count)
        {
            OnDownloadCompleted();
        }
        else
		{
            OnDownloadInterrupted();
		}
    }

    /// <summary>
    /// Downloads the m3u8 manifest and extracts all file URLs.
    /// </summary>
    private IEnumerator DownloadM3U8(CancellationToken token)
    {
        if (token.IsCancellationRequested) yield break;

        string url = Url;
        //Debug.Log($"Downloading m3u8 manifest: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            activeRequests.Add(request);

            var operation = request.SendWebRequest();

            // Wait for completion or cancellation
            yield return new WaitUntil(() => operation.isDone || token.IsCancellationRequested);

            //Debug.Log("Completed downloading the manifest file");

            if (token.IsCancellationRequested)
            {
                if (request != null)
                {
                    request.Abort();
                    request.Dispose();
                }
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                //Debug.LogError($"Failed to download m3u8: {request.error}");
                isManifestFileDownloaded = false;
                if (request != null) request.Dispose();
                yield break;
            }

            string m3u8Content = request.downloadHandler.text;
            string m3u8FileName = Path.GetFileName(new Uri(url).LocalPath);
            string m3u8FilePath = Path.Combine(cacheFolderPath, m3u8FileName);

            // Save the m3u8 locally
            File.WriteAllText(m3u8FilePath, m3u8Content);
            isManifestFileDownloaded = true;
            ExtractFileUrls(m3u8Content, url);

            if (request != null)
            {
                request.Dispose();
            }
        }
    }

    /// <summary>
    /// Parses the m3u8 file and extracts .ts and key URLs.
    /// </summary>
    private void ExtractFileUrls(string m3u8Content, string baseUrl)
    {
        string[] lines = m3u8Content.Split('\n');
        Uri baseUri = new Uri(baseUrl);
        string basePath = baseUri.GetLeftPart(UriPartial.Authority) + baseUri.AbsolutePath.Substring(0, baseUri.AbsolutePath.LastIndexOf('/') + 1);

        fileUrls.Clear();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("#EXT-X-KEY"))
            {
                string keyFileName = ExtractKeyFileName(trimmedLine);
                if (!string.IsNullOrEmpty(keyFileName))
                {
                    fileUrls.Add($"{basePath}{keyFileName}");
                }
            }

            if (trimmedLine.EndsWith(".ts"))
            {
                fileUrls.Add($"{basePath}{trimmedLine}");
            }
        }
    }

    private string ExtractKeyFileName(string keyLine)
    {
        int startIndex = keyLine.IndexOf("URI=\"") + 5;
        int endIndex = keyLine.IndexOf("\"", startIndex);
        return (startIndex > 0 && endIndex > startIndex) ? keyLine.Substring(startIndex, endIndex - startIndex) : string.Empty;
    }

    /// <summary>
    /// Downloads all .ts and key files concurrently.
    /// </summary>
    /// <summary>
    /// Downloads all .ts and key files concurrently with throttling and cancellation support.
    /// </summary>
    /// <summary>
    /// Downloads all .ts and key files concurrently with throttling and proper cancellation handling.
    /// </summary>
    private IEnumerator DownloadFilesConcurrently(CancellationToken token)
    {
        int maxConcurrentDownloads = 4;
        List<UnityWebRequestAsyncOperation> activeDownloads = new List<UnityWebRequestAsyncOperation>();
        //downloadedFilesCount = 0;
        int filesCount = fileUrls.Count;

        for (int i = 0; i < fileUrls.Count; i++)
        {
            if (token.IsCancellationRequested)
            {
                CleanupRequests();
                yield break;
            }

            string fileUrl = fileUrls[i];
            string fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            string localPath = Path.Combine(cacheFolderPath, fileName);

            UnityWebRequest request = UnityWebRequest.Get(fileUrl);
            activeRequests.Add(request);

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            activeDownloads.Add(operation);

            // Attach completed event to handle the request asynchronously
            operation.completed += (op) =>
            {
                if (token.IsCancellationRequested)
                {
                    try
                    {
                        if (request != null)
                        {
                            request.Abort();
                            request.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logs.LogWarning($"Exception while disposing request: {ex.Message}");
                    }
                }
                else if (request.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = request.downloadHandler.data;
                    File.WriteAllBytes(localPath, data ?? new byte[0]);
                    downloadedFilesCount += 1;

                    float progress = (float)downloadedFilesCount / filesCount;
                    //progress *= 100f;
                    //Debug.Log($"Downloaded: {fileName}, downloadedFiles : {downloadedFilesCount}, total files count {fileUrls.Count} |{filesCount} & progress :{progress}");
                    OnDownloadProgress(progress);
                }
                else
                {
                    Logs.LogError($"Failed: {fileName} - {request.error} - {request.responseCode}");
                }

                // Cleanup
                //request.Dispose();
                if (activeRequests.Contains(request))
                    activeRequests.Remove(request);
                if (activeDownloads.Contains(operation))
                    activeDownloads.Remove(operation);
            };

            // Throttle concurrent downloads
            if (activeDownloads.Count >= maxConcurrentDownloads)
            {
                yield return new WaitUntil(() => activeDownloads.Count < maxConcurrentDownloads);
            }
        }

        // Wait for all downloads to finish before completing
        yield return new WaitUntil(() => activeDownloads.Count == 0);

        Logs.Log("Downloaded successfully!");
    }

    public float getCurrentProgress()
	{
        if (fileUrls.Count == 0) return 0;
        if (isManifestFileDownloaded && downloadedFilesCount == 0) return (float)1 / fileUrls.Count;
        return (float)downloadedFilesCount / fileUrls.Count;
	}

    private async Task SaveFileAsync(string path, byte[] bytes)
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllBytesAsync(path, bytes);
            Logs.Log($"Image saved: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            Logs.LogError($"Error saving image to cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Mark the download as complete by creating a flag file.
    /// </summary>
    private void MarkDownloadComplete(string cacheFolder)
    {
        string completeFlagPath = Path.Combine(cacheFolder, "complete.flag");

        try
        {
            // Write timestamp for completed downloads
            File.WriteAllText(completeFlagPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //Debug.Log($"Download marked as complete: {completeFlagPath}");
        }
        catch (Exception ex)
        {
            Logs.LogError($"Failed to mark download complete: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleanup all UnityWebRequests properly.
    /// </summary>
    /// <summary>
    /// Cleans up all active UnityWebRequests safely.
    /// </summary>
    private void CleanupRequests()
    {
        try
        {
            // Clean the list from null references before iterating
            activeRequests.RemoveAll(req => req == null);

            foreach (var request in activeRequests)
            {
                if (request != null)
                {
                    try
                    {
                        request.Abort();
                        request.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logs.LogWarning($"Exception during cleanup: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logs.LogError($"Error during CleanupRequests: {ex.Message}");
        }
        finally
        {
            activeRequests.Clear();
        }
    }

    private void OnDownloadStarted()
    {
        Logs.Log("##Download started.");
        downloadState = DownloadState.DOWNLOADING;
        //DownloadStarted?.Invoke(this, EventArgs.Empty);
    }

    private void OnDownloadProgress(float progress)
    {
        Logs.Log("Updating download progress.");
        downloadState = DownloadState.DOWNLOADING;
        //DownloadProgress?.Invoke(this, new DownloadProgressEventArgs(progress));
        DownloadProgress?.Invoke(progress);
    }

    private void OnDownloadCompleted()
    {
        Logs.Log("##Download completed");
        downloadState = DownloadState.DOWNLOADED;
        if (cts != null)
        {
            cts.Dispose();
            cts = null;
        }
        MarkDownloadComplete(cacheFolderPath);
        unsubscribeInternetConnectionChange();
        //DownloadCompleted?.Invoke(this, new DownloadCompleteEventArgs(FileName));
        DownloadCompleted?.Invoke(this.Url);
    }

    private void OnDownloadCanceled()
    {
        Logs.Log("##Download cancelled.");
        downloadState = DownloadState.NOT_DOWNLOADED;
        unsubscribeInternetConnectionChange();
        //DownloadCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void OnDownloadInterrupted()
	{
        Logs.LogError("##Download Interrupted.");
        if (isInterrupted) return;
        isInterrupted = true;
        downloadState = DownloadState.NOT_DOWNLOADED;
        unsubscribeInternetConnectionChange();
        DownloadInterrupted?.Invoke(Url);
    }

    internal void DeleteCache()
    {
        string path = cacheFolderPath;
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, true);  // Recursively delete
                //Debug.Log($"Deleted directory: {path}");
            }
            catch (IOException ioEx)
            {
                Logs.LogError($"IO Exception: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Logs.LogError($"Access Denied: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Logs.LogError($"Failed to delete directory: {ex.Message}");
            }
        }
        else
        {
            Logs.LogWarning($"Directory not found");
        }
    }
}