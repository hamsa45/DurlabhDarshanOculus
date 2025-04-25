using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoData
{
    public string videoName;
    public string videoDuration;
    public string videoLocation;
    public Sprite thumbnail;
    public string videoPath;
    public string videoDurationDescription;

    public VideoQualityOptions videoQualityOptions;
}

public class VideoThumbnailPanel : ThumbnailPanel<VideoData>
{
    [SerializeField] private TextMeshProUGUI videoNameText;
    [SerializeField] private TextMeshProUGUI videoDurationText;
    [SerializeField] private TextMeshProUGUI videoLocationText;
    [SerializeField] private Button button;
    
    protected override void UpdateUI()
    {
        if (data != null)
        {
            if (thumbnailImage != null)
                thumbnailImage.sprite = data.thumbnail;
            
            if (videoNameText != null)
                videoNameText.text = data.videoName;

            if (videoDurationText != null)
                videoDurationText.text = data.videoDuration;

            if (videoLocationText != null)
                videoLocationText.text = data.videoLocation;
        }
    }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnPlayButtonClicked);
        }
    }

    private void OnPlayButtonClicked()
    {
        if (data != null)
        {
            // Handle video playback here
            // You can call a video player manager or trigger an event
            Debug.Log($"Playing video: {data.videoName} from path: {data.videoPath}");
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnPlayButtonClicked);
        }
    }
} 