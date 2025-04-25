using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

// public class LiveShowThumbnail : ThumbnailBase
// {
// 	[Header("Live Show Specific UI")]

// 	private bool isPreviousLive;
// 	private string liveFileTimingUrl;
// 	private string liveshowTimeKey;
// 	private LiveShowMediaLinks liveShowLinks;
// 	private string cacheFileName;

// 	public void UpdateMediaLinks(LiveShowMediaLinks liveLinks)
// 	{
// 		this.liveShowLinks = liveLinks;
// 	}

// 	public void UpdateLiveShowTimeText(string liveTiming)
// 	{
// 		liveshowTimeKey = liveTiming;
// 	}

// 	protected override void CustomInitialize(ThumbnailDTO dto)
// 	{
// 		isPreviousLive = dto.isPrevLiveShowThumbnail;
// 		liveFileTimingUrl = dto.liveFileTimingTextUrl;

// 		ThumbnailType type = dto.GetThumbnailType();
// 		ui.setThumbnailTypeIcon(type);
// 		ui.setLiveShowTimingIcon(type == ThumbnailType.LiveShow);
// 		ui.setDurationIcon(type != ThumbnailType.LiveShow);
		
// 		string iconImageUrl = getIconImageUrl();
// 		ui.SetImageFromUrl(iconImageUrl, this, getCacheFileName());

// 		ui.watchButton.gameObject.SetActive(false);
// 		ui.SetDurationText("Fetching...");

// 		AddToLiveShowCheck();

// 		GetComponent<Button>().onClick.AddListener(OnThumbnailClick);

// 	    UpdateVideoLinksOnThumbnailData();
// 	}

//     private string getCacheFileName()
//     {
// 		if(string.IsNullOrEmpty(cacheFileName))
// 		{
// 			setCacheFileName();
// 		}

//         return cacheFileName;
//     }

// 	private void setCacheFileName()
// 	{
// 		string title = thumbnailData.title;
// 		title = title.Replace(" ", "");
// 		cacheFileName = $"{title}_{liveshowTimeKey}";
// 	}

//     private void UpdateVideoLinksOnThumbnailData()
// 	{
// 		if(Application.platform == RuntimePlatform.Android)
// 		{
// 			thumbnailData.quality.mediumQualityVideoURL = liveShowLinks.MediaQuality_2K;
// 			thumbnailData.quality.highQualityVideoURL = liveShowLinks.MediaQuality_4K;
// 		}
// 		else if(Application.platform == RuntimePlatform.IPhonePlayer)
// 		{
// 			thumbnailData.quality.NonEn_mediumQualityVideoURL = liveShowLinks.MediaQuality_2K;
// 			thumbnailData.quality.NonEn_highQualityVideoURL = liveShowLinks.MediaQuality_4K;
// 		}
// 	}

//     private string getIconImageUrl()
//     {
//         return AppData.AAJ_KE_DARSHAN_DAILYIMAGES_API_BASE_URL + constructSecondPartOfUrl();
//     }

//     private string constructSecondPartOfUrl()
//     {
//         if (string.IsNullOrEmpty(liveshowTimeKey)) return string.Empty;

//         // Extract date components from liveshowTimeKey (format: ddMMyyyyAM/PM)
//         string day = liveshowTimeKey.Substring(0, 2);
//         string month = liveshowTimeKey.Substring(2, 2); 
//         string year = liveshowTimeKey.Substring(4, 4);

//         return $"{year}/{month}/{day}.png";
//     }

//     protected override void OnThumbnailClick()
// 	{
// 		launchPageGO.GetComponent<LaunchPageData>().populateLaunchPage(thumbnailData);
// 		base.CheckAndAdjustNewVideoTag();
// 	}

// 	protected override void ValidateClickAndPlay()
// 	{
// 		base.ValidateClickAndPlay();
// 	}

//     private string GetCurrentVideoUrl()
// 	{
// 		return thumbnailData.quality.mediumQualityVideoURL;
// 	}

// 	private void AddToLiveShowCheck()
// 	{
// 		if (liveShowLinks == null)
// 		{
// 			Debug.LogError("Live show links are null");
// 			return;
// 		}
		
// 		if(GameManager.Instance.liveshowManager.DateUrlCheckerMap.ContainsKey(liveshowTimeKey))
// 		{
// 			//Debug.LogWarning("Live show check already exists for time: " + liveshowTimeKey);
// 			FillDataLocally();
// 			return;
// 		}
// 		GameManager.Instance.liveshowManager.AddToLiveShowCheck(liveshowTimeKey, liveShowLinks);
// 		SubscribeToUrlChecks();
// 		GameManager.Instance.liveshowManager.StartUrlChecker(liveshowTimeKey);
// 	}

// 	private void FillDataLocally()
// 	{
// 		if (GameManager.Instance.liveshowManager.DateUrlCheckerMap.TryGetValue(liveshowTimeKey, out var item))
// 		{
// 			ui.SetDurationText(item.getDateTimeFileText());
// 			thumbnailData.liveFileTimingText = item.getDateTimeFileText();
// 		}
// 		else
// 		{
// 			DestroyThumbnail();
// 		}
// 	}

// 	private void SubscribeToUrlChecks()
// 	{
// 		var item = GameManager.Instance.liveshowManager.DateUrlCheckerMap[liveshowTimeKey];
// 		item.UrlCheckCompleted += OnUrlCheckCompleted;
// 		item.UrlCheckStarted += OnUrlCheckStarted;
// 		//item.UrlCheckProgress += (_, __) => { };
// 	}

// 	private void OnUrlCheckStarted(object sender, EventArgs e)
// 	{
// 		ui.setPrevLiveShowLoader(true);
// 	}

// 	private void OnUrlCheckCompleted(object sender, UrlCheckCompletedEventArgs e)
// 	{
// 		ui.setPrevLiveShowLoader(false);

// 		if (e.DateTimeText is null)
// 		{
// 			ui.SetDurationText("FILE NOT FOUND");
// 			// HomeManager.OnThumbnailsPopulated -= FetchLiveShowData;
// 			// HomeManager.OnInternetConnectionRestored -= FetchLiveShowData;
// 			DestroyThumbnail();
// 		}
// 		else
// 		{
// 			ui.SetDurationText(e.DateTimeText);
// 			thumbnailData.liveFileTimingText = e.DateTimeText;
// 			GetComponent<Button>().interactable = true;
// 		}
// 	}

// 	private void DestroyThumbnail()
//     {
// 		GameManager.Instance.liveshowManager.RemoveFromLiveShowCheck(liveshowTimeKey);
//         categoryManagement.removeLiveShow(this.gameObject);
//         Destroy(this.gameObject);
//     }

// }
