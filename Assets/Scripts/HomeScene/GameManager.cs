// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;

// public class GameManager : MonoBehaviour
// {
// 	#region Roles and Responsibilities
// 	/// <summary>
// 	/// meant to be singleton to do interSCENE dependency work.
// 	/// Know if this user had already submitted an app review.
// 	/// Handle the rating UI weather to display or not.
// 	/// get ref of the assetbundle
// 	/// </summary>
// 	#endregion

// 	public static GameManager Instance;

// 	private int showsWatchedInARowCount;
// 	internal bool canAskForRating = true;

// 	internal AssetBundle loadedAssetBundle;

//     public string[] deviceSupportText_English;
//     public string[] deviceSupportText_Hindi;


//     private void Awake()
// 	{
// 		if (Instance == null)
// 		{
// 			Instance = this;
// 			DontDestroyOnLoad(this);
// 		}
// 		else
// 		{
// 			Destroy(gameObject);
// 		}

// 		initializeManagers();
// 		unloadAssetBundle();
//         dm = this;
//         connectmanager = FindObjectOfType<NetworkMonitor>();

//         if (connectmanager == null)
//         {
//             connectmanager = new GameObject().AddComponent<NetworkMonitor>();
//         }
//         //deviceSupportMediaQuality = tryGetMediaSupportFromPlayerPrefs();
//     }

// 	private void initializeManagers()
// 	{
// 		// liveshowManager = new GameObject("LiveshowManager").AddComponent<LiveShowManager>();
// 		// liveshowManager.Initialize();
// 	}

// 	// private mediaFormat tryGetMediaSupportFromPlayerPrefs()
// 	// {
//     //     string defaultvalue = "NA";
//     //     if (PlayerPrefs.GetString("deviceSupportHighQualityVideo", defaultvalue) == "true")
//     //     {
//     //         return mediaFormat.HIGH;
//     //     }
//     //     if (PlayerPrefs.GetString("deviceSupportMediumQualityVideo", defaultvalue) == "true")
//     //     {
//     //         return mediaFormat.MEDIUM;
//     //     }
//     //     //if adaptive false check once again
//     //     if (PlayerPrefs.GetString("deviceSupportAdaptiveStreamVideo", defaultvalue) == "true")
//     //     {
//     //         return mediaFormat.ADAPTIVE;
//     //     }
//     //     if (PlayerPrefs.GetString("deviceConfigCheckCompleted", defaultvalue) == "true")
//     //     {
//     //         return mediaFormat.NOTSUPPORTED;
//     //     }
//     //     return mediaFormat.DEFAULT;
//     // }

// 	// internal void unloadAssetBundle()
// 	// {
// 	// 	if (loadedAssetBundle != null)
// 	// 	{
// 	// 		loadedAssetBundle.UnloadAsync(true);
// 	// 	}
// 	// }

// 	internal void updatedShowsWatchedInARowCount()
// 	{
// 		showsWatchedInARowCount++;
// 	}

// 	// internal bool shouldAskUserForAppRating()
// 	// {
// 	// 	return canAskForRating && showsWatchedInARowCount > 1;
// 	// }

// 	// internal void resetRatingStateToDefault()
// 	// {
// 	// 	Debug.Log("resetting no of shows watched to 0.");
// 	// 	showsWatchedInARowCount = 0;
// 	// 	canAskForRating = true;
// 	// }

// 	// internal void update_CanAskForRating(bool value)
// 	// {
// 	// 	Debug.Log("can ask for rating" + value);
// 	// 	canAskForRating = value;
// 	// }

// 	// Start is called before the first frame update
// 	void Start()
// 	{
// 		//resetRatingStateToDefault();
//         //downloads = new Dictionary<string, DownloadItem>();
// 		//generateDateLinksMap(NoOfDaysChecks);
// 	}

//     // #region AssetBundles - Download
//     // public static GameManager dm { get; private set; }
//     // public Dictionary<string, DownloadItem> downloads;
//     // private NetworkMonitor connectmanager;


