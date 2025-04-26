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

    [SerializeField] private GameObject thumbnailLoaderPanel;


    private Texture thumbnailTexture;

    void Awake()
    {
        thumbnailTexture = thumbnailImage.texture;
    }

    protected override async void UpdateUI()
    {
        if (data != null)
        {
            if (thumbnailLoaderPanel != null)
            {
                thumbnailLoaderPanel.SetActive(true);
            }
            if (videoNameText != null)
                videoNameText.text = data.title;

            if (videoDurationText != null)
                videoDurationText.text = VideoDurationString(data.videoDuration);

            if (videoLocationText != null)
                videoLocationText.text = data.city;

            if (thumbnailImage != null){
                gameObject.TryGetComponent<ImageLoader>(out ImageLoader loader);
                
                if(loader == null){
                    loader = gameObject.AddComponent<ImageLoader>();
                }
                loader.setUp(thumbnailImage, data.thumbnailUrl);
                await loader.StartLoadingImage();
                if(thumbnailImage.texture == null){
                    thumbnailImage.texture = thumbnailTexture;
                }
                autoAdjustRawImage.adjustImage(thumbnailImage);
            }

            if (thumbnailLoaderPanel != null)
            {
                thumbnailLoaderPanel.SetActive(false);
            }
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