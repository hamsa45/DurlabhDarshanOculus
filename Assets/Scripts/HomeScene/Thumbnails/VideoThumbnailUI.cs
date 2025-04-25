// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.UI;
// using UnityEngine;
// using TMPro;
// using System.Threading.Tasks;

// public class VideoThumbnailUI : MonoBehaviour
// {
// 	[Header("Common UI Elements")]
// 	//public Image thumbnailIcon;
// 	public RawImage thumbnailIconRawImage;
// 	public TMP_Text videoTitle;
// 	public TMP_Text city;
// 	public TMP_Text videoDuration;
// 	public Button watchButton;
// 	public Button liveShowButton;
// 	public GameObject comingSoonButton;
// 	public GameObject newVideoTag;
// 	public GameObject adaptiveStreamingTag;
// 	public GameObject durationIcon;
// 	public GameObject aarthiIcon;
//     public GameObject documentaryIcon;
// 	public GameObject liveShowIcon;
// 	public GameObject liveTimingIcon;
// 	[SerializeField] private Image prevLiveShowLoader;

// 	public void SetBasicInfo(string title, string cityText, string titleHindi, string cityHindi)
// 	{
// 		videoTitle.text = title;
// 		city.text = cityText;

// 		var ctl = videoTitle.GetComponent<ChangeTextLanguage>();
// 		ctl.englishText = title;
// 		ctl.hindiText = titleHindi;

// 		var ctlCity = city.GetComponent<ChangeTextLanguage>();
// 		ctlCity.englishText = cityText;
// 		ctlCity.hindiText = cityHindi;
// 	}

// 	public void SetDuration(int durationSeconds)
// 	{
// 		var time = System.TimeSpan.FromSeconds(durationSeconds);
// 		videoDuration.text = time.Hours > 0 ? $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2} hr" : $"{time.Minutes}:{time.Seconds:D2} min";
// 	}

// 	public void SetDurationText(string text)
// 	{
// 		videoDuration.text = text;
// 	}

// 	public void ShowAdaptiveStreamingTag(bool show)
// 	{
// 		adaptiveStreamingTag.SetActive(show);
// 	}

// 	public void SetNewVideoTag(bool isNew)
// 	{
// 		newVideoTag.SetActive(isNew);
// 	}

// 	public void SetComingSoonState(bool isAvailable)
// 	{
// 		comingSoonButton.SetActive(!isAvailable);
// 		watchButton.interactable = isAvailable;
// 	}

// 	public async void SetImageFromUrl(string url, MonoBehaviour host, string cacheFileName = null)
// 	{
// 		if (string.IsNullOrEmpty(url)) return;
// 		ImageLoader loader = host.gameObject.AddComponent<ImageLoader>();
// 		loader.setUp(thumbnailIconRawImage, url);
// 		if(!string.IsNullOrWhiteSpace(cacheFileName)) loader.CacheFileName = cacheFileName;
// 		await loader.StartLoadingImage();
// 	}

// 	public void setThumbnailTypeIcon(ThumbnailType type)
// 	{
// 		aarthiIcon.SetActive(type == ThumbnailType.Aarti);
// 		documentaryIcon.SetActive(type == ThumbnailType.Documentary);
// 		liveShowIcon.SetActive(type == ThumbnailType.LiveShow);
// 	}

// 	public void setLiveShowTimingIcon(bool isActive)
// 	{
// 		liveTimingIcon.SetActive(isActive);
// 	}

// 	public void setDurationIcon(bool isActive)
// 	{
// 		durationIcon.SetActive(isActive);
// 	}

// 	public void setPrevLiveShowLoader(bool isActive)
// 	{
// 		prevLiveShowLoader.gameObject.SetActive(isActive);
// 	}
	
// }