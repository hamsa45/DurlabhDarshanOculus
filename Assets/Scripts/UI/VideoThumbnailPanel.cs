using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class VideoData
{
    public string videoName;
    public int videoDuration;
    public string videoLocation;
    public Sprite thumbnail;
    public string videoPath;
    public string videoDescription;

    public static string VideoDurationString(int videoDuration){
        int minutes = (videoDuration % 3600) / 60;
        int seconds = videoDuration % 60;
        return $"{minutes}:{seconds}";
    }
}

public class VideoThumbnailPanel : ThumbnailPanel<VideoData>
{
    [SerializeField] private TextMeshProUGUI videoNameText;
    [SerializeField] private TextMeshProUGUI videoDurationText;
    [SerializeField] private TextMeshProUGUI videoLocationText;
    [SerializeField] private Button button;
    [SerializeField] private AutoAdjustImage autoAdjustRawImage;

    protected override void UpdateUI()
    {
        if (data != null)
        {
            if (thumbnailImage != null){
                thumbnailImage.sprite = data.thumbnail;
                autoAdjustRawImage.adjustImage(thumbnailImage);
            }
            
            if (videoNameText != null)
                videoNameText.text = data.videoName;

            if (videoDurationText != null)
                videoDurationText.text = VideoData.VideoDurationString(data.videoDuration);

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
            UIManagerMono.instance.ShowSelectedVideoPanel();
            SelectedVideoPanel.instance.SetData(data);
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