//     // public bool CheckConnectivity()
//     // {
//     //     return connectmanager.IsConnected;
//     // }


//     // public void CancelDownload(string experienceName)
//     // {
//     //     if (downloads.ContainsKey(experienceName))
//     //     {
//     //         DownloadItem item = downloads[experienceName];
//     //         item.CancelDownload();
//     //         downloads.Remove(experienceName);
//     //     }
//     // }

//     // public void AddDownload(string experienceName, string url)
//     // {
//     //     if (DownloadExists(experienceName))
//     //     {
//     //         Debug.Log($"Download '{experienceName}' already exists.");
//     //         return;
//     //     }

//     //     DownloadItem newItem = new DownloadItem(experienceName, url);
//     //     downloads.Add(experienceName, newItem);

//     //     newItem.StartDownload();
//     // }

//     // public bool DownloadExists(string experienceName)
//     // {
//     //     return downloads.ContainsKey(experienceName);
//     // }
// 	// #endregion

// }

// public class DownloadItem
// {
//     public string ExperienceName { get; }
//     public string Url { get; }
//     public bool IsDownloading { get; private set; }
//     public bool IsCompleted { get; private set; }
//     public bool IsCanceled { get; private set; }
//     public int Progress { get; private set; }
//     public byte[] DownloadedData { get; private set; }

//     public event EventHandler DownloadStarted;
//     public event EventHandler<DownloadProgressEventArgs> DownloadProgress;
//     public event EventHandler DownloadCompleted;
//     public event EventHandler DownloadCanceled;

//     public DownloadItem(string experienceName, string url)
//     {
//         ExperienceName = experienceName;
//         Url = url;
//     }

//     public void StartDownload()
//     {
//         IsDownloading = true;
//         IsCompleted = false;
//         IsCanceled = false;

//         OnDownloadStarted();
//         CoroutineRunner.Instance.StartCoroutine(DownloadCoroutine());
//     }

//     public void CancelDownload()
//     {
//         if (IsDownloading)
//         {
//             IsCanceled = true;
//             IsDownloading = false;

//             OnDownloadCanceled();
//         }
//     }

//     private void OnDownloadStarted()
//     {
//         DownloadStarted?.Invoke(this, EventArgs.Empty);
//     }

//     private void OnDownloadProgress(float progress)
//     {
//         DownloadProgress?.Invoke(this, new DownloadProgressEventArgs(progress));
//     }

//     private void OnDownloadCompleted()
//     {
//         DownloadCompleted?.Invoke(this, EventArgs.Empty);
//     }

//     private void OnDownloadCanceled()
//     {
//         DownloadCanceled?.Invoke(this, EventArgs.Empty);
//     }

//     private IEnumerator<UnityWebRequestAsyncOperation> DownloadCoroutine()
//     {
//         UnityWebRequest request = UnityWebRequest.Get(Url);
//         Debug.Log($"Downloading: {ExperienceName}");
//         request.SendWebRequest();

//         while (!request.isDone)
//         {
//             //Progress = Mathf.RoundToInt(request.downloadProgress * 100);
//             OnDownloadProgress(request.downloadProgress);

//             if (IsCanceled)
//             {
//                 request.Abort();
//                 yield break;
//             }

//             // Check connectivity and cancel download if internet is paused or disconnected
//             if (!GameManager.dm.CheckConnectivity())
//             {
//                 CancelDownload();
//                 Debug.Log($"Download canceled: {ExperienceName} - {Url} - Internet paused or disconnected");
//                 yield break;
//             }

//             yield return null;
//         }

//         if (request.result != UnityWebRequest.Result.Success)
//         {
//             CancelDownload();
//             Debug.LogError($"Download failed: {ExperienceName} - {Url} - Error: {request.error}");
//             yield break;
//         }

//         DownloadedData = request.downloadHandler.data;

//         IsDownloading = false;
//         IsCompleted = true;

