using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

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
            downloadButton.onClick.AddListener(() => DownloadVideo(thumbnailDTO));

        if (downloadCancelButton != null)
            downloadCancelButton.onClick.AddListener(() => CancelDownload(thumbnailDTO));

        if (playButton != null)
            playButton.onClick.AddListener(() => PlayVideo(thumbnailDTO));

        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => DeleteVideo(thumbnailDTO));

        if (backButton != null)
            backButton.onClick.AddListener(() => BackButton());

        if (videoSizeText != null)
            videoSizeText.text = thumbnailDTO.HighQualityMediaSize;
        
        CheckVideoState(thumbnailDTO);
        StartCoroutine(UpdateSelectedVideo());
    }


    private async void UpdateSelectedImageImage(string url){
        
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
    private void CheckVideoState(ThumbnailDTO thumbnailDTO){

        // Check if the video is downloaded
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

    private void OnDownloadProgress(float progress)
    {
        if (downloadingProgressBar != null)
            downloadingProgressBar.fillAmount = progress;

        if (downloadingProgressText != null)
            downloadingProgressText.text = $"{progress * 100}%";
    }

    private void DownloadVideo(ThumbnailDTO thumbnailDTO)
    {
        // Download the video           
    }

    private void CancelDownload(ThumbnailDTO thumbnailDTO)
    {
        // Cancel the download
    }

    private void PlayVideo(ThumbnailDTO thumbnailDTO) 
    {
        // Play the video
    }

    private void DeleteVideo(ThumbnailDTO thumbnailDTO)
    {
        // Delete the video
    }

    private void BackButton()
    {
        UIManagerMono.instance.ShowVideoListPanel();

        Debug.Log("On Back Button Click");
    }
}
