using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;

public enum VideoState
{
    NotDownloaded,
    Downloading,
    Downloaded,
}

public class SelectedVideoPanel : MonoBehaviour
{
    public static SelectedVideoPanel instance;

    [SerializeField] private Texture defaultTexture;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI videoNameText;
    [SerializeField] private TextMeshProUGUI videoLocationText;
    [SerializeField] private TextMeshProUGUI videoDurationText;
    [SerializeField] private TextMeshProUGUI videoDescriptionText;

    [SerializeField] private TextMeshProUGUI videoSizeText;
    
    [SerializeField] private RawImage videoThumbnailImage;

    [Header("Panels")]
    [SerializeField] private GameObject NotDownloededpanel;
    [SerializeField] private GameObject downloadingPanel;
    [SerializeField] private GameObject Downloededpanel;
    [SerializeField] private GameObject LoadingPanel;

    [Header("Buttons")]
    [SerializeField] private Button downloadButton;
    [SerializeField] private Button downloadCancelButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;

    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup MainCanvasGroup;

    [Header("Downloading Progress")]
    [SerializeField] private TextMeshProUGUI downloadingProgressText;
    [SerializeField] private Image downloadingProgressBar;

    private VideoState videoState = VideoState.NotDownloaded;
    private AutoAdjustImage autoAdjustImage;