//         OnDownloadCompleted();
//     }
// }

// public class DownloadProgressEventArgs : EventArgs
// {
//     public float Progress { get; }

//     public DownloadProgressEventArgs(float progress)
//     {
//         Progress = progress;
//     }
// }
// public class CoroutineRunner : MonoBehaviour
// {
// 	private static CoroutineRunner instance;
// 	public static CoroutineRunner Instance
// 	{
// 		get
// 		{
// 			if (instance == null)
// 			{
// 				GameObject runnerObj = new GameObject("CoroutineRunner");
// 				instance = runnerObj.AddComponent<CoroutineRunner>();
// 				DontDestroyOnLoad(runnerObj);
// 			}
// 			return instance;
// 		}
// 	}
// }

// public class LiveShowUrlCheckerItem
// {
// 	public string Date { get; }
// 	public string TimeOfDay { get; }
// 	private LiveShowMediaLinks Urls { get; }
// 	public string LiveShowTimingText { get; private set; }
// 	public Dictionary<string, bool> UrlExistenceRecord { get; private set; }
// 	public bool IsChecking { get; private set; }
// 	public bool IsUrlsCheckCompleted { get; private set; }
// 	private string DateTimeText { get; set; }

// 	public event EventHandler UrlCheckStarted;
// 	public event EventHandler<UrlCheckProgressEventArgs> UrlCheckProgress;
// 	public event EventHandler<UrlCheckCompletedEventArgs> UrlCheckCompleted;

// 	internal string getDateTimeFileText()
// 	{
// 		return DateTimeText;
// 	}
// 	public LiveShowUrlCheckerItem(string dateTimeText, LiveShowMediaLinks urls)
// 	{
// 		this.Urls = urls;
// 		LiveShowTimingText = dateTimeText;
// 		Date = LiveShowTimingText.Substring(0, 8);
// 		TimeOfDay = LiveShowTimingText.Substring(8, 2);
// 		//Debug.Log(Date + "--" + TimeOfDay + Urls[Urls.Count - 1] + " is the datetimeoftheday.");
// 		UrlExistenceRecord = new Dictionary<string, bool>();
// 	}

// 	public void StartUrlCheck()
// 	{
// 		IsChecking = true;
// 		IsUrlsCheckCompleted = false;

// 		OnUrlCheckStarted();
// 		string dateTimeUrl = Urls.UploadTimeText;
// 		//Debug.LogWarning("------dateTimeUrl: started checking" + dateTimeUrl);
// 		//Debug.LogWarning(dateTimeUrl+ " is the date time url in the specific class");
// 		CoroutineRunner.Instance.StartCoroutine(CheckUrlsCoroutine());
// 	}

// 	private void OnUrlCheckStarted()
// 	{
// 		UrlCheckStarted?.Invoke(this, EventArgs.Empty);
// 	}

// 	private void OnUrlCheckProgress(float progress)
// 	{
// 		UrlCheckProgress?.Invoke(this, new UrlCheckProgressEventArgs(progress));
// 	}

// 	private void OnUrlCheckCompleted(string dateTimeText, string dateTimeTexturl, Dictionary<string, bool> keyValuePairs)
// 	{
// 		UrlCheckCompleted?.Invoke(this, new UrlCheckCompletedEventArgs(dateTimeText, dateTimeTexturl, keyValuePairs));
// 	}

// 	private IEnumerator CheckUrlsCoroutine()
// 	{
// 		// Only log once per URL check to help debug duplicate checks
// 		//Debug.LogWarning($"------dateTimeUrl: started checking {Urls.UploadTimeText} for {Date}{TimeOfDay}");
// 		UnityWebRequest dateTimeRequest = UnityWebRequest.Get(Urls.UploadTimeText);
// 		yield return dateTimeRequest.SendWebRequest();

// 		bool isSuccess = dateTimeRequest.result == UnityWebRequest.Result.Success && 
// 						dateTimeRequest.responseCode == 200;

