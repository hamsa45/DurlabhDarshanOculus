// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using UnityEngine.SceneManagement;
// using System.Collections;

// public abstract class ThumbnailBase : MonoBehaviour
// {
// 	protected ThumbnailDTO thumbnailData;
// 	protected Coroutine coroutine;

// 	protected VideoThumbnailUI ui;

// 	public GameObject networkErrorPage;
// 	//public LaunchPageData launchPageGO;
// 	public GameObject activationPopUps;
// 	public GameObject manager;

// 	public CategoryManagement categoryManagement;

// 	public void InjectUI(VideoThumbnailUI uiRef)
// 	{
// 		ui = uiRef;
// 	}

// 	public void Initialize(ThumbnailDTO dto, bool isApiData)
// 	{
// 		thumbnailData = dto;

// 		if(ui == null)
// 		{
// 			Debug.LogError("UI is null, make sure to inject UI in Initialize");
// 			return;
// 		}
		
// 		ui.SetBasicInfo(dto.title, dto.city, dto.hindiTitleText, dto.hindiCityText);

// 		if (isApiData)
// 		{
// 			if(dto.GetThumbnailType() != ThumbnailType.LiveShow) ui.SetImageFromUrl(dto.thumbnailIconUrl, this);
// 		}

// 		ui.SetDuration(dto.videoDuration);
// 		ui.ShowAdaptiveStreamingTag(dto.isAdaptiveStreamingEnabled);
// 		ui.SetNewVideoTag(dto.isNewVideo);

// 		bool isAvailable = ThumbnailDataInApp.approvedVideoShowIds.Contains(dto.show_id);
// 		ui.SetComingSoonState(isAvailable);

// 		if (isAvailable)
// 		{
// 			ui.watchButton.onClick.AddListener(ValidateClickAndPlay);
// 			ui.liveShowButton.onClick.AddListener(ValidateClickAndPlay);
// 		}

// 		GetComponent<Button>().onClick.AddListener(OnThumbnailClick);
// 		ui.watchButton.onClick.AddListener(CheckAndAdjustNewVideoTag);

// 		// Add thumbnail to category
//         categoryManagement.AddToCategory(dto.GetThumbnailType(), gameObject, dto.show_id);

// 		// Reorder if adaptive streaming or new
// 		if (dto.isAdaptiveStreamingEnabled || dto.isNewVideo)
// 			transform.SetAsFirstSibling();

// 		CustomInitialize(dto);
// 	}

// 	protected void CheckAndAdjustNewVideoTag()
// 	{
// 		if (ui.newVideoTag.activeInHierarchy && !PlayerPrefs.HasKey($"{thumbnailData.title}NewVideoTag"))
// 		{
// 			PlayerPrefs.SetInt($"{thumbnailData.title}NewVideoTag", 0);
// 			ui.SetNewVideoTag(false);
// 		}
// 	}

// 	private bool isUserSubscribed()
//     {
//         return UserContentManager.instance != null && UserContentManager.instance.isSubscribed;
//     }

// 	private void setUpCurrentVideoDataToPlay()
// 	{
//         CurrentVideoDataInPlay.thumbnailDTO = thumbnailData;
//         CurrentVideoDataInPlay.currentVideoUrl = getCurrentVideoUrl();
//         CurrentVideoDataInPlay.isLocalFile = false;
//     }

