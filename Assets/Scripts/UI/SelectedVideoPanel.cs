using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum VideoState
{
    NotDownloaded,
    Downloading,
    Downloaded,
}

public class SelectedVideoPanel : MonoBehaviour
{
    public static SelectedVideoPanel instance;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI videoNameText;
    [SerializeField] private TextMeshProUGUI videoLocationText;
    [SerializeField] private TextMeshProUGUI videoDurationText;
    [SerializeField] private TextMeshProUGUI videoDurationDescriptionText;
    
    [SerializeField] private Image videoThumbnailImage;

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


    void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        
        instance = this;
    }

    public void SetData(VideoData videoData)
    {
        if (videoLocationText != null)
            videoLocationText.text = videoData.videoLocation;

        if (videoDurationText != null)
            videoDurationText.text = VideoData.VideoDurationString(videoData.videoDuration);

        if (videoThumbnailImage != null)
            videoThumbnailImage.sprite = videoData.thumbnail;

        if (videoDurationDescriptionText != null)
            videoDurationDescriptionText.text = videoData.videoDescription;

        if (videoNameText != null)
            videoNameText.text = videoData.videoName;

        if (downloadButton != null)
            downloadButton.onClick.AddListener(() => DownloadVideo(videoData));

        if (downloadCancelButton != null)
            downloadCancelButton.onClick.AddListener(() => CancelDownload(videoData));

        if (playButton != null)
            playButton.onClick.AddListener(() => PlayVideo(videoData));

        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => DeleteVideo(videoData));

        if (backButton != null)
            backButton.onClick.AddListener(() => BackButton());
            
        CheckVideoState(videoData);
        UpdateUI();
    }

    private void CheckVideoState(VideoData videoData){

        // Check if the video is downloaded
    }

    public IEnumerator UpdateSelectedVideo(VideoData videoData)
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

    private void OnDownloadProgress(float progress)
    {
        if (downloadingProgressBar != null)
            downloadingProgressBar.fillAmount = progress;

        if (downloadingProgressText != null)
            downloadingProgressText.text = $"{progress * 100}%";
    }

    private void DownloadVideo(VideoData videoData)
    {
        // Download the video           
    }

    private void CancelDownload(VideoData videoData)
    {
        // Cancel the download
    }

    private void PlayVideo(VideoData videoData) 
    {
        // Play the video
    }

    private void DeleteVideo(VideoData videoData)
    {
        // Delete the video
    }

    private void BackButton()
    {
        UIManagerMono.instance.ShowVideoListPanel();
    }
}