// 		UrlExistenceRecord[Urls.UploadTimeText] = isSuccess;

// 		if (!isSuccess)
// 		{
// 			//Debug.LogError($"------Date-time request failed: {dateTimeRequest.error}");
// 			HandleDateTimeFailure();
// 			yield break;
// 		}

// 		try
// 		{
// 			LiveShowTimingText = dateTimeRequest.downloadHandler.text;
// 			DateTimeText = LiveShowTimingText;
// 			//Debug.Log("LiveShowTimingText: " + LiveShowTimingText);
// 		}
// 		catch (Exception e)
// 		{
// 			Debug.LogError($"Error parsing date-time response: {e.Message}");
// 			HandleDateTimeFailure();
// 			yield break;
// 		}

// 		UnityWebRequest request2K = UnityWebRequest.Head(Urls.MediaQuality_2K);
// 		yield return request2K.SendWebRequest();
		
// 		if (request2K.result == UnityWebRequest.Result.ConnectionError ||
// 			request2K.result == UnityWebRequest.Result.ProtocolError)
// 		{
// 			Debug.LogWarning($"2K media request failed: {request2K.error}");
// 		}
		
// 		UrlExistenceRecord[Urls.MediaQuality_2K] = request2K.result == UnityWebRequest.Result.Success && 
// 												 request2K.responseCode == 200;
// 		OnUrlCheckProgress(0.66f);

// 		UnityWebRequest request4K = UnityWebRequest.Head(Urls.MediaQuality_4K);
// 		yield return request4K.SendWebRequest();

// 		if (request4K.result == UnityWebRequest.Result.ConnectionError ||
// 			request4K.result == UnityWebRequest.Result.ProtocolError)
// 		{
// 			Debug.LogWarning($"4K media request failed: {request4K.error}");
// 		}

// 		UrlExistenceRecord[Urls.MediaQuality_4K] = request4K.result == UnityWebRequest.Result.Success && 
// 												 request4K.responseCode == 200;
// 		OnUrlCheckProgress(1f);

// 		CompleteCheck(3, 3, DateTimeText, Urls.UploadTimeText);
// 	}

// 	private void HandleDateTimeFailure()
// 	{
// 		DateTimeText = "FILE NOT FOUND";
// 		UrlExistenceRecord[Urls.MediaQuality_2K] = false;
// 		UrlExistenceRecord[Urls.MediaQuality_4K] = false;
// 		OnUrlCheckProgress(1f);
// 		CompleteCheck(3, 3, null, Urls.UploadTimeText);
// 	}

// 	private void CompleteCheck(int urlsChecked, int totalUrls, string dateTimeText, string dateTimeTextUrl)
// 	{
// 		IsChecking = false;
// 		IsUrlsCheckCompleted = true;
// 		//Debug.Log($" ------check completed for {dateTimeText} and {dateTimeTextUrl}");
// 		OnUrlCheckCompleted(dateTimeText, dateTimeTextUrl, UrlExistenceRecord);
// 		//Debug.Log($"All URL checks completed. {urlsChecked}/{totalUrls} URLs checked.");
// 	}
// }

// public class UrlCheckProgressEventArgs : EventArgs
// {
// 	public float Progress { get; }

// 	public UrlCheckProgressEventArgs(float progress)
// 	{
// 		Progress = progress;
// 	}
// }

// public class UrlCheckCompletedEventArgs : EventArgs
// {
// 	public string DateTimeText { get; }
// 	public string DateTimeTextUrl { get; }
// 	public Dictionary<string, bool> UrlExistenceRecord { get; }
// 	public UrlCheckCompletedEventArgs(string dateTimeText, string dateTimeTextUrl, Dictionary<string, bool> keyValuePairs)
// 	{
// 		DateTimeText = dateTimeText;
// 		DateTimeTextUrl = dateTimeTextUrl;
// 		UrlExistenceRecord = keyValuePairs;
// 	}
// }