    void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        
        instance = this;
    }

    void Start()
    {
        videoThumbnailImage.TryGetComponent<AutoAdjustImage>(out AutoAdjustImage auto_);
        if(auto_ == null) auto_ = videoThumbnailImage.AddComponent<AutoAdjustImage>();
    }

    public void SetData(ThumbnailDTO thumbnailDTO)
    {
        if (videoLocationText != null)
            videoLocationText.text = thumbnailDTO.city;

        if (videoDurationText != null)
            videoDurationText.text = VideoThumbnailPanel.VideoDurationFullString(thumbnailDTO.videoDuration);

        if (videoThumbnailImage != null)
            UpdateSelectedImageImage(thumbnailDTO.thumbnailUrl);

        if (videoDescriptionText != null)
            videoDescriptionText.text = thumbnailDTO.description;

        if (videoNameText != null)
            videoNameText.text = thumbnailDTO.title;

        if (downloadButton != null)
            downloadButton.onClick.AddListener(() => onClickDownloadButton(thumbnailDTO));

        if (downloadCancelButton != null)
            downloadCancelButton.onClick.AddListener(() => onClickDownloadCancelButton(thumbnailDTO));

        if (playButton != null)
            playButton.onClick.AddListener(() => onClickPlayButton(thumbnailDTO));

        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => onClickDeleteDownloadButton(thumbnailDTO));

        if (backButton != null)
            backButton.onClick.AddListener(() => onClickBackButton());

        if (videoSizeText != null)
            videoSizeText.text = thumbnailDTO.HighQualityMediaSize;
        
        updateDownloadStatus(thumbnailDTO);
        StartCoroutine(UpdateSelectedVideo());
    }

    private void updateDownloadStatus(ThumbnailDTO thumbnailDTO)
    {
        //throw new NotImplementedException();
        if(thumbnailDTO.showUrls.high != null){
            string url = thumbnailDTO.showUrls.high;
            if(M3U8DownloadManager.Instance.HasDownload(url))
            {
                //confirm that download in on going
                subscribeToDownloadCallbacks(url);
                handleDownloadProgress();
                updatedDownloadProgress(M3U8DownloadManager.Instance.GetDownloadItem(url).getCurrentProgress());
            }
            else if (M3U8DownloadManager.Instance.IsDownloaded(url))
            {
                handleDownloadCompleted(url);
            }
            else
            {
                handleDownloadNotDownloaded();
            }
        }
    }

    private void subscribeToDownloadCallbacks(string url)
    {
        if (!M3U8DownloadManager.Instance.HasDownload(url)) return;

        var downloadItem = M3U8DownloadManager.Instance.GetDownloadItem(url);

        // Unsubscribe first to prevent duplicate subscriptions
        unsubscribeFromDownloadCallbacks(url);

        // Subscribe to events
        downloadItem.DownloadProgress += updatedDownloadProgress;
        downloadItem.DownloadCompleted += handleDownloadCompleted;
        downloadItem.DownloadInterrupted += handleDownloadInterrupted;

    }

    /// <summary>
    /// Unsubscribes a MediaQualityCard from download callbacks.
    /// </summary>
    private void unsubscribeFromDownloadCallbacks(string url)
    {
        if (!M3U8DownloadManager.Instance.HasDownload(url)) return;

        var downloadItem = M3U8DownloadManager.Instance.GetDownloadItem(url);

        // Unsubscribe from events
        downloadItem.DownloadProgress -= updatedDownloadProgress;
        downloadItem.DownloadCompleted -= handleDownloadCompleted;
        downloadItem.DownloadInterrupted -= handleDownloadInterrupted;

        //Debug.Log($"Unsubscribed from download callbacks for {url}");
    }

    private void updatedDownloadProgress(float progress)
    {
        if(downloadingProgressBar != null)
            downloadingProgressBar.fillAmount = progress;

        if(downloadingProgressText != null)
            downloadingProgressText.text = $"{progress * 100:0.00}%";
    }

    private void handleDownloadInterrupted(string obj)
    {
        videoState = VideoState.NotDownloaded;
        UpdateUI();
    }

    private void handleDownloadCompleted(string obj)
    {
        videoState = VideoState.Downloaded;
        UpdateUI();
    }

    private void handleDownloadProgress()
    {
        videoState = VideoState.Downloading;
        UpdateUI();
    }

    private void handleDownloadNotDownloaded()
    {
        videoState = VideoState.NotDownloaded;
        UpdateUI();
    }
    
    private async void UpdateSelectedImageImage(string url)
    {
        gameObject.TryGetComponent<ImageLoader>(out ImageLoader loader);
        
        if(loader == null){
            loader = gameObject.AddComponent<ImageLoader>();
        }
        loader.setUp(videoThumbnailImage, url);
        await loader.StartLoadingImage();
        if(videoThumbnailImage.texture == null){
            videoThumbnailImage.texture = defaultTexture;
        }
        if(autoAdjustImage != null){
            autoAdjustImage.adjustImage();
        }
    }

    public IEnumerator UpdateSelectedVideo()
    {
        LoadingPanel.SetActive(true);
        MainCanvasGroup.alpha = 0;
        UpdateUI(); 
        yield return new WaitForSeconds(0.1f);

        MainCanvasGroup.alpha = 1;
        LoadingPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        NotDownloededpanel.SetActive(videoState == VideoState.NotDownloaded);
        downloadingPanel.SetActive(videoState == VideoState.Downloading);
        Downloededpanel.SetActive(videoState == VideoState.Downloaded);
    }

    // private void OnDownloadProgress(float progress)
    // {
    //     if (downloadingProgressBar != null)
    //         downloadingProgressBar.fillAmount = progress;

    //     if (downloadingProgressText != null)
    //         downloadingProgressText.text = $"{progress * 100}%";
    // }

    private void onClickDownloadButton(ThumbnailDTO thumbnailDTO)
    {
        // Download the video

        //set the video state to downloading
        handleDownloadProgress();

        //start the download, but confirm internet connection
        NetworkMonitor.Instance.CheckInternetConntection(isConnected =>
        {
            if (!isConnected)
			{
                //Debug.LogWarning("Internet connection is Not available.");
                ToastManager.Toast.ErrorToast("Internet connection is Not available.");
                handleDownloadNotDownloaded();
                return;
            }

            M3U8DownloadManager.Instance.StartDownload(thumbnailDTO.showUrls.high);
            subscribeToDownloadCallbacks(thumbnailDTO.showUrls.high);
        });

    }

    private void onClickDownloadCancelButton(ThumbnailDTO thumbnailDTO)
    {
        // Cancel the download

        //inform the manager to cancel the download
        M3U8DownloadManager.Instance.StopDownload(thumbnailDTO.showUrls.high);

        //unsubscribe from the download callbacks
        unsubscribeFromDownloadCallbacks(thumbnailDTO.showUrls.high);

        //set the video state to not downloaded
        handleDownloadNotDownloaded();
    }

    private void onClickPlayButton(ThumbnailDTO thumbnailDTO) 
    {
        // Play the video
        //set this video data to current playing video data
        setCurrentVideoData(thumbnailDTO);

        //play the video
    }

    private void setCurrentVideoData(ThumbnailDTO thumbnailDTO)
    {
        CurrentVideoDataInPlay.thumbnailDTO = thumbnailDTO;
        CurrentVideoDataInPlay.currentVideoUrl = thumbnailDTO.showUrls.high;
        CurrentVideoDataInPlay.isLocalFile = true;
        CurrentVideoDataInPlay.isOptStreamFile = false;
    }

    private void onClickDeleteDownloadButton(ThumbnailDTO thumbnailDTO)
    {
        // Delete the video
        //inform manager to delete the video
        M3U8DownloadManager.Instance.Delete(thumbnailDTO.showUrls.high);

        //unsubscribe from the download callbacks
        unsubscribeFromDownloadCallbacks(thumbnailDTO.showUrls.high);

        //set the video state to not downloaded
        handleDownloadNotDownloaded();
    }

    private void onClickBackButton()
    {
        UIManagerMono.instance.ShowVideoListPanel();

        Debug.Log("On Back Button Click");
    }
}