// 	private string getCurrentVideoUrl()
// 	{
//         //get the lowest supported video url if we do not have the UserQualityPref saved
//         //Debug.LogWarning("trying to get currnet URl");
//         if (PlayerPrefs.HasKey(AppData.videoQualityPrefKey))
//         {
//             string videoQualityPrefs = PlayerPrefs.GetString(AppData.videoQualityPrefKey);
//             if (videoQualityPrefs == "High")
// 			{
//                 //Debug.LogWarning("Playing HIGH quality url on clicking watch button");
//                 return thumbnailData.quality.highQualityVideoURL;
// 			}
//             else if (videoQualityPrefs == "Medium")
//             {
//                 //Debug.LogWarning("Playing Medium quality url on clicking watch button");
//                 return thumbnailData.quality.mediumQualityVideoURL;
//             }
//             else if (videoQualityPrefs == "Adaptive")
//             {
//                 if (thumbnailData.isAdaptiveStreamingEnabled)
// 				{
//                     //Debug.LogWarning("Playing adaptive quality url on clicking watch button");
//                     return thumbnailData.quality.AdaptiveStreamingVideoURL;
//                 }
//                 else
// 				{
//                     //based on platfrom set the url
//                     return getPlatformBasedMediumUrl();
//                 }
//             }
//         }
//         else
// 		{
//             mediaFormat D_supportedMediaFormat = GameManager.Instance.DeviceSupportMediaQuality;
//             //Debug.Log(D_supportedMediaFormat);
//             if (D_supportedMediaFormat == mediaFormat.ADAPTIVE)
//             {
//                 if (thumbnailData.isAdaptiveStreamingEnabled)
//                 {
//                     //Debug.Log("------isAdaptiveStreamingEnabled: " + thumbnailData.isAdaptiveStreamingEnabled);
//                     //based on platform set the url, as adaptive streaming file is same for both platforms
//                     return thumbnailData.quality.AdaptiveStreamingVideoURL;
//                 }
//             }
//             else if (D_supportedMediaFormat == mediaFormat.HIGH || D_supportedMediaFormat == mediaFormat.MEDIUM)
//             {
//                 //based on platfrom set the url
//                 return getPlatformBasedMediumUrl();
//             }
//         }

//         return getPlatformBasedMediumUrl();
//     }

//     private string getPlatformBasedMediumUrl()
// 	{
//         //Debug.Log("Playing from platform based medium urls.");
// #if UNITY_ANDROID
//         return thumbnailData.quality.mediumQualityVideoURL;
// #elif UNITY_IOS
//          return thumbnailData.quality.NonEn_mediumQualityVideoURL;
// #endif
//     }

//     private string getCacheableUrl()
//     {
//         string Url;
//         //consider the quality toggle and get corresponding url
//         if (PlayerPrefs.HasKey(AppData.videoQualityPrefKey))
//         {
//             string videoQualityPref = PlayerPrefs.GetString(AppData.videoQualityPrefKey);
//             if (videoQualityPref == "High")
//             {
//                 Url = thumbnailData.quality.highQualityVideoURL;
//                 return Url;
//             }
//             else if (videoQualityPref == "Medium")
//             {
//                 Url = thumbnailData.quality.mediumQualityVideoURL;
//                 return Url;
//             }
//             else if (videoQualityPref == "Adaptive")
//             {
//                 if (thumbnailData.isAdaptiveStreamingEnabled)
//                 {
//                     Url = thumbnailData.quality.AdaptiveStreamingVideoURL;
//                     return Url;
//                 }
//                 else
//                 {
//                     //based on platfrom set the url
//                     getPlatformBasedMediumUrl();
//                 }
//             }
//         }
//         return getDefaultUrl(false);
//         //Debug.Log("++++++++++++" + Url);
//         //return Url;
//     }

//     private string getDefaultUrl(bool isAndroid)
// 	{
//         mediaFormat supportedFormat = GameManager.Instance.DeviceSupportMediaQuality;
//         if (supportedFormat == mediaFormat.HIGH)
//         {
//             return thumbnailData.quality.highQualityVideoURL;
//         }
//         else if (supportedFormat == mediaFormat.MEDIUM)
//         {
//             return thumbnailData.quality.mediumQualityVideoURL;
//         }
//         else if (supportedFormat == mediaFormat.ADAPTIVE)
//         {
//             return thumbnailData.quality.AdaptiveStreamingVideoURL;
//         }

//         return thumbnailData.quality.AdaptiveStreamingVideoURL;
//     }

// 	private void openVideoPlayerScene()
//     {
// 		SceneManager.LoadScene(AppData.VideoPlayerScene);
// 	}

// 	private bool checkIfVideoIsCachedIOS()
//     {
//         return CacheManager.instance.IsMediaCached(getCacheableUrl());
// 	}

//     private bool checkIfVideoIsCachedAndroid()
//     {
//         return M3U8DownloadManager.Instance != null && M3U8DownloadManager.Instance.IsDownloaded(getCurrentVideoUrl());
// 	}

