using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoThumbnailPanel : ThumbnailPanel<ThumbnailDTO>
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
                //thumbnailImage.sprite = data.thumbnail;
                //autoAdjustRawImage.adjustImage(thumbnailImage);
            }
            
            if (videoNameText != null)
                videoNameText.text = data.title;

            if (videoDurationText != null)
                videoDurationText.text = VideoDurationString(data.videoDuration);

            if (videoLocationText != null)
                videoLocationText.text = data.city;
        }
    }

    public static string VideoDurationString(int videoDuration){
        int minutes = (videoDuration % 3600) / 60;
        int seconds = videoDuration % 60;
        return $"{minutes}:{seconds}";
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