// 	private void requestSubscription()
// 	{
//         Debug.Log("You Are Not subscribed, please subscribe");
//         activationPopUps.SetActive(true);
//         GameObject bg = activationPopUps.transform.GetChild(0).gameObject;//.SetActive(true);
//         bg.SetActive(true);

//         if (!HomeManager.isSubscriptionChecked)
//         {
//             foreach (Transform child in bg.transform)
//             {
//                 if (child.gameObject.name == "CheckingUserData")
//                 {
//                     child.gameObject.SetActive(true);
//                 }
//                 else
//                 {
//                     child.gameObject.SetActive(false);
//                 }
//             }
//         }
//         else if (UserContentManager.instance.isSubscribedBefore)
//         {
//             foreach (Transform child in bg.transform)
//             {
//                 if (child.gameObject.name == "LogOutFromOtherDevices")
//                 {
//                     child.gameObject.SetActive(true);
//                 }
//                 else
//                 {
//                     child.gameObject.SetActive(false);
//                 }
//             }
//         }
//         else
//         {
//             foreach (Transform child in bg.transform)
//             {
//                 if (child.gameObject.name == "ActivateToWatch")
//                 {
//                     child.gameObject.SetActive(true);
//                 }
//                 else
//                 {
//                     child.gameObject.SetActive(false);
//                 }
//             }
//         }
//     }

// 	protected abstract void CustomInitialize(ThumbnailDTO dto);
// 	protected abstract void OnThumbnailClick();
// 	protected virtual void ValidateClickAndPlay()
// 	{
// 		setUpCurrentVideoDataToPlay();

// 		if(deviceNotSupported())
// 		{
// 			ToastManager.Toast.ErrorToast("3D 360 videos aren't supported on this device.");
// 			return;
// 		}

// 		if(liveShowNeedsHDSupport())
// 		{
// 			ToastManager.Toast.InfoToast("Live show needs HD/HD+ support. Try Aartis or Documentaries instead.");
// 			return;
// 		}

// 		if(isUserSubscribed()) playMediaForPlatform();
// 		else requestSubscription();
// 	}

// 	 private bool deviceNotSupported()
//     {
//         return GameManager.Instance.DeviceSupportMediaQuality == mediaFormat.NOTSUPPORTED;
//     }

// 	private bool liveShowNeedsHDSupport()
// 	{
// 		return GameManager.Instance.DeviceSupportMediaQuality == mediaFormat.ADAPTIVE && !thumbnailData.isAdaptiveStreamingEnabled;
// 	}

// 	private void playMediaForPlatform()
//     {
// 		manager.GetComponent<HomeManager>().hasToLoadVRScene = true;

// 		if (Application.platform == RuntimePlatform.Android)
// 		{
// 			if (checkIfVideoIsCachedAndroid())
// 			{
// 				openVideoPlayerScene();
// 			}
// 			else CheckInternetAndPlay();
// 		}
// 		else if (Application.platform == RuntimePlatform.IPhonePlayer)
// 		{
// 			if (checkIfVideoIsCachedIOS())
// 			{
// 				openVideoPlayerScene();
// 			}
// 			else CheckInternetAndPlay();
// 		}
//     }

// 	private void CheckInternetAndPlay()
// 	{
// 		int callbackCount = 0;
// 		StartCoroutine(CheckInternetConnection(isConnected =>
// 		{
// 			if (callbackCount < 3)
// 				callbackCount++;

// 			if (isConnected)
// 			{
// 				networkErrorPage.SetActive(false);
// 				if (callbackCount == 1) SceneManager.LoadScene("VideoPlayer");
// 			}
// 			else
// 			{
// 				if (!networkErrorPage.activeSelf)
// 					networkErrorPage.SetActive(true);
// 			}
// 		}));
// 	}
	
// 	private IEnumerator CheckInternetConnection(System.Action<bool> syncResult)
// 	{
// 		using (var request = UnityEngine.Networking.UnityWebRequest.Head("https://google.com"))
// 		{
// 			request.timeout = 5;
// 			yield return request.SendWebRequest();
// 			bool result = request.result == UnityEngine.Networking.UnityWebRequest.Result.Success;
// 			syncResult(result);
// 		}
// 	}
